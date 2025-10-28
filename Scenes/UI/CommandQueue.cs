using Godot;
using TwitchBrb.Autoloads;

namespace TwitchBrb.Scenes.UI;

public partial class CommandQueue : RichTextLabel
{
    public override void _Ready()
    {
        SignalManager.Instance.Connect(
            nameof(SignalManager.CommandQueued),
            new Callable(this, nameof(OnCommandQueued))
        );

        SignalManager.Instance.Connect(
            nameof(SignalManager.CommandDequeued),
            new Callable(this, nameof(OnCommandDequeued))
        );
    }

    private void OnCommandQueued(string command)
    {
        RefreshQueue();
    }

    private void OnCommandDequeued()
    {
        RefreshQueue();
    }

    private void RefreshQueue()
    {
        Clear();

        for (int i = 0; i < QueueManager.Instance.CommandQueue.Count; i++)
        {
            var command = QueueManager.Instance.CommandQueue[i];
            AppendText($"[i]Queued Command:[/i] {command}\n");
        }

        ScrollToLine(GetLineCount() - 1);
    }
}
