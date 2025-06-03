using System;
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
    public async Task RunAsync([TimerTrigger("0 */1 * * * *")] TimerInfo myTimer)
    {
        _logger.LogInformation("C# Timer trigger function executed at: {executionTime}", DateTime.Now);

        string response = await _aiAgentsService.GetResponseAsync("Hi, can you tell me the specials today?");
        _logger.LogInformation("AI Response: {response}", response);
        
        if (myTimer.ScheduleStatus is not null)
        {
            _logger.LogInformation("Next timer schedule at: {nextSchedule}", myTimer.ScheduleStatus.Next);
        }
    }
}