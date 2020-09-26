using Azure.Communication.Administration;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SimpleChat.Server;
using System;

[assembly: FunctionsStartup(typeof(Startup))]

namespace SimpleChat.Server
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddTransient(provider =>
              {
                  var c = provider.GetRequiredService<IConfiguration>();
                  return new CommunicationIdentityClient(c.GetValue<string>("AzureCommunicationServices"));
              });
        }
    }
}
