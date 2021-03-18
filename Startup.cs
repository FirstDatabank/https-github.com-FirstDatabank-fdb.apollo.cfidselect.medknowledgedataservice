
using System;
using System.Collections.Generic;
using System.Text;
using Azure.Identity;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(fdb.apollo.cfidselect.mkdataservice.Startup))]
namespace fdb.apollo.cfidselect.mkdataservice
{
    public class Startup : FunctionsStartup
    {
        public IConfigurationRoot Configuration { get; set; }

        public override void ConfigureAppConfiguration(IFunctionsConfigurationBuilder builder)
        {
            var managedIdentityCredentials = new ManagedIdentityCredential();
            var defaultCredentials = new DefaultAzureCredential();
            builder.ConfigurationBuilder
            .AddEnvironmentVariables()
            .AddAzureAppConfiguration((options) =>
            {
                var configconnection = Environment.GetEnvironmentVariable("AppConfigConnectionString");
                options.Connect(configconnection);
                options.ConfigureKeyVault(vault => vault.SetCredential(new DefaultAzureCredential()));

                //use following after setting managed identity
                //options.Connect(new Uri(Environment.GetEnvironmentVariable("ConfigConnString")), new ManagedIdentityCredential()));

            });

            Configuration = builder.ConfigurationBuilder.Build();
        }

        public void ConfigureServices(IServiceCollection services)
        {

        }

        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.Configure<AppSettings>(Configuration);
            builder.Services.AddAzureAppConfiguration();
            builder.Services.AddOptions();
        }

    }
}

