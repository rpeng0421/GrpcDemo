using System;
using System.Collections;
using System.Linq;
using System.Net;
using System.Net.Http;
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
        private readonly BidirectionalSenderService bidirectionalSenderService;
        private readonly IIndex<string, IActionHandler> handlerSet;


        public BidirectionalService(
            ILogger<BidirectionalService> logger,
            IIndex<string, IActionHandler> handlerSet, BidirectionalSenderService bidirectionalSenderService)
        {
            _logger = logger;
            this.handlerSet = handlerSet;
            this.bidirectionalSenderService = bidirectionalSenderService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                var channel = GrpcChannelService.GrpcChannel;
                var header = new Metadata() { new Metadata.Entry("id", "1") };
                var client = new Bidirectional.BidirectionalClient(channel);
                var bidirection = client.BindAction(new CallOptions(header));
                bidirectionalSenderService.RequestStream = bidirection.RequestStream;
                await foreach (var action in bidirection
                                   .ResponseStream
                                   .ReadAllAsync(cancellationToken: stoppingToken))
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