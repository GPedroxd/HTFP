using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HTFP.Shared.Models;

namespace HTFP.SubFileProcessor.Services;

public static class OrderComparer
{
    public static List<(ExecutionOrder executedOrder, ExecutionOrder expectedOrder)> GetDivergentOrders(List<ExecutionOrder> ordersExecuted, List<ExecutionOrder> existingOrders)
    {
        var hashSetOrdersFromFile = ordersExecuted.ToDictionary(o => o.Id, o => o);

        var ordersDivergents = new ConcurrentBag<(ExecutionOrder, ExecutionOrder)>();

        Parallel.ForEach(existingOrders, order =>
        {
            var matchingOrder = hashSetOrdersFromFile[order.Id];

            if (!CompareOrders(matchingOrder, order))
                ordersDivergents.Add((matchingOrder, order));
        });

        return ordersDivergents.ToList();
    }

    private static bool CompareOrders(ExecutionOrder orderFromFile, ExecutionOrder orderFromDb)
        => orderFromFile.AssetId == orderFromDb.AssetId &&
                orderFromFile.TradingAccount == orderFromDb.TradingAccount &&
                orderFromFile.Quantity == orderFromDb.Quantity &&
                orderFromFile.UnitPrice == orderFromDb.UnitPrice;
}