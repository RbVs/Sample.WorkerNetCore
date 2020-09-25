using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Sample.WorkerNetCore
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private HttpClient client;

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            client = new HttpClient();
            _logger.LogInformation("Worker is starting");
            return base.StartAsync(cancellationToken);
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            client.Dispose();
            _logger.LogInformation("Worker is shutting down");
            return base.StopAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var uri = "https://google.com";

            while (!stoppingToken.IsCancellationRequested)
            {
                var result = await client.GetAsync(uri, stoppingToken);

                if (result.IsSuccessStatusCode)
                {
                    _logger.LogInformation($"{uri} is up and running!");
                }
                else
                {
                    _logger.LogInformation($"{uri} is down!");
                }
                
                await Task.Delay(5000, stoppingToken);
            }
        }
    }
}