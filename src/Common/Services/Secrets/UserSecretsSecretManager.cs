using Microsoft.Extensions.Configuration;

namespace CatCam.Common.Services.Secrets;
public class UserSecretsSecretManager : ISecretsManager
{
    private readonly IConfiguration _configuration;
    public UserSecretsSecretManager(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    public string GetSecret(string secretName)
    {
        return _configuration[secretName] ?? throw new Exception($"Secret name {secretName} does not exist");
    }
}
