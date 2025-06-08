using EmailAgent.Utils;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Octokit;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights()
    .AddSingleton<KeyVaultSecretProvider>(provider =>
    {
        var keyVaultUri = builder.Configuration["keyVaultUri"];
        return new KeyVaultSecretProvider(keyVaultUri);
    })
    .AddSingleton<GitHubClient>(provider =>
    {
        var config = provider.GetRequiredService<IConfiguration>();
        var keyVaultSecretProvider = provider.GetRequiredService<KeyVaultSecretProvider>();
        return GitHubUtil.GetGitHubInstallationClient(config, keyVaultSecretProvider).GetAwaiter().GetResult();
    })
    .AddSingleton<AIAgentsService>();

builder.Build().Run();
