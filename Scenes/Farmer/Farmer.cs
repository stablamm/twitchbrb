using Godot;
using TwitchBrb.Autoloads;

namespace TwitchBrb.Scenes.Farmer;

public partial class Farmer : CharacterBody2D
{
    [Export] public float MoveSpeed { get; set; } = 100f;

    private Vector2 _targetPosition;
    private bool _isMoving = false;

    public override void _PhysicsProcess(double delta)
    {
        if (!_isMoving) return;

        Vector2 direction = _targetPosition - GlobalPosition;
        float distance = direction.Length();

        if (distance < 1f)
        {
            _isMoving = false;
            Velocity = Vector2.Zero;
            MoveAndSlide();
            StateManager.Instance.IsFarmerBusy = false;
            SignalManager.Instance.EmitFarmerArrived();
            return;
        }

        direction = direction.Normalized();
        Velocity = direction * MoveSpeed;

        StateManager.Instance.IsFarmerBusy = true;
        MoveAndSlide();
    }

    public void MoveTo(Vector2 position)
    {
        _targetPosition = position;
        _isMoving = true;
    }
}
