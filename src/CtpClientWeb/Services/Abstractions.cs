using CtpClientWeb.Models;

namespace CtpClientWeb.Services;

public interface ITradingGateway
{
    event Func<AccountSession, Task>? FundsReceived;
    event Func<OrderEvent, Task>? OrderReceived;
    event Func<TradeEvent, Task>? TradeReceived;

    Task<AccountSession> LoginAsync(string brokerId, string userId, string password, string investorId);
    Task StartAsync(CancellationToken cancellationToken);
}

public interface IMarketDataGateway
{
    event Func<MarketTick, Task>? TickReceived;
    Task SubscribeAccountAsync(string accountId, IEnumerable<string> symbols);
    Task StartAsync(CancellationToken cancellationToken);
}

public interface IPositionService
{
    IReadOnlyCollection<Position> GetPositions(string accountId);
    void ApplyTrade(TradeEvent tradeEvent);
}
