using System.Collections.Generic;
using System.Linq;
using BotPlayer.Classes.AStar;
using BotPlayer.Classes.AStar.Options;

namespace BotPlayer.PlayerData.XMap;

public class XMapAStar
{
    internal static Stack<Tile> FindPath(Map map, Tile start, Tile destination)
    {
        var pathfinderOptions = new PathFinderOptions
        {
            PunishChangeDirection = true,
            UseDiagonals = false
        };
        var tiles = new short[map.Tmh, map.Tmw];

        for (var y = 0; y < map.Tmh; y++)
        for (var x = 0; x < map.Tmw; x++)
        {
            tiles[y, x] = 1;
            if (map.Maps[y * map.Tmw + x] == 0)
            {
            }
            else if (!map.TileTypeAt(x * 24, y * 24, 2) &&
                     !IsTileMapICantEnter(map, x * 24, y * 24))
            {
            }
            else
            {
                tiles[y, x] = 0;
            }
        }

        var worldGrid = new WorldGrid(tiles);
        var pathfinder = new PathFinder(worldGrid, pathfinderOptions);
        var points = pathfinder
            .FindPath(new Point(start.X, start.Y), new Point(destination.X, destination.Y)).ToList();
        if (points.Count <= 0)
            return new Stack<Tile>();
        for (var i = points.Count - 3; i >= 0; i--)
            if (IsStraightLine(points[i], points[i + 1], points[i + 2]))
                points.RemoveAt(i + 1);
        points.RemoveAt(0);
        return new Stack<Tile>(points.Reverse<Point>().Select(p => new Tile(p.X, p.Y)));
    }

    private static bool IsTileMapICantEnter(Map map, int px, int py)
    {
        return map.TileTypeAt(px, py, 4) || map.TileTypeAt(px, py, 8) || map.TileTypeAt(px, py, 8192);
    }

    private static bool IsStraightLine(Point a, Point b, Point c)
    {
        return (a.X * (b.Y - c.Y) + b.X * (c.Y - a.Y) + c.X * (a.Y - b.Y)) / 2 == 0;
    }
}