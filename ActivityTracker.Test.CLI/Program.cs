using System;
using ActivityTracker.OSX;

namespace ActivityTracker.Test.CLI
{
    class Program
    {
        static void Main()
        {
            var t = new Tracker();
            var snapShot = t.Now();

            Console.WriteLine("--- ALL PROCESS AND WINDOWS AT {0} ---", snapShot.Time);
            foreach (var snap in snapShot.Processes)
            {
                Console.WriteLine("PROC: {0} - {1}", snap.Value.Id, snap.Value.Name);
                
                foreach (var win in snap.Value.Windows)
                {
                    Console.WriteLine("    - {0} - {1}", win.Value.Id, win.Value.Name);
                }
            }
            Console.WriteLine("\n--- ACTIVE WINDOW ---");
            Console.WriteLine("{0}", snapShot.ActiveWindow);
            Console.WriteLine("------");
        }
    }
}