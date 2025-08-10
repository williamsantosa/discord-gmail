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
    private ulong _guildId = 0;
    private ulong _channelId = 0;
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

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("{Service} is running.", nameof(EmailPollingService));
        
        if (_config["Discord:GuildId"] == null || _config["Discord:ChannelId"] == null)
        {
            throw new InvalidOperationException("Discord GuildId or ChannelId not configured.");
        }

        _guildId = ulong.Parse(_config["Discord:GuildId"]!);;
        _channelId = ulong.Parse(_config["Discord:ChannelId"]!); ;

        _timer = new Timer(async _ =>
        {
            try
            {
                await DoWork(null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in {Service} timer callback.", nameof(EmailPollingService));
            }
        }, null, TimeSpan.Zero, TimeSpan.FromSeconds(5));
    }

    private async Task DoWork(object? state)
    {
        int count = Interlocked.Increment(ref _executionCount);

        _logger.LogInformation(
            "{Service} is working, execution count: {Count:#,0}",
            nameof(EmailPollingService),
            count);

        var guild = _client.GetGuild(_guildId);
        if (guild == null)
        {
            Console.WriteLine("Guild not found.");
            return;
        }

        var channel = guild.GetTextChannel(_channelId);
        if (channel == null)
        {
            Console.WriteLine("Channel not found or is not a text channel.");
            return;
        }

        // Send a message asynchronously
        await channel.SendMessageAsync("Hello from Discord.NET!");
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation(
           "{Service} is stopping.", nameof(EmailPollingService));

        _timer?.Change(Timeout.Infinite, 0);

        return _completedTask;
    }

    public async ValueTask DisposeAsync()
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