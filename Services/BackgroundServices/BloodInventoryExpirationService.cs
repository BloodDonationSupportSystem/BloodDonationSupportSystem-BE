using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Services.Interface;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Services.BackgroundServices
{
    public class BloodInventoryExpirationService : Microsoft.Extensions.Hosting.BackgroundService
    {
        private readonly ILogger<BloodInventoryExpirationService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly TimeSpan _checkInterval = TimeSpan.FromSeconds(1);

        public BloodInventoryExpirationService(
            ILogger<BloodInventoryExpirationService> logger,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Blood Inventory Expiration Service is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessExpiredInventoriesAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while processing expired blood inventories.");
                }

                await Task.Delay(_checkInterval, stoppingToken);
            }

            _logger.LogInformation("Blood Inventory Expiration Service is stopping.");
        }

        private async Task ProcessExpiredInventoriesAsync()
        {
            _logger.LogInformation("Checking for expired blood inventories at {time}", DateTimeOffset.UtcNow);

            using (var scope = _serviceProvider.CreateScope())
            {
                var bloodInventoryService = scope.ServiceProvider.GetRequiredService<IBloodInventoryService>();

                var expiredInventoriesResponse = await bloodInventoryService.GetExpiredInventoryAsync();

                if (!expiredInventoriesResponse.Success)
                {
                    _logger.LogError("Failed to retrieve expired inventories: {Message}", expiredInventoriesResponse.Message);
                    return;
                }

                var expiredInventories = expiredInventoriesResponse.Data.ToList();
                _logger.LogInformation("Found {Count} expired blood inventories", expiredInventories.Count);

                foreach (var inventory in expiredInventories)
                {
                    if (inventory.Status.ToLower() != "expired")
                    {
                        var updateResponse = await bloodInventoryService.UpdateInventoryStatusAsync(inventory.Id, "Expired");

                        if (updateResponse.Success)
                        {
                            _logger.LogInformation("Updated blood inventory ID {Id} status to Expired", inventory.Id);
                        }
                        else
                        {
                            _logger.LogWarning("Failed to update blood inventory ID {Id} status: {Message}",
                                inventory.Id, updateResponse.Message);
                        }
                    }
                }
            }

            _logger.LogInformation("Finished processing expired blood inventories at {time}", DateTimeOffset.UtcNow);
        }
    }
}