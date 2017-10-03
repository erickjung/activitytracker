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
        private const string HtmlTemplate = "<head>" +
                                            "    <script type=\"text/javascript\" src=\"https://www.gstatic.com/charts/loader.js\"></script>" +
                                            "    <script type=\"text/javascript\">" +
                                            "        google.charts.load(\"current\", {packages: [\"timeline\", \"bar\", \"corechart\"]});" +
                                            "        google.charts.setOnLoadCallback(drawChart);" +
                                            "        function drawChart() {" +
                                            "            var dataPie = google.visualization.arrayToDataTable([" +
                                            "            ['Process', 'Count']," +
                                            "            {PIE_DATA}" +
                                            "            ]);" +
                                            "            var optionsPie = {" +
                                            "                legend: 'none'" +
                                            "            };" +
                                            "            var chartPie = new google.visualization.PieChart(document.getElementById('piechart'));" +
                                            "            chartPie.draw(dataPie, optionsPie);" +
                                            "            var dataBar = new google.visualization.arrayToDataTable([" +
                                            "            ['Process', 'Count']," +
                                            "            {BAR_DATA}" +
                                            "            ]);" +
                                            "            var optionsBar = {" +
                                            "                bars: 'horizontal'," +
                                            "                legend: { position: 'none' }" +
                                            "            };" +
                                            "            var chartBar = new google.charts.Bar(document.getElementById('barchart'));" +
                                            "            chartBar.draw(dataBar, optionsBar);" +
                                            "            var dataTimeline = new google.visualization.DataTable();" +
                                            "            dataTimeline.addColumn({type: 'string', id: 'Proc'});" +
                                            "            dataTimeline.addColumn({type: 'date', id: 'Start'});" +
                                            "            dataTimeline.addColumn({type: 'date', id: 'End'});" +
                                            "            dataTimeline.addRows([" +
                                            "            {TIMELINE_DATA}                " +
                                            "            ]);" +
                                            "            var optionsTimeline = {" +
                                            "                timeline: {colorByRowLabel: true}" +
                                            "            };" +
                                            "            var chartTimeline = new google.visualization.Timeline(document.getElementById('timelinechart'));" +
                                            "            chartTimeline.draw(dataTimeline, optionsTimeline);" +
                                            "        }" +
                                            "    </script>" +
                                            "</head>" +
                                            "<body>" +
                                            "<div style=\"padding-top: 10px;text-align: center;height: 65px;font-weight: bold;\">{TITLE_DATA}</div>" +
                                            "<div style=\"display: flex;max-width: 1280px;margin: 0 auto;flex-flow: row wrap;\">" +
                                            "    <div style=\"display: flex;width: 500px;margin: 7px;background-color: #ffffff;box-shadow: 0 0 10px 0 rgba(110, 123, 140, 0.3);flex-flow: column wrap;flex: auto;\">" +
                                            "        <div style=\"padding: 0 20px;\">" +
                                            "            <h5>Process usage %</h5>" +
                                            "        </div>" +
                                            "        <div class=\"card-img\">" +
                                            "            <div id=\"piechart\" style=\"height: 320px;\"></div>" +
                                            "        </div>" +
                                            "    </div>" +
                                            "    <div style=\"display: flex;width: 500px;margin: 7px;background-color: #ffffff;box-shadow: 0 0 10px 0 rgba(110, 123, 140, 0.3);flex-flow: column wrap;flex: auto;\">" +
                                            "        <div style=\"padding: 0 20px;\">" +
                                            "            <h5>Process usage count</h5>" +
                                            "        </div>" +
                                            "        <div class=\"card-img\">" +
                                            "            <div id=\"barchart\" style=\"height: 320px;\"></div>" +
                                            "        </div>" +
                                            "    </div>" +
                                            "    <div style=\"display: flex;width: 500px;margin: 7px;background-color: #ffffff;box-shadow: 0 0 10px 0 rgba(110, 123, 140, 0.3);flex-flow: column wrap;flex: auto;\">" +
                                            "        <div style=\"padding: 0 20px;\">" +
                                            "            <h5>Process usage timeline</h5>" +
                                            "        </div>" +
                                            "        <div class=\"card-img\">" +
                                            "            <div id=\"timelinechart\" style=\"height: 500px;\"></div>" +
                                            "        </div>" +
                                            "    </div>" +
                                            "</div>" +
                                            "</body>";


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
                var barInfo = dataDetailsSorted.Aggregate("",
                    (current, dataDetail) => current + $"['{dataDetail.Key}', {dataDetail.Value}],");

                var parameters = new Dictionary<string, string>
                {
                    {"{TITLE_DATA}", $"ActivityTracker Report ({DateTime.Now})"},
                    {"{PIE_DATA}", barInfo},
                    {"{BAR_DATA}", barInfo},
                    {"{TIMELINE_DATA}", timelineInfo}
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