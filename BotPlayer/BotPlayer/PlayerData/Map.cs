using System.Collections.Generic;
using System.Linq;

namespace BotPlayer.PlayerData;

public class Map
{
    public List<ItemMap> ItemMaps = [];
    public int MapId;
    public int[] Maps;
    public int[] MaxPlayer;
    public List<Mob> Mobs = [];
    public int[] NumPlayer;
    public List<Player> Players = [];
    public int[][][] TileIndex;
    public int[][] TileType;

    public int Tmh;
    public int Tmw;

    public int[] Types;
    public List<Waypoint> Waypoints = [];

    public int ZoneId;
    public int[] Zones;

    public int FindZoneLowPlayer()
    {
        if (Zones == null) return -1;
        if (NumPlayer[ZoneId] <= MaxPlayer[ZoneId] * 0.7) return -2;
        for (var i = 0; i < Zones.Length; i++)
            if (NumPlayer[i] <= MaxPlayer[i] * 0.7 && i != ZoneId)
                return i;

        return -1;
    }

    public Player FindCharInMap(int charId)
    {
        return Players.FirstOrDefault(p => p.CharId == charId, null);
    }

    public Player FindCharInMapFlag(Player me)
    {
        return Players.FirstOrDefault(p => p.CharId > 0 && p.CharId != me.CharId && p.Flag == 8, null);
    }

    public static int TileXOfPixel(int px)
    {
        return px / 24 * 24;
    }

    public int TileTypeAtPixel(int px, int py)
    {
        try
        {
            return Types[py / 24 * Tmw + px / 24];
        }
        catch
        {
            return 1000;
        }
    }

    public int TileTypeAt(int x, int y)
    {
        try
        {
            return Types[y * Tmw + x];
        }
        catch
        {
            return 1000;
        }
    }

    public bool TileTypeAt(int px, int py, int t)
    {
        try
        {
            return (Types[py / 24 * Tmw + px / 24] & t) == t;
        }
        catch
        {
            return false;
        }
    }

    public void Dispose()
    {
        Waypoints?.Clear();
        ItemMaps?.Clear();
        Players?.Clear();
        Mobs?.Clear();
    }
}