using Godot;
using System;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Text;
using System.Threading;

namespace TwitchBrb.Autoloads;

public partial class TwitchChatManager : Node
{
    public static TwitchChatManager Instance { get; private set; }
    
    private volatile bool _isRunning;
    public bool IsRunning
    {
        get { return _isRunning; }
        set { _isRunning = value; }
    }

    private Thread _chatThread;
    private TcpClient _client;
    private SslStream _ssl;
    private StreamReader _reader;
    private StreamWriter _writer;

    private string _username;
    private string _channel;
    private string _oauthToken;

    public override void _Ready() => Instance = this;

    public void StartChatListener(string username, string channel, string oauthToken)
    {
        if (_isRunning)
        {
            GD.Print("⚠️ Listener already running. Use RestartChatListener.");
            return;
        }

        _username = username;
        _channel = channel;
        _oauthToken = oauthToken;

        _isRunning = true;
        _chatThread = new Thread(ChatWorker) { IsBackground = true };
        _chatThread.Start();
    }

    public void StopChatListener()
    {
        if (!_isRunning) return;
        GD.Print("🛑 Stopping Twitch chat listener...");

        _isRunning = false;

        try { _reader?.Dispose(); } catch { }
        try { _writer?.Dispose(); } catch { }
        try { _ssl?.Dispose(); } catch { }
        try { _client?.Close(); } catch { }

        _chatThread?.Join();
        _chatThread = null;

        _client = null;
        _ssl = null;
        _reader = null;
        _writer = null;

        GD.Print("✅ Chat listener stopped.");
    }

    public void RestartChatListener(string username, string channel, string oauthToken)
    {
        StopChatListener();
        StartChatListener(username, channel, oauthToken);
    }

    public void SendMessage(string text)
    {
        if (!_isRunning || _writer == null)
        {
            GD.PrintErr("❌ Chat not connected — cannot send message.");
            return;
        }

        if (string.IsNullOrWhiteSpace(text))
            return;

        try
        {
            _writer.WriteLine($"PRIVMSG #{_channel} :{text}");
            GD.Print($"📤 Sent message: {text}");
        }
        catch (Exception ex)
        {
            GD.PrintErr($"❌ Failed to send message: {ex.Message}");
        }
    }

    private void ChatWorker()
    {
        const string host = "irc.chat.twitch.tv";
        const int port = 6697; // TLS

        try
        {
            _client = new TcpClient();
            _client.NoDelay = true;
            _client.Connect(host, port);

            _ssl = new SslStream(_client.GetStream(), leaveInnerStreamOpen: false);

            _ssl.AuthenticateAsClient(host, null, SslProtocols.Tls12 | SslProtocols.Tls13, checkCertificateRevocation: true);

            var utf8NoBom = new System.Text.UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

            // After ssl.AuthenticateAsClient(...)
            _reader = new StreamReader(_ssl, utf8NoBom, detectEncodingFromByteOrderMarks: false, bufferSize: 8192, leaveOpen: true);
            _writer = new StreamWriter(_ssl, utf8NoBom, bufferSize: 8192, leaveOpen: true)
            {
                NewLine = "\r\n",
                AutoFlush = true
            };

            // Now these will be parsed correctly by Twitch IRC:
            _writer.WriteLine($"PASS {_oauthToken}");      // must be: oauth:xxxxxxxx
            _writer.WriteLine($"NICK {_username}");
            _writer.WriteLine("CAP REQ :twitch.tv/tags twitch.tv/commands twitch.tv/membership");
            _writer.WriteLine($"JOIN #{_channel}");

            GD.Print($"✅ Connected to Twitch chat #{_channel}");

            // 5) Read loop
            while (_isRunning)
            {
                var line = _reader.ReadLine(); // will block; StopChatListener() closes socket to unblock
                if (line == null) StopChatListener();

                if (line.StartsWith("PING", StringComparison.Ordinal))
                {
                    _writer.WriteLine(line.Replace("PING", "PONG"));
                    continue;
                }

                if (line.Contains("PRIVMSG", StringComparison.Ordinal))
                {
                    var msg = ParseIrcPrivMsg(line);
                    if (msg != null)
                    {
                        // Simple signal (back-compat)
                        SignalManager.Instance?.CallDeferred(
                            nameof(SignalManager.EmitChatMessageReceived),
                            msg["username"], msg["message"]
                        );
                        // Rich signal (everything you parsed)
                        SignalManager.Instance?.CallDeferred(
                            nameof(SignalManager.EmitChatMessageReceivedRich),
                            msg
                        );
                    }
                }
                else if (line.Contains("NOTICE", StringComparison.Ordinal))
                {
                    //:tmi.twitch.tv NOTICE * :Login authentication failed
                    if (line.ToLower().Contains("authentication failed"))
                    {
                        GD.PrintErr("❌ Twitch chat authentication failed. Check your OAuth token.");
                        StopChatListener();
                    }
                }
            }
        }
        catch (Exception e)
        {
            if (_isRunning)
                GD.PrintErr($"❌ Twitch chat connection error: {e.Message}");
        }
        finally
        {
            try { _reader?.Dispose(); } catch { }
            try { _writer?.Dispose(); } catch { }
            try { _ssl?.Dispose(); } catch { }
            try { _client?.Close(); } catch { }
        }
    }

    public override void _ExitTree() => StopChatListener();

    public void RefreshUsername(string username) => _username = username;
    public void RefreshChannel(string channel) => _channel = channel;
    public void RefreshOAuthToken(string oauthToken) => _oauthToken = oauthToken;

    private Godot.Collections.Dictionary ParseIrcPrivMsg(string line)
    {
        int idx = 0;
        var tags = new Godot.Collections.Dictionary<string, string>();

        // 1) Tags
        if (idx < line.Length && line[idx] == '@')
        {
            int sp = line.IndexOf(' ');
            if (sp == -1) return null;
            var tagStr = line.Substring(1, sp - 1);
            foreach (var pair in tagStr.Split(';'))
            {
                var kv = pair.Split('=', 2);
                var key = kv[0];
                var val = kv.Length > 1 ? UnescapeTag(kv[1]) : "";
                tags[key] = val;
            }
            idx = sp + 1;
        }

        // 2) Prefix
        string prefix = null;
        if (idx < line.Length && line[idx] == ':')
        {
            int sp = line.IndexOf(' ', idx);
            if (sp == -1) return null;
            prefix = line.Substring(idx + 1, sp - idx - 1);
            idx = sp + 1;
        }

        // 3) Command
        int spc = line.IndexOf(' ', idx);
        if (spc == -1) return null;
        var command = line.Substring(idx, spc - idx);
        idx = spc + 1;

        // 4) Params (channel + trailing)
        string channel = null;
        string message = null;

        // next token (channel)
        spc = line.IndexOf(' ', idx);
        if (spc == -1) spc = line.Length;
        if (idx < line.Length)
        {
            channel = line.Substring(idx, spc - idx);
            idx = spc;
        }

        // trailing after " :"
        int colon = line.IndexOf(" :", idx, StringComparison.Ordinal);
        if (colon >= 0)
            message = line.Substring(colon + 2);
        else
            message = "";

        // Username from prefix
        string username = prefix;
        int bang = prefix?.IndexOf('!') ?? -1;
        if (bang > 0) username = prefix.Substring(0, bang);

        // Friendly fields from tags
        string displayName = tags.TryGetValue("display-name", out var dn) && !string.IsNullOrEmpty(dn) ? dn : username;
        string color = tags.TryGetValue("color", out var col) ? col : "";
        bool isMod = tags.TryGetValue("mod", out var mod) && mod == "1";
        bool isSub = tags.TryGetValue("subscriber", out var sub) && sub == "1";
        bool isBroadcaster = false;
        if (tags.TryGetValue("badges", out var badgesStr))
            isBroadcaster = badgesStr.Contains("broadcaster/");

        string userId = tags.TryGetValue("user-id", out var uid) ? uid : "";
        string roomId = tags.TryGetValue("room-id", out var rid) ? rid : "";
        long sentTs = tags.TryGetValue("tmi-sent-ts", out var ts) && long.TryParse(ts, out var ms) ? ms : 0;

        // Parse badges to an array of {name, version}
        var badgeArray = new Godot.Collections.Array<Godot.Collections.Dictionary>();
        if (!string.IsNullOrEmpty(badgesStr))
        {
            foreach (var b in badgesStr.Split(',', StringSplitOptions.RemoveEmptyEntries))
            {
                var nv = b.Split('/', 2);
                var d = new Godot.Collections.Dictionary
                {
                    ["name"] = nv[0],
                    ["version"] = nv.Length > 1 ? nv[1] : ""
                };
                badgeArray.Add(d);
            }
        }

        // Pack tags into a Godot Dictionary for convenience
        var tagsDict = new Godot.Collections.Dictionary();
        foreach (var kv in tags) tagsDict[kv.Key] = kv.Value;

        var dict = new Godot.Collections.Dictionary
        {
            ["raw"] = line,
            ["command"] = command,
            ["channel"] = channel?.TrimStart('#') ?? "",
            ["message"] = message ?? "",
            ["username"] = username ?? "",
            ["display_name"] = displayName,
            ["color"] = color,
            ["user_id"] = userId,
            ["room_id"] = roomId,
            ["sent_at"] = sentTs, // ms since epoch
            ["is_mod"] = isMod,
            ["is_sub"] = isSub,
            ["is_broadcaster"] = isBroadcaster,
            ["badges"] = badgeArray,
            ["tags"] = tagsDict
        };

        return dict;
    }

    // IRCv3 tag unescape per spec: \: => ;  \s => space  \n => \n  \r => \r  \\ => \
    private static string UnescapeTag(string v)
    {
        if (string.IsNullOrEmpty(v)) return v;
        var sb = new StringBuilder(v.Length);
        for (int i = 0; i < v.Length; i++)
        {
            char c = v[i];
            if (c == '\\' && i + 1 < v.Length)
            {
                char n = v[++i];
                sb.Append(n switch
                {
                    ':' => ';',
                    's' => ' ',
                    'n' => '\n',
                    'r' => '\r',
                    '\\' => '\\',
                    _ => n
                });
            }
            else sb.Append(c);
        }
        return sb.ToString();
    }
}
