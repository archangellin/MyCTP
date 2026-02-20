using CtpClientWeb.Hubs;
using CtpClientWeb.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddSignalR();
builder.Services.AddSingleton<IPositionService, PositionService>();
builder.Services.AddSingleton<ITradingGateway, MockCtpTradingGateway>();
builder.Services.AddSingleton<IMarketDataGateway, MockCtpMarketDataGateway>();
builder.Services.AddHostedService<GatewaySubscriptionService>();

var app = builder.Build();

app.UseStaticFiles();
app.UseRouting();

app.MapRazorPages();
app.MapHub<TradingHub>("/hubs/trading");

app.MapPost("/api/accounts/login", async (AccountLoginRequest request, ITradingGateway tradingGateway, IMarketDataGateway marketDataGateway) =>
{
    var account = await tradingGateway.LoginAsync(request.BrokerId, request.UserId, request.Password, request.InvestorId);
    await marketDataGateway.SubscribeAccountAsync(account.AccountId, request.Symbols);
    return Results.Ok(account);
});

app.Run();

public sealed record AccountLoginRequest(string BrokerId, string UserId, string Password, string InvestorId, string[] Symbols);
