using System;
using System.Collections.Generic;
using System.IO;
using ActivityTracker.OSX;
using Newtonsoft.Json;

namespace ActivityTracker.Test.CLI
{
    class Program
    {
        private const string OutputFile = "out.json";

        private static void SaveSnapshot(Snapshot snap, string outFile)
        {
            if (File.Exists(outFile))
            {
                var json = File.ReadAllText(outFile);
                var newSnap = JsonConvert.DeserializeObject<List<Snapshot>>(json);
                newSnap.Add(snap);
                File.WriteAllText(outFile, JsonConvert.SerializeObject(newSnap));
                return;
            }

            var list = new List<Snapshot> {snap};
            File.WriteAllText(outFile, JsonConvert.SerializeObject(list));
        }

        static void Main()
        {
            var t = new Tracker();    
            var snap = t.Now();
            SaveSnapshot(snap, OutputFile);
            Console.WriteLine("Processes saved at {0}", snap.Time);
        }
    }
}