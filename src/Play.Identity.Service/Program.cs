using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure.Extensions.AspNetCore;
using Azure.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Play.Identity.Service
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                //here what we are doing is to add another configuration source to our app (normally we used appsettings.json, here we are adding another one to read our secrets from the azure key vault)
                .ConfigureAppConfiguration((context, configurationBuilder) =>
                {
                    //we are only interested in using Azure Key Vault for production environment
                    if (context.HostingEnvironment.IsProduction())
                    {
                        configurationBuilder.AddAzureKeyVault(
                            new Uri("<azure_key_vault_uri>"),
                            new DefaultAzureCredential()
                        );
                    }
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
