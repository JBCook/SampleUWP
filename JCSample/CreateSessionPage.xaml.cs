using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System.RemoteSystems;
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
    public sealed partial class CreateSessionPage : Page
    {
        //local variables
        public RemoteSystemSession m_currentSession;
        private String m_currentSessionName;
        private RemoteSystemSessionMessageChannel m_msgChannel;
        private const string Key = "data";

        public CreateSessionPage()
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
            //discover devices
            createSession();

        }

        private void printSessionStatus(string str)
        {
            statustext.DataContext = new TextboxText() { textdata = str };
        }

        public async Task<bool> createSession()
        {

            bool status = false;

            // m_currentSessionName = "JC Rome Sample";
            var manager = new RemoteSystemSessionController("JC Rome Sample");
            manager.JoinRequested += JoinRequested;
            try
            {
                RemoteSystemSessionCreationResult createResult = await manager.CreateSessionAsync();
                if (createResult.Status == RemoteSystemSessionCreationStatus.Success)
                {
                    RemoteSystemSession currentSession = createResult.Session;
                    currentSession.Disconnected += (sender, args) =>
                    {
                        //SessionDisconnected(sender, args);
                        Debug.WriteLine("disconected");
                    };

                    m_currentSession = currentSession;
                    printSessionStatus($"Session {m_currentSession.DisplayName} created successfully.");
                    status = true;
                    StartRecievingMessages();


                }
                else if (createResult.Status == RemoteSystemSessionCreationStatus.SessionLimitsExceeded)
                {
                    status = false;
                    printSessionStatus("Session limits exceeded.");
                }
                else
                {
                    status = false;
                    printSessionStatus("Failed to create session.");
                }
            }

            catch (Win32Exception)
            {
                status = false;
                printSessionStatus("Failed to Create Session");
            }
            return status;

        }

        private void JoinRequested(RemoteSystemSessionController sender, RemoteSystemSessionJoinRequestedEventArgs args)
        {
            Debug.WriteLine("joinrequested");
            var deferral = args.GetDeferral();
            Debug.WriteLine(args.JoinRequest.Participant);
            args.JoinRequest.Accept();
            //ParticipantJoined(this, new ParticipantJoinedEventArgs() { Participant = args.JoinRequest.Participant });
            deferral.Complete();
        }
        public async void StartRecievingMessages()
        {
            Debug.WriteLine("StartRecievingMessages");
            m_msgChannel = new RemoteSystemSessionMessageChannel(m_currentSession, "OpenChannel");
            m_msgChannel.ValueSetReceived += Event_ValueSetRecieved; //raises event that data was received

        }

        private async void Event_ValueSetRecieved(RemoteSystemSessionMessageChannel sender, RemoteSystemSessionValueSetReceivedEventArgs args)
        {

            string senderdisplayname = args.Sender.RemoteSystem.DisplayName;
            //Debug.WriteLine(args.Message[Key]);
            byte[] by = (byte[])args.Message[Key];
            string result = System.Text.Encoding.UTF8.GetString(by);
            //Debug.WriteLine(result);
            //Debug.WriteLine(sender.ToString());
            string recv = senderdisplayname + ": " + result;
            receivedtext.DataContext = new TextboxText() { textdata = recv };

        }


    }
    public class TextboxText
    {
        public string textdata { get; set; }

    }
}
