using GrpcDemo.Client.Model;
using GrpcDemo.Grpc.Message;
using Microsoft.Extensions.Logging;

namespace GrpcDemo.Client.Handler
{
    public class PingActionHandler : IActionHandler
    {
        private readonly ILogger<PingActionHandler> _logger;

        public PingActionHandler(ILogger<PingActionHandler> logger)
        {
            _logger = logger;
        }


        public void Handle(Action action)
        {
            this._logger.LogInformation($"PingActionHandler get action {action.ToString()}");
        }
    }
}