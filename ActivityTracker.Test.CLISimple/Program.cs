using ActivityTracker.Windows;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace ActivityTracker.Test.CLISimple
{
    class Program
    {
        [DllImport("user32")]
        private static extern int GetWindowLongA(IntPtr hWnd, int index);

        [DllImport("USER32.DLL")]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        // Delegate to filter which windows to include 
        public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        [DllImport("USER32.DLL")]
        private static extern bool EnumWindows(EnumWindowsProc enumFunc, int lParam);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        private const int GWL_STYLE = -16;

        private const ulong WS_VISIBLE = 0x10000000L;
        private const ulong WS_BORDER = 0x00800000L;
        private const ulong TARGETWINDOW = WS_BORDER | WS_VISIBLE;

        internal class Window
        {
            public string Title;
            public IntPtr Handle;

            public override string ToString()
            {
                return Title;
            }
        }

        static private List<Window> windows;

        static private void GetWindows()
        {
            windows = new List<Window>();
            EnumWindows(Callback, 0);
        }

        static bool Callback(IntPtr hWnd, IntPtr lParam)
        {
            if ((GetWindowLongA(hwnd, GWL_STYLE) & TARGETWINDOW) == TARGETWINDOW)
            {
                StringBuilder sb = new StringBuilder(100);
            GetWindowText(hWnd, sb, sb.Capacity);

            Window t = new Window();
            t.Handle = hWnd;
            t.Title = sb.ToString();
            windows.Add(t);
            }

            return true; //continue enumeration
        }

        static void Main()
        {
            GetWindows();

            Process[] processlist = Process.GetProcesses();

            foreach (Process process in processlist)
            {
                if (!String.IsNullOrEmpty(process.MainWindowTitle))
                {
                    Console.WriteLine("Process: {0} ID: {1} Window title: {2}", process.ProcessName, process.Id, process.MainWindowTitle);
                }
            }
            int t = 1;

            //var t = new Tracker();
            //var snapShot = t.Now();

            //Console.WriteLine("--- ALL PROCESS AND WINDOWS AT {0} ---", snapShot.Time);
            //foreach (var snap in snapShot.Processes)
            //{
            //    Console.WriteLine("PROC: {0} - {1}", snap.Value.Id, snap.Value.Name);
                
            //    foreach (var win in snap.Value.Windows)
            //    {
            //        Console.WriteLine("    - {0} - {1}", win.Value.Id, win.Value.Name);
            //    }
            //}
            //Console.WriteLine("\n--- ACTIVE WINDOW ---");
            //Console.WriteLine("{0}", snapShot.ActiveWindow);
            //Console.WriteLine("------");
        }
    }
}