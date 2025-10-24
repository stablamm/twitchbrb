using Godot;
using System;
using System.Collections.Generic;

namespace TwitchBrb.Scenes.Worlds;

public partial class SoilMapLayer : TileMapLayer
{
    // Key: Map Location (Vector2), Value: IsWatered (bool)
    public Dictionary<Vector2, bool> SoilWateredStatus = new Dictionary<Vector2, bool>();

    private Vector2 _drySoilTile = new Vector2(0, 2);
    private Vector2 _wetSoilTile = new Vector2(0, 3);

    public override void _Ready()
    {
        base._Ready();

        SignalManager.Instance.Connect(
            SignalManager.SignalName.WaterCrop,
            new Callable(this, nameof(OnWaterCrop))
        );
        SignalManager.Instance.Connect(
            SignalManager.SignalName.OnTick,
            new Callable(this, nameof(OnTick))
        );
    }

    private void InitializeSoilWaterStatus()
    {
        var usedRect = GetUsedRect();
        for (int x = usedRect.Position.X; x < usedRect.End.X; x++)
        {
            for (int y = usedRect.Position.Y; y < usedRect.End.Y; y++)
            {
                Vector2I cell = new Vector2I(x, y);
                TileData tileData = GetCellTileData(cell);
                var isSoilTile = tileData.GetCustomData("IsSoil").As<bool>();
                if (isSoilTile)
                {
                    SoilWateredStatus[cell] = false;
                }
            }
        }
    }

    public void OnWaterCrop(Vector2 pos)
    {
        SoilWateredStatus[pos] = true;
        SetCell((Vector2I)pos, 0, (Vector2I)_wetSoilTile);
    }

    public void OnTick()
    {

    }
}
