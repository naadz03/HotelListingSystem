using HotelListingSystem.BackgroundTask;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Owin;
using Owin;
using HotelListingSystem.BackgroundTask;

[assembly: OwinStartupAttribute(typeof(HotelListingSystem.Startup))]
namespace HotelListingSystem
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
        public void ConfigureServices(IServiceCollection services)
        {
            //services.AddHostedService<BackTaskJob>();
            // Register the dependencies
            services.AddScoped<SystemUserManager>();
            services.AddScoped<ApplicationSignInManager>();

            // ...
            //services.AddSingleton<IWorker, Worker>();

        }

    }
}
