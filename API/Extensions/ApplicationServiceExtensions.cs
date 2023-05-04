using API.Data;
using API.Services;
using Microsoft.EntityFrameworkCore;
using API.Interfaces;
using API.Helpers;
using API.SignalR;

namespace API.Extensions
{
    public static class ApplicationServiceExtensions
    {
        public static void AddApplicationServices(this IServiceCollection services, IConfiguration config)
        {
            services.AddDbContext<DataContext>(opt => 
            {
                opt.UseNpgsql(config.GetConnectionString("DefaultConnection"));
            });

            services.AddCors();
            services.AddScoped<ITokenService, TokenService>();
            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
            services.Configure<CloudinarySettings>(config.GetSection("CloudinarySettings"));
            services.AddScoped<IPhotoService,PhotoService>();
            services.AddScoped<LogUserActivity>();
            services.AddSingleton<PresenceTracker>(); // do not want this to be destroyed once an HTTP request has been completed for instance, we need this to live as long as our application does 
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddSignalR();
        }
    }
}