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
    private SocketGuild _guild;
    private SocketTextChannel _channel;
    private ulong _guildId = 0;
    private ulong _channelId = 0;
    private int _executionCount = 0;
    private Timer? _timer;

    public EmailPollingService(ILogger<EmailPollingService> logger, DiscordSocketClient client, IConfiguration config)
    {
        _logger = logger;
        _client = client;
        _config = config;

        if (_config["Discord:GuildId"] == null || _config["Discord:ChannelId"] == null)
        {
            throw new InvalidOperationException("Discord GuildId or ChannelId not configured.");
        }

        _guildId = ulong.Parse(_config["Discord:GuildId"]!); ;
        _channelId = ulong.Parse(_config["Discord:ChannelId"]!); ;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("{Service} is running.", nameof(EmailPollingService));

        _client.Ready += async () =>
        {
            _guild = _client.GetGuild(_guildId != null ? _guildId : 0);
            if (_guild == null)
            {
                _logger.LogInformation("{Service}. Guild not found. GuildId: {_guildId}", nameof(EmailPollingService), _guildId);
                return;
            }

            _channel = _guild.GetTextChannel(_channelId != null ? _channelId : 0);
            if (_channel == null)
            {
                _logger.LogInformation("{Service}. Channel not found. GuildId: {_channelId}", nameof(EmailPollingService), _channelId);
                return;
            }

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
        };
    }

    private async Task DoWork(object? state)
    {
        int count = Interlocked.Increment(ref _executionCount);

        _logger.LogInformation(
            "{Service} is working, execution count: {Count:#,0}",
            nameof(EmailPollingService),
            count);

        // Send a message asynchronously
        await _channel.SendMessageAsync(embed:
            new EmbedBuilder()
                .WithTitle("Hello from EmbedBuilder!")
                .WithDescription("This is a richly formatted embed message.")
                .WithColor(Color.Purple)
                .AddField("Field 1", "This is the value for field 1", inline: true)
                .AddField("Field 2", "This is the value for field 2", inline: true)
                .WithFooter(footer => footer.Text = "Footer text here")
                .WithTimestamp(DateTimeOffset.UtcNow)
                .Build());
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
}