using System;
using System.IO;

namespace ToolPart.Classes;

public class Settings
{
    public static string nameTool = "Part";
    public static string version = "2.5.1";

    public static string localLowPath =
        Path.GetFullPath(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) +
                         @$"\..\LocalLow\XTOOLS247\{nameTool}");
}