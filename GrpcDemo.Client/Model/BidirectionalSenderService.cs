using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using GrpcDemo.Grpc.Message;
using GrpcDemo.Grpc.Service;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Action = GrpcDemo.Grpc.Message.Action;

namespace GrpcDemo.Client.Model;

public class BidirectionalSenderService
{
    private readonly ILogger<BidirectionalSenderService> logger;
    private readonly ConcurrentQueue<IDomainEvent> domainEventsCollection = new ConcurrentQueue<IDomainEvent>();
    private readonly Bidirectional.BidirectionalClient bidirectionalClient;

    public BidirectionalSenderService(ILogger<BidirectionalSenderService> logger,
        Bidirectional.BidirectionalClient bidirectionalClient)
    {
        this.logger = logger;
        this.bidirectionalClient = bidirectionalClient;
        Task.Run(StartSendStream);
    }

    private async Task StartSendStream()
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(1));
        while (await timer.WaitForNextTickAsync())
        {
            var eventlist = new List<IDomainEvent>();
            while (domainEventsCollection.TryDequeue(out var domainEvent))
            {
                eventlist.Add(domainEvent);
            }

            await SendActionStreamAsync(eventlist);
        }
    }

    public IClientStreamWriter<StreamAction>? RequestStream { get; set; }

    public async Task SendActionAsync(IDomainEvent domainEvent)
    {
        try
        {
            await bidirectionalClient.SendActionAsync(new Action
            {
                Type = domainEvent.Type,
                Id = domainEvent.EventId,
                Content = domainEvent.Data,
                CreateDateTime = domainEvent.Timestamp
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "SendActionAsync fail {req}", JsonConvert.SerializeObject(domainEvent));
            throw;
        }
    }

    public void SendActionStream(IDomainEvent domainEvent)
    {
        domainEventsCollection.Enqueue(domainEvent);
    }

    private async Task SendActionStreamAsync(IEnumerable<IDomainEvent> domainEvents)
    {
        try
        {
            if (RequestStream == null)
            {
                throw new Exception("request stream is invalidate");
            }

            var actions = domainEvents.Select(domainEvent => new Action
            {
                Type = domainEvent.Type,
                Id = domainEvent.EventId,
                Content = domainEvent.Data,
                CreateDateTime = domainEvent.Timestamp
            });
            var streamAction = new StreamAction
            {
                Actions = { actions }
            };

            await RequestStream.WriteAsync(streamAction);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "send action fail {domainEvent}", JsonConvert.SerializeObject(domainEvents));
        }
    }
}