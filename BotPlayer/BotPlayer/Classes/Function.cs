using System;
using System.Text;
using BotPlayer.PlayerData;

namespace BotPlayer.Classes;

public static class Function
{
    private static readonly Random Random = new();
    public static string[] ChatDataArray;

    public static void InitCharData()
    {
        var data = Convert.FromBase64String(Const.ChatData);
        var decodedLine = Encoding.UTF8.GetString(data);
        ChatDataArray = decodedLine.Split("\n");
    }

    public static bool IsInWayPoint(Player player, Waypoint p)
    {
        return player.X + 100 >= p.MinX && player.X <= p.MaxX + 100 && player.Y + 100 >= p.MinY &&
               player.Y <= p.MaxY + 100;
    }

    public static int GetDistance(int x1, int y1, int x2, int y2)
    {
        return (int)Math.Sqrt(Math.Pow(x1 - x2, 2) + Math.Pow(y1 - y2, 2));
    }

    public static int GetDistance(Mob mob1, Player p2)
    {
        return (int)Math.Sqrt(Math.Pow(mob1.x - p2.X, 2) + Math.Pow(mob1.y - p2.Y, 2));
    }

    public static int GetDistance(Player p1, Player p2)
    {
        return (int)Math.Sqrt(Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2));
    }

    public static string GenerateRandomChat()
    {
        return ChatDataArray[NextInt(0, ChatDataArray.Length - 1)];
    }

    public static string GenerateString(int length)
    {
        const string chars = "abcdefghijklmnopqrstuvwxyz0123456789";
        StringBuilder result = new(length);
        for (var i = 0; i < length; i++) result.Append(chars[Random.Next(chars.Length)]);
        return result.ToString();
    }

    public static int NextInt(int a, int b)
    {
        if (a == b) return a;
        return a + Random.Next(b - a);
    }
}