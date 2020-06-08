using MassTransit;
using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;

namespace MassTransitActiveMQIssue
{
    public class BussWorker : BackgroundService, IHostedService
    {
        private IBusControl busControl;

        public BussWorker(IBusControl busControl)
        {
            this.busControl = busControl;
        }

        protected override Task ExecuteAsync(CancellationToken token)
        {
            return busControl.StartAsync();
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            return busControl.StopAsync();
        }
    }
}
