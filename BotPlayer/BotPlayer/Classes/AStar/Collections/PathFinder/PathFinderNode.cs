using System.Runtime.InteropServices;

namespace BotPlayer.Classes.AStar.Collections.PathFinder;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal readonly struct PathFinderNode
{
    public Position Position { get; }

    public int G { get; }

    public int H { get; }

    public Position ParentNodePosition { get; }

    public int F { get; }

    public bool HasBeenVisited => F > 0;

    public PathFinderNode(Position position, int g, int h, Position parentNodePosition)
    {
        Position = position;
        G = g;
        H = h;
        ParentNodePosition = parentNodePosition;

        F = g + h;
    }
}