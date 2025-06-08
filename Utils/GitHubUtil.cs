using Microsoft.Extensions.Configuration;
using Octokit;

namespace EmailAgent.Utils
{
    public static class GitHubUtil
    {
        private static string CreateJwtToken(string appId, string privateKey)
        {
            var generator = new GitHubJwt.GitHubJwtFactory(
                new GitHubJwt.FilePrivateKeySource(privateKey),
                new GitHubJwt.GitHubJwtFactoryOptions
                {
                    AppIntegrationId = int.Parse(appId),
                    ExpirationSeconds = 540 // 9 minutes
                });

            return generator.CreateEncodedJwtToken();
        }

        public static async Task<GitHubClient> GetGitHubInstallationClient(IConfiguration config, KeyVaultSecretProvider keyVaultSecretProvider)
        {
            var appId = config["githubAppId"];
            long installationId = long.Parse(config["githubInstallationId"]);

            var privateKey = await keyVaultSecretProvider.GetSecretAsync("github-app");
            var privateKeyPath = "github-app.pem";
            File.WriteAllText(privateKeyPath, privateKey);

            var jwtToken = CreateJwtToken(appId, privateKeyPath);

            var appClient = new GitHubClient(new ProductHeaderValue("EmailAgent"))
            {
                Credentials = new Credentials(jwtToken, AuthenticationType.Bearer)
            };

            // Get the installation token for the GitHub App (to do more than top level calls)
            var token = await appClient.GitHubApps.CreateInstallationToken(installationId);

            return new GitHubClient(new ProductHeaderValue("EmailAgent"))
            {
                Credentials = new Credentials(token.Token)
            };
        }
    }
}
