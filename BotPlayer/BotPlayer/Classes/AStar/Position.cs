namespace BotPlayer.Classes.AStar;

public readonly struct Position
{
    public int Row { get; }

    public int Column { get; }

    public Position(int row = 0, int column = 0)
    {
        Row = row;
        Column = column;
    }

    public bool IsDiagonalTo(Position other)
    {

        return Row != other.Row &&
               Column != other.Column;
    }

    public static bool operator ==(Position a, Position b)
    {
        return a.Equals(b);
    }

    public static bool operator !=(Position a, Position b)
    {
        return !a.Equals(b);
    }

    public override bool Equals(object other)
    {
        if (other is Position otherPoint) return Row == otherPoint.Row && Column == otherPoint.Column;

        return false;
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hash = 17;
            hash = hash * 23 + Row.GetHashCode();
            hash = hash * 23 + Column.GetHashCode();
            return hash;
        }
    }

    public override string ToString()
    {
        return $"[{Row}.{Column}]";
    }
}