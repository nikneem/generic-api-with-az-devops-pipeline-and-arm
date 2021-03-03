using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Demo.Core.Configuration;

namespace Demo.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        private static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {

                    webBuilder.ConfigureAppConfiguration((hostingContext, config) =>
                    {
                        var currentSettings = config.Build();
                        if (hostingContext.HostingEnvironment.IsDevelopment())
                        {
                            var credential = new DefaultAzureCredential();
                            var secretClient = new SecretClient(new Uri("https://confoo-test-keyvault.vault.azure.net/"), credential);
                            var connectionString = currentSettings[$"{AppConfigurationService.SectionName}:ConnectionString"];
                            config.AddAzureAppConfiguration(act => 
                                act.Connect(connectionString)
                                    .ConfigureKeyVault(kvConfig =>
                                    {
                                        kvConfig.Register(secretClient);
                                    })
                            );

                        }
                        else
                        {
                            var managedIdentity = new ManagedIdentityCredential();
                            var endpoint = currentSettings[$"{AppConfigurationService.SectionName}:Endpoint"];
                            config.AddAzureAppConfiguration(act => 
                                act.Connect(new Uri(endpoint), managedIdentity)
                                    .ConfigureKeyVault(kvConfig =>
                                    {
                                        kvConfig.SetCredential(managedIdentity);
                                    })
                                );
                        }
                    });

                    webBuilder.UseStartup<Startup>();
                });
    }
}
