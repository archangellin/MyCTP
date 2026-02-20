using CtpClientWeb.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace CtpClientWeb.Services;

public sealed class GatewaySubscriptionService : BackgroundService
{
    private readonly ITradingGateway _tradingGateway;
    private readonly IMarketDataGateway _marketDataGateway;
    private readonly IPositionService _positionService;
    private readonly IHubContext<TradingHub> _hubContext;

    public GatewaySubscriptionService(ITradingGateway tradingGateway, IMarketDataGateway marketDataGateway, IPositionService positionService, IHubContext<TradingHub> hubContext)
    {
        _tradingGateway = tradingGateway;
        _marketDataGateway = marketDataGateway;
        _positionService = positionService;
        _hubContext = hubContext;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _tradingGateway.FundsReceived += funds => _hubContext.Clients.Group(funds.AccountId).SendAsync("FundsReceived", funds, stoppingToken);
        _tradingGateway.OrderReceived += order => _hubContext.Clients.Group(order.AccountId).SendAsync("OrderReceived", order, stoppingToken);
        _tradingGateway.TradeReceived += async trade =>
        {
            _positionService.ApplyTrade(trade);
            await _hubContext.Clients.Group(trade.AccountId).SendAsync("TradeReceived", trade, stoppingToken);
            await _hubContext.Clients.Group(trade.AccountId).SendAsync("PositionsUpdated", _positionService.GetPositions(trade.AccountId), stoppingToken);
        };
        _marketDataGateway.TickReceived += tick => _hubContext.Clients.Group(tick.AccountId).SendAsync("TickReceived", tick, stoppingToken);

        await Task.WhenAll(
            _tradingGateway.StartAsync(stoppingToken),
            _marketDataGateway.StartAsync(stoppingToken));
    }
}
