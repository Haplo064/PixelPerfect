using System.Numerics;
using Dalamud.Configuration;

namespace PixelPerfect.Data;

public class Config : IPluginConfiguration
{
    public int Version { get; set; } = 0;
    
    public bool Enabled = true;
    public bool Combat = true;
    public bool Circle;
    public bool Instance;
    public Vector4 Col = new(1f, 1f, 1f, 1f);
    public Vector4 Col2 = new(0.4f, 0.4f, 0.4f, 1f);
    public Vector4 ColRing = new(0.4f, 0.4f, 0.4f, 0.5f);
    public int Segments = 100;
    public float Thickness = 10f;
    public bool Ring;
    public float Radius = 2f;
    public bool Ring2;
    public float Radius2 = 2f;
    public Vector4 ColRing2 = new(0.4f, 0.4f, 0.4f, 0.5f);
    public int Segments2 = 100;
    public float Thickness2 = 10f;
    public bool North1;
    public bool North2;
    public bool North3;
    public float LineOffset = 0.5f;
    public float LineLength = 1f;
    public float ChevLength = 1f;
    public float ChevOffset = 1f;
    public float ChevRad = 11.5f;
    public float ChevSin = -1.5f;
    public float ChevThicc = 5f;
    public float LineThicc = 5f;
    public Vector4 ChevCol = new(0.4f, 0.4f, 0.4f, 0.5f);
    public Vector4 LineCol = new(0.4f, 0.4f, 0.4f, 0.5f);
    
    public void Save()
    {
        Service.PluginInterface.SavePluginConfig(this);
    }
}