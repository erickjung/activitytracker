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
using System.Threading.Tasks;
using ActivityTracker.OSX;

namespace ActivityTracker.Test.CLISimple
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            MainAsync(args).GetAwaiter().GetResult();
        }

        private static async Task MainAsync(string[] args)
        {
            var t = new Tracker();
            var snapShot = await t.Now(TrackerOptions.FullProcess);

            Console.WriteLine("--- ALL PROCESS AND WINDOWS AT {0} ---", snapShot.Time);
            foreach (var snap in snapShot.Processes)
            {
                Console.WriteLine("PROC: {0} - {1}", snap.Value.Id, snap.Value.Name);

                foreach (var win in snap.Value.Windows)
                    Console.WriteLine("    - {0} - {1}", win.Value.Id, win.Value.Name);
            }
            Console.WriteLine("\n--- ACTIVE WINDOW ---");
            Console.WriteLine("{0}", snapShot.ActiveProcess.Name);
            Console.WriteLine("------");
        }
    }
}