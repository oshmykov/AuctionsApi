using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.IdentityModel.Tokens.Jwt;
using AuctionsApi.Helpers;

namespace AuctionsApi
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSwagger(Configuration);

            // Add framework services.
            services.AddMvc();

            if (Configuration["Store"].Equals("MongoDB"))
            {
                services.AddMongoStore(Configuration);
            }
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            var authorityUrl = Configuration["authorityUrl"];
            var apiScope = Configuration["apiScope"];
            bool requireHttps = Configuration.GetValue<bool>("requireHttps");

            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

            app.UseIdentityServerAuthentication(new IdentityServerAuthenticationOptions
            {
                Authority = authorityUrl,
                RequireHttpsMetadata = requireHttps,
                ApiName = apiScope
            });

            app.UseStaticFiles();

            app.UseMvcWithDefaultRoute();

            app.UseSwagger();
            app.UseSwaggerUi();
        }
    }
}
