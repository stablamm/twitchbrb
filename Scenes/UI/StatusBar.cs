using Godot;
using System;
using TwitchBrb.Autoloads;

public partial class StatusBar : Node2D
{
    private Label _connectionStatus;

    public override void _Ready()
    {
        _connectionStatus = GetNode<Label>("%ConnectionStatus");
    }

    public override void _Process(double delta)
    {
        bool isConnected = TwitchChatManager.Instance.IsRunning;
        _connectionStatus.Text = isConnected ? "Connected" : "Disconnected";
    }
}
