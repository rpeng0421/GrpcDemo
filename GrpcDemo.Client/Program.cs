using System.Reflection;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using GrpcDemo.Client.Applibs;
using GrpcDemo.Client.Model;
using GrpcDemo.Client.Worker;
using GrpcDemo.Grpc.Service;
using Line.Bot.Manager.Applibs;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Extensions.Logging;
using NLog.Web;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;


var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.SetMinimumLevel(LogLevel.Trace);
builder.Host.UseNLog();
LogManager.Configuration = new NLogLoggingConfiguration(ConfigHelper.Config.GetSection("NLog"));

builder.WebHost.UseUrls($"http://*:{8086}");

builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());

builder.Host.ConfigureContainer<ContainerBuilder>(builder =>
{
    var asm = Assembly.GetExecutingAssembly();

    builder.RegisterType<Bidirectional.BidirectionalClient>()
        .WithParameter("channel", GrpcChannelService.GrpcChannel)
        .PropertiesAutowired(PropertyWiringOptions.AllowCircularDependencies)
        .SingleInstance();

    // 指定處理client指令的handler
    builder.RegisterAssemblyTypes(asm)
        .Where(t => t.IsAssignableTo<IActionHandler>())
        .Named<IActionHandler>(t => t.Name.Replace("Handler", string.Empty).ToLower())
        .PropertiesAutowired(PropertyWiringOptions.AllowCircularDependencies)
        .SingleInstance();

    builder.RegisterType<BidirectionalSenderService>()
        .PropertiesAutowired(PropertyWiringOptions.AllowCircularDependencies)
        .SingleInstance();
});

builder.Services.AddHostedService<BidirectionalService>();
builder.Services.AddHostedService<SendMessageWorker>();
builder.Services.AddControllers();

var app = builder.Build();

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.UseEndpoints(endpoints => { endpoints.MapControllers(); });

app.Run();