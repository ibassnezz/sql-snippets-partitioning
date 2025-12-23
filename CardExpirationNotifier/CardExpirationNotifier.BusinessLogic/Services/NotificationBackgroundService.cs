using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CardExpirationNotifier.BusinessLogic.Services;

public class NotificationBackgroundService : BackgroundService
{
    private readonly ICardService _cardService;
    private readonly ILogger<NotificationBackgroundService> _logger;
    private readonly TimeSpan _interval;

    public NotificationBackgroundService(
        ICardService cardService,
        ILogger<NotificationBackgroundService> logger,
        int intervalSeconds = 60)
    {
        _cardService = cardService;
        _logger = logger;
        _interval = TimeSpan.FromSeconds(intervalSeconds);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Notification Background Service is starting");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _logger.LogInformation("Notification Background Service is processing");
                await _cardService.ProcessCardNotificationsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while processing card notifications");
            }

            await Task.Delay(_interval, stoppingToken);
        }

        _logger.LogInformation("Notification Background Service is stopping");
    }
}
