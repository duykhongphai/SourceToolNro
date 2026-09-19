using System;
using System.IO;

namespace DrawMap.Classes;

public class Settings
{
    public static string nameTool { get; set; } = "DrawMap";
    public static string version { get; set; } = "2.2.0";

    public static string folderBackground { get; set; } = "Data//Background";
    public static string folderTileMap { get; set; } = "Data//ItemMap";
    public static string folderItemBackground { get; set; } = "Data//ItemBackground";

    public static string folderOutputTileInfo { get; set; } = "Output//tileInfo";
    public static string folderOutputTileMap { get; set; } = "Output//TileMap//";
    public static string folderOutputMap { get; set; } = "Output//Map//";
    public static string folderOutputItemBgMap { get; set; } = "Output//ItemBg//";
    public static string folderOutputEffect { get; set; } = "Output//Effect//";
    public static string folderOutputBackground { get; set; } = "Output//Background";
    public static string folderOutputImageItemBg { get; set; } = "Output//Image Item Background";

    public static int numItemInPage { get; set; } = 10;
    public static int numEffectInPage { get; set; } = 7;
    public static int numActorInPage { get; set; } = 15;

    public static string localLowPath { get; set; } =
        Path.GetFullPath(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) +
                         @$"\..\\LocalLow\\XTOOLS247\\{nameTool}");
}