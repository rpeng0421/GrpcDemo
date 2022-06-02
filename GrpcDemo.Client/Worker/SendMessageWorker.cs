using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GrpcDemo.Client.Model;
using GrpcDemo.Grpc.Message;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Action = GrpcDemo.Grpc.Message.Action;

namespace GrpcDemo.Client.Worker;

public class SendMessageWorker : BackgroundService
{
    private readonly BidirectionalSenderService bidirectionalSenderService;
    private readonly ILogger<SendMessageWorker> logger;

    public SendMessageWorker(BidirectionalSenderService bidirectionalSenderService, ILogger<SendMessageWorker> logger)
    {
        this.bidirectionalSenderService = bidirectionalSenderService;
        this.logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (true)
        {
            try
            {
                var users = Enumerable.Range(1, 50).Select(x => new User
                {
                    Id = $"Id-{x}",
                    Name = $"Name-{x}"
                });
                var data = JsonConvert.SerializeObject(users);
                for (int i = 0; i < 100; i++)
                {
                    bidirectionalSenderService.SendActionStream(
                        new DomainEvent()
                        {
                            Type = "UserEvent",
                            EventId = Guid.NewGuid().ToString(),
                            Data = data,
                            Timestamp = 0
                        });
                }

                Thread.Sleep(TimeSpan.FromMilliseconds(100));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "fail");
            }
        }
    }
}