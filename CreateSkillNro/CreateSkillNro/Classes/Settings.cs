using System;
using System.IO;

namespace CreateSkillNro.Classes;

public class Settings
{
    public static string nameTool = "CreateSkillNro";
    public static string version = "1.0.1";

    public static string localLowPath =
        Path.GetFullPath(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) +
                         @$"\..\LocalLow\XTOOLS247\{nameTool}");
}