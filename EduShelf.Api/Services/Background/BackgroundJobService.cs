using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EduShelf.Api.Services.Background
{
    public class BackgroundJobService : BackgroundService
    {
        private readonly IBackgroundJobQueue _queue;
        private readonly ILogger<BackgroundJobService> _logger;

        public BackgroundJobService(IBackgroundJobQueue queue, ILogger<BackgroundJobService> logger)
        {
            _queue = queue;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Background Job Service is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var workItem = await _queue.DequeueAsync(stoppingToken);

                    try
                    {
                        await workItem(stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error occurred executing background work item.");
                    }
                }
                catch (OperationCanceledException)
                {
                    // Prevent throwing if stoppingToken was signaled
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred dequeuing background work item.");
                }
            }

            _logger.LogInformation("Background Job Service is stopping.");
        }
    }
}
