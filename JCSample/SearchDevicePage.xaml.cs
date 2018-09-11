using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.UserActivities;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;
using Windows.System.RemoteSystems;
using Windows.UI.Core;
using Windows.UI.Popups;
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
    public sealed partial class SearchDevicePage : Page
    {
        //Local Variables
        private RemoteSystemWatcher remoteSystemWatcher;
        ObservableCollection<RemoteSystem> dataList = new ObservableCollection<RemoteSystem>();
        UserActivitySession _currentActivity;

        public SearchDevicePage()
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
            discoverDevices();


        }
        /// <summary>

        /// Signs in the current user.

        /// </summary>

        /// <returns></returns>



        private async void discoverDevices()
        {
            //Describes what happens when the button in the application is clicked
            //Verify Access for Remote Systems
            //RequestAccessAsync() is a method within the 
            //RemoteSystem class that gets the status of calling app's access to the RemoteSystem feature
            RemoteSystemAccessStatus accessStat = await RemoteSystem.RequestAccessAsync();
            int accessInt = (int)accessStat;
            if (accessStat == RemoteSystemAccessStatus.Allowed)
            {
                //build a watcher to monitor for remote systems
                remoteSystemWatcher = RemoteSystem.CreateWatcher();
                // Start the watcher.
                remoteSystemWatcher.Start();
                //events for changes in remote system discovery
                remoteSystemWatcher.RemoteSystemAdded += RemoteSystemWatcher_RemoteSystemAdded;
                remoteSystemWatcher.RemoteSystemUpdated += RemoteSystemWatcher_RemoteSystemUpdated;
            }
            else
            {
                var errDialog = new MessageDialog("Cannot access Remote Systems.." + accessInt.ToString());
                await errDialog.ShowAsync();
            }
        }

        private async void selectFromList(object sender, SelectionChangedEventArgs e)
        {
            RemoteSystem selection = ((sender as ListBox).SelectedItem as RemoteSystem);

            if (selection != null)
            {
                /**
                    
                Windows.Storage.StorageFolder storageFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
                Windows.Storage.StorageFile sampleFile =
                    await storageFolder.CreateFileAsync("helloworld.pdf",
                        Windows.Storage.CreationCollisionOption.ReplaceExisting);

                sampleFile = await storageFolder.GetFileAsync("helloworld.pdf");
                await Windows.Storage.FileIO.WriteTextAsync(sampleFile, "Hello World");
               // var success = await Windows.System.Launcher.LaunchFileAsync(sampleFile);
                //after the device is selected, create a new session for particpants to join
                Debug.WriteLine(sampleFile.Path);
                var uri = new System.Uri(sampleFile.Path);
                var converted = uri.AbsoluteUri;

                Windows.System.LauncherOptions options = new Windows.System.LauncherOptions();
                options.ContentType = "application/pdf";
                Windows.System.Launcher.LaunchUriAsync(new Uri(sampleFile.Path), options);
                **/
                RemoteLaunchUriStatus launchURIstatus = await RemoteLauncher.LaunchUriAsync(
                    new RemoteSystemConnectionRequest(selection),
                    new Uri($"ms-chat:?Body={"Hello World"}"));
                // Debug.WriteLine(uri.AbsoluteUri);
                Debug.WriteLine("3");
                //await CreateSession(selection);
                //StartRecievingMessages();

                //rmeote system connection request
                //RemoteSystemConnectionRequest connRequest = new RemoteSystemConnectionRequest(selection);
            }
        }

        private async void RemoteSystemWatcher_RemoteSystemAdded(RemoteSystemWatcher sender, RemoteSystemAddedEventArgs args)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                dataList.Add(args.RemoteSystem);
                SystemList.ItemsSource = dataList;
            });

        }
        private async void RemoteSystemWatcher_RemoteSystemUpdated(RemoteSystemWatcher sender, RemoteSystemUpdatedEventArgs args)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                Debug.WriteLine($"Entered Remote System Updating");
            });
        }


    }
}
