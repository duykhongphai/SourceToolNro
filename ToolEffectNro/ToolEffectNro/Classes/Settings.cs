using System;
using System.IO;

namespace ToolEffectNro.Classes;

public class Settings
{
    public static string nameTool = "Effect";
    public static string version = "2.4.3";

    public static string localLowPath =
        Path.GetFullPath(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) +
                         @$"\..\LocalLow\XTOOLS247\{nameTool}");
}