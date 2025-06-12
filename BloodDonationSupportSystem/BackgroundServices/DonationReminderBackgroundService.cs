using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Services.Interface;
using System;
using System.Threading;
using System.Threading.Tasks;
using BloodDonationSupportSystem.Config;

namespace BloodDonationSupportSystem.BackgroundServices
{
    public class DonationReminderBackgroundService : BackgroundService
    {
        private readonly ILogger<DonationReminderBackgroundService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly DonationReminderSettings _settings;

        public DonationReminderBackgroundService(
            ILogger<DonationReminderBackgroundService> logger,
            IServiceProvider serviceProvider,
            IOptions<DonationReminderSettings> settings)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _settings = settings.Value;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("D?ch v? nh?c nh? hi?n máu ?ã b?t ??u.");

            using PeriodicTimer timer = new(TimeSpan.FromMinutes(1));

            while (await timer.WaitForNextTickAsync(stoppingToken) && !stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var now = DateTime.Now;
                    
                    // Parse th?i gian t? c?u hình
                    if (TimeSpan.TryParse(_settings.ScheduledRunTime, out TimeSpan executionTime))
                    {
                        var scheduledTime = new DateTime(now.Year, now.Month, now.Day, 
                            executionTime.Hours, executionTime.Minutes, executionTime.Seconds);
                        
                        // N?u th?i gian hi?n t?i ?ã qua th?i gian d? ki?n ch?y, ??i ??n ngày mai
                        if (now > scheduledTime)
                        {
                            scheduledTime = scheduledTime.AddDays(1);
                        }
                        
                        // Tính th?i gian ch?
                        var delay = scheduledTime - now;
                        if (delay <= TimeSpan.Zero)
                        {
                            // N?u ??n th?i gian ch?y, th?c hi?n g?i nh?c nh?
                            await SendRemindersAsync();
                            
                            // ??i ??n ngày hôm sau
                            await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
                        }
                    }
                    else
                    {
                        _logger.LogError("??nh d?ng th?i gian không h?p l? trong c?u hình: {ScheduledRunTime}", 
                            _settings.ScheduledRunTime);
                        
                        // N?u không parse ???c th?i gian, ch?y m?c ??nh vào 8h sáng
                        var defaultTime = new DateTime(now.Year, now.Month, now.Day, 8, 0, 0);
                        if (now > defaultTime)
                        {
                            defaultTime = defaultTime.AddDays(1);
                        }
                        
                        var delay = defaultTime - now;
                        if (delay <= TimeSpan.Zero)
                        {
                            await SendRemindersAsync();
                            await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "L?i trong d?ch v? n?n nh?c nh? hi?n máu");
                }
            }
        }

        private async Task SendRemindersAsync()
        {
            _logger.LogInformation("B?t ??u ki?m tra và g?i nh?c nh? hi?n máu");
            
            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var reminderService = scope.ServiceProvider.GetRequiredService<IDonationReminderService>();
                    var result = await reminderService.CheckAndSendRemindersAsync(_settings.DefaultDaysBeforeEligible);
                    
                    _logger.LogInformation("?ã g?i {Count} nh?c nh? hi?n máu", result.Data);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "L?i khi g?i nh?c nh? hi?n máu t? ??ng");
            }
        }
    }
}