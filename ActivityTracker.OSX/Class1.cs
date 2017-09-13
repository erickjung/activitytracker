using System;

namespace ActivityTracker.OSX
{
    public class Class1
    {
    }
}

//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.Linq;
//
//namespace ConsoleApplication
//{
//    class SnapshotProcess
//    {
//        public string Type { get; set; }
//        public long ID { get; set; }
//        public string Name { get; set; }
//        public bool Visible { get; set; }
//    }
//
//    class Snapshot
//    {
//        public DateTime Time { get; set; }
//        public Dictionary<long, SnapshotProcess> AllProcess { get; set; }
//        public long ActiveWindow { get; set; }
//    }
//
//    class Program
//    {
//        private const string allWindowsScript = "tell application \"System Events\"\n" +
//                                                "   set listOfProcesses to (every application process where background only is false)\n" +
//                                                "end tell\n" +
//                                                "repeat with proc in listOfProcesses\n" +
//                                                "   set procName to (name of proc)\n" +
//                                                "   set procID to (id of proc)\n" +
//                                                "   log \"PROCESS|\" & procID & \"|\" & procName\n" +
//                                                "   try\n" +
//                                                "      tell application procName\n" +
//                                                "         repeat with i from 1 to (count windows)\n" +
//                                                "            log \"WINDOW|\" & (id of window i) & \"|\" & (name of window i) as string\n" +
//                                                "         end repeat\n" +
//                                                "      end tell\n" +
//                                                "   end try\n" +
//                                                "end repeat";
//
//        private const string activeWindowsScript = "tell application \"System Events\"\n" +
//                                                   "   set proc to (first application process whose frontmost is true)\n" +
//                                                   "end tell\n" +
//                                                   "set procName to (name of proc)\n" +
//                                                   "try\n" +
//                                                   "   tell application procName\n" +
//                                                   "      log \"WINDOW|\" & (id of window 1) & \"|\" & (name of window 1)\n" +
//                                                   "   end tell\n" +
//                                                   "on error e\n" +
//                                                   "   log \"WINDOW|\" & (id of proc) & \"|\" & (name of first window of proc)\n" +
//                                                   "end try";
//
//        private const string visibleWindowsScript = "tell application \"System Events\"\n" +
//                                                    "   set listOfProcesses to (every process whose visible is true)\n" +
//                                                    "end tell\n" +
//                                                    "repeat with proc in listOfProcesses\n" +
//                                                    "   set procName to (name of proc)\n" +
//                                                    "   set procID to (id of proc)\n" +
//                                                    "   log \"PROCESS|\" & procID & \"|\" & procName\n" +
//                                                    "   set app_windows to (every window of proc)\n" +
//                                                    "   repeat with each_window in app_windows\n" +
//                                                    "      log \"WINDOW|-1|\" & (name of each_window) as string\n" +
//                                                    "   end repeat\n" +
//                                                    "end repeat";
//
//        private const string test = "set new_file to choose file\nset ffff to POSIX path of new_file";
//
//        static Dictionary<long, SnapshotProcess> Execute(string command)
//        {
//            var process = new Process
//            {
//                StartInfo = new ProcessStartInfo
//                {
//                    FileName = "osascript",
//                    UseShellExecute = false,
//                    CreateNoWindow = true,
//                    RedirectStandardInput = true,
//                    RedirectStandardOutput = true,
//                    RedirectStandardError = true
//                }
//            };
//
//            process.Start();
//            process.StandardInput.Write(command);
//            process.StandardInput.Close();
//            // process.StandardInput.Dispose();
////            process.StandardInput.Close();
//
//            var snapList = new Dictionary<long, SnapshotProcess>();
//            while (!process.StandardError.EndOfStream)
//            {
//                var line = process.StandardError.ReadLine();
//                var parts = line.Split('|');
//
//                if (parts.Length > 2)
//                {
//                    var proc = new SnapshotProcess
//                    {
//                        Type = parts[0],
//                        ID = long.Parse(parts[1]),
//                        Name = parts[2],
//                        Visible = false
//                    };
//
//                    if (proc.ID != -1 && !snapList.ContainsKey(proc.ID))
//                    {
//                        snapList.Add(proc.ID, proc);
//                    }
//                }
//            }
//            return snapList;
//        }
//
//        static long GetActive(Dictionary<long, SnapshotProcess> list)
//        {
//            if (list.Count <= 0)
//            {
//                return -1;
//            }
//
//            return list.First().Value.ID;
//        }
//
//        static void Main(string[] args)
//        {
//            var snapShot = new Snapshot();
//            snapShot.Time = DateTime.Now;
//            snapShot.AllProcess = Execute(allWindowsScript);
//
//            Console.WriteLine("--- ALL WINDOWS ---");
//            foreach (var snap in snapShot.AllProcess)
//            {
//                Console.WriteLine(string.Format("{0} - {1} - {2}", snap.Value.Type, snap.Value.ID, snap.Value.Name));
//            }
//            Console.WriteLine("------");
//
//            snapShot.ActiveWindow = GetActive(Execute(activeWindowsScript));
//            Console.WriteLine("--- ACTIVE WINDOW ---");
//            Console.WriteLine(string.Format("{0}", snapShot.ActiveWindow));
//            Console.WriteLine("------");
//
//            Console.WriteLine("--- VISIBLE WINDOWS ---");
//            var visibleProcess = Execute(visibleWindowsScript);
//            foreach (var proc in snapShot.AllProcess)
//            {
//                foreach (var visible in visibleProcess)
//                {
//                    if (proc.Value.Name.Equals(visible.Value.Name))
//                    {
//                        proc.Value.Visible = true;
//                        Console.WriteLine(string.Format("{0}", visible.Value.ID));
//                    }
//                }
//            }
//            Console.WriteLine("------");
//
//        }
//    }
//}