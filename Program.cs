using System.Threading.Tasks;
using GreenPipes;
using MassTransit;
using MassTransit.ActiveMqTransport;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using NLog.Web;
using System;

namespace MassTransitActiveMQIssue
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var host = Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((context, builder) =>
                {
                    builder.AddJsonFile("appsettings.json", false);
                })
                .ConfigureLogging((context, builder) =>
                {
                    builder.ClearProviders();
                    builder.SetMinimumLevel(LogLevel.Trace);
                })
                .ConfigureServices((context, services) =>
                {
                    services.AddMassTransit(cfg =>
                    {
                        cfg.AddConsumer<ValueConsumer>();
                        cfg.AddBus(provider => Bus.Factory.CreateUsingActiveMq(cfg =>
                        {
                            cfg.Host(context.Configuration["host"], hst =>
                            {
                                hst.Username(context.Configuration["userName"]);
                                hst.Password(context.Configuration["password"]);
                            });
                            cfg.UseActiveMqMessageScheduler();
                            cfg.ReceiveEndpoint("value", e =>
                            {
                                e.UseScheduledRedelivery(r => r.Exponential(3, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(2440), TimeSpan.FromMinutes(5)));
                                //e.UseScheduledRedelivery(r => r.Intervals(new TimeSpan[] { TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(10) }));
                                e.UseMessageRetry(m => m.Interval(5, 1000));
                                e.ConfigureConsumer<ValueConsumer>(provider);
                            });
                        }));
                    });
                    services.AddHostedService<BussWorker>();
                })
                .UseNLog()
                .UseConsoleLifetime()
                .Build();
            var task = host.RunAsync();
            await host.Services.GetRequiredService<IBusControl>().Publish<Value>(new { message = "My message" });
            await task;
        }
    }
}
