using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.System.RemoteSystems;
using Windows.UI.Popups;
using Windows.UI.Core;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Runtime.Serialization.Json;
using Windows.System;
using System.Diagnostics;
using System.ComponentModel;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using System.Text;
using Windows.ApplicationModel.UserActivities;

using Windows.Security.Authentication.Web;
using Windows.Security.Authentication.Web.Core;
using Microsoft.Identity.Client;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409


namespace JCSample
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public enum SessionCreationResult
        {
            Success,
            PermissionError,
            TooManySessions,
            Failure
        }


        private const string V = "http://www.microsoft.com";
        private RemoteSystemSessionMessageChannel m_msgChannel;
        UserActivitySession _currentActivity;

        private RemoteSystemSession m_currentSession;
        private RemoteSystemSessionController _controller;

        private const string Key = "data";

        //create new ListItemCollection

        public RemoteSystemSessionParticipant SessionHost { get; set; }
        public RemoteSystemSessionParticipant SessionUser { get; set; }

        public MainPage()
        {
            this.InitializeComponent();
        }
        public event EventHandler<SessionEventArgs> SessionFound = delegate { };

        private async void Discover_Click(object sender, RoutedEventArgs e)
        {
            //DiscoverSessions();
            this.Frame.Navigate(typeof(DiscoverSessionPage), null);
        }
        //method that occurs when the button is clicked to discover a device
        //async 
        private void DeviceSearch_Click(object sender, RoutedEventArgs e)
        {

            this.Frame.Navigate(typeof(SearchDevicePage), null);

        }
        private void CreateSession_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(CreateSessionPage), null);
        }
        private async void CreateActivity_Click(object sender, RoutedEventArgs e)
        {
            //DiscoverSessions();
            //this.Frame.Navigate(typeof(DiscoverSessionPage), null);
            await GenerateActivityAsync();
        }
        private async void MSGraph_Click(object sender, RoutedEventArgs e)
        {
            //DiscoverSessions();
            this.Frame.Navigate(typeof(MSGraphPage), null);

            //var token = await getTokenForUser();

        }



        public async Task<bool> SendMessageToParticipant(string message, RemoteSystemSessionParticipant participant)
        {
            bool status = false;
            if (m_msgChannel == null)
            {
                m_msgChannel = new RemoteSystemSessionMessageChannel(m_currentSession, "ParticipantChannel");
            }
            byte[] data;
            using (var stream = new MemoryStream())
            {
                new DataContractJsonSerializer(message.GetType()).WriteObject(stream, message);
                data = stream.ToArray();
            }
            // Send message to specific participant, in this case, the host.
            ValueSet sentMessage = new ValueSet
            {
                [Key] = data
            };
            await m_msgChannel.SendValueSetAsync(sentMessage, participant);
            //Debug.WriteLine($"Message successfully sent to {Particpant}");
            status = true;
            return status;
        }

        private async Task GenerateActivityAsync()
        {
            // Get the default UserActivityChannel and query it for our UserActivity. If the activity doesn't exist, one is created.
            UserActivityChannel channel = UserActivityChannel.GetDefault();
            UserActivity userActivity = await channel.GetOrCreateUserActivityAsync("MainPage");

            // Populate required properties
            userActivity.VisualElements.DisplayText = "First Activity";
            userActivity.ActivationUri = new Uri("JCSample:navigate?page=MainPage");

            //Save
            await userActivity.SaveAsync(); //save the new metadata

            // Dispose of any current UserActivitySession, and create a new one.
            _currentActivity?.Dispose();
            _currentActivity = userActivity.CreateSession();

            Debug.WriteLine("activity generated?");
        }


    }





    public class SessionEventArgs

    {

        public RemoteSystemSessionInfo SessionInfo { get; set; }

    }
}


