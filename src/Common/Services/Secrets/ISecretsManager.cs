namespace CatCam.Common.Services.Secrets;

public interface ISecretsManager
{
    string GetSecret(string secretName);
}