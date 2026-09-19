using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Create_Monster.Class
{
    public class Settings
    {
        public static string nameTool = "MobNRO";
        public static string version = "1.0.0";
        public static string localLowPath = Path.GetFullPath(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @$"\..\LocalLow\XTOOLS247\{Settings.nameTool}");

    }
}
