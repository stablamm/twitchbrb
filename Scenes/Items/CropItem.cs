using Godot;
using System;
using static TwitchBrb.Autoloads.CropManager;

namespace TwitchBrb.Scenes.Items;

public partial class CropItem : Node2D
{
    public CROP_TYPE CropType { get; set; }
}
