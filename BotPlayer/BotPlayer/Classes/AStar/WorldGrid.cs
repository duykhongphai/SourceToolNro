using BotPlayer.Classes.AStar.Collections.MultiDimensional;

namespace BotPlayer.Classes.AStar;

public class WorldGrid : Grid<short>
{
    public WorldGrid(int height, int width) : base(height, width)
    {
    }

    public WorldGrid(short[,] worldArray) : base(worldArray.GetLength(0), worldArray.GetLength(1))
    {
        for (var row = 0; row < worldArray.GetLength(0); row++)
        for (var column = 0; column < worldArray.GetLength(1); column++)
            this[row, column] = worldArray[row, column];
    }
}