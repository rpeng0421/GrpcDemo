using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Autofac.Features.Indexed;
using Grpc.Core;
using Grpc.Net.Client;
using GrpcDemo.Client.Applibs;
using GrpcDemo.Client.Model;
using GrpcDemo.Grpc.Service;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GrpcDemo.Client.Worker
{
    public class BidirectionalService : BackgroundService
    {
        private readonly ILogger<BackgroundService> _logger;

        private readonly IIndex<string, IActionHandler> handlerSet;

        public BidirectionalService(
            ILogger<BackgroundService> logger,
            IIndex<string, IActionHandler> handlerSet
        )
        {
            _logger = logger;
            this.handlerSet = handlerSet;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using (var channel = GrpcChannel.ForAddress(ConfigHelper.GrpcServerUrl))
            {
                var header = new Metadata() { new Metadata.Entry("id", "1") };
                var client = new Bidirectional.BidirectionalClient(channel);
                var bidirection = client.BindAction(new CallOptions(header));
                await foreach (var action in bidirection.ResponseStream
                                   .ReadAllAsync(cancellationToken: stoppingToken)
                              )
                {
                    _logger.LogInformation($"get action {action.ToString()}");
                    if (this.handlerSet.TryGetValue(action.Type, out var handler))
                    {
                        handler.Handle(action);
                    }
                }
            }
        }
    }
}