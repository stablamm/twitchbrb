using Godot;
using System;
using TwitchBrb.Scenes.Crops;

public partial class Corn : Crop
{
    // Frame 52-55

    public override void _Ready()
    {
        base._Ready();
        CropID = Guid.NewGuid().ToString();
        CropName = "Corn";
        CropState = ICrop.CROP_STATE.SEEDLING;
    }

    public override void InitializeCrop(Vector2 pos)
    {
        base.InitializeCrop(pos);

        CropLocation = pos;
    }
}
