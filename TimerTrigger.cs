using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Company.Function;

public class TimerTrigger
{
    private readonly ILogger _logger;
    private readonly AIAgentsService _aiAgentsService;

    public TimerTrigger(ILoggerFactory loggerFactory, AIAgentsService aiAgentsService)
    {
        _logger = loggerFactory.CreateLogger<TimerTrigger>();
        _aiAgentsService = aiAgentsService;
    }

    [Function(nameof(TimerTrigger))]
    public async Task RunAsync([TimerTrigger("0 */5 * * * *")] TimerInfo myTimer)
    {
        _logger.LogInformation("C# Timer trigger function executed at: {executionTime}", DateTime.Now);

        Email email = new()
        {
            Id = 1,
            Subject = "Problem with Credit Card Transaction",
            From = "john.doe@example.com",
            Body = "I noticed a charge of $124.99 on my credit card that I did not make. Can you please help me dispute this transaction?"
        };

        string response = await _aiAgentsService.GetResponseAsync(email);
        _logger.LogInformation(response);

        if (myTimer.ScheduleStatus is not null)
        {
            _logger.LogInformation("Next timer schedule at: {nextSchedule}", myTimer.ScheduleStatus.Next);
        }
    }
}