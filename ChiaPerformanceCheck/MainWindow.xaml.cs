using Microsoft.Win32;
using ScottPlot;
using ScottPlot.Plottable;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows;

using Histogram = ScottPlot.Statistics.Histogram;

namespace ChiaPerformanceCheck {
    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();
            histogram.Plot.Title("Chia performance");
        }

        void Recreate(string fileName, double[] values) {
            Plot plt = histogram.Plot;
            plt.Clear();
            Histogram hist = new Histogram(values, min: 0, max: 61, binCount: 61);
            BarPlot bars = plt.AddBar(hist.countsFrac, hist.bins);
            bars.BarWidth = hist.binSize * 1.05;
            plt.Title("Chia performance in " + fileName);
            plt.YLabel("Ratio (searches / total searches)");
            plt.XLabel("Search time (seconds)");
            plt.SetAxisLimits(null, null, 0, null);
            plt.Grid(lineStyle: LineStyle.Dot);
        }

        void LoadFile(object _, RoutedEventArgs e) {
            OpenFileDialog opener = new OpenFileDialog();
            int under30 = 0, over60 = 0, total = 0;
            if (opener.ShowDialog().Value) {
                StreamReader reader = new StreamReader(opener.FileName);
                string line;
                List<double> values = new List<double>();
                while ((line = reader.ReadLine()) != null) {
                    if (!line.Contains(eligible))
                        continue;
                    int plotIdx = line.IndexOf(plot), timeIdx = line.IndexOf(time);
                    if (plotIdx < 0 || timeIdx < 0)
                        continue;
                    int startIdx = line.LastIndexOf(' ', plotIdx - 2);
                    if (line.Substring(startIdx + 1, plotIdx - startIdx - 2) == zero)
                        continue;
                    line = line.Substring(timeIdx + 6, line.IndexOf('.', timeIdx + 7) - timeIdx - 6);
                    if (int.TryParse(line, out int /*you are creator of*/ secs)) {
                        if (secs < 30)
                            ++under30;
                        if (secs < 60)
                            values.Add(secs);
                        else {
                            values.Add(60);
                            ++over60;
                        }
                        ++total;
                    }
                }
                if (total == 0) {
                    MessageBox.Show("This file does not contain any lines about search speed (plots eligible for farming). " +
                        "Please use the INFO loglevel or load a correct file.", "Invalid file", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                Recreate(Path.GetFileName(opener.FileName), values.ToArray());
                results.Text = $"Results under 30 seconds: {under30} ({under30 / (double)total:0.00%}), " +
                    $"over 60 seconds: {over60} ({over60 / (double)total:0.00%}) total: {total}.";
            }
        }

        void Ad(object sender, RoutedEventArgs e) => Process.Start("http://en.sbence.hu");

        const string eligible = "eligible for farming", plot = "plot", zero = "0", time = "Time:";
    }
}