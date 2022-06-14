using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WeatherSensors.Service.Extensions;
using WeatherSensors.Service.GrpcServices;
using WeatherSensors.Service.Middleware;
using WeatherSensors.Service.Options;

namespace WeatherSensors.Service
{
    public class Startup
    {
        private readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSensors(options =>
            {
                options.Config = _configuration.GetSection("SensorConfig").Get<SensorConfig>();
            });

            services.AddGrpc();

            services.AddControllers();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseRouting();

            app.UseMiddleware<LoggingMiddleware>();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapGrpcService<SensorGrpcService>();
            });
        }
    }
}