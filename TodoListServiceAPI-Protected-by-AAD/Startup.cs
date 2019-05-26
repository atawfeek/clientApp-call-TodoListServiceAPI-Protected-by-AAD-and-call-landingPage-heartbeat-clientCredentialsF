using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.AzureAD.UI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace TodoListServiceAPI_Protected_by_AAD
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
            //add authentication to DI container defining bearer token technique rather than cookies to authenticate
            services.AddAuthentication(AzureADDefaults.BearerAuthenticationScheme)
                //add the handler to validate the bearer token.
                .AddAzureADBearer(options => Configuration.Bind("AzureAd", options));
            //Add Api Versioning
            services.AddApiVersioning();
            //Add MVC
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            //Use authentication before MVC project to authenticate all requests by default
            app.UseAuthentication();
            app.UseMvc();
        }
    }

    static class ConfigureServicesExtensions
    {
        public static IServiceCollection AddApiVersioning(this IServiceCollection services)
        {

            services.AddApiVersioning(a =>
            {
                a.AssumeDefaultVersionWhenUnspecified = true;
                a.DefaultApiVersion = new Microsoft.AspNetCore.Mvc.ApiVersion(1, 0);
                a.Conventions = new Microsoft.AspNetCore.Mvc.Versioning.Conventions.ApiVersionConventionBuilder();
            });

            return services;
        }
    }
}
