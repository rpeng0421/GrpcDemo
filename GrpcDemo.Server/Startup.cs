using Autofac;
using GrpcDemo.Server.Jobs;
using GrpcDemo.Server.Model;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Quartz;

namespace GrpcDemo.Server
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddGrpc();

            services.AddQuartz(q =>
            {
                q.UseMicrosoftDependencyInjectionJobFactory();
                var pingJobKey = new JobKey(nameof(SendPingActionJob));
                q.AddJob<SendPingActionJob>(opt =>
                    opt.WithIdentity(pingJobKey)
                );
                q.AddTrigger(opt => opt
                    .ForJob(pingJobKey)
                    .WithIdentity($"{nameof(SendPingActionJob)}-Trigger")
                    .WithCronSchedule("*/5 * * * * ?")
                );
            });

            services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment()) app.UseDeveloperExceptionPage();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGrpcService<GreeterService>();
                endpoints.MapGrpcService<BidirectionalService>();

                endpoints.MapGet("/",
                    async context =>
                    {
                        await context.Response.WriteAsync(
                            "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");
                    });
            });
        }

        public void ConfigureContainer(ContainerBuilder builder)
        {
            builder.RegisterType<ClientCollection>()
                .PropertiesAutowired(PropertyWiringOptions.AllowCircularDependencies)
                .SingleInstance();
            ;
        }
    }
}