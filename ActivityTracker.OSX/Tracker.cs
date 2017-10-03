//
// ActivityTracker
//
// Copyright (c) 2017 Erick Jung http://github.com/erickjung
//
// Permission is hereby granted, free of charge, to any person obtaining a copy 
// of this software and associated documentation files (the "Software"), to deal 
// in the Software without restriction, including without limitation the rights 
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies 
// of the Software, and to permit persons to whom the Software is furnished to do so, 
// subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all 
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, 
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A 
// PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT 
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION 
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE 
// SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ActivityTracker.OSX
{
    public class Tracker : ITracker
    {
        private const string AllWindowsScript = "tell application \"System Events\"\n" +
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

        private const string ActiveWindowsScript = "tell application \"System Events\"\n" +
                                                   "   set proc to (first application process whose frontmost is true)\n" +
                                                   "end tell\n" +
                                                   "try\n" +
                                                   "   log \"PROCESS|\" & (id of proc) & \"|\" & (name of proc)\n" +
                                                   "end try";

        public Task<Snapshot> Now(TrackerOptions options)
        {
            return Task.Run(() => new Snapshot
            {
                Time = DateTime.Now,
                Processes = options == TrackerOptions.FullProcess ? Execute(AllWindowsScript) : null,
                ActiveProcess = ParseActiveWindow()
            });
        }

        private Dictionary<long, SnapshotProcess> Execute(string command)
        {
            using (var process = new Process())
            {
                process.StartInfo = new ProcessStartInfo
                {
                    FileName = "osascript",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardInput = true,
                    RedirectStandardError = true
                };
                
                process.Start();
                process.StandardInput.Write(command);
                process.StandardInput.Close();
                process.StandardInput.Dispose();

                var snapList = new Dictionary<long, SnapshotProcess>();
                while (!process.StandardError.EndOfStream)
                {
                    var line = process.StandardError.ReadLine();
                    var parts = line.Split('|');

                    if (parts.Length < 2)
                        throw new InvalidDataException();

                    var procId = Convert.ToInt64(parts[1]);
                    var procName = parts[2];

                    if (parts.Length < 4)
                    {
                        var proc = new SnapshotProcess
                        {
                            Id = procId,
                            Name = procName,
                            Windows = new Dictionary<long, SnapshotWindow>()
                        };

                        if (proc.Id != -1 && !snapList.ContainsKey(proc.Id))
                            snapList.Add(proc.Id, proc);
                    }
                    else
                    {
                        var winId = Convert.ToInt64(parts[4]);
                        var winName = parts[5];

                        var win = new SnapshotWindow
                        {
                            Id = winId,
                            Name = winName
                        };

                        var proc = snapList[procId];

                        if (win.Id != -1 && !proc.Windows.ContainsKey(win.Id))
                            proc.Windows.Add(win.Id, win);
                    }
                }

                process.WaitForExit();
                return snapList;
            }
        }

        private SnapshotProcess ParseActiveWindow()
        {
            var list = Execute(ActiveWindowsScript);
            return list.Count <= 0 ? null : list.First().Value;
        }
    }
}