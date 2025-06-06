using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using AIIT.NVR.Core.Services;
using AIIT.NVR.Core.Models;
using AIIT.NVR.Web.Hubs;
using AIIT.NVR.Web.Services;
using AIIT.NVR.Web.Services.BackgroundServices;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace AIIT.NVR.Web
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

            // Add SignalR with enhanced configuration
            services.AddSignalR(options =>
            {
                options.EnableDetailedErrors = true;
                options.KeepAliveInterval = TimeSpan.FromSeconds(15);
                options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
                options.HandshakeTimeout = TimeSpan.FromSeconds(15);
                options.MaximumReceiveMessageSize = 1024 * 1024; // 1MB
            })
            .AddJsonProtocol(options =>
            {
                options.PayloadSerializerOptions.PropertyNamingPolicy = null;
            });

            // Add CORS with SignalR support
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAll",
                    builder =>
                    {
                        builder
                            .AllowAnyOrigin()
                            .AllowAnyMethod()
                            .AllowAnyHeader()
                            .AllowCredentials();
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

                // Configure JWT for SignalR
                x.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];
                        var path = context.HttpContext.Request.Path;
                        
                        if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/nvrhub"))
                        {
                            context.Token = accessToken;
                        }
                        return Task.CompletedTask;
                    }
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

            // Add SignalR services
            services.AddScoped<ISignalRNotificationService, SignalRNotificationService>();
            services.AddHostedService<SystemMonitoringService>();

            // Add HTTP Client for external API calls
            services.AddHttpClient();

            // Add session support
            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            // Add memory cache
            services.AddMemoryCache();

            // Add response compression
            services.AddResponseCompression(options =>
            {
                options.EnableForHttps = true;
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
            app.UseResponseCompression();

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

                // Map SignalR Hub
                endpoints.MapHub<NVRHub>("/nvrhub");
            });
        }
    }
}
