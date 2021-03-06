﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CourseApi.V2.Filters;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using CourseApi.V2.Services.Interfaces;
using CourseApi.V2.Services.Implementations;
using Swashbuckle.Swagger.Model;

namespace CourseApi.V2
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

            if (env.IsEnvironment("Development"))
            {
                // This will push telemetry data through Application Insights pipeline faster, allowing you to view results immediately.
                builder.AddApplicationInsightsSettings(developerMode: true);
            }

            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddApplicationInsightsTelemetry(Configuration);

            services.AddMvc(options =>
            {
                options.Filters.Add(typeof(CustomExceptionHandler));
            });
            var xmlDocs = Configuration["XML:Output"];
            
            services.AddSwaggerGen();
            services.ConfigureSwaggerGen(options =>
            {
                options.SingleApiVersion(new Info
                {
                    Version = "v1",
                    Title = "Course API",
                    Description = "An API to search, add and modify courses and students",
                    TermsOfService = "None"
                });
                options.IncludeXmlComments(xmlDocs);
                options.DescribeAllEnumsAsStrings();
            });

            // Add my own DI
            services.AddTransient<ICourseService, CourseService>();
            services.AddTransient<IStudentService, StudentService>();

            CourseApi.V2.Services.Startup.Initialize(services);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            app.UseApplicationInsightsRequestTelemetry();

            app.UseApplicationInsightsExceptionTelemetry();

            app.UseMvc();

            app.UseSwagger();
            app.UseSwaggerUi("docs");
        }
    }
}
