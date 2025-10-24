using Godot;
using static ICrop;

namespace TwitchBrb.Scenes.Crops;

public partial class Crop : Node2D, ICrop
{
    public string CropID { get; set; }
    public string CropName { get; set; }
    public CROP_STATE CropState { get; set; }
    public Vector2 CropLocation { get; set; }
    public bool IsWatered { get; set; } = false;

    public int GrowingStages = 4;
    public int GrowthStage = 0;
    protected int _ticksPerGrowth = 2;
    protected int _currentGrowthTick = 0;

    protected float _maxWaterState = 0.5f;
    protected float _minWaterState = 0.0f;
    protected float _currentWaterState = 0.0f;
    protected int _ticksPerDry = 2;
    protected int _currentDryTick = 0;

    public Sprite2D Sprite;
    private ColorRect _waterState;

    public override void _Ready()
    {
        Sprite = GetNode<Sprite2D>("Sprite");
        _waterState = GetNode<ColorRect>("WaterState");

        SignalManager.Instance.Connect(
            SignalManager.SignalName.OnTick
            , new Callable(this, nameof(OnTick))
        );
    }

    public virtual void OnTick()
    {
        GrowCrop();   
        DryCrop();
    }

    public virtual void GrowCrop()
    {
        if (_currentGrowthTick >= _ticksPerGrowth)
        {
            GrowStage();
            _currentGrowthTick = 0;
        }
        _currentGrowthTick++;
    }

    public virtual void InitializeCrop(Vector2 pos) { }

    public virtual void GrowStage()
    {
        if (CropState == CROP_STATE.GROWN) return;

        CropState++;
        Sprite.Frame = (int)CropState;
    }

    public virtual void WaterCrop()
    {
        SignalManager.Instance.EmitWaterCrop(CropLocation);
    }

    public virtual void HarvestCrop()
    {
        QueueFree();
    }

    public virtual void DryCrop()
    {
        if (IsWatered && _currentDryTick >= _ticksPerDry)
        {
            _currentWaterState -= 0.1f;
            if (_currentWaterState <= _minWaterState)
            {
                _currentWaterState = _minWaterState;
                IsWatered = false;
                //_waterState.Visible = false;
            }
            else
            {
                float alpha = (_currentWaterState / _maxWaterState) * 0.5f;
                _waterState.Color = new Color(0, 0, 0, alpha);
            }

            _currentDryTick = 0;
        }

        _currentDryTick++;
    }
}
