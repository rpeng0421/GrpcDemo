using System;
using System.Threading.Tasks;
using GrpcDemo.Grpc.Message;
using GrpcDemo.Server.Model;
using Microsoft.Extensions.Logging;
using Quartz;
using Action = GrpcDemo.Grpc.Message.Action;

namespace GrpcDemo.Server.Jobs
{
    [DisallowConcurrentExecution]
    public class SendPingActionJob : IJob
    {
        private readonly ClientCollection _clientCollection;
        private readonly ILogger<SendPingActionJob> _logger;

        public SendPingActionJob(
            ClientCollection clientCollection,
            ILogger<SendPingActionJob> logger
        )
        {
            _clientCollection = clientCollection;
            _logger = logger;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                _logger.LogInformation("Start Job");
                await _clientCollection.Send("1", new Action
                {
                    Type = "SendPingAction",
                    Id = Guid.NewGuid().ToString(),
                    Content = "hello",
                    CreateDateTime = DateTime.Now.ToFileTimeUtc()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "process job fail");
            }
        }
    }
}