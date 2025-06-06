using System.Net;
using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace Company.Function;

public class EmailProcessor
{
    private readonly ILogger _logger;
    private readonly AIAgentsService _aiAgentsService;

    public EmailProcessor(ILoggerFactory loggerFactory, AIAgentsService aiAgentsService)
    {
        _logger = loggerFactory.CreateLogger<EmailProcessor>();
        _aiAgentsService = aiAgentsService;
    }

    [Function(nameof(EmailProcessor))]
    public async Task<HttpResponseData> RunAsync(
        [HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req)
    {
        _logger.LogInformation("Email Processor function executed");

        var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        var email = JsonSerializer.Deserialize<Email>(requestBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (email == null)
        {
            var badResponse = req.CreateResponse(HttpStatusCode.BadRequest);
            await badResponse.WriteStringAsync("Invalid email payload.");
            return badResponse;
        }

        string response = await _aiAgentsService.GetResponseAsync(email);
        _logger.LogInformation(response);

        var okResponse = req.CreateResponse(HttpStatusCode.OK);
        await okResponse.WriteStringAsync(response);
        return okResponse;
    }
}