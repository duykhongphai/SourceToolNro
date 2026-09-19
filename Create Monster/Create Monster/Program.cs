using System.Runtime.InteropServices;
using Create_Monster.Class;
using Create_Monster.Form_Mob;
using static Guna.UI2.Native.WinApi;

namespace Create_Monster
{
    internal static class Program
    {
        static Mutex mutex = new Mutex(true, "{CreateEffectMainForm}");

        [STAThread]
        static void Main(string[] arg)
        {
            string path = Path.GetFullPath(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @$"\..\LocalLow\XTOOLS247\");
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            string key = string.Empty;
            if (arg.Length > 0)
            {
                key = arg[0];
            }
            if (mutex.WaitOne(TimeSpan.Zero, true))
            {
                ComWrappers.RegisterForMarshalling(WinFormsComInterop.WinFormsComWrappers.Instance);
                ApplicationConfiguration.Initialize();
                Application.Run(new MobForm());
            }
            else
            {
                MessageBox.Show("Ứng dụng đang chạy.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}