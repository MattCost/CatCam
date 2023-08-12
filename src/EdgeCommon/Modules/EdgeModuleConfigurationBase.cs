namespace CatCam.EdgeCommon.Modules;

public abstract class EdgeModuleConfigurationBase
{
    public abstract void Validate();
    public override string ToString()
    {
        return System.Text.Json.JsonSerializer.Serialize(this, GetType());
    }
}