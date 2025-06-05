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
    .AddSingleton<GitHubClient>(provider =>
    {
        var config = provider.GetRequiredService<IConfiguration>();
        return GitHubUtil.GetGitHubInstallationClient(config).Result;
    })
    .AddSingleton<AIAgentsService>();

builder.Build().Run();
