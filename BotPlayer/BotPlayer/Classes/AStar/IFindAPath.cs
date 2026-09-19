namespace BotPlayer.Classes.AStar;

public interface IFindAPath
{
    Position[] FindPath(Position start, Position end);

    Point[] FindPath(Point start, Point end);
}