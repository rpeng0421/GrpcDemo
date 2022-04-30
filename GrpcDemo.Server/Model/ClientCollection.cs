using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Action = GrpcDemo.Grpc.Message.Action;

namespace GrpcDemo.Server.Model
{
    public class ClientCollection
    {
        private ConcurrentDictionary<string, IServerStreamWriter<Action>> _clientDictionary =
            new ConcurrentDictionary<string, IServerStreamWriter<Action>>();

        private readonly ILogger<ClientCollection> _logger;

        public ClientCollection(ILogger<ClientCollection> logger)
        {
            _logger = logger;
        }

        public void Send(string id, Action action)
        {
            Task t = null;
            try
            {
                lock (id)
                {
                    if (!this._clientDictionary.ContainsKey(id))
                    {
                        throw new Exception("not found this id in dict");
                    }

                    var client = this._clientDictionary[id];
                    t = client.WriteAsync(action);
                }

                t.Wait();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
        }
    }
}