// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with EmptyBot .NET Template version v4.12.2

using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

using Buaa.AIBot.Utils;
using Buaa.AIBot.Utils.Logging;
using Buaa.AIBot.Services;
using Buaa.AIBot.Repository;
using Buaa.AIBot.Repository.Models;
using Buaa.AIBot.Bot;

namespace Buaa.AIBot
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public static readonly string DebugCorsPolicy = "DebugCorsPolicy";
        public static readonly string[] DebugOrigins = new string[]
        {
            @"http://localhost:8080",
            @"https://localhost:8080"
        };

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy(DebugCorsPolicy, builder =>
                {
                    builder
                        .AllowAnyOrigin()
                        // .WithOrigins(DebugOrigins)
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
            });

            services
                .AddControllers()
                // .AddNewtonsoftJson()
                ;

            services
                // Create HttpContextAccessor to access HttpContext at any time
                // .AddTransient<IHttpContextAccessor, HttpContextAccessor>()

                // Create IpAddressRecorder for Middleware UseIpAddressRecord.
                .AddSingleton<IpAddressRecorder>()
                // Create HttpHeaderRecorder for Middleware UseHttpHeaderRecord.
                .AddSingleton<HttpHeaderRecorder>()
                .AddScoped<GlobalCancellationTokenSource>();
            ;
            
            services
                // Add UserServices
                .AddUserServices(Configuration)

                .AddNLPServices(Configuration) 

                .AddTransient<IQuestionService, QuestionService>()

                .AddCodeAnalyze(Configuration)

                .AddHotList(Configuration)

                .AddRepository(Configuration.GetConnectionString("MySQL"))
                ;

            services
                // Add Bot
                //.AddEchoBot(Configuration)
                // .AddAlphaBot(Configuration)
                .AddBetaBot(Configuration)
                ;
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();

                // Record headers of request and response
                app.UseHttpHeaderRecord();
            }

            app
                // Use serilog request-logging instead of origin.
                // .UseSerilogRequestLogging()

                // We use Nginx, So redirection to https is not necessary
                // .UseHttpsRedirection()

                // set global timeout
                // .UseGlobalTimeout(TimeSpan.FromSeconds(10))

                .UseRouting();

            if (env.IsDevelopment())
            {
                // Log.Warning("Using Debug CORS policy, which allow these origins:{AllowOrigins}", DebugOrigins);
                Log.Warning("Using Debug CORS policy, which allows all origins");
                app.UseCors(DebugCorsPolicy);
            }

            app
                // Start Authentication and Authorization
                .UseAuthentication()
                .UseAuthorization();
            
            app

                // Record IP Address of the requester.
                .UseIpAddressRecord()

                .UseEndpoints(endpoints =>
                {
                    endpoints.MapControllers();
                });
        }
    }
}
