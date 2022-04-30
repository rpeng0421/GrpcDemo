using System;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using GrpcDemo.Grpc.Service;
using Microsoft.Extensions.Logging;
using Action = GrpcDemo.Grpc.Message.Action;

namespace GrpcDemo.Server
{
    public class BidirectionalService : Bidirectional.BidirectionalBase
    {
        private readonly ILogger<BidirectionalService> _logger;

        public BidirectionalService(ILogger<BidirectionalService> logger)
        {
            _logger = logger;
        }

        public override async Task BindAction(
            IAsyncStreamReader<Action> requestStream,
            IServerStreamWriter<Action> responseStream,
            ServerCallContext context
        )
        {
            var id = context.RequestHeaders.FirstOrDefault(p => p.Key == "id")?.Value ?? Guid.NewGuid().ToString();
            await Task.Run(async () =>
            {
                try
                {
                    await foreach (var action in requestStream.ReadAllAsync())
                        _logger.LogInformation($"get action {action}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "get action process error");
                    throw;
                }
            });
        }
    }
}