using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ActivityTracker.OSX;
using Newtonsoft.Json;

namespace ActivityTracker.Test.CLI
{
    internal class Program
    {
        private const string HtmlTemplate = "<html>" +
                                            "<head>" +
                                            "<script type=\"text/javascript\" src=\"https://www.gstatic.com/charts/loader.js\"></script>" +
                                            "<script type=\"text/javascript\">" +
                                            "google.charts.load(\"current\", {packages: [\"timeline\", \"bar\"]});" +
                                            "google.charts.setOnLoadCallback(drawChart);" +
                                            "function drawChart() {" +
                                            "var container = document.getElementById('timeline');" +
                                            "var chart = new google.visualization.Timeline(container);" +
                                            "var dataTable = new google.visualization.DataTable();" +
                                            "dataTable.addColumn({type: 'string', id: 'Proc'});" +
                                            "dataTable.addColumn({type: 'date', id: 'Start'});" +
                                            "dataTable.addColumn({type: 'date', id: 'End'});" +
                                            "dataTable.addRows([" +
                                            "{TIMELINE_DATA}" +
                                            "]);" +
                                            "var options = {" +
                                            "timeline: {colorByRowLabel: true}" +
                                            "};" +
                                            "chart.draw(dataTable, options);" +
                                            "var dataBar = google.visualization.arrayToDataTable([" +
                                            "['Process', 'Count']," +
                                            "{BAR_DATA}" +
                                            "]);" +
                                            "var optionsBar = {" +
                                            "bars: 'horizontal'};" +
                                            "var chartBar = new google.charts.Bar(document.getElementById('barchart'));" +
                                            "chartBar.draw(dataBar, google.charts.Bar.convertOptions(optionsBar));" +
                                            "}</script></head>" +
                                            "<body>" +
                                            "<h3>Process count</h3>" +
                                            "<div id=\"barchart\" style=\"width: 700px; height: 300px;\"></div>" +
                                            "<h3>Process timeline</h3>" +
                                            "<div id=\"timeline\" style=\"height: 580px;\"></div>" +
                                            "</body>" +
                                            "</html>";


        private static string ParseTemplate(IReadOnlyDictionary<string, string> parameters, string template)
        {
            return parameters.Keys.Aggregate(template, (current, key) => current.Replace(key, parameters[key]));
        }

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

        private static void CountData(ref Dictionary<string, int>list, Snapshot snap)
        {
            if (list.ContainsKey(snap.ActiveProcess.Name))
            {
                list[snap.ActiveProcess.Name] += 1;
                return;
            }
            
            list.Add(snap.ActiveProcess.Name, 1);
        }

        
        private static void ConvertJsonToHTML(string jsonFile, int interval, string outputHtml)
        {
            if (File.Exists(jsonFile))
            {
                var dataDetails = new Dictionary<string, int>();
                var json = File.ReadAllText(jsonFile);
                var snapList = JsonConvert.DeserializeObject<List<Snapshot>>(json);

                var timelineInfo = "";
                for (var i = 0; i < snapList.Count; i++)
                {
                    var current = snapList[i];
                    var start = current.Time;
                    var end = start.AddMilliseconds(interval);
                    CountData(ref dataDetails, current);
                    
                    for (var j = i + 1; j < snapList.Count; j++, i++)
                    {
                        var next = snapList[j];
                        end = next.Time;
                        if (!next.ActiveProcess.Name.Equals(current.ActiveProcess.Name))
                        {
                            break;
                        }
                        else
                        {
                            CountData(ref dataDetails, next);
                        }
                    }

                    timelineInfo += $"['{current.ActiveProcess.Name}', new Date('{start}'), new Date('{end}')],";
                }

                var dataDetailsSorted = from entry in dataDetails orderby entry.Value descending select entry;
                var barInfo = dataDetailsSorted.Aggregate("", (current, dataDetail) => current + $"['{dataDetail.Key}', {dataDetail.Value}],");

                var parameters = new Dictionary<string, string>
                {
                    {"{TIMELINE_DATA}", timelineInfo},
                    {"{BAR_DATA}", barInfo}
                };

                File.WriteAllText(outputHtml, ParseTemplate(parameters, HtmlTemplate));

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