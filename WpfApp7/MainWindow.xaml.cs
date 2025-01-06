﻿using _2024_WpfApp7;
using LiveCharts;
using LiveCharts.Wpf;
using System.Net.Http;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;

namespace WpfApp7
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    
    public partial class MainWindow : Window
    {
        string defaultURL = "https://data.moenv.gov.tw/api/v2/aqx_p_432?api_key=540e2ca4-41e1-4186-8497-fdd67024ac44&limit=1000&sort=ImportDate%20desc&format=JSON";
        AQIdata aqiData = new AQIdata();
        List<Field> fields = new List<Field>();
        List<Record> records = new List<Record>();
        List<Record> selectedRecords = new List<Record>();

        SeriesCollection seriesCollection = new SeriesCollection();
        public MainWindow()
        {
            InitializeComponent();
            UrlTextBox.Text = defaultURL;
        }

        private async void GetAQIButton_Click(object sender, RoutedEventArgs e)
        {
            ContentTextBox.Text = "抓取資料中...";

            string data = await FetchContentAsync(defaultURL);
            ContentTextBox.Text = data;
            aqiData = JsonSerializer.Deserialize<AQIdata>(data);
            fields = aqiData.fields.ToList();
            records = aqiData.records.ToList();
            selectedRecords = records;

            statusTextBlock.Text = $"共有{records.Count}筆資料";
            DisplayAQIData();
        }

        private void DisplayAQIData()
        {
            RecordDataGrid.ItemsSource = records;

            Record record = records[0];

            foreach (Field field in fields)
            {
                var propertyInfo  = record.GetType().GetProperty(field.id);
                if (propertyInfo != null)
                {
                    var value = propertyInfo.GetValue(record, null) as string;
                    if (double.TryParse(value, out double v))
                    {
                        CheckBox cb = new CheckBox
                        {
                            Content = field.info.label,
                            Tag = field.id,
                            Margin = new Thickness(3),
                            FontSize = 14,
                            FontWeight = FontWeights.Bold,
                            Width = 120
                        };
                        cb.Checked += UpdateChart;
                        cb.Unchecked += UpdateChart;
                        DataWrapPanel.Children.Add(cb);
                    }
                }
            }
        }

        private void UpdateChart(object sender, RoutedEventArgs e)
        {
            seriesCollection.Clear();

            foreach (CheckBox cb in DataWrapPanel.Children)
            {
                if (cb.IsChecked == true)
                {
                    List<string> labels = new List<string>();
                    string tag = cb.Tag as string;
                    ColumnSeries columnSeries = new ColumnSeries();                    
                    ChartValues<double> values = new ChartValues<double>();

                    foreach (Record r in selectedRecords)
                    {
                        var propertyInfo = r.GetType().GetProperty(tag);
                        if (propertyInfo != null)
                        {
                            var value = propertyInfo.GetValue(r, null) as string;
                            if (double.TryParse(value, out double v))
                            {
                                labels.Add(r.sitename);
                                values.Add(v);
                            }
                        }
                    }
                    columnSeries.Values = values;
                    columnSeries.Title = tag;
                    columnSeries.LabelPoint = point => $"{labels[(int)point.X]}:{point.Y.ToString()}";
                    seriesCollection.Add(columnSeries);
                }
            }
            AQIChart.Series = seriesCollection;
        }

        private async Task<string> FetchContentAsync(string url)
        {
            using (var client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromSeconds(100);
                try
                {
                    HttpResponseMessage response = await client.GetAsync(url);
                    response.EnsureSuccessStatusCode();
                    string responseBody = await response.Content.ReadAsStringAsync();
                    ContentTextBox.Text = responseBody;
                    return responseBody;
                }
                catch (HttpRequestException e)
                {
                    MessageBox.Show($"Request exception: {e.Message}");
                    return null;
                }
            }
        }

        private void RecordDataGrid_LoadingRow(object sender, System.Windows.Controls.DataGridRowEventArgs e)
        {
            e.Row.Header = (e.Row.GetIndex() + 1).ToString();
        }

        private void RecordDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selectedRecords = RecordDataGrid.SelectedItems.Cast<Record>().ToList();
            statusTextBlock.Text = $"共選擇{selectedRecords.Count}筆資料";
            UpdateChart(null, null);
        }
    }
}