using System.Reflection.Metadata;
using GrpcDemo.Grpc.Message;

namespace GrpcDemo.Client.Model
{
    public interface IActionHandler
    {
        void Handle(Action action);
    }
}