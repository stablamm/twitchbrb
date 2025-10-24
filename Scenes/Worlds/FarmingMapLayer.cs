using Godot;
using System;

namespace TwitchBrb.Scenes.Worlds;

public partial class FarmingMapLayer : TileMapLayer
{
    public Vector2I GetMouseCell() => LocalToMap(ToLocal(GetGlobalMousePosition()));
    public Vector2I GetMouseCell(Vector2 worldPosition) => LocalToMap(ToLocal(worldPosition));

    public T GetCustomTileData<[MustBeVariant] T>(Vector2I cell, string customDataLayer)
    {
        TileData tileData = GetCellTileData(cell);
        if (tileData == null)
        {
            GD.PrintErr($"No tile data found at position {cell}");
            return default;
        }
        return tileData.GetCustomData(customDataLayer).As<T>();
    }
}
