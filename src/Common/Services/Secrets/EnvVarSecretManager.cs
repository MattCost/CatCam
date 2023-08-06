namespace CatCam.Common.Services.Secrets;
public class EnvVarSecretManager : ISecretsManager
{
    public string GetSecret(string secretName)
    {
        return Environment.GetEnvironmentVariable(secretName) ?? throw new Exception($"Secret name {secretName} does not exist");
    }
}