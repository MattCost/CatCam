namespace CatCam.Common.Services.IOTHub;

public static class TwinDictExtensions
{
    public static string GetTwinPatch(this Dictionary<string, object?> incoming)
    {
        var patchObject = new { properties = new { desired = incoming}};
        return System.Text.Json.JsonSerializer.Serialize(patchObject);
    }
}