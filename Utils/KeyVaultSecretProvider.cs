using Azure;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;

namespace EmailAgent.Utils;
public class KeyVaultSecretProvider
{
    private readonly SecretClient _client;

    public KeyVaultSecretProvider(string vaultUrl)
    {
        _client = new SecretClient(new Uri(vaultUrl), new DefaultAzureCredential());
    }

    public async Task<string?> GetSecretAsync(string name)
        {
            try
            {
                KeyVaultSecret secret = await _client.GetSecretAsync(name);
                return secret.Value;
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                Console.Error.WriteLine($"Secret '{name}' not found in Key Vault.");
            }
            catch (AuthenticationFailedException authEx)
            {
                Console.Error.WriteLine($"Authentication to Azure Key Vault failed: {authEx.Message}");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Unexpected error accessing secret '{name}': {ex.Message}");
            }

            return null;
        }
}
