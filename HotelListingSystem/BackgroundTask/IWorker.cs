using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System.Web;

namespace HotelListingSystem.BackgroundTask
{
    public interface IWorker
    {
        Task DoWork(CancellationToken cancellationToken);
    }
}