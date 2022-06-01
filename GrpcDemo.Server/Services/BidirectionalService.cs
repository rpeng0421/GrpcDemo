using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using GrpcDemo.Grpc.Service;
using GrpcDemo.Server.Model;
using Microsoft.Extensions.Logging;
using Action = GrpcDemo.Grpc.Message.Action;

namespace GrpcDemo.Server
{
    public class BidirectionalService : Bidirectional.BidirectionalBase
    {
        private readonly ILogger<BidirectionalService> _logger;
        private readonly ClientCollection _clientCollection;

        public BidirectionalService(ILogger<BidirectionalService> logger, ClientCollection clientCollection)
        {
            _logger = logger;
            _clientCollection = clientCollection;
        }

        public override async Task BindAction(
            IAsyncStreamReader<Action> requestStream,
            IServerStreamWriter<Action> responseStream,
            ServerCallContext context
        )
        {
            var id = context.RequestHeaders.FirstOrDefault(p => p.Key == "id")?.Value ?? Guid.NewGuid().ToString();
            _clientCollection.TryAdd(id, responseStream);
            await Task.Run(async () =>
            {
                try
                {
                    await foreach (var msg in requestStream.ReadAllAsync())
                    {
                        _logger.LogInformation("get action {Msg}", msg);
                        // Thread.Sleep();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "get action process error");
                    this._clientCollection.TryRemove(id);
                    throw;
                }
            });
            this._clientCollection.TryRemove(id);
        }

        public async override Task<Action> SendAction(Action request, ServerCallContext context)
        {
            _logger.LogInformation("get action {Msg}", request);
            return request;
        }
    }
}