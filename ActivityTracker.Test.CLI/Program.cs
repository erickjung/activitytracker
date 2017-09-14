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

            Console.WriteLine("--- ALL WINDOWS ---");
            foreach (var snap in snapShot.Process)
            {
                Console.WriteLine(string.Format("{0} - {1} - {2}", snap.Value.Type, snap.Value.ID, snap.Value.Name));
            }
            Console.WriteLine("------");
            Console.WriteLine("--- ACTIVE WINDOW ---");
            Console.WriteLine(string.Format("{0}", snapShot.ActiveWindow));
            Console.WriteLine("------");

            Console.WriteLine("--- VISIBLE WINDOWS ---");
            foreach (var snap in snapShot.Process)
            {
                if (snap.Value.Visible)
                {
                    Console.WriteLine(string.Format("{0} - {1} - {2}", snap.Value.Type, snap.Value.ID,
                        snap.Value.Name));
                }
            }
            Console.WriteLine("------");
        }
    }
}