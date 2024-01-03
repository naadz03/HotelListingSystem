using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System.Web;
using HotelListingSystem.Models;

namespace HotelListingSystem.BackgroundTask
{
    public class Worker : IWorker
    {
        private readonly IBackgroundJobHelper _backgroundJobHelper;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<Worker> logger;
        private int number = 0;

        public Worker(ILogger<Worker> logger)
        {
            this._context = new ApplicationDbContext(); 
            this.logger = logger;
            this._backgroundJobHelper = new BackgroundJobHelper(_context);
        }

        public async Task DoWork(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                _backgroundJobHelper.NotificationsOrReminders();
                Interlocked.Increment(ref number);
                logger.LogInformation($"Worker printing number: {number}");
                await Task.Delay(1000 * 5);
            }
        }
    }
}