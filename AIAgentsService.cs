using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.Extensions.Configuration;
using System.ComponentModel;
using System.ClientModel;
using OpenAI;

public class AIAgentsService
{
    private readonly ChatCompletionAgent _agent;
        public AIAgentsService(IConfiguration config)
    {
        var modelId = "openai/gpt-4.1-mini";
        var uri = "https://models.github.ai/inference";
        var githubPAT = config["githubPAT"];

        var client = new OpenAIClient(new ApiKeyCredential(githubPAT), new OpenAIClientOptions { Endpoint = new Uri(uri) });

        var builder = Kernel.CreateBuilder();
        builder.AddOpenAIChatCompletion(modelId, client);

        var kernel = builder.Build();
        kernel.Plugins.Add(KernelPluginFactory.CreateFromType<MenuPlugin>());

        _agent = new ChatCompletionAgent
        {
            Name = "SK-Assistant",
            Instructions = "You are a helpful assistant.",
            Kernel = kernel,
            Arguments = new KernelArguments(new PromptExecutionSettings
            {
                FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
            })
        };
    }

    public async Task<string> GetResponseAsync(string prompt)
    {
        await foreach (var response in _agent.InvokeAsync(prompt))
        {
            return response.Message.Content;
        }

        return "No response.";
    }
}

sealed class MenuPlugin
{
    [KernelFunction, Description("Provides a list of specials from the menu.")]
    public string GetSpecials() =>
        """
        Special Soup: Clam Chowder
        Special Salad: Cobb Salad
        Special Drink: Chai Tea
        """;

    [KernelFunction, Description("Provides the price of the requested menu item.")]
    public string GetItemPrice([Description("The name of the menu item.")] string menuItem) =>
        "$9.99";
}
