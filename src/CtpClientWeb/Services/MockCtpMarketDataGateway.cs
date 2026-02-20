using System.Collections.Concurrent;
using CtpClientWeb.Models;

namespace CtpClientWeb.Services;

public sealed class MockCtpMarketDataGateway : IMarketDataGateway
{
    public event Func<MarketTick, Task>? TickReceived;

    private readonly ConcurrentDictionary<string, HashSet<string>> _subscriptions = new();
    private readonly Random _random = new();

    public Task SubscribeAccountAsync(string accountId, IEnumerable<string> symbols)
    {
        var set = _subscriptions.GetOrAdd(accountId, _ => new HashSet<string>(StringComparer.OrdinalIgnoreCase));
        lock (set)
        {
            foreach (var symbol in symbols.Where(s => !string.IsNullOrWhiteSpace(s)))
            {
                set.Add(symbol.Trim());
            }
        }

        return Task.CompletedTask;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            foreach (var kv in _subscriptions)
            {
                var accountId = kv.Key;
                var symbols = kv.Value.ToArray();

                foreach (var symbol in symbols)
                {
                    var tick = new MarketTick(accountId, symbol, 3500 + _random.Next(-30, 30), 3500 + _random.Next(-30, 30), 3500 + _random.Next(-30, 30), _random.Next(1, 1000), DateTimeOffset.Now);
                    if (TickReceived is not null) await TickReceived.Invoke(tick);
                }
            }

            await Task.Delay(TimeSpan.FromMilliseconds(500), cancellationToken);
        }
    }
}
