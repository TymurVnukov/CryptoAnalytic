using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Navigation;
using LiveCharts;
using LiveCharts.Wpf;
using System.Diagnostics;

namespace TestTask
{
    public partial class CoinsPage : Page
    {
        public CoinsPage()
        {
            InitializeComponent();
        }
        private const string CoinCapApiBaseUrl = "https://api.coincap.io/v2/";
        private string CoinEndpoint;
        private string currentCoin = "";
        private string currentInterval = "d1";

        private void HomePage_Button_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new HomePage());
        }
        private void ExchangePage_Button_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Exchange());
        }
        private void Search_Click(object sender, RoutedEventArgs e)
        {
            if (InputCoinName.Text.Count() > 0)
            {
                InputCoinName.Text = InputCoinName.Text.ToLower();
                currentCoin = InputCoinName.Text;
                CoinEndpoint = $"assets/{currentCoin}";
                LoadCoininfoAsync(currentInterval);
                SearchButton.IsEnabled = false;
            }
        }

        private async void LoadCoininfoAsync(string timeInterval)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    var requestUrl = $"{CoinCapApiBaseUrl}{CoinEndpoint}";
                    var response = await client.GetStringAsync(requestUrl);
                    var result = JsonConvert.DeserializeObject<CoinCapResponse>(response);
                    DataContext = result?.Data;
                }
                try
                {
                    using (HttpClient client = new HttpClient())
                    {
                        var requestUrl = $"{CoinCapApiBaseUrl}{CoinEndpoint}/history?interval={timeInterval}";
                        var response = await client.GetStringAsync(requestUrl);
                        var result = JsonConvert.DeserializeObject<ChartDataResponse>(response);
                        GenerateChart(result?.Data);
                    }
                    try
                    {
                        using (HttpClient client = new HttpClient())
                        {
                            var requestUrl = $"{CoinCapApiBaseUrl}{CoinEndpoint}/markets";
                            var response = await client.GetStringAsync(requestUrl);
                            var result = JsonConvert.DeserializeObject<MarketsDataResponse>(response);
                            GenerateMarketsButtons(result?.Data);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error when enabling data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        SearchButton.IsEnabled = true;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error when enabling data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    SearchButton.IsEnabled = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error when enabling data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                SearchButton.IsEnabled = true;
            }
        }

        private void GenerateChart(List<ChartData> Data)
        {
            Chart.Children.Clear();
            List<string> times = new List<string>();
            ChartValues<double> priceUSD = new ChartValues<double>();
            foreach (ChartData chartData in Data)
            {
                times.Add((DateTimeOffset.FromUnixTimeMilliseconds((long)Convert.ToDouble(chartData.Time)).DateTime).ToString("yyyy-MM-dd HH:mm"));
                priceUSD.Add(Convert.ToDouble(chartData.PriceUsd.Substring(0, chartData.PriceUsd.IndexOf(".")) + "," + chartData.PriceUsd.Substring(chartData.PriceUsd.IndexOf(".") + 1)));
            }

            CartesianChart ch = new CartesianChart();
            ch.Series = new SeriesCollection
            {
                new LineSeries
                {
                    Title = "USD: ",
                    Values = priceUSD,
                    PointGeometrySize = 1,
                }
            };
            ch.AxisX.Add(new Axis
            {
                Title = "Date",
                Labels = times,
                
            });

            ch.AxisY.Add(new Axis
            {
                Title = "USD price",
            });

            Chart.Children.Add(ch);
        }

        private async void GenerateMarketsButtons(List<MarketData> Data)
        {
            MarketButtons.Items.Clear();
            for (int i = 0; i < Data.Count; i++)
            {
                try
                {
                    using (HttpClient client = new HttpClient())
                    {
                        var requestUrl = $"{CoinCapApiBaseUrl}exchanges";
                        var response = await client.GetStringAsync(requestUrl);
                        var result = JsonConvert.DeserializeObject<ExchangesDataResponse>(response);
                        for (int j = 0; j < result?.Data.Count; j++)
                        {
                            if (result.Data[j].Name == Data[i].ExchangeId && Data[i].BaseId == currentCoin)
                            {
                                Button marketButton = CreateButton($"Buy {Data[i].BaseId} on {Data[i].ExchangeId} for {Data[i].PriceUsd}$ through [{Data[i].QuoteSymbol} -> {Data[i].BaseSymbol}]", result.Data[j].ExchangeUrl);
                                MarketButtons.Items.Add(marketButton);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error when enabling data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    SearchButton.IsEnabled = true;
                }
            }
            SearchButton.IsEnabled = true;

            Button CreateButton(string buttonText, string websiteUrl)
            {
                Button newButton = new Button();

                newButton.Content = buttonText;
                newButton.Width = 500;
                newButton.Height = 30;
                Color color = (Color)ColorConverter.ConvertFromString("#25364a");
                newButton.Background = new SolidColorBrush(color);
                newButton.Foreground = new SolidColorBrush(Colors.WhiteSmoke);

                newButton.Tag = websiteUrl;

                newButton.Click += MarketLinkButton_Click;

                return newButton;
            }

            void MarketLinkButton_Click(object sender, RoutedEventArgs e)
            {
                string websiteUrl = (sender as Button).Tag.ToString();
                Process.Start(new ProcessStartInfo(websiteUrl));
            }
        }

        private void HistoryPriceInterval_SettingsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            object selectedItem = HistoryPriceInterval_SettingsComboBox.SelectedItem;
            if (selectedItem != null)
            {
                ComboBoxItem selectedComboBoxItem = (ComboBoxItem)selectedItem;
                currentInterval = selectedComboBoxItem.Content.ToString();
            }
        }
    }
    public class ExchangesDataResponse
    {
        public List<ExchangeData> Data { get; set; }
    }
    public class ExchangeData
    {
        private string id;
        private string name;
        private string rank;
        private string percentTotalVolume;
        private string volumeUsd;
        private string tradingPairs;
        private string socket;
        private string exchangeUrl;

        public string Id
        {
            get { return id; }
            set { id = value; }
        }
        public string Name
        {
            get { return name; }
            set { name = value; }
        }
        public string Rank
        {
            get { return rank; }
            set { rank = value; }
        }
        public string PercentTotalVolume
        {
            get { return percentTotalVolume; }
            set { percentTotalVolume = value; }
        }
        public string VolumeUsd
        {
            get { return volumeUsd; }
            set { volumeUsd = value; }
        }
        public string TradingPairs
        {
            get { return tradingPairs; }
            set { tradingPairs = value; }
        }
        public string Socket
        {
            get { return socket; }
            set { socket = value; }
        }
        public string ExchangeUrl
        {
            get { return exchangeUrl; }
            set { exchangeUrl = value; }
        }
    }
    public class MarketsDataResponse
    {
        public List<MarketData> Data { get; set; }
    }
    public class MarketData
    {
        private string exchangeId;
        private string baseId;
        private string quoteId;
        private string baseSymbol;
        private string quoteSymbol;
        private string volumeUsd24Hr;
        private string priceUsd;
        private string volumePercent;

        public string ExchangeId
        {
            get { return exchangeId; }
            set { exchangeId = value; }
        }
        public string BaseId
        {
            get { return baseId; }
            set { baseId = value; }
        }
        public string QuoteId
        {
            get {  return quoteId; }
            set {  quoteId = value; }
        }
        public string BaseSymbol
        {
            get { return baseSymbol; }
            set { baseSymbol = value; }
        }
        public string QuoteSymbol
        {
            get { return quoteSymbol; }
            set { quoteSymbol = value; }
        }
        public string VolumeUsd24Hr
        {
            get { return volumeUsd24Hr; }
            set { volumeUsd24Hr = value; }
        }
        public string PriceUsd
        {
            get { return priceUsd; }
            set { priceUsd = value; }
        }
        public string VolumePercent
        {
            get { return volumePercent; }
            set { volumePercent = value; }
        }
    }
    public class ChartDataResponse
    {
        public List<ChartData> Data { get; set; }
    }
    public class ChartData
    {
        private string priceUsd;
        private string time;

        public string PriceUsd
        {
            get { return priceUsd; }
            set { priceUsd = value; }
        }
        public string Time
        {
            get { return time; }
            set { time = value; }
        }
    }
    public class CoinCapResponse
    {
        public CoinData Data { get; set; }
    }
    public class CoinData : INotifyPropertyChanged
    {
        private string id;
        private string rank;
        private string symbol;
        private string name;
        private string volumeUsd24Hr;
        private string priceUsd;
        private string changePercent24Hr;


        public string Id
        {
            get
            {
                return id;
            }
            set
            {
                id = value;
                OnPropertyChanged("Id");
            }
        }
        public string Rank
        {
            get
            {
                return rank;
            }
            set
            {
                rank = value;
                OnPropertyChanged("Rank");
            }
        }
        public string Symbol
        {
            get
            {
                return symbol;
            }
            set
            {
                symbol = value;
                OnPropertyChanged("Symbol");
            }
        }
        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                name = value;
                OnPropertyChanged("Name");
            }
        }
        public string VolumeUsd24Hr
        {
            get
            {
                return volumeUsd24Hr;
            }
            set
            {
                volumeUsd24Hr = value;
                OnPropertyChanged("VolumeUsd24Hr");
            }
        }
        public string PriceUsd
        {
            get
            {
                return priceUsd;
            }
            set
            {
                priceUsd = value;
                OnPropertyChanged("PriceUsd");
            }
        }
        public string ChangePercent24Hr
        {
            get
            {
                return changePercent24Hr;
            }
            set
            {
                changePercent24Hr = value;
                OnPropertyChanged("ChangePercent24Hr");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
