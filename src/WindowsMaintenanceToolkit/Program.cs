using System;
using System.Windows.Forms;

namespace WindowsMaintenanceToolkit;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new MainForm());
    }
}
