﻿using System.Net.Http;
using System.Windows;

namespace WpfApp7
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string defaultURL = "https://data.moenv.gov.tw/api/v2/aqx_p_432?api_key=540e2ca4-41e1-4186-8497-fdd67024ac44&limit=1000&sort=ImportDate%20desc&format=JSON";
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
    }
}