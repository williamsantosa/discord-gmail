using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using DiscordGmail.Services;

public class Program
{
    public static async Task Main(string[] args)
    {
        await Host.CreateDefaultBuilder(args)
            .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.AddConsole();
                    logging.SetMinimumLevel(LogLevel.Information);
                })
            .ConfigureAppConfiguration(config =>
            {
                config.AddJsonFile(Path.Combine("appsettings.json"), optional: false)
                        .AddEnvironmentVariables();
            })
            .ConfigureServices((context, services) =>
            {
                services.AddSingleton<DiscordSocketClient>();
                services.AddHostedService<BotService>(); // Hosted service
                services.AddHostedService<EmailPollingService>(); // Email polling service
            })
            .RunConsoleAsync(); // Keeps the app alive until Ctrl+C
    }
}
