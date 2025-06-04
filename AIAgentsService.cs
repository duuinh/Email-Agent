using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.Extensions.Configuration;
using System.ComponentModel;
using System.ClientModel;
using OpenAI;
using Microsoft.SemanticKernel.Agents.Orchestration;
using Microsoft.SemanticKernel.Agents.Orchestration.Handoff;
using Microsoft.SemanticKernel.Agents.Runtime.InProcess;
using Microsoft.Extensions.Logging;
using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;
public class AIAgentsService
{
#pragma warning disable SKEXP0110 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
    private readonly HandoffOrchestration<Email, string> _orchestration;
#pragma warning restore SKEXP0110 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

    public AIAgentsService(IConfiguration config, ILoggerFactory loggerFactory)
    {
        KernelPlugin emailPlugin = KernelPluginFactory.CreateFromType<EmailPlugin>();
        KernelPlugin srPlugin = KernelPluginFactory.CreateFromType<ServiceRequestPlugin>();

        ChatCompletionAgent emailTriageAgent = new ChatCompletionAgent
        {
            Id = "EmailTriageAgent",
            Name = "EmailTriageAgent",
            Instructions = "Classify the email and determine if it requires a Service Request (SR) to be created or if it is a follow-up inquiry.",
            Description = " An Agent that triages customer email",
            Kernel = CreateKernelWithChatCompletion(config)
        };

        ChatCompletionAgent srCreatorAgent = new ChatCompletionAgent
        {
            Id = "SRCreatorAgent",
            Name = "SRCreatorAgent",
            Instructions = "Handling SR creation based on email content.",
            Description = "An Agent that handles the creation of a Service Request (SR) based on the email content.",
            Kernel = CreateKernelWithChatCompletion(config)
        };
        srCreatorAgent.Kernel.Plugins.Add(srPlugin);

        ChatCompletionAgent followUpHandlerAgent = new ChatCompletionAgent
        {
            Id = "FollowUpHandlerAgent",
            Name = "FollowUpHandlerAgent",
            Instructions = "Handles customer inquiries related to SR status.",
            Description = "An Agent that handles follow-up inquiries related to the status of a Service Request (SR).",
            Kernel = CreateKernelWithChatCompletion(config)
        };
        followUpHandlerAgent.Kernel.Plugins.Add(srPlugin);

        ChatCompletionAgent replyGeneratorAgent = new ChatCompletionAgent
        {
            Id = "ReplyGeneratorAgent",
            Name = "ReplyGeneratorAgent",
            Instructions = "Generates a reply to customer email based on SR status. Reply in natural tone using contextual understanding.",
            Description = "An Agent that generates a reply to customer email based on SR status.",
            Kernel = CreateKernelWithChatCompletion(config)
        };
        replyGeneratorAgent.Kernel.Plugins.Add(emailPlugin);

#pragma warning disable SKEXP0110 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
        _orchestration =
            new(OrchestrationHandoffs
                    .StartWith(emailTriageAgent)
                    .Add(emailTriageAgent, srCreatorAgent, followUpHandlerAgent) // Transfer to these two agents based on email classification
                    .Add(srCreatorAgent, replyGeneratorAgent, "Transfer to this agent if the SR is created")
                    .Add(followUpHandlerAgent, replyGeneratorAgent, "Transfer to this agent if the SR status is received"),
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
    protected static Kernel CreateKernelWithChatCompletion(IConfiguration config)
    {
        var modelId = "openai/gpt-4.1-mini";
        var uri = "https://models.github.ai/inference";
        var githubPAT = config["githubPAT"];

        var client = new OpenAIClient(new ApiKeyCredential(githubPAT), new OpenAIClientOptions { Endpoint = new Uri(uri) });

        var builder = Kernel.CreateBuilder();
        builder.AddOpenAIChatCompletion(modelId, client);

        builder.Services.AddLogging(services => services.AddConsole().SetMinimumLevel(LogLevel.Trace));

        return builder.Build();
    }
    public async Task<string> GetResponseAsync(string task)
    {
        Email email = new()
        {
            Id = 7,
            Subject = "Re: Update My Registered Mobile Number",
            From = "sara.lee@example.com",
            Body = "Can you confirm if my mobile number has been updated in your system?"
        };

        InProcessRuntime runtime = new InProcessRuntime();
        await runtime.StartAsync();

#pragma warning disable SKEXP0110 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
        OrchestrationResult<string> result = await _orchestration.InvokeAsync(email, runtime);
#pragma warning restore SKEXP0110 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

#pragma warning disable SKEXP0110 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
        string resultText = await result.GetValueAsync(TimeSpan.FromSeconds(20));
#pragma warning restore SKEXP0110 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

        await runtime.RunUntilIdleAsync();

        return $"RESULT: {resultText}";
    }

    sealed class Email
    {
        [JsonPropertyName("id")]
        public int Id { get; set; } = 0;

        [JsonPropertyName("from")]
        public string From { get; set; } = string.Empty;

        [JsonPropertyName("subject")]
        public string Subject { get; set; } = string.Empty;

        [JsonPropertyName("body")]
        public string Body { get; set; } = string.Empty;
    }

    sealed class EmailPlugin
    {
        [KernelFunction, Description("Writes a response to an email.")]
        public string WriteResponse(string emailAddress, string subject, string response) =>
        $"""
            To: {emailAddress}
            Subject: {subject}
            Body: {response}
        """;
    }

    sealed class ServiceRequestPlugin
    {
        [KernelFunction, Description("Creates a service request (SR) based on the email content.")]
        public string CreateServiceRequest(string emailContent) =>
            $"Service Request created for email: {emailContent}";

        [KernelFunction, Description("Gets the status of a service request.")]
        public string GetServiceRequestStatus(string srId) =>
            $"Status of Service Request {srId} is: In Progress";
    }
}