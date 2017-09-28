using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using ActivityTracker.OSX;
using Newtonsoft.Json;

namespace ActivityTracker.Test.CLI
{
    internal class Program
    {
        private const string HtmlHead = "<html>" +
                                        "<head>" +
                                        "<script type=\"text/javascript\" src=\"https://www.gstatic.com/charts/loader.js\"></script>" +
                                        "<script type=\"text/javascript\">" +
                                        "google.charts.load(\"current\", {packages: [\"timeline\"]});" +
                                        "google.charts.setOnLoadCallback(drawChart);" +
                                        "function drawChart() {" +
                                        "var container = document.getElementById('timeline');" +
                                        "var chart = new google.visualization.Timeline(container);" +
                                        "var dataTable = new google.visualization.DataTable();" +
                                        "dataTable.addColumn({type: 'string', id: 'Proc'});" +
                                        "dataTable.addColumn({type: 'date', id: 'Start'});" +
                                        "dataTable.addColumn({type: 'date', id: 'End'});" +
                                        "dataTable.addRows([";

        private const string HtmlBottom = "]);" +
                                          "var options = {" +
                                          "timeline: {colorByRowLabel: true}" +
                                          "};" +
                                          "chart.draw(dataTable, options);" +
                                          "}</script></head>" +
                                          "<body>" +
                                          "<div id=\"timeline\" style=\"height: 580px;\"></div>" +
                                          "</body>" +
                                          "</html>";

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

        private static void ConvertJsonToHTML(string jsonFile, int interval, string outputHtml)
        {
            if (File.Exists(jsonFile))
            {
                var json = File.ReadAllText(jsonFile);
                var snapList = JsonConvert.DeserializeObject<List<Snapshot>>(json);

                var snapInfo = "";
                for (var i = 0; i < snapList.Count; i++)
                {
                    var current = snapList[i];
                    var start = current.Time;
                    var end = start.AddMilliseconds(interval);

                    for (var j = i + 1; j < snapList.Count; j++, i++)
                    {
                        if (j < snapList.Count - 1)
                        {
                            var next = snapList[j];
                            end = next.Time;
                            if (!next.ActiveProcess.Name.Equals(current.ActiveProcess.Name))
                            {
                                break;
                            }
                        }
                    }

                    snapInfo += $"['{current.ActiveProcess.Name}', new Date(\"{start}\"), new Date(\"{end}\")],";
                }

                var html = $"{HtmlHead}{snapInfo}{HtmlBottom}";

                File.WriteAllText(outputHtml, html);
            }
        }

        private static void Main(string[] args)
        {
            MainAsync(args).GetAwaiter().GetResult();
        }

        private static async Task MainAsync(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("ActivityTracker sample");
                Console.WriteLine(
                    "parameters: [track outfile interval(milliseconds)] or [convert jsonfile jsoninterval outfile]");
                return;
            }

            for (var i = 0; i < args.Length; i++)
                switch (args[i])
                {
                    case "track":
                    {
                        var fileJson = args[i + 1];
                        var interval = int.Parse(args[i + 2]);

                        Console.WriteLine("Presse CTRLˆC to finish");
                        var count = 0;

                        var track = new Tracker();
                        do
                        {
                            var snap = await track.Now(TrackerOptions.ActiveProcess);
                            SaveSnapshot(snap, fileJson);
                            Console.WriteLine("{0} - Snapshot saved at {1}", count, snap.Time);
                            await Task.Delay(interval);
                            count++;
                        } while (true);
                    }
                    case "convert":
                    {
                        var fileJson = args[i + 1];
                        var interval = int.Parse(args[i + 2]);
                        var fileHtml = args[i + 3];
                        ConvertJsonToHTML(fileJson, interval, fileHtml);
                        Console.WriteLine("Html saved");
                        break;
                    }
                }
        }
    }
}