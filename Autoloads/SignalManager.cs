using Godot;

public partial class SignalManager : Node
{
    public static SignalManager Instance { get; private set; }

    [Signal] public delegate void ChatMessageReceivedEventHandler(string user, string message);
    [Signal] public delegate void ChatMessageReceivedRichEventHandler(Godot.Collections.Dictionary msg);
    [Signal] public delegate void PlantSeedEventHandler(); // Will always plant at a random location
    [Signal] public delegate void PlantSeedCommandEventHandler(string command);
    [Signal] public delegate void WaterCropCommandEventHandler(string command);
    [Signal] public delegate void WaterCropEventHandler(Vector2 pos);
    [Signal] public delegate void OnTickEventHandler();

    public override void _Ready()
    {
        Instance = this;
    }

    public void EmitChatMessageReceived(string user, string message)
        => EmitSignal(SignalName.ChatMessageReceived, user, message);

    public void EmitChatMessageReceivedRich(Godot.Collections.Dictionary msg)
        => EmitSignal(SignalName.ChatMessageReceivedRich, msg);

    public void EmitPlantSeedCommand(string command) 
        => EmitSignal(SignalName.PlantSeedCommand, command);
    
    public void EmitPlantSeed() => EmitSignal(SignalName.PlantSeed);
    
    public void EmitWaterCropCommand(string command) 
        => EmitSignal(SignalName.WaterCropCommand, command);
    
    public void EmitWaterCrop(Vector2 pos) => EmitSignal(SignalName.WaterCrop, pos);
    
    public void EmitTick() => EmitSignal(SignalName.OnTick);
}
