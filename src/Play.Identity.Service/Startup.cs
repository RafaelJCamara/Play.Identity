using GreenPipes;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using Play.Common.MassTransit;
using Play.Common.Settings;
using Play.Identity.Service.Entities;
using Play.Identity.Service.Exceptions;
using Play.Identity.Service.HostedServices;
using Play.Identity.Service.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Play.Identity.Service
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
            BsonSerializer.RegisterSerializer(new GuidSerializer(BsonType.String));
            var serviceSettings = Configuration.GetSection(nameof(ServiceSettings)).Get<ServiceSettings>();
            var mongoDbSettings = Configuration.GetSection(nameof(MongoDbSettings)).Get<MongoDbSettings>();
            var identityServerSettings = Configuration.GetSection(nameof(IdentityServerSettings)).Get<IdentityServerSettings>();

            services
                .AddMassTransitWithRabbitMq(retryConfigurator =>
                {
                    retryConfigurator.Interval(3, TimeSpan.FromSeconds(5));
                    retryConfigurator.Ignore(typeof(UnknownUserException));
                    retryConfigurator.Ignore(typeof(InsufficientFundsException));
                });

            services
                .AddIdentityServer(options =>
                {
                    //these configurations are added to give more verbosity to log events of Identity Server
                    options.Events.RaiseSuccessEvents = true;
                    options.Events.RaiseFailureEvents = true;
                    options.Events.RaiseErrorEvents = true;
                })
                .AddAspNetIdentity<ApplicationUser>()
                .AddInMemoryApiScopes(identityServerSettings.ApiScopes)
                .AddInMemoryApiResources(identityServerSettings.ApiResources)
                .AddInMemoryClients(identityServerSettings.Clients)
                .AddInMemoryIdentityResources(identityServerSettings.IdentityResources)
                .AddDeveloperSigningCredential();

            //configure Asp.NET identity and connect it to our mongoDB database
            services
                .Configure<IdentitySettings>(Configuration.GetSection(nameof(IdentitySettings)))
                .AddDefaultIdentity<ApplicationUser>()
                .AddRoles<ApplicationRole>()
                /*
                    each one of this db stores registred here, will generate a respective collection
                    here we would have the ApplicationUser collection as well has the ApplicationRole collection
                 */
                .AddMongoDbStores<ApplicationUser, ApplicationRole, Guid>(
                    mongoDbSettings.ConnectionString,
                    //this down below will generate the name of the database
                    serviceSettings.ServiceName
                );

            //this is special because we are performing authentication inside the server itself
            services
                .AddLocalApiAuthentication();

            //adds the hosted service that creates the roles and users
            services
                .AddHostedService<IdentitySeedHostedService>();

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Play.Identity.Service", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Play.Identity.Service v1"));
                app.UseCors(builder =>
                {
                    /*
                        By doing this, we are allowing any header from the server and any method (GET, POST, PUT, etc..)
                     */
                    builder
                        .WithOrigins(Configuration["AllowedOrigin"])
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
            }

            app.UseHttpsRedirection();

            app.UseStaticFiles();

            app.UseRouting();

            app.UseIdentityServer();

            app.UseAuthorization();

            /*
                Our identity server, by default, doesn't deal very well with very well with communication done over HTTP (which will be the communication type done inside our microservices in the cloud)
                In the cloud this would not be a problem, since our clients communicate with the microservices via API Gateway
                The cookie is the Identity.External, which by default has the SameSite=None and must be used with HTTPS
                SameSite=None means that any browser application can communicate with the service
             */
            app.UseCookiePolicy(new CookiePolicyOptions
            {
                /*
                    With LAX, you allow people to communicate explicitly with us sends/starts the request with us, and supports HTTP
                 */
                MinimumSameSitePolicy = SameSiteMode.Lax
            });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapRazorPages();
            });
        }
    }
}
