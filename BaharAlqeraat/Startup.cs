using System;
using System.IO.Compression;
using System.Threading.RateLimiting;
using Bahar.Domain.Data.Dto;
using Bahar.Service;
using BaharAlqeraat.Domain;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi;

namespace BaharAlqeraat
{
    public class Startup
    {
        /// <summary>
        /// Limits how often one address may register a device.
        /// </summary>
        /// <remarks>
        /// That endpoint writes a row for any token it is handed, so without a
        /// limit anyone can fill the table at the speed of their connection.
        /// <para>
        /// The allowance is deliberately loose. A device registers once per
        /// launch, but many readers share one address behind a carrier or a
        /// mosque's wifi, and a limit tight enough to stop abuse quickly would
        /// shut out a room of people opening the app after a prayer. This stops
        /// the table growing without bound; it is not meant to stop a determined
        /// attacker, which is a job for the endpoint requiring something it
        /// cannot mint for itself.
        /// </para>
        /// </remarks>
        public const string RegisterDevicePolicy = "register-device";

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddControllers().AddNewtonsoftJson(options =>
                options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore);
            services.AddMemoryCache();
            services.AddCors();

            // Responses here are JSON and compress well: measured, the reader
            // list goes from 2116 bytes to 581 and a sura from 4997 to 1833.
            // Enabled over HTTPS as well, which is safe only because nothing in
            // these responses is secret -- they carry the Qur'an and a page
            // number, no session and no credentials.
            services.AddResponseCompression(options =>
            {
                options.EnableForHttps = true;
                options.Providers.Add<BrotliCompressionProvider>();
                options.Providers.Add<GzipCompressionProvider>();
            });
            // Both default to Fastest, and brotli at Fastest is worse than gzip
            // -- 823 bytes against 734 on the reader list. Since brotli is
            // offered first it would win and hand back the larger answer. These
            // responses are small and cached, so the extra work is nothing.
            services.Configure<BrotliCompressionProviderOptions>(o => o.Level = CompressionLevel.Optimal);
            services.Configure<GzipCompressionProviderOptions>(o => o.Level = CompressionLevel.Optimal);

            services.AddRateLimiter(options =>
            {
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
                options.AddPolicy(RegisterDevicePolicy, context =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                        _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 60,
                            Window = TimeSpan.FromMinutes(1),
                            QueueLimit = 0,
                        }));
            });

            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"));
            });

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "BaharAlqeraat", Version = "v1" });
            });
            services.Configure<AppSettings>(Configuration.GetSection("AppSettings"));
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IQuranService, QuranService>();
            services.AddScoped<IAudioScrapingService, AudioScrapingService>();
            services.AddSingleton<IBackgroundServiceStatus, BackgroundServiceStatus>();
            services.AddHostedService<AudioScrapingBackgroundService>();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // Before anything that writes a response.
            app.UseResponseCompression();

            if (env.IsDevelopment())
            {
                // Both of these used to run everywhere. The exception page
                // answers an unhandled error with a stack trace -- and every
                // controller rethrows, so errors do reach it -- while Swagger
                // published the whole surface of the API to anyone who asked.
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "BaharAlqeraat v1"));
            }
            else
            {
                // Says that something failed, and nothing about what. Written
                // inline rather than pointed at a route so there is no error
                // endpoint of its own to find.
                app.UseExceptionHandler(handler => handler.Run(async context =>
                {
                    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                    context.Response.ContentType = "application/json; charset=utf-8";
                    await context.Response.WriteAsync("{\"error\":\"Something went wrong.\"}");
                }));
            }

            app.UseCors(
           options => options.WithOrigins("*").AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()
       );
            app.UseHttpsRedirection();

            app.UseRouting();
            app.UseRateLimiter();
            app.UseStaticFiles();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
