using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace JCSample
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MSGraphPage : Page
    {
        string graphAPIEndpoint = "https://graph.microsoft.com/v1.0/me";
        string[] scopes = new string[] { "device.read", "device.command" };
        ObservableCollection<DeviceFromGraphModel> devicesFromGraph = new ObservableCollection<DeviceFromGraphModel>();
        DeviceFromGraphModel device;
        Dictionary<string, string> Headers { get; set; }
        string tokenToUse;
        public MSGraphPage()
        {
            this.InitializeComponent();
        }
        private void MainPage(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(MainPage), null);
        }
        private async void CallGraphButton_Click(object sender, RoutedEventArgs e)
        {
            AuthenticationResult authResult = null;
            ResultText.Text = string.Empty;
            //TokenInfoText.Text = string.Empty;
            IEnumerable<IAccount> accounts = await App.PublicClientApp.GetAccountsAsync();
            IAccount firstAccount = accounts.FirstOrDefault();

            try
            {
                authResult = await App.PublicClientApp.AcquireTokenSilentAsync(scopes, firstAccount);
            }
            catch (MsalUiRequiredException ex)
            {
                // A MsalUiRequiredException happened on AcquireTokenSilentAsync. This indicates you need to call AcquireTokenAsync to acquire a token
                System.Diagnostics.Debug.WriteLine($"MsalUiRequiredException: {ex.Message}");

                try
                {
                    authResult = await App.PublicClientApp.AcquireTokenAsync(scopes);
                }
                catch (MsalException msalex)
                {
                    ResultText.Text = $"Error Acquiring Token:{System.Environment.NewLine}{msalex}";
                }
            }
            catch (Exception ex)
            {
                ResultText.Text = $"Error Acquiring Token Silently:{System.Environment.NewLine}{ex}";
                return;
            }

            if (authResult != null)
            {
                ResultText.Text = await GetHttpContentWithToken(graphAPIEndpoint, authResult.AccessToken);
                tokenToUse = authResult.AccessToken;
                Debug.WriteLine(tokenToUse);
                this.SignOutButton.Visibility = Visibility.Visible;
                //CODE FOR ROME

                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenToUse);
                Uri uri = new Uri("https://graph.microsoft.com/beta/me/devices");
                HttpResponseMessage httpResponse = await client.GetAsync(uri);
                var responseText = await httpResponse.Content.ReadAsStringAsync();
                Debug.WriteLine(responseText);
                Debug.WriteLine(responseText);

                Newtonsoft.Json.Linq.JObject jObject = Newtonsoft.Json.Linq.JObject.Parse(responseText);

                dynamic obj = Newtonsoft.Json.JsonConvert.DeserializeObject(responseText);
                var devices = obj.value;
                foreach (var device_model in devices)
                {
                    DeviceFromGraphModel device = new DeviceFromGraphModel();
                    device.id = device_model.id;
                    device.Name = device_model.Name;
                    devicesFromGraph.Add(device);

                }
                foreach (DeviceFromGraphModel m_device in devicesFromGraph)
                {
                    Debug.WriteLine(m_device.id);

                }
                //show the names on the list in hte MSGraphPageXAML
                //SessionList.ItemsSource = sessionNames;
                //inXAML instead of DeviceName use Name
                DeviceFromGraphList.ItemsSource = devicesFromGraph;
            }
            //further options to select the device and send a command will come from @selectDeviceFromGraph where the user selects the device 
            //selected device will then send a command saying "hello world"
        }

        private void selectDeviceFromGraph(object sender, SelectionChangedEventArgs e)
        {
            //device is selected from the graph
            device = ((sender as ListBox).SelectedItem as DeviceFromGraphModel);
            this.LaunchURIButton.Visibility = Visibility.Visible;
            this.AppServiceButton.Visibility = Visibility.Visible;
            //GOAL:send a command

        }
        private async void LaunchURI(object sender, RoutedEventArgs e)
        {
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenToUse);
            var id = device.id;
            string uriid = ($"https://graph.microsoft.com/beta/me/devices/{id}/commands");
            Debug.WriteLine(uriid);
            Uri uri = new Uri(uriid);
            //HttpResponseMessage httpResponse = await client.PostAsync(uri);
            //var responseText = await httpResponse.Content.ReadAsStringAsync();
            //Debug.WriteLine(responseText);
            string body = "{ \"type\" : \"LaunchUri\", \"payload\" : { \"uri\":\"http://bing.com\"}}";
            var request = new HttpRequestMessage(new HttpMethod("POST"), uri)
            {
                Content = new StringContent(body, Encoding.UTF8, "application/json"),

            };
            HttpResponseMessage httpResponse = await client.SendAsync(request);
            var responseText = await httpResponse.Content.ReadAsStringAsync();
            Debug.WriteLine(responseText);
        }
        private async void AppService(object sender, RoutedEventArgs e)
        {
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenToUse);
            var id = device.id;
            string uriid = ($"https://graph.microsoft.com/beta/me/devices/{id}/commands");
            Debug.WriteLine(uriid);
            Uri uri = new Uri(uriid);
            //HttpResponseMessage httpResponse = await client.PostAsync(uri);
            //var responseText = await httpResponse.Content.ReadAsStringAsync();
            //Debug.WriteLine(responseText);
            string body = "{ \"type\" : \"AppService\", \"appServiceName\" : \"com.microsoft.test.cdppingpongservice\", " +
                "\"packageFamilyName\" : \"5085ShawnHenry.RomanTestApp_jsjw7knzsgcce\"," +
                 "\"payload\" : { \"Type\":\"Toast\",\"Title\":\"Hello\", \"Subtitle\":\"World!\"}}";
            var request = new HttpRequestMessage(new HttpMethod("POST"), uri)
            {
                Content = new StringContent(body, Encoding.UTF8, "application/json"),

            };

            HttpResponseMessage httpResponse = await client.SendAsync(request);
            var responseText = await httpResponse.Content.ReadAsStringAsync();
            Debug.WriteLine(responseText);

        }
        public async Task<string> GetHttpContentWithToken(string url, string token)
        {
            var httpClient = new System.Net.Http.HttpClient();
            System.Net.Http.HttpResponseMessage response;
            try
            {
                var request = new System.Net.Http.HttpRequestMessage(System.Net.Http.HttpMethod.Get, url);
                // Add the token in Authorization header
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                response = await httpClient.SendAsync(request);
                var content = await response.Content.ReadAsStringAsync();
                return content;
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }


        private async void SignOutButton_Click(object sender, RoutedEventArgs e)
        {
            IEnumerable<IAccount> accounts = await App.PublicClientApp.GetAccountsAsync();
            IAccount firstAccount = accounts.FirstOrDefault();
            if (accounts.Any())
            {
                try
                {
                    await App.PublicClientApp.RemoveAsync(firstAccount);
                    this.ResultText.Text = "User has signed-out";
                    this.CallGraphButton.Visibility = Visibility.Visible;
                    this.SignOutButton.Visibility = Visibility.Collapsed;
                }
                catch (MsalException ex)
                {
                    ResultText.Text = $"Error signing-out user: {ex.Message}";
                }
            }
        }
    }

    class DeviceFromGraphModel
    {
        public string id { get; set; }
        public string Name { get; set; }
        public string Manufacturer { get; set; }
        public string Model { get; set; }
        public string Kind { get; set; }
        public string Status { get; set; }
        public string Platform { get; set; }

    }
    class DeviceSummary
    {
        public ObservableCollection<DeviceFromGraphModel> listOfDevices;


    }
}
