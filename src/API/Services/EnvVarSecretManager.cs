namespace CatCam.API.Services;
public class EnvVarSecretManager : ISecretsManager
{
    public string GetSecret(string secretName)
    {
        return Environment.GetEnvironmentVariable(secretName) ?? string.Empty;
    }
}