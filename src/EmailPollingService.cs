namespace DiscordGmail.Services;

using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using DiscordGmail.Utils;

public class EmailPollingService : IHostedService, IAsyncDisposable
{
    private readonly ILogger<EmailPollingService> _logger;
    private readonly DiscordSocketClient _client;
    private readonly IConfiguration _config;
    private readonly Task _completedTask = Task.CompletedTask;
    private int _executionCount = 0;
    private Timer? _timer;

    public EmailPollingService(ILogger<EmailPollingService> logger, DiscordSocketClient client, IConfiguration config)
    {
        _logger = logger;
        _client = client;
        _config = config;
        _client.Log += LogAsync;
        _client.MessageReceived += MessageReceivedAsync;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("{Service} is running.", nameof(EmailPollingService));
        _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromSeconds(5));

        return _completedTask;
    }

    private void DoWork(object? state)
    {
        int count = Interlocked.Increment(ref _executionCount);

        _logger.LogInformation(
            "{Service} is working, execution count: {Count:#,0}",
            nameof(EmailPollingService),
            count);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation(
           "{Service} is stopping.", nameof(EmailPollingService));

        _timer?.Change(Timeout.Infinite, 0);

        return _completedTask;
    }

    public async  ValueTask DisposeAsync()
    {
        if (_timer is IAsyncDisposable timer)
        {
            await timer.DisposeAsync();
        }

        _timer = null;
    }

    private Task LogAsync(LogMessage message)
    {
        Console.WriteLine(message.ToString());
        return Task.CompletedTask;
    }

    private async Task MessageReceivedAsync(SocketMessage message)
    {
        if (message.Author.IsBot) return;

        if (message.Content == "!ping")
            await message.Channel.SendMessageAsync("Pong!");
    }
}