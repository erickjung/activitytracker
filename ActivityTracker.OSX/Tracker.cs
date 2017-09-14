using System;
using System.Collections.Generic;
using System.Diagnostics;
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
                                                "            log \"WINDOW|\" & (id of window i) & \"|\" & (name of window i) as string\n" +
                                                "         end repeat\n" +
                                                "      end tell\n" +
                                                "   end try\n" +
                                                "end repeat";

        private const string activeWindowsScript = "tell application \"System Events\"\n" +
                                                   "   set proc to (first application process whose frontmost is true)\n" +
                                                   "end tell\n" +
                                                   "set procName to (name of proc)\n" +
                                                   "try\n" +
                                                   "   tell application procName\n" +
                                                   "      log \"WINDOW|\" & (id of window 1) & \"|\" & (name of window 1)\n" +
                                                   "   end tell\n" +
                                                   "on error e\n" +
                                                   "   log \"WINDOW|\" & (id of proc) & \"|\" & (name of first window of proc)\n" +
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

                if (parts.Length > 2)
                {
                    var proc = new SnapshotProcess
                    {
                        Type = parts[0],
                        ID = long.Parse(parts[1]),
                        Name = parts[2],
                        Visible = false
                    };

                    if (proc.ID != -1 && !snapList.ContainsKey(proc.ID))
                    {
                        snapList.Add(proc.ID, proc);
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
                Process = Execute(allWindowsScript),
                ActiveWindow = ParseActiveWindow()
            };
        }
    }
}