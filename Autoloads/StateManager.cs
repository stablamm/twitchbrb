using Godot;

namespace TwitchBrb.Autoloads;

public partial class StateManager : Node
{
    public static StateManager Instance;

    public bool IsFarmerBusy { get; set; } = false;

    public override void _Ready()
    {
        Instance = this;
    }
}
