using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using TwitchBrb.Scenes.Crops;

namespace TwitchBrb.Autoloads;

public partial class CropManager : Node
{
    public static CropManager Instance;

    public enum CROP_TYPE
    {
        CORN,

    }

    private Dictionary<CROP_TYPE, PackedScene> cropScenes = new Dictionary<CROP_TYPE, PackedScene>()
    {
        { CROP_TYPE.CORN, GD.Load<PackedScene>("res://Scenes/Crops/Inherited/Corn.tscn") }
    };

    // Key: Map Location (Vector2), Value: CropID (string)
    public Dictionary<Vector2, string> CropsByLocation { get; private set; } = new Dictionary<Vector2, string>();

    // Key: CropID (string), Value: ICrop
    public Dictionary<string, ICrop> CropsByID { get; private set; } = new Dictionary<string, ICrop>();

    public override void _Ready()
    {
        Instance = this;
    }

    public T GetCrop<T>(CROP_TYPE cropType) where T : Node2D
    {
        if (cropScenes.ContainsKey(cropType))
        {
            var cropScene = cropScenes[cropType];
            var cropInstance = cropScene.Instantiate<T>();
            return cropInstance;
        }
        else
        {
            throw new ArgumentException($"Crop type {cropType} not found.");
        }
    }

    public void RegisterLocation(Vector2 location)
    {
        CropsByLocation[location] = string.Empty;
    }

    public void RegisterCropAtLocation(Vector2 location, ICrop crop)
    {
        if (CropsByLocation.ContainsKey(location))
        {
            GD.Print($"Location: {location}; CropID: {crop.CropID}");
            GD.Print(CropsByLocation[location]);

            var cropCount = CropsByLocation.Where(kv => kv.Value == string.Empty).ToList();
            GD.Print(cropCount.Count);
            crop.InitializeCrop(location);
            CropsByLocation[location] = crop.CropID;
            CropsByID[crop.CropID] = crop;
        }
    }

    public void GetRandomUnplantedLocation(out Vector2? location)
    {
        var unplantedLocations = new List<Vector2>();
        foreach (var entry in CropsByLocation)
        {
            if (string.IsNullOrEmpty(entry.Value))
            {
                unplantedLocations.Add(entry.Key);
            }
        }
        if (unplantedLocations.Count == 0)
        {
            GD.PrintErr("No unplanted locations available.");
            location = null;
            return;
        }
        var randomIndex = GD.Randi() % unplantedLocations.Count;
        location = unplantedLocations[(int)randomIndex];
    }

    public string GetRandomPlantedCropID()
    {
        var plantedCrops = CropsByID.Values.ToList();
        
        if (plantedCrops.Count == 0)
        {
            GD.PrintErr("No planted crops available.");
            return null;
        }

        var randomIndex = GD.Randi() % plantedCrops.Count;
        return plantedCrops[(int)randomIndex].CropID;
    }

    public void GrowRandomCrop()
    {
        var grownCrops = CropsByID.Values.ToList().Where(c => c.CropState != ICrop.CROP_STATE.GROWN).ToList();
        if (grownCrops.Count == 0)
        {
            GD.PrintErr("No crops available to grow.");
            return;
        }
        var randomIndex = GD.Randi() % grownCrops.Count;
        var crop = grownCrops[(int)randomIndex];
        if (crop is Scenes.Crops.Crop concreteCrop)
        {
            concreteCrop.GrowStage();
        }
    }

    public bool IsCropWatered(string cropID)
    {
        if (CropsByID.ContainsKey(cropID))
        {
            return CropsByID[cropID].IsWatered;
        }
        else
        {
            GD.PrintErr($"No crop found with CropID: {cropID}");
            return false;
        }
    }

    public void WaterCropByCropID(string cropID)
    {
        var wateredCrops = CropsByID.Values.ToList().Where(c => c.IsWatered == false).ToList();
        if (wateredCrops.Where(x => x.CropID == cropID).ToList().Count > 0)
        {
            var crop = CropsByID[cropID];
            if (crop is Crop concreteCrop)
            {
                concreteCrop.WaterCrop();
            }
        }
        else
        {
            GD.PrintErr($"No unwatered crop found with CropID: {cropID}");
        }
    }
}
