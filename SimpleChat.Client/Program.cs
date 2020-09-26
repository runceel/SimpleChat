using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SimpleChat.Client.Services;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;

namespace SimpleChat.Client
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("app");

            builder.Services.AddScoped(sp => new HttpClient 
            { 
                BaseAddress = new Uri(builder.Configuration["BaseAddress"] ?? builder.HostEnvironment.BaseAddress) 
            });

            // 設定を読み込み
            builder.Services.AddSingleton(sp =>
            {
                // Configure メソッド使いたかったけど何回試しても動かなかったので泣く泣く…
                var c = sp.GetRequiredService<IConfiguration>();
                return new CommunicationServiceSettings
                {
                    Endpoint = c[nameof(CommunicationServiceSettings)]
                };
            });
            builder.Services.AddScoped<CommunicationService>();

            await builder.Build().RunAsync();
        }
    }
}
