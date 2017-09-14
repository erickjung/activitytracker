using System;
using ActivityTracker.OSX;

namespace ActivityTracker.Test.CLI
{
    class Program
    {
        static void Main(string[] args)
        {
            var t = new Tracker();
            var snapShot = t.Now();

            Console.WriteLine("--- ALL PROCESS ---");
            foreach (var snap in snapShot.Processes)
            {
                Console.WriteLine(string.Format("{0} - {1}", snap.Value.ID, snap.Value.Name));
            }
            Console.WriteLine("------");
            Console.WriteLine("--- ACTIVE WINDOW ---");
            Console.WriteLine(string.Format("{0}", snapShot.ActiveWindow));
            Console.WriteLine("------");

            Console.WriteLine("--- ALL WINDOWS ---");
            foreach (var snap in snapShot.Processes)
            {
                foreach (var win in snap.Value.Windows)
                {
                    Console.WriteLine(string.Format("{0} - {1} -> {2} - {3}", snap.Value.ID, snap.Value.Name, win.Value.ID, win.Value.Name));
                }
            }
            Console.WriteLine("------");
        }
    }
}