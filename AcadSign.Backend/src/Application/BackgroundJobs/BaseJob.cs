using Hangfire;
using Microsoft.Extensions.Logging;

namespace AcadSign.Backend.Application.BackgroundJobs;

[AutomaticRetry(Attempts = 6, DelaysInSeconds = new[] { 0, 60, 300, 900, 3600, 21600 }, OnAttemptsExceeded = AttemptsExceededAction.Delete)]
public abstract class BaseJob
{
    protected readonly ILogger _logger;
    
    protected BaseJob(ILogger logger)
    {
        _logger = logger;
    }
    
    protected static int[] GetRetryDelays()
    {
        return new[] 
        { 
            0,       // Immédiat
            60,      // 1 minute
            300,     // 5 minutes
            900,     // 15 minutes
            3600,    // 1 heure
            21600    // 6 heures
        };
    }
}
