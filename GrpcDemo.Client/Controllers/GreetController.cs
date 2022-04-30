using System;
using System.Net;
using System.Threading.Tasks;
using Grpc.Net.Client;
using GrpcDemo.Client.Applibs;
using GrpcDemo.Grpc.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace GrpcDemo.Client.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class GreetController : ControllerBase
    {
        private readonly ILogger<GreetController> _logger;

        public GreetController(ILogger<GreetController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public async Task<string> Get()
        {
            try
            {
                using (var channel = GrpcChannel.ForAddress(ConfigHelper.GrpcServerUrl))
                {
                    var client = new Greeter.GreeterClient(channel);
                    var helloResult = await client.SayHelloAsync(new HelloRequest());

                    Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    return helloResult.Message;
                }
            }
            catch (Exception ex)
            {
                Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                _logger.LogError(ex, "process error");
                throw;
            }
        }
    }
}