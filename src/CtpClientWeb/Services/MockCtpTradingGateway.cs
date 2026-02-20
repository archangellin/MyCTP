using System.Collections.Concurrent;
using CtpClientWeb.Models;

namespace CtpClientWeb.Services;

public sealed class MockCtpTradingGateway : ITradingGateway
{
    public event Func<AccountSession, Task>? FundsReceived;
    public event Func<OrderEvent, Task>? OrderReceived;
    public event Func<TradeEvent, Task>? TradeReceived;

    private readonly ConcurrentDictionary<string, AccountSession> _accounts = new();
    private readonly Random _random = new();

    public Task<AccountSession> LoginAsync(string brokerId, string userId, string password, string investorId)
    {
        var accountId = $"{brokerId}-{userId}";
        var session = new AccountSession(accountId, 1_000_000m, 1_000_000m);
        _accounts[accountId] = session;
        return Task.FromResult(session);
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            foreach (var account in _accounts.Values)
            {
                var funds = account with { AvailableFunds = account.AvailableFunds + _random.Next(-2000, 2000), Balance = account.Balance + _random.Next(-2500, 2500) };
                _accounts[account.AccountId] = funds;
                if (FundsReceived is not null) await FundsReceived.Invoke(funds);

                var orderRef = _random.Next(100000, 999999).ToString();
                var order = new OrderEvent(account.AccountId, orderRef, "rb2410", "PartTraded", 10, _random.Next(1, 9), 3521 + _random.Next(-12, 12), DateTimeOffset.Now);
                if (OrderReceived is not null) await OrderReceived.Invoke(order);

                var direction = _random.NextDouble() > 0.5 ? "Buy" : "Sell";
                var trade = new TradeEvent(account.AccountId, Guid.NewGuid().ToString("N")[..10], orderRef, "rb2410", _random.Next(1, 3), 3521 + _random.Next(-5, 5), direction, DateTimeOffset.Now);
                if (TradeReceived is not null) await TradeReceived.Invoke(trade);
            }

            await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
        }
    }
}
