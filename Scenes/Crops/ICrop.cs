using Godot;

public interface ICrop
{
    public enum CROP_TYPE
    {
        CORN,
    }

    public enum CROP_STATE
    {
        SEEDLING,
        GROW_STAGE_1,
        GROW_STAGE_2,
        GROWN
    }

    public string CropID { get; set; }
    public string CropName { get; set; }
    public CROP_TYPE CropType { get; set; }
    public CROP_STATE CropState { get; set; }
    public Vector2 CropLocation { get; set; }
    public bool IsWatered { get; set; }

    public void InitializeCrop(Vector2 pos) { }

    public virtual void HarvestCrop() { }
}
