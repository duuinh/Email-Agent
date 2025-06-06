using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel.Agents.Orchestration;
using Microsoft.SemanticKernel.Agents.Orchestration.Handoff;
using Microsoft.SemanticKernel.Agents.Runtime.InProcess;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using EmailAgent.Plugins;
using Octokit;

public class AIAgentsService
{
#pragma warning disable SKEXP0110 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
    private readonly HandoffOrchestration<Email, string> _orchestration;
#pragma warning restore SKEXP0110 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

    public AIAgentsService(IConfiguration config, ILoggerFactory loggerFactory, GitHubClient client)
    {
        KernelPlugin emailPlugin = KernelPluginFactory.CreateFromType<EmailPlugin>();
        KernelPlugin srPlugin = KernelPluginFactory.CreateFromObject(new ServiceRequestPlugin(client));
        Kernel kernel = CreateKernelWithChatCompletion(config);
        kernel.Plugins.Add(srPlugin);
        kernel.Plugins.Add(emailPlugin);

        ChatCompletionAgent emailTriageAgent = new ChatCompletionAgent
        {
            Id = "EmailTriageAgent",
            Name = "EmailTriageAgent",
            Instructions = "Classify the email and determine if it requires a Service Request (SR) to be created or if it is a follow-up inquiry.",
            Description = " An Agent that triages customer email",
            Kernel = kernel
        };

        ChatCompletionAgent srCreatorAgent = new ChatCompletionAgent
        {
            Id = "SRCreatorAgent",
            Name = "SRCreatorAgent",
            Instructions = "Handling SR creation based on email content. Always transfer to ReplyGeneratorAgent for generating a reply.",
            Description = "An Agent that handles the creation of a Service Request (SR) based on the email content.",
            Kernel = kernel
        };

        ChatCompletionAgent followUpHandlerAgent = new ChatCompletionAgent
        {
            Id = "FollowUpHandlerAgent",
            Name = "FollowUpHandlerAgent",
            Instructions = "Handles customer inquiries related to SR status. Always transfer to ReplyGeneratorAgent for generating a reply.",
            Description = "An Agent that handles follow-up inquiries related to the status of a Service Request (SR) number.",
            Kernel = kernel
        };

        ChatCompletionAgent replyGeneratorAgent = new ChatCompletionAgent
        {
            Id = "ReplyGeneratorAgent",
            Name = "ReplyGeneratorAgent",
            Instructions = """
                         Generate a reply to a customer email based on the status of a service request (SR).                         
                         
                         Follow this logic when writing the reply:
 
                         - If the SR was successfully created, confirm creation and include the SR number in this format: 
                                "Service Request created with number: NUMBER."
                            Instruct the customer to use this number for any future follow-ups.
                         - If the SR is being processed, inform the customer that the request is in progress. Include the latest known update if available.
                         - If the SR is resolved and closed, let the customer know that the issue has been resolved and the request is closed.
                         - If the SR cannot be found, inform the customer to recheck the SR number as it may be invalid.
 
                         Use a polite, professional, and context-aware tone. Always end with a courteous closing message on behalf of the customer support team.
                         """,
            Description = "An Agent that generates a reply to customer email.",
            Kernel = kernel
        };

#pragma warning disable SKEXP0110 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
        _orchestration =
            new(OrchestrationHandoffs
                    .StartWith(emailTriageAgent)
                    .Add(emailTriageAgent, srCreatorAgent, followUpHandlerAgent) // Transfer to these two agents based on email classification
                    .Add(srCreatorAgent, replyGeneratorAgent, "Always transfer to this agent to generate reply for new service request")
                    .Add(followUpHandlerAgent, replyGeneratorAgent, "Always transfer to this agent to generate reply for follow-up inquiries"),
                emailTriageAgent,
                srCreatorAgent,
                followUpHandlerAgent,
                replyGeneratorAgent)
            {
                LoggerFactory = loggerFactory
            };
#pragma warning restore SKEXP0110 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
    }

    // Helper function to create a kernel with chat completion
    private static Kernel CreateKernelWithChatCompletion(IConfiguration config)
    {
        var modelId = config["modelId"];
        var uri = config["uri"];
        var apiKey = config["openaiApiKey"];

        var builder = Kernel.CreateBuilder();
        builder.AddAzureOpenAIChatCompletion(modelId: modelId, deploymentName: modelId, endpoint: uri, apiKey: apiKey);

        builder.Services.AddLogging(services => services.AddConsole().SetMinimumLevel(LogLevel.Trace));

        return builder.Build();
    }
    public async Task<string> GetResponseAsync(Email input)
    {
        InProcessRuntime runtime = new InProcessRuntime();
        await runtime.StartAsync();

#pragma warning disable SKEXP0110 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
        OrchestrationResult<string> result = await _orchestration.InvokeAsync(input, runtime);
#pragma warning restore SKEXP0110 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

#pragma warning disable SKEXP0110 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
        string resultText = await result.GetValueAsync(TimeSpan.FromSeconds(300));
#pragma warning restore SKEXP0110 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

        await runtime.RunUntilIdleAsync();

        return $"RESULT: {resultText}";
    }
}