using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace InventorySystem.Helpers
{
    /// <summary>
    /// Periodically runs scheduled database backups while the app / API server is running.
    /// </summary>
    public sealed class AutoBackupBackgroundService : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try { await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken); }
            catch (OperationCanceledException) { return; }

            while (!stoppingToken.IsCancellationRequested)
            {
                try { BackupService.RunAutoBackupIfDue(); }
                catch { /* never stop the host */ }

                try { await Task.Delay(TimeSpan.FromMinutes(15), stoppingToken); }
                catch (OperationCanceledException) { break; }
            }
        }
    }
}
