using System;
using System.Collections;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Autofac.Features.Indexed;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Net.Client;
using GrpcDemo.Client.Applibs;
using GrpcDemo.Client.Model;
using GrpcDemo.Grpc.Message;
using GrpcDemo.Grpc.Service;
using Line.Bot.Manager.Applibs;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Action = GrpcDemo.Grpc.Message.Action;

namespace GrpcDemo.Client.Worker
{
    public class BidirectionalService : BackgroundService
    {
        private readonly ILogger<BidirectionalService> _logger;

        private readonly IIndex<string, IActionHandler> handlerSet;


        public BidirectionalService(
            ILogger<BidirectionalService> logger,
            IIndex<string, IActionHandler> handlerSet
        )
        {
            _logger = logger;
            this.handlerSet = handlerSet;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                var channel = GrpcChannelService.GrpcChannel;
                var header = new Metadata() { new Metadata.Entry("id", "1") };
                var client = new Bidirectional.BidirectionalClient(channel);
                var bidirection = client.BindAction(new CallOptions(header));
                Task.Run(() =>
                {
                    while (true)
                    {
                        var users = Enumerable.Range(1, 100).Select(x => new User
                        {
                            Id = $"Id-{x}",
                            Name = $"Name-{x}"
                        });
                        var data = JsonConvert.SerializeObject(users);
                        var tasks = Enumerable.Range(1, 10).Select(x => client.SendActionAsync(new Action
                        {
                            Type = "UserEvent",
                            Id = Guid.NewGuid().ToString(),
                            Content = data,
                            CreateDateTime = 0
                        }).ResponseAsync).ToList();
                        Task.WaitAll(tasks.ToArray());
                        Thread.Sleep(TimeSpan.FromSeconds(30));
                    }
                });
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "connect process error maybe disconnect");
                Thread.Sleep(5000);
                await this.ExecuteAsync(stoppingToken);
            }
        }
    }
}