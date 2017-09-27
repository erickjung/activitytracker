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
            var snapShot = await t.Now();

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