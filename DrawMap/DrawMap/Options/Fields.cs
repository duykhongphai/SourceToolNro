using System.Collections.Concurrent;
using System.Collections.Generic;
using Avalonia.Media.Imaging;

namespace DrawMap.Options;

public class Fields
{
    public const int T_TOP = 2;
    public const int T_LEFT = 4;
    public const int T_RIGHT = 8;
    public const int T_BOTTOM = 8192;

    public static ConcurrentDictionary<int, List<PictureCustom>> ResourceTitleMap { get; set; }
    public static ConcurrentDictionary<int, List<PictureCustom>> ResourceBackground { get; set; }
    public static ConcurrentDictionary<short, Bitmap> ResourceImageItemBackground { get; set; }
    public static readonly ConcurrentDictionary<short, Bitmap> ResourceImagePart = new();
    public static readonly ConcurrentDictionary<short, Part> ResourcePartData = new();
    public static readonly ConcurrentDictionary<int, PictureCustom> ResourceItemBackground = new();
    public static readonly ConcurrentDictionary<int, EffectData> ResourceEffect = new();
    public static readonly ConcurrentDictionary<int, EffectData> ResourceMonster = new();
    public static readonly ConcurrentDictionary<int, PictureCustom> ResourceNpc = new();
    public static PictureCustom PictureWaypoint { get; set; }
    public static readonly byte[] LayerSpeed = [1, 3, 5, 7];
    public static readonly byte[] DeltaY = [2, 3, 4, 6];

    public static List<List<List<int>>> TileIndex { get; set; } =
    [
        new()
        {
            new List<int> { 28, 29, 30, 39 },
            new List<int> { 26 },
            new List<int> { 18, 24, 29, 39, 28, 29, 30 },
            new List<int> { 1, 23, 28, 39, 28, 29, 30 },
            new List<int> { 1, 2, 3, 4, 5, 6, 18, 31, 32, 33, 34, 39 }
        },

        new()
        {
            new List<int> { 35 },
            new List<int> { 26 },
            new List<int> { 22, 24, 20, 14, 35 },
            new List<int> { 21, 23, 15, 9, 35 },
            new List<int> { 9, 10, 11, 12, 13, 14, 26, 30, 31, 32, 33, 35 },
            new List<int> { 0, 1, 2, 3, 4, 5, 6, 7, 8 }
        },

        new()
        {
            new List<int> { 48, 52, 53, 47, 28, 51, 54 },
            new List<int> { 31 },
            new List<int> { 29, 26, 27, 47, 40, 53, 54, 52, 48 },
            new List<int> { 23, 25, 39, 24, 47, 51, 53, 40, 54, 52, 48 },
            new List<int> { 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 29, 23, 41, 42, 43, 45, 46, 47, 54 }
        },

        new()
        {
            new List<int> { 19 },
            new List<int> { 17, 19 },
            new List<int> { 17, 19 },
            new List<int> { 1, 2, 3, 4, 5, 6, 17, 19 }
        },

        new()
        {
            new List<int> { 38 },
            new List<int> { 23 },
            new List<int> { 19, 20, 21, 29, 31, 38 },
            new List<int> { 16, 17, 18, 28, 30, 38 },
            new List<int> { 5, 6, 7, 8, 9, 10, 16, 21, 30, 31, 33, 34, 38 }
        },

        new()
        {
            new List<int> { 33 },
            new List<int> { 33 },
            new List<int> { 33 },
            new List<int> { 1, 2, 3, 4, 5, 6, 7, 13, 33 }
        },

        new()
        {
            new List<int> { 33 },
            new List<int> { 10, 13, 8, 15, 17, 33 },
            new List<int> { 9, 12, 7, 14, 16, 33 },
            new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 15, 14, 33 }
        },

        new()
        {
            new List<int> { 42 },
            new List<int> { 26 },
            new List<int> { 43 },
            new List<int> { 18 },
            new List<int> { 11, 15, 28, 42, 24, 11 },
            new List<int> { 4, 10, 27, 42, 25, 10 },
            new List<int> { 1, 2, 3, 4, 15, 12, 16, 17, 40, 32, 34, 42 }
        },

        new()
        {
            new List<int> { 21, 25 },
            new List<int> { 18, 20, 26, 21, 25 },
            new List<int> { 12, 19, 26, 21, 25 },
            new List<int> { 9, 10, 11, 12, 18, 26 }
        },

        new()
        {
            new List<int> { 28 },
            new List<int> { 2, 5, 16, 28 },
            new List<int> { 1, 4, 15, 28 },
            new List<int> { 4, 5, 10, 11, 12, 13, 18, 28 }
        },

        new()
        {
            new List<int> { 23 },
            new List<int> { 23 },
            new List<int> { 23 },
            new List<int> { 17, 18, 23 }
        },

        new()
        {
            new List<int> { 34 },
            new List<int> { 6, 11, 34 },
            new List<int> { 1, 10, 34 },
            new List<int> { 1, 2, 3, 4, 5, 6, 7, 34 }
        },

        new()
        {
            new List<int> { 2, 4 },
            new List<int> { 2, 3 },
            new List<int> { 1, 2, 3, 4 }
        },

        new()
        {
            new List<int> { 8, 7, 9, 10, 11, 12, 13, 14, 15, 16, 17 },
            new List<int> { 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17 },
            new List<int> { 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17 },
            new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17 }
        },

        new()
        {
            new List<int> { 7, 8, 9 },
            new List<int> { 6, 11 },
            new List<int> { 6, 10 },
            new List<int> { 6, 7, 8, 9, 10, 11 }
        },

        new()
        {
            new List<int> { 1, 2, 3, 7, 8 },
            new List<int> { 1, 2, 3, 6, 8 },
            new List<int> { 4, 5, 6, 7, 8 }
        },

        new()
        {
            new List<int> { 5, 6, 8 },
            new List<int> { 34 },
            new List<int> { 23, 21, 26, 27, 36 },
            new List<int> { 23, 21, 24, 25, 35 },
            new List<int> { 28, 1, 12, 21, 22, 23, 24, 27, 35, 36 }
        },

        new()
        {
            new List<int> { 16, 19, 22, 23 },
            new List<int> { 14, 18, 20, 23 },
            new List<int> { 1, 2, 4, 13, 14, 15, 16, 23 }
        },

        new()
        {
            new List<int> { 19, 27, 28 },
            new List<int> { 5, 12, 27, 19, 28, 26, 24 },
            new List<int> { 1, 6, 27, 19, 28, 25, 24 },
            new List<int> { 1, 2, 3, 4, 5, 17, 18, 21, 22, 25, 26, 27 }
        },

        new()
        {
            new List<int> { 6, 8, 9, 13, 14, 18, 27, 34, 37, 38 },
            new List<int> { 7, 9, 19, 36, 38, 34, 37, 18, 34, 38 },
            new List<int> { 8, 10, 14, 20, 34, 37, 18, 14, 34 },
            new List<int> { 1, 2, 3, 4, 5, 10, 15, 16, 19, 20, 36, 37 }
        },

        new()
        {
            new List<int> { 16, 17 },
            new List<int> { 15, 16, 17, 18 },
            new List<int> { 10, 14, 16, 17 },
            new List<int> { 1, 3, 4, 5, 6, 7, 8, 11, 14, 15, 17 }
        },

        new()
        {
            new List<int> { 14, 15, 16, 17 },
            new List<int> { 11, 12, 13, 17 },
            new List<int> { 1, 2, 3, 4, 5, 6, 7, 11, 12, 13, 14, 15, 16, 17 }
        },

        new()
        {
            new List<int> { 23 },
            new List<int> { 15, 17, 19, 20 },
            new List<int> { 14, 16, 18, 20 },
            new List<int> { 5, 6, 7, 8, 9, 10, 11, 12, 14, 15, 16, 17, 18, 20 }
        },

        new()
        {
            new List<int> { 12, 13, 14, 23 },
            new List<int> { 7, 2, 11, 14, 23 },
            new List<int> { 6, 1, 10, 13, 23 },
            new List<int> { 15, 16, 17, 18, 2, 14, 1, 13, 23 }
        },

        new()
        {
            new List<int> { 18, 19, 20, 21, 22 },
            new List<int> { 7, 22, 17, 23 },
            new List<int> { 4, 21, 16, 23 },
            new List<int> { 4, 5, 6, 7, 23 }
        },

        new()
        {
            new List<int> { 6, 7, 8 },
            new List<int> { 6, 8 },
            new List<int> { 6, 8 },
            new List<int> { 6, 7, 4, 5 }
        },

        new()
        {
            new List<int> { 13, 14, 16, 17 },
            new List<int> { 11, 12, 14, 17 },
            new List<int> { 1, 11, 16, 17 }
        },

        new()
        {
            new List<int> { 8, 9, 10, 11 },
            new List<int> { 35 },
            new List<int> { 19, 7, 9, 34 },
            new List<int> { 17, 4, 9, 34 },
            new List<int> { 17, 18, 19, 22, 23, 25, 26, 34 }
        },

        new()
        {
            new List<int> { 7, 14, 18, 19, 37 },
            new List<int> { 6, 13, 18, 19, 37 },
            new List<int> { 1, 2, 3, 4, 5, 6, 7, 18, 19 }
        },

        new()
        {
            new List<int> { 25, 26, 27 },
            new List<int> { 1, 8, 16, 27, 28 },
            new List<int> { 1, 9, 25, 28 },
            new List<int> { 1, 2, 3, 4, 5, 6, 7, 8 }
        },

        new()
        {
            new List<int> { 1, 2, 3, 4, 31, 32, 33 },
            new List<int> { 1, 2, 3, 4, 3, 9, 30, 33 },
            new List<int> { 1, 2, 3, 4, 1, 7, 30, 31 },
            new List<int> { 1, 2, 3, 4, 7, 8, 9, 10, 11, 12, 13 }
        },

        new()
        {
            new List<int> { 33 },
            new List<int> { 26 },
            new List<int> { 5, 10, 15, 20, 25, 33 },
            new List<int> { 1, 6, 11, 16, 21, 33 },
            new List<int> { 1, 2, 3, 4, 5, 33, 28, 29, 30, 31, 32, 33 }
        }
    ];

    public static List<List<int>> TileType { get; set; } =
    [
        new() { 8192, 512, 8, 4, 2 },
        new() { 8192, 1024, 8, 4, 2, 0 },
        new() { 8192, 64, 8, 4, 2 },
        new() { 8192, 8, 4, 2 },
        new() { 8192, 64, 8, 4, 2 },
        new() { 8192, 8, 4, 2 },
        new() { 8192, 8, 4, 2 },
        new() { 8192, 32, 128, 64, 8, 4, 2 },
        new() { 8192, 8, 4, 2 },
        new() { 8192, 8, 4, 2 },
        new() { 8192, 8, 4, 2 },
        new() { 8192, 8, 4, 2 },
        new() { 8, 4, 2 },
        new() { 8192, 8, 4, 2 },
        new() { 8192, 8, 4, 2 },
        new() { 8, 4, 2 },
        new() { 8192, 64, 8, 4, 2 },
        new() { 8, 4, 2 },
        new() { 8192, 8, 4, 2 },
        new() { 8192, 8, 4, 2 },
        new() { 8192, 8, 4, 2 },
        new() { 8, 4, 2 },
        new() { 8192, 8, 4, 2 },
        new() { 8192, 8, 4, 2 },
        new() { 8192, 8, 4, 2 },
        new() { 8192, 8, 4, 2 },
        new() { 8, 4, 2 },
        new() { 8192, 64, 8, 4, 2 },
        new() { 8, 4, 2 },
        new() { 8192, 8, 4, 2 },
        new() { 8192, 8, 4, 2 },
        new() { 8192, 1048576, 8, 4, 2 }
    ];

    public static List<int> FindTileType(int id, int idParent)
    {
        List<int> tileType = [];
        try
        {
            for (var j = 0; j < TileIndex[idParent - 1].Count; j++)
            {
                var type = TileType[idParent - 1][j];
                if (TileIndex[idParent - 1][j].Contains(id)) tileType.Add(type);
            }
        }
        catch
        {
        }

        return tileType;
    }


    public static void RemoveTileIndex(PictureCustom info, int tile)
    {
        try
        {
            if (!TileType[info.GetIdParent() - 1].Contains(tile)) return;
            for (var j = 0; j < TileIndex[info.GetIdParent() - 1].Count; j++)
            {
                var type = TileType[info.GetIdParent() - 1][j];
                if (type != tile || !TileIndex[info.GetIdParent() - 1][j].Contains(info.GetId())) continue;
                TileIndex[info.GetIdParent() - 1][j].Remove(info.GetId());
                info.TypeBlock.Remove(type);
            }
        }
        catch
        {
        }
    }

    public static void AddTileIndex(PictureCustom info, int tile)
    {
        try
        {
            if (info.GetIdParent() - 1 >= TileType.Count) TileType.Add([]);
            if (info.GetIdParent() - 1 >= TileIndex.Count) TileIndex.Add([]);

            if (!TileType[info.GetIdParent() - 1].Contains(tile)) TileType[info.GetIdParent() - 1].Add(tile);
            for (var j = 0; j < TileType[info.GetIdParent() - 1].Count; j++)
            {
                var type = TileType[info.GetIdParent() - 1][j];
                if (j >= TileIndex[info.GetIdParent() - 1].Count)
                    TileIndex[info.GetIdParent() - 1].Add([]);
                if (type != tile || TileIndex[info.GetIdParent() - 1][j].Contains(info.GetId())) continue;
                TileIndex[info.GetIdParent() - 1][j].Add(info.GetId());
                info.TypeBlock.Add(type);
            }
        }
        catch
        {
        }
    }
}