using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace ActivityTracker.OSX
{
    public class Tracker : ITracker
    {
        private const string allWindowsScript = "tell application \"System Events\"\n" +
                                                "   set listOfProcesses to (every application process where background only is false)\n" +
                                                "end tell\n" +
                                                "repeat with proc in listOfProcesses\n" +
                                                "   set procName to (name of proc)\n" +
                                                "   set procID to (id of proc)\n" +
                                                "   log \"PROCESS|\" & procID & \"|\" & procName\n" +
                                                "   try\n" +
                                                "      tell application procName\n" +
                                                "         repeat with i from 1 to (count windows)\n" +
                                                "            log \"PROCESS|\" & procID & \"|\" & procName & \"|WINDOW|\" & (id of window i) & \"|\" & (name of window i) as string\n" +
                                                "         end repeat\n" +
                                                "      end tell\n" +
                                                "   end try\n" +
                                                "end repeat";

        private const string activeWindowsScript = "tell application \"System Events\"\n" +
                                                   "   set proc to (first application process whose frontmost is true)\n" +
                                                   "end tell\n" +
                                                   "try\n" +
                                                   "   log \"PROCESS|\" & (id of proc) & \"|\" & (name of proc)\n" +
                                                   "end try";

        private Dictionary<long, SnapshotProcess> Execute(string command)
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "osascript",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                }
            };

            process.Start();
            process.StandardInput.Write(command);
            process.StandardInput.Close();

            var snapList = new Dictionary<long, SnapshotProcess>();
            while (!process.StandardError.EndOfStream)
            {
                var line = process.StandardError.ReadLine();
                var parts = line.Split('|');

                if (parts.Length < 2)
                {
                    throw new InvalidDataException();
                }
                
                var procId = Convert.ToInt64(parts[1]);
                var procName = parts[2];

                if (parts.Length < 4)
                {
                    var proc = new SnapshotProcess
                    {
                        ID = procId,
                        Name = procName,
                        Windows = new Dictionary<long, SnapshotWindow>()
                    };

                    if (proc.ID != -1 && !snapList.ContainsKey(proc.ID))
                    {
                        snapList.Add(proc.ID, proc);
                    }
                }
                else 
                {
                    var winId = Convert.ToInt64(parts[4]);
                    var winName = parts[5];

                    var win = new SnapshotWindow
                    {
                        ID = winId,
                        Name = winName,
                    };

                    var proc = snapList[procId];

                    if (win.ID != -1 && !proc.Windows.ContainsKey(win.ID))
                    {
                        proc.Windows.Add(win.ID, win);
                    }
                }
            }
            
            return snapList;
        }

        private long ParseActiveWindow()
        {
            var list = Execute(activeWindowsScript);

            if (list.Count <= 0)
            {
                return -1;
            }

            return list.First().Value.ID;
        }

        public Snapshot Now()
        {
            return new Snapshot
            {
                Time = DateTime.Now,
                Processes = Execute(allWindowsScript),
                ActiveWindow = ParseActiveWindow()
            };
        }
    }
}