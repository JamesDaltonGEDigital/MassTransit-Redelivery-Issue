using System;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace MassTransitActiveMQIssue
{
    public class ValueConsumer: IConsumer<Value>
    {
        ILogger logger;

        public ValueConsumer(ILogger<ValueConsumer> logger) => this.logger = logger;

         public Task Consume(ConsumeContext<Value> context)
        {
            logger.LogInformation($"Retry {context.GetRetryCount()} of Redelivery attempt {context.GetRedeliveryCount()}");
            throw new Exception();
        }
    }
}
