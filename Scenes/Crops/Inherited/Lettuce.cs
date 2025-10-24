using System;

namespace TwitchBrb.Scenes.Crops.Inherited;

public partial class Lettuce : Crop
{
    public override void _Ready()
    {
        base._Ready();

        CropID = Guid.NewGuid().ToString();
        CropName = "Lettuce";
        CropState = ICrop.CROP_STATE.SEEDLING;

        //GrowingStages = 2;
        //GrowingFrame = new int[2];
        //GrowingFrame[0] = 2;
        //GrowingFrame[1] = 3;
        //GrownFrame = 4;
        //DecayedFrame = 5;
    }
}
