using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace TestTask
{
    public partial class HomePage : Page
    {
        private const string CoinCapApiBaseUrl = "https://api.coincap.io/v2/";
        private const string TopCoinsEndpoint = "assets";
        public HomePage()
        {
            InitializeComponent();
            LoadTopCoinsAsync();
        }
        private async void LoadTopCoinsAsync()
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    var queryParams = "?limit=10&order=marketCap";
                    var requestUrl = $"{CoinCapApiBaseUrl}{TopCoinsEndpoint}{queryParams}";
                    var response = await client.GetStringAsync(requestUrl);
                    var result = JsonConvert.DeserializeObject<CoinCapResponseList>(response);
                    coinListView.ItemsSource = result?.Data;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error when enabling data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CoinPage_Button_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new CoinsPage());
        }
        private void ExchangePage_Button_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Exchange());
        }
    }
    public class CoinCapResponseList
    {
        public List<TopCoinData> Data { get; set; }
    }

    public class TopCoinData : INotifyPropertyChanged
    {
        private string id;
        private string rank;
        private string symbol;
        private string name;
        private string marketCapUsd;
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
        public string MarketCapUsd
        {
            get
            {
                return marketCapUsd;
            }
            set
            {
                marketCapUsd = value;
                OnPropertyChanged("MarketCapUsd");
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
