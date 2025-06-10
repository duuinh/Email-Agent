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
        KernelPlugin srPlugin = KernelPluginFactory.CreateFromObject(new ServiceRequestPlugin(client));
        Kernel kernel = CreateKernelWithChatCompletion(config);
        kernel.Plugins.Add(srPlugin);

        ChatCompletionAgent emailTriageAgent = new ChatCompletionAgent
        {
            Id = "EmailTriageAgent",
            Name = "EmailTriageAgent",
            Instructions = """
                Your task is to:
                - Carefully read the email.
                - Classify the email into one of the following categories:
                    1. NewRequest - The email requires a new service request (SR) to be created.
                    2. FollowUp - The email is a follow-up to an existing service request.

                Decision Logic:
                - If the customer is asking about the status, providing an SR number, or referencing a previous request → classify as FollowUp.
                - If the customer is reporting a new issue or requesting a new service → classify as NewRequest.
            """,
            Description = " An Agent that triages customer email",
            Kernel = CreateKernelWithChatCompletion(config)
        };

        ChatCompletionAgent srCreatorAgent = new ChatCompletionAgent
        {
            Id = "SRCreatorAgent",
            Name = "SRCreatorAgent",
            Instructions = """
            Summarize the issue in third person and create the SR with title and details from the summary.
            Always transfer to ReplyGeneratorAgent for generating a reply.
            """,
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
                         Generate a html email body based on the status of a service request (SR).
                         Do NOT mix messages from different statuses.
                         Use a polite, professional, and context-aware tone.

                         Format Requirements:
                         - Write the reply as a complete, formal email in HTML.
                         - Include:
                             - A greeting, e.g., <p>Dear Customer,</p>
                             - Clear paragraph separation using <p> tags.
                             - A courteous closing, e.g., <p>Best regards,<br/>Customer Support Team</p>
                             - Do not use inline styles or excessive formatting. Keep it clean and readable.
                         
                         Follow this logic when writing the reply:
                         - If the SR was successfully created:
                            - Confirm creation and include this sentence: <strong>Service Request created with number: [SR_NUMBER].</strong>
                            - Instruct the customer to use this number for any future follow-ups.
                         - If the SR is being processed:
                             - Inform the customer that their request is currently being processed.
                             - Include the latest known update, if available.
                         - If the SR is closed:
                             - Inform the customer that their issue has been resolved and the request is now closed.
                         - If the SR cannot be found:
                             - Inform the customer that the provided SR number may be invalid and ask them to recheck and provide the correct number.
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

        return resultText;
    }
}