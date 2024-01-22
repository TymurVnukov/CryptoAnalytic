using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TestTask
{
    public partial class Exchange : Page
    {
        private const string CoinCapApiBaseUrl = "https://api.coincap.io/v2/";
        private const string TopCoinsEndpoint = "assets";
        public Exchange()
        {
            InitializeComponent();

            LoadExchangeCoinsAsync();
        }
        private List<ExchangeCoinData> currentLoadedCoinsData = new List<ExchangeCoinData>();
        private async void LoadExchangeCoinsAsync()
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    var requestUrl = $"{CoinCapApiBaseUrl}{TopCoinsEndpoint}";
                    var response = await client.GetStringAsync(requestUrl);
                    var result = JsonConvert.DeserializeObject<ExchangeCoinResponseList>(response);
                    currentLoadedCoinsData = result?.Data;
                    ObservableCollection<string> comboBoxItems = new ObservableCollection<string>();
                    for(int i = 0; i < currentLoadedCoinsData.Count; i++)
                    {
                        comboBoxItems.Add(currentLoadedCoinsData[i].Name);
                    }
                    LeftExchangeComboBox.ItemsSource = comboBoxItems;
                    RightExchangeComboBox.ItemsSource = comboBoxItems;
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

        private void HomePage_Button_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new HomePage());
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (LeftExchangeComboBox.SelectedItem != null && RightExchangeComboBox.SelectedItem != null)
            {
                if (double.TryParse(FromCoinTextBox.Text, out double fromCoinCount))
                {
                    double result = 0, fromPrice = 0, toPrice = 0;
                    foreach (var item in currentLoadedCoinsData)
                    {
                        if (LeftExchangeComboBox.SelectedItem.ToString() == item.Name)
                        {
                            fromPrice = Convert.ToDouble(item.PriceUsd.Substring(0, item.PriceUsd.IndexOf(".")) + "," + item.PriceUsd.Substring(item.PriceUsd.IndexOf(".") + 1));
                        }
                        if (RightExchangeComboBox.SelectedItem.ToString() == item.Name)
                        {
                            toPrice = Convert.ToDouble(item.PriceUsd.Substring(0, item.PriceUsd.IndexOf(".")) + "," + item.PriceUsd.Substring(item.PriceUsd.IndexOf(".") + 1));
                        }
                    }
                    result = (fromCoinCount * fromPrice) / toPrice;

                    ToCoinTextBox.Text = result.ToString();
                }
                else
                {
                    MessageBox.Show($"Error incorrect format, try ###,###. Example: 12,3456", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show($"Error choise coins", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    public class ExchangeCoinResponseList
    {
        public List<ExchangeCoinData> Data { get; set; }
    }

    public class ExchangeCoinData
    {
        private string symbol;
        private string name;
        private string priceUsd;

        public string Symbol
        {
            get
            {
                return symbol;
            }
            set
            {
                symbol = value;
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
            }
        }
    }
}
