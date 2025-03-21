using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ZeDNA
{
    internal static class Program
    {
        /// <summary>
        /// Point d'entrée principal de l'application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            bool isNewInstance;
            using (Mutex mutex = new Mutex(true, "ZeDNA", out isNewInstance))
            {
                if (isNewInstance)
                {
                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);
                    Application.Run(new MainForm());
                }
                else
                {
                    // Replace "MainForm" with the actual title of your application's main window
                    IntPtr hWnd = FindWindowByTitlePrefix("ZeDNA ");
                    if (hWnd != IntPtr.Zero)
                    {
                        RestoreAndActivateWindow(hWnd);
                    }
                }
            }
        }
        [DllImport("user32.dll")]
        private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        private static extern bool IsIconic(IntPtr hWnd);

        private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        private static IntPtr FindWindowByTitlePrefix(string titlePrefix)
        {
            IntPtr foundHWnd = IntPtr.Zero;

            EnumWindows((hWnd, lParam) =>
            {
                StringBuilder sb = new StringBuilder(256);
                GetWindowText(hWnd, sb, sb.Capacity);
                string windowTitle = sb.ToString();

                if (windowTitle.StartsWith(titlePrefix, StringComparison.OrdinalIgnoreCase))
                {
                    foundHWnd = hWnd;
                    return false; // Stop enumeration once found
                }

                return true; // Continue enumeration
            }, IntPtr.Zero);

            return foundHWnd;
        }
        private static void RestoreAndActivateWindow(IntPtr hWnd)
        {
            // If window is minimized, restore it
            if (IsIconic(hWnd))
            {
                ShowWindow(hWnd, 9); // SW_RESTORE (9) brings it back from minimized state
            }

            // Bring it to the foreground
            SetForegroundWindow(hWnd);
        }
    }
}
