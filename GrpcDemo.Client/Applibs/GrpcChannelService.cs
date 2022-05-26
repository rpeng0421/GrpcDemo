using System;
using System.Net.Http;
using Grpc.Net.Client;
using GrpcDemo.Client.Applibs;

namespace Line.Bot.Manager.Applibs
{
    internal static class GrpcChannelService
    {
        private static Lazy<GrpcChannel>? lazyGrpcChannel;

        public static GrpcChannel GrpcChannel
        {
            get
            {
                if (lazyGrpcChannel == null)
                {
                    lazyGrpcChannel = new Lazy<GrpcChannel>(() =>
                    {
                        var httpHandler = new HttpClientHandler();
                        // Return `true` to allow certificates that are untrusted/invalid
                        httpHandler.ServerCertificateCustomValidationCallback =
                            HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;

                        return GrpcChannel.ForAddress(ConfigHelper.GrpcServerUrl,
                            new GrpcChannelOptions { HttpHandler = httpHandler });
                    });
                }

                return lazyGrpcChannel.Value;
            }
        }
    }
}