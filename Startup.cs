using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using NorthwindContextLib;
using NorthwindService.Repositories;
using Swashbuckle.AspNetCore.SwaggerUI;

namespace NorthwindService
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            string databasePath = System.IO.Path.Combine("..", "Northwind.db");
            services.AddDbContext<Northwind>(oprtions => oprtions.UseSqlite($"Data Source={databasePath}"));
            
            
            services.AddControllers(options =>
            {
                Console.WriteLine("Default output formatters:");
                foreach (IOutputFormatter formatter in options.OutputFormatters)
                {
                    var mediaFormatter = formatter as OutputFormatter;
                    if (mediaFormatter == null)
                    {
                        Console.WriteLine($" {formatter.GetType().Name}");
                    }
                    else // OutputFormatter class has SupportedMediaTypes
                    {
                        Console.WriteLine(" {0}, Media types: {1}",
                            arg0: mediaFormatter.GetType().Name,
                            arg1: string.Join(", ", mediaFormatter.SupportedMediaTypes));
                    }
                }
            }).AddXmlDataContractSerializerFormatters()
                .AddXmlSerializerFormatters()
                .SetCompatibilityVersion(CompatibilityVersion.Version_3_0);
            
            services.AddScoped<ICustomerRepository, CustomerRepository>();
            
            
            // Register the Swagger generator and define a Swagger document
            // for Northwind service
            services.AddSwaggerGen(opetions => 
                opetions.SwaggerDoc(
                    name: "v1", 
                    info: new OpenApiInfo { Title = "Northwind Service API", Version = "v1" }));


            // Added to set configuration to give services to MVC project from Northwindmvc on https://localhost:5002  
            services.AddCors();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            
            // Added to set configuration to give services to MVC project from Northwindmvc on https://localhost:5002  
            app.UseCors(options =>
            {
                
                options.WithMethods("Get" , "Post" , "Put" , "Delete");
                options.WithOrigins("https://localhost:5002"); // for MVC client
                });

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });


            app.UseSwagger();

            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "Northwind Service API Version 1");
                
                options.SupportedSubmitMethods(new[] {
                    SubmitMethod.Get, SubmitMethod.Post,
                    SubmitMethod.Put, SubmitMethod.Delete });
            });
        }
    }
}