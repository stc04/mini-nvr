using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NVR.Core.Services;
using NVR.Core.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace NVR.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            // Add MVC
            services.AddControllersWithViews();

            // Add SignalR for real-time updates
            services.AddSignalR();

            // Add CORS
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAll",
                    builder =>
                    {
                        builder
                            .AllowAnyOrigin()
                            .AllowAnyMethod()
                            .AllowAnyHeader();
                    });
            });

            // Add JWT Authentication
            var jwtKey = Configuration["Jwt:Key"];
            var key = Encoding.ASCII.GetBytes(jwtKey);

            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(x =>
            {
                x.RequireHttpsMetadata = false;
                x.SaveToken = true;
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false
                };
            });

            // Register NVR Services
            services.AddSingleton<NASStorage>(provider =>
            {
                var nasConfig = Configuration.GetSection("NASStorage");
                return new NASStorage
                {
                    Name = "Primary NAS",
                    NetworkPath = nasConfig["NetworkPath"],
                    Username = nasConfig["Username"],
                    Password = nasConfig["Password"],
                    IsConnected = true
                };
            });

            services.AddSingleton<VideoRecordingService>();
            services.AddSingleton<StreamingService>();
            services.AddSingleton<CameraManager>();

            // Add Smart Home Integrations
            services.AddSingleton<SmartHomeIntegration.AlexaIntegration>();
            services.AddSingleton<SmartHomeIntegration.GoogleHomeIntegration>();
            services.AddSingleton<SmartHomeIntegration.AppleHomeKitIntegration>();

            // Add HTTP Client for external API calls
            services.AddHttpClient();

            // Add session support
            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();
            app.UseCors("AllowAll");

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseSession();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=WebViewer}/{action=Index}/{id?}");

                endpoints.MapHub<NVRHub>("/nvrhub");
            });
        }
    }
}
