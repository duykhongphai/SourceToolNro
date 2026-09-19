using System.Collections.Generic;

namespace BotPlayer.PlayerData.XMap;

public class XMapAlgorithm
{
    public static List<int> FindWay(Player player, int idMapStart, int idMapEnd)
    {
        var wayPassed = GetWayPassedStart(idMapStart);
        var way = FindWay(player, idMapEnd, wayPassed);
        return way;
    }

    private static List<int> FindWay(Player player, int idMapEnd, List<int> wayPassed)
    {
        var idMapLast = wayPassed[^1];

        if (idMapLast == idMapEnd)
            return wayPassed;
        if (!player.XMapData.CanGetMapNexts(idMapLast))
            return null;

        List<List<int>> ways = [];
        var mapNexts = player.XMapData.GetMapNexts(idMapLast);
        foreach (var map in mapNexts)
        {
            List<int> wayContinue = null;
            if (!wayPassed.Contains(map.MapId))
            {
                var wayPassedNext = GetWayPassedNext(wayPassed, map.MapId);
                wayContinue = FindWay(player, idMapEnd, wayPassedNext);
            }

            if (wayContinue != null)
                ways.Add(wayContinue);
        }

        var bestWay = GetBestWay(ways);
        return bestWay;
    }

    private static List<int> GetBestWay(List<List<int>> ways)
    {
        if (ways.Count == 0)
            return null;

        var bestWay = ways[0];
        for (var i = 1; i < ways.Count; i++)
            if (IsWayBetter(ways[i], bestWay))
                bestWay = ways[i];
        return bestWay;
    }

    private static List<int> GetWayPassedStart(int idMapStart)
    {
        List<int> wayPassed = [idMapStart];
        return wayPassed;
    }

    private static List<int> GetWayPassedNext(List<int> wayPassed, int idMapNext)
    {
        List<int> wayNext = [..wayPassed, idMapNext];
        return wayNext;
    }

    private static bool IsWayBetter(List<int> way1, List<int> way2)
    {
        var flag1 = IsBadWay(way1);
        var flag2 = IsBadWay(way2);
        return flag1 switch
        {
            true when !flag2 => false,
            false when flag2 => true,
            _ => way1.Count < way2.Count
        };
    }

    private static bool IsBadWay(List<int> way)
    {
        return IsWayGoFutureAndBack(way);
    }

    private static bool IsWayGoFutureAndBack(List<int> way)
    {
        List<int> mapsGoFuture = [27, 28, 29];
        for (var i = 1; i < way.Count - 1; i++)
            if (way[i] == 102 && way[i + 1] == 24 && mapsGoFuture.Contains(way[i - 1]))
                return true;
        return false;
    }
}