using Godot;

namespace TwitchBrb.Scenes.Items;

public partial class CropItem : Node2D
{
    public ICrop.CROP_TYPE CropType { get; set; }
}
