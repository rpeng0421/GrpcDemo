using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Action = GrpcDemo.Grpc.Message.Action;

namespace GrpcDemo.Server.Model
{
    public class ClientCollection
    {
        private readonly ConcurrentDictionary<string, IServerStreamWriter<Action>> _clientDictionary =
            new ConcurrentDictionary<string, IServerStreamWriter<Action>>();

        private readonly SemaphoreSlim _locker = new SemaphoreSlim(1, 1);

        private readonly ILogger<ClientCollection> _logger;

        public ClientCollection(ILogger<ClientCollection> logger)
        {
            _logger = logger;
        }

        public async Task Send(string id, Action action)
        {
            try
            {
                await _locker.WaitAsync();
                if (!_clientDictionary.ContainsKey(id))
                {
                    throw new Exception("not found this id in dict");
                }

                var client = _clientDictionary[id];
                await client.WriteAsync(action);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"process send action fail id: {id}, action: {action.ToString()}");
                throw;
            }
            finally
            {
                _locker.Release();
            }
        }
    }
}