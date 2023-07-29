namespace CatCam.API.Services;

public interface ISecretsManager
{
    string GetSecret(string secretName);
}