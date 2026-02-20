namespace CtpClientWeb.Models;

public sealed record AccountSession(string AccountId, decimal AvailableFunds, decimal Balance);

public sealed record Position(string AccountId, string InstrumentId, int LongQty, int ShortQty, decimal AvgPrice)
{
    public int NetQty => LongQty - ShortQty;
}

public sealed record OrderEvent(string AccountId, string OrderRef, string InstrumentId, string Status, int VolumeTotalOriginal, int VolumeTraded, decimal LimitPrice, DateTimeOffset Time);

public sealed record TradeEvent(string AccountId, string TradeId, string OrderRef, string InstrumentId, int Volume, decimal Price, string Direction, DateTimeOffset Time);

public sealed record MarketTick(string AccountId, string InstrumentId, decimal LastPrice, decimal BidPrice1, decimal AskPrice1, int Volume, DateTimeOffset Time);
