using iTextSharp.text.log;
using Microsoft.Build.Framework;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace HotelListingSystem.BackgroundTask
{
    public class BackgroundJob : IHostedService
    {
        private readonly ILogger<BackgroundJob> logger;
        private readonly IWorker worker;

        public BackgroundJob(ILogger<BackgroundJob> logger, IWorker worker)
        {
            this.logger = logger;
            this.worker = worker;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await worker.DoWork(cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("job now stopping");
            return Task.CompletedTask;
        }
    }
}