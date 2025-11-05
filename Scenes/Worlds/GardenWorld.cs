using Godot;
using TwitchBrb.Autoloads;

namespace TwitchBrb.Scenes.Worlds;

public partial class GardenWorld : Node2D
{
    private int _farmWidth;
    private int _farmHeight;

    [Export] public Farmer.Farmer TwitchFarmer;

    private FarmingMapLayer _farmingMapLayer;

    public override void _Ready()
    {
        _farmingMapLayer = GetNode<FarmingMapLayer>("%FarmingMapLayer");

        _farmWidth = _farmingMapLayer.GetUsedRect().Size.X;
        _farmHeight = _farmingMapLayer.GetUsedRect().Size.Y;

        InitializeFarm();

        SignalManager.Instance.Connect(
            SignalManager.SignalName.PlantSeed,
            new Callable(this, nameof(OnPlantSeed))
        );

        SignalManager.Instance.Connect(
            SignalManager.SignalName.PlantSeedCommand,
            new Callable(this, nameof(OnPlantSeedCommand)));

        SignalManager.Instance.Connect(
            SignalManager.SignalName.WaterCropCommand,
            new Callable(this, nameof(OnWaterCropCommand))
        );

        SignalManager.Instance.Connect(
            SignalManager.SignalName.CommandRunNextCommand
            , new Callable(this, nameof(OnRunNextCommand))
        );

        SignalManager.Instance.Connect(
            SignalManager.SignalName.FarmerArrived,
            new Callable(this, nameof(OnFarmerArrived))
        );
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseButton)
        {
            if (mouseButton.ButtonIndex == MouseButton.Left && mouseButton.IsPressed() && !mouseButton.IsEcho())
            {
                var isSoilTile = _farmingMapLayer.GetCustomTileData<bool>(_farmingMapLayer.GetMouseCell(), "IsSoil");
                if (!isSoilTile) return;

                PrintDebugData();
                TwitchFarmer.MoveTo(_farmingMapLayer.GetGlobalMousePosition());

            }
        }
        else if (@event.IsAction("Spawn_Crop"))
        {
            var corn = CropManager.Instance.GetCrop<Corn>(CropManager.CROP_TYPE.CORN);
            CropManager.Instance.GetRandomUnplantedLocation(out Vector2? target);
            
            if (target == null)
            {
                GD.Print("No unplanted locations available.");
                return;
            }

            corn.Position = (Vector2)(target * 32 + new Vector2(16, 16));
            AddChild(corn);

            CropManager.Instance.RegisterCropAtLocation(target ?? Vector2.Zero, corn);
        }
    }

    public void OnPlantSeed() => PlantRandomCrop();
    public void OnPlantSeedCommand(string command)
    {
        var tileToPlant = TwitchCommandManager.Instance.ParseCommand<Vector2>(
            TwitchCommandManager.COMMAND_TYPE.PLANT,
            command
        );

        if (tileToPlant <= Vector2.Zero)
        {
            //PlantRandomCrop();
            GD.PrintErr($"Unable to plant at {tileToPlant}. Out of bounds.");
            return;
        }

        PlantCrop(tileToPlant);
    }

    public void OnWaterCrop(Vector2 pos) => WaterCrop(pos);
    public void OnWaterCropCommand(string command)
    {
        var tileToWater = TwitchCommandManager.Instance.ParseCommand<Vector2>(
            TwitchCommandManager.COMMAND_TYPE.WATER,
            command
        );

        if (tileToWater <= Vector2.Zero)
        {
            WaterRandomCrop();
            return;
        }

        WaterCrop(tileToWater);
    }

    public void OnRunNextCommand()
    
    {
        if (QueueManager.Instance.CommandQueue.Count == 0) return;
        var command = QueueManager.Instance.GetNextCommand();
        if (string.IsNullOrEmpty(command)) return;

        var nextPosition = TwitchCommandManager.Instance.ParseCommand<Vector2>(
            TwitchCommandManager.Instance.GetCommandType(command),
            command
        );

        if (nextPosition <= Vector2.Zero)
        {
            GD.PrintErr($"Invalid next position parsed from command: {command}");
            return;
        }

        var offset = new Vector2(32, 64);
        var nextPos = nextPosition * 32 * Scale;

        TwitchFarmer.MoveTo(nextPos + offset);


    }

    public void OnFarmerArrived()
    {
        var command = QueueManager.Instance.DequeueCommand();
        var commandType = TwitchCommandManager.Instance.GetCommandType(command);
        
        switch (commandType)
        {
            case TwitchCommandManager.COMMAND_TYPE.PLANT:
                OnPlantSeedCommand(command);
                break;
            case TwitchCommandManager.COMMAND_TYPE.WATER:
                OnWaterCropCommand(command);
                break;
            default:
                GD.PrintErr($"Unknown command type for command: {command}");
                break;
        }
    }

    private void InitializeFarm()
    {
        for (int x = 0; x < _farmWidth; x++)
        {
            for (int y = 0; y < _farmHeight; y++)
            {
                Vector2I cell = new Vector2I(x, y);
                bool isSoilTile = _farmingMapLayer.GetCustomTileData<bool>(cell, "IsSoil");
                if (isSoilTile)
                {
                    CropManager.Instance.RegisterLocation(cell);
                }
            }
        }
    }

    private void PlantRandomCrop()
    {
        var corn = CropManager.Instance.GetCrop<Corn>(CropManager.CROP_TYPE.CORN);
        CropManager.Instance.GetRandomUnplantedLocation(out Vector2? target);

        if (target == null)
        {
            GD.Print("No unplanted locations available.");
            return;
        }

        corn.Position = (Vector2)(target * 32 + new Vector2(16, 16));
        AddChild(corn);

        CropManager.Instance.RegisterCropAtLocation(target ?? Vector2.Zero, corn);
    }

    private void WaterRandomCrop()
    {
        int bailoutCounter = 1000;
        while (true)
        {
            var randomCropID = CropManager.Instance.GetRandomPlantedCropID();
            if (randomCropID == null) break;
            var crop = CropManager.Instance.CropsByID[randomCropID];
            if (!crop.IsWatered)
            {
                GD.Print(randomCropID);
                CropManager.Instance.WaterCropByCropID(randomCropID);
                break;
            }

            bailoutCounter--;
            if (bailoutCounter <= 0) break;
        }
    }

    private void WaterCrop(Vector2 pos)
    {
        if (pos == new Vector2(-1, -1))
        {
            WaterRandomCrop();
            return;
        }

        var cropID = CropManager.Instance.CropsByLocation[pos];
        if (string.IsNullOrEmpty(cropID)) return;
        CropManager.Instance.WaterCropByCropID(cropID);
    }

    private void PlantCrop(Vector2 pos)
    {
        var cropID = CropManager.Instance.CropsByLocation[pos];
        if (!string.IsNullOrEmpty(cropID)) return; // Already planted

        var crop = CropManager.Instance.GetCrop<Corn>(CropManager.CROP_TYPE.CORN);
        crop.Position = (Vector2)(pos * 32 + new Vector2(16, 16));
        AddChild(crop);

        CropManager.Instance.RegisterCropAtLocation(pos, crop);
    }

    private void PrintDebugData()
    {
        GD.Print(_farmingMapLayer.GetMouseCell());
        GD.Print(CropManager.Instance.CropsByLocation[_farmingMapLayer.GetMouseCell()]);
        GD.Print(GetGlobalMousePosition());
    }
}
