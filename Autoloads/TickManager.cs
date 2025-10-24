using Godot;
using System;

namespace TwitchBrb.Autoloads;

public partial class TickManager : Node
{
    public static TickManager Instance;

    private const double TICK_INTERVAL = 5.0; // Tick every 5 seconds
    private double _timer;

    public override void _Ready()
    {
        Instance = this;
    }

    public override void _Process(double delta)
    {
        _timer += delta;
        if (_timer >= TICK_INTERVAL)
        {
            _timer = 0.0;
            Tick();
        }
    }

    private void Tick() => SignalManager.Instance.EmitTick();
}