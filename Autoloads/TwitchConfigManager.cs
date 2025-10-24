using Godot;
using System;
using System.IO;
using System.Text.Json;
using TwitchBrb.Scripts;

namespace TwitchBrb.Autoloads;

public partial class TwitchConfigManager : Node
{
    public readonly string TokenFile = Path.Combine(ProjectSettings.GlobalizePath("user://"), "twitch_tokens.json");
    public readonly string AuthFile = Path.Combine(ProjectSettings.GlobalizePath("user://"), "twitch_auth.json");
    public readonly string ChatManagerFile = Path.Combine(ProjectSettings.GlobalizePath("user://"), "twitch_chat_manager.json");
    
    public static TwitchConfigManager Instance { get; private set; }
    
    public override void _Ready()
    {
        Instance = this;
    }

    #region Token File
    public void TokenConfigSave(TokenResponse tokens)
    {
        string json = JsonSerializer.Serialize(tokens, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(TokenFile, json);
        GD.Print($"💾 Tokens saved to {TokenFile}");
    }
    #endregion

    #region Auth File
    public void AuthConfigSave(string clientId, string clientSecret)
    {
        var authData = new AuthData
        {
            EncodedClientId = TwitchUtilities.Encode(clientId),
            EncodedClientSecret = TwitchUtilities.Encode(clientSecret)
        };

        var json = JsonSerializer.Serialize(authData, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(AuthFile, json);

        GD.Print($"💾 Saved encoded credentials to: {AuthFile}");
    }

    public (string ClientId, string ClientSecret) AuthConfigLoadCredentials()
    {
        if (!File.Exists(AuthFile))
        {
            GD.Print("No saved credentials found.");
            return (string.Empty, string.Empty);
        }

        try
        {
            var json = File.ReadAllText(AuthFile);
            var authData = JsonSerializer.Deserialize<AuthData>(json);
            if (authData == null) return (string.Empty, string.Empty);

            string clientId = TwitchUtilities.Decode(authData.EncodedClientId);
            string clientSecret = TwitchUtilities.Decode(authData.EncodedClientSecret);

            return (clientId, clientSecret);
        }
        catch (Exception ex)
        {
            GD.PrintErr($"Error loading credentials: {ex.Message}");
            return (string.Empty, string.Empty);
        }
    }

    #endregion

    #region Chat Manager File
    public void ChatManagerConfigSave(string username, string channel)
    {
        var cfg = new ChannelConfig
        {
            Username = username,
            Channel = channel
        };

        var json = JsonSerializer.Serialize(cfg, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(ChatManagerFile, json);

        GD.Print($"💾 Saved channel config to: {ChatManagerFile}");
    }

    public (string username, string channel) ChatManagerConfigLoad()
    {
        if (!File.Exists(ChatManagerFile))
        {
            GD.Print("No saved channel config found.");
            return ("", "");
        }

        try
        {
            var json = File.ReadAllText(ChatManagerFile);
            var cfg = JsonSerializer.Deserialize<ChannelConfig>(json);
            if (cfg == null) return ("", "");
            return (cfg.Username, cfg.Channel);
        }
        catch (Exception ex)
        {
            GD.PrintErr($"Error loading channel config: {ex.Message}");
            return ("", "");
        }
    } 
    #endregion

    private class ChannelConfig
    {
        public string Username { get; set; }
        public string Channel { get; set; }
    }
}
