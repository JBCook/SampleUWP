using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Runtime.Serialization.Json;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System.RemoteSystems;
using Windows.UI.Core;
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
    public sealed partial class DiscoverSessionPage : Page
    {
        //Local Variables
        private RemoteSystemSessionWatcher m_sessionWatcher;
        private RemoteSystemSession m_currentSession;
        ObservableCollection<RemoteSystemSessionInfo> sessionNames = new ObservableCollection<RemoteSystemSessionInfo>();
        private RemoteSystemSessionMessageChannel m_msgChannel;
        private const string Key = "data";

        public DiscoverSessionPage()
        {
            this.InitializeComponent();
        }
        private void MainPage(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(MainPage), null);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            //debug on console
            Debug.WriteLine($"Entered { this}");
            //discover sessions and display for click to enter 
            DiscoverSessions();

        }
        public async void DiscoverSessions()
        {
            Debug.WriteLine("InsideSessionDiscovery");
            try
            {
                RemoteSystemAccessStatus status = await RemoteSystem.RequestAccessAsync();
                if (status != RemoteSystemAccessStatus.Allowed)
                {
                    Debug.WriteLine("Access Denied");
                    return;
                }
                m_sessionWatcher = RemoteSystemSession.CreateWatcher();
                m_sessionWatcher.Added += RemoteSystemSessionWatcher_RemoteSessionAdded;
                m_sessionWatcher.Start();
                Debug.WriteLine("Starting Discovery");
            }
            catch (Win32Exception)
            {
                Debug.WriteLine("Discovery Failed");
            }
        }

        private async void RemoteSystemSessionWatcher_RemoteSessionAdded(RemoteSystemSessionWatcher sender, RemoteSystemSessionAddedEventArgs args)
        {
            Debug.WriteLine($"Discovered Session {args.SessionInfo.DisplayName}:{args.SessionInfo.ControllerDisplayName}.");
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {

                sessionNames.Add(args.SessionInfo);
                SessionList.ItemsSource = sessionNames;
            });

        }

        private async void selectSession(object sender, SelectionChangedEventArgs e)
        {

            //bool status = false;
            RemoteSystemSessionInfo sessionInfo = ((sender as ListBox).SelectedItem as RemoteSystemSessionInfo);
            Debug.WriteLine($"Session {sessionInfo.DisplayName} selected");
            //Request to Join
            RemoteSystemSessionJoinResult joinresult = await sessionInfo.JoinAsync();
            //create a particpant watcher
            //var watcher = joinresult.Session.CreateParticipantWatcher();
            //event that a watcher is added:
            /**
            watcher.Added += (s, e1) => {
                if (e1.Participant.RemoteSystem.DisplayName.Equals(sessionInfo.ControllerDisplayName))
                {
                    SessionHost = e1.Participant;
                    Debug.WriteLine("added");
                    Debug.WriteLine($"{e1.Participant.RemoteSystem.DisplayName}");
                    Debug.WriteLine($"{s}");
                    SessionHost = e1.Participant;

                }
            };
            watcher.Start();
    **/
            //process the result to ensure remote system access is allowed
            if (joinresult.Status == RemoteSystemSessionJoinStatus.Success)
            {
                //successful join
                Debug.WriteLine($"Session {sessionInfo.DisplayName} Joined Successfully");
                m_currentSession = joinresult.Session;
                //status = true;

            }

            StartRecievingMessages();
            await SendMessageToHostAsync("hello world");
            // return status;
        }

        public async Task<bool> SendMessageToHostAsync(string message)
        {
            bool status = false;
            try
            {
                if (m_msgChannel == null)
                {
                    m_msgChannel = new RemoteSystemSessionMessageChannel(m_currentSession, "OpenChannel");
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
                // await m_msgChannel.SendValueSetAsync(sentMessage, SessionHost);
                await m_msgChannel.BroadcastValueSetAsync(sentMessage);
                Debug.WriteLine("Message successfully sent to host.");
                status = true;
            }
            catch (Win32Exception)
            {
                Debug.WriteLine("failed to end host msg");
            }
            return status;
        }


        //MOVE TO CREATESESSION PAGE
        //THIS IS FOR THE CONTROLLER TO RECIEVE MESSAGES
        public async void StartRecievingMessages()
        {
            Debug.WriteLine("StartRecievingMessages");
            m_msgChannel = new RemoteSystemSessionMessageChannel(m_currentSession, "OpenChannel");
            m_msgChannel.ValueSetReceived += Event_ValueSetRecieved; //raises event that data was received

        }

        private async void Event_ValueSetRecieved(RemoteSystemSessionMessageChannel sender, RemoteSystemSessionValueSetReceivedEventArgs args)
        {
            //code
            Debug.WriteLine("ValueSetRecieved");
            Debug.WriteLine(args.Message[Key]);
            byte[] by = (byte[])args.Message[Key];
            string result = System.Text.Encoding.UTF8.GetString(by);
            Debug.WriteLine(result);
            //await BroadcastMessage("HelloWorld", "HelloWorld");
        }
    }
}
