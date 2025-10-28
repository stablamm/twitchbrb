using Godot;
using System.Collections.Generic;

namespace TwitchBrb.Autoloads;

public partial class QueueManager : Node
{
    public static QueueManager Instance;

    public List<string> CommandQueue { get; private set; } = new List<string>();

    public override void _Ready()
    {
        Instance = this;
    }

    public void EnqueueCommand(string command)
    { 
        CommandQueue.Add(command);
        SignalManager.Instance.EmitCommandQueued(command);
    }

    public string GetNextCommand()
    {
        if (CommandQueue.Count == 0)
        {
            return string.Empty;
        }
        
        return CommandQueue[0];
    }

    public string DequeueCommand()
    {
        if (CommandQueue.Count == 0)
        {
            return string.Empty;
        }
        
        var command = CommandQueue[0];
        CommandQueue.RemoveAt(0);
        
        SignalManager.Instance.EmitCommandDequeued();

        return command;
    }
}
