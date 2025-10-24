using Godot;

namespace TwitchBrb.Scenes.UI;

public partial class TwitchChat : RichTextLabel
{
    public override void _Ready()
    {
        SignalManager.Instance.Connect(
            nameof(SignalManager.ChatMessageReceived),
            new Callable(this, nameof(OnChatMessageReceived))
        );
        SignalManager.Instance.Connect(
            nameof(SignalManager.ChatMessageReceivedRich),
            new Callable(this, nameof(OnChatMessageReceivedRich))
        );
    }

    private void OnChatMessageReceived(string user, string message)
    {
        AppendText($"[b]{user}:[/b] {message}\n");
        ScrollToLine(GetLineCount() - 1);
    }

    private void OnChatMessageReceivedRich(Godot.Collections.Dictionary msg)
    {
        string user = msg["user"].ToString();
        string message = msg["message"].ToString();
        GD.Print(msg);
        AppendText($"[b]{user}:[/b] {message}\n");
        ScrollToLine(GetLineCount() - 1);
    }
}
