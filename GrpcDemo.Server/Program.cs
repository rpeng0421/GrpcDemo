using System;
using System.Net;
using Autofac;
using Autofac.Core;
using Autofac.Extensions.DependencyInjection;
using GrpcDemo.Server;
using GrpcDemo.Server.Applibs;
using GrpcDemo.Server.Jobs;
using GrpcDemo.Server.Model;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Extensions.Logging;
using NLog.Web;
using Quartz;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.SetMinimumLevel(LogLevel.Trace);
builder.Host.UseNLog();
LogManager.Configuration = new NLogLoggingConfiguration(ConfigHelper.Config.GetSection("NLog"));

builder.Services.AddGrpc();
builder.WebHost.UseUrls($"http://*:{8085}");

builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());

builder.Services.AddQuartz(q =>
{
    q.UseMicrosoftDependencyInjectionJobFactory();
    // var pingJobKey = new JobKey(nameof(SendPingActionJob));
    // q.AddJob<SendPingActionJob>(opt =>
    //     opt.WithIdentity(pingJobKey)
    // );
    // q.AddTrigger(opt => opt
    //     .ForJob(pingJobKey)
    //     .WithIdentity($"{nameof(SendPingActionJob)}-Trigger")
    //     .WithCronSchedule("*/5 * * * * ?")
    // );
});

builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

builder.Host.ConfigureContainer<ContainerBuilder>(builder =>
{
    builder.RegisterType<ClientCollection>()
        .PropertiesAutowired(PropertyWiringOptions.AllowCircularDependencies)
        .SingleInstance();
});

var app = builder.Build();

app.MapGrpcService<GreeterService>();
app.MapGrpcService<BidirectionalService>();

LogManager.GetCurrentClassLogger().Info($"Process Count:{Environment.ProcessorCount}");

app.Run();