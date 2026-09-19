using System;
using System.IO;

namespace BotPlayer.Classes;

public class Settings
{
    public static string nameTool = "BotPlayer";
    public static string version = "0.0.4";

    public static string localLowPath =
        Path.GetFullPath(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) +
                         @$"\..\LocalLow\XTOOLS247\{nameTool}");
}