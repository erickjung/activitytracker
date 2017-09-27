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

        private static void ConvertJsonToHTML(string jsonFile, string outputHtml)
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
                    var end = start.AddSeconds(5);

                    for (var j = i + 1; j < snapList.Count; j++)
                        if (j < snapList.Count - 1)
                        {
                            var next = snapList[j];
                            if (!next.ActiveProcess.Name.Equals(current.ActiveProcess.Name))
                            {
                                end = next.Time.AddSeconds(5);
                                i = j;
                                break;
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
                Console.WriteLine("parameters: [track outfile] or [convert jsonfile outfile]");
                return;
            }

            for (var i = 0; i < args.Length; i++)
                switch (args[i])
                {
                    case "track":
                    {
                        var fileJson = args[i + 1];

                        var track = new Tracker();
                        var snap = await track.Now();
                        SaveSnapshot(snap, fileJson);
                        Console.WriteLine("Snapshot saved at {0}", snap.Time);
                        break;
                    }
                    case "convert":
                    {
                        var fileJson = args[i + 1];
                        var fileHtml = args[i + 2];
                        ConvertJsonToHTML(fileJson, fileHtml);
                        Console.WriteLine("Html saved");
                        break;
                    }
                }
        }
    }
}