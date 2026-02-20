using System.Collections.Concurrent;
using CtpClientWeb.Models;

namespace CtpClientWeb.Services;

public sealed class PositionService : IPositionService
{
    private readonly ConcurrentDictionary<string, Position> _positions = new();

    public IReadOnlyCollection<Position> GetPositions(string accountId)
    {
        return _positions.Values.Where(x => x.AccountId == accountId).OrderBy(x => x.InstrumentId).ToArray();
    }

    public void ApplyTrade(TradeEvent tradeEvent)
    {
        var key = $"{tradeEvent.AccountId}:{tradeEvent.InstrumentId}";

        _positions.AddOrUpdate(key,
            _ => CreateByTrade(tradeEvent),
            (_, current) => MergeByTrade(current, tradeEvent));
    }

    private static Position CreateByTrade(TradeEvent tradeEvent)
    {
        return tradeEvent.Direction.Equals("Buy", StringComparison.OrdinalIgnoreCase)
            ? new Position(tradeEvent.AccountId, tradeEvent.InstrumentId, tradeEvent.Volume, 0, tradeEvent.Price)
            : new Position(tradeEvent.AccountId, tradeEvent.InstrumentId, 0, tradeEvent.Volume, tradeEvent.Price);
    }

    private static Position MergeByTrade(Position current, TradeEvent tradeEvent)
    {
        if (tradeEvent.Direction.Equals("Buy", StringComparison.OrdinalIgnoreCase))
        {
            var qty = current.LongQty + tradeEvent.Volume;
            var avg = qty == 0 ? 0 : ((current.AvgPrice * current.LongQty) + (tradeEvent.Price * tradeEvent.Volume)) / qty;
            return current with { LongQty = qty, AvgPrice = avg };
        }

        var shortQty = current.ShortQty + tradeEvent.Volume;
        var shortAvg = shortQty == 0 ? 0 : ((current.AvgPrice * current.ShortQty) + (tradeEvent.Price * tradeEvent.Volume)) / shortQty;
        return current with { ShortQty = shortQty, AvgPrice = shortAvg };
    }
}
