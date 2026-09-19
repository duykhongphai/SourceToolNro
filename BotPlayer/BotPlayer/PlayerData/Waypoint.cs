namespace BotPlayer.PlayerData;

public class Waypoint
{
    public readonly short MaxX;

    public readonly short MaxY;
    public readonly short MinX;

    public readonly short MinY;
    public bool IsEnter;

    public bool IsOffline;
    public string Name;

    public Waypoint(short minX, short minY, short maxX, short maxY, bool isEnter, bool isOffline, string name)
    {
        MinX = minX;
        MinY = minY;
        MaxX = maxX;
        MaxY = maxY;
        IsEnter = isEnter;
        IsOffline = isOffline;
        Name = name;
    }
}