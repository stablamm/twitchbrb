using Godot;
using System;

namespace TwitchBrb.Scenes.Menus;

public partial class SettingsMenu : Node2D
{
    public void OnBackButtonPressed()
    {
        GetTree().ChangeSceneToFile("res://Scenes/main_menu.tscn");
    }
}
