namespace DiscordGmail.Services;

using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using DiscordGmail.Utils;

public class BotService : IHostedService, IAsyncDisposable
{
    private readonly ILogger<BotService> _logger;
    private readonly DiscordSocketClient _client;
    private readonly IConfiguration _config;
    private readonly Task _completedTask = Task.CompletedTask;

    public BotService(ILogger<BotService> logger, DiscordSocketClient client, IConfiguration config)
    {
        _logger = logger;
        _client = client;
        _config = config;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {

        string? token = _config["Discord:Token"];
        if (string.IsNullOrEmpty(token))
        {
            throw new InvalidOperationException("Discord token not configured or token invalid.");
        }

        try
        {
            _logger.LogInformation(
                "Attempting to start discord client... Service: {ServiceName}, Token: [REDACTED]",
                nameof(BotService));
            await _client.LoginAsync(TokenType.Bot, token);

            _logger.LogInformation(
                "Login successful. Starting client asynchronously... Service: {ServiceName}, Token: [REDACTED]",
                nameof(BotService));
            await _client.StartAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting {Service}", nameof(BotService));
            throw;
        }
        
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await _client.StopAsync();
        await _client.LogoutAsync();

        _logger.LogInformation(
            "{Service} is stopping.", nameof(BotService));
    }

    public ValueTask DisposeAsync()
    {
        return _client.DisposeAsync();
    }
}