using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Background;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Foundation.Diagnostics;
using Windows.Storage;
using Windows.System.Power;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace BatterySession
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
            RegisterBackgroundTask();
            lazyGiantUILoader();
        }

        private async void RegisterBackgroundTask()
        {
            switch (await BackgroundExecutionManager.RequestAccessAsync())
            {
                case BackgroundAccessStatus.DeniedBySystemPolicy:
                case BackgroundAccessStatus.DeniedByUser:
                    return; //TODO: error message
                default:
                    break;
            }

            //Is there a better way to validate the tasks have already been registered?
            if (BackgroundTaskRegistration.AllTasks.Count == 1)
            {
                //new LoggingChannel("BatterySession", new LoggingChannelOptions()).LogMessage("background task already registered, skipping");
                //Debug.WriteLine("background task already registered, skipping");
                return;
            }

            //Unregister all tasks if they exist
            foreach (KeyValuePair<Guid, IBackgroundTaskRegistration> task in BackgroundTaskRegistration.AllTasks)
                task.Value.Unregister(true);

            //Register new background task exclusively for PowerStateChange
            BackgroundTaskBuilder taskBuilder = new BackgroundTaskBuilder();
            taskBuilder.Name = "LogAndUpdateTile";
            taskBuilder.TaskEntryPoint = "BackgroundTasks.LogAndUpdateTile";
            taskBuilder.SetTrigger(new SystemTrigger(SystemTriggerType.PowerStateChange, false));
            BackgroundTaskRegistration registration = taskBuilder.Register();
        }

        private void AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(LogsPage), null);
        }

        //TODO: very bad, just a placeholder for main page for now.
        private async void lazyGiantUILoader()
        {
            TextBlock block = new TextBlock();

            IList<string> csvFile;
            try
            {
                StorageFolder localFolder = ApplicationData.Current.LocalFolder;
                StorageFile sampleFile = await localFolder.GetFileAsync("BatteryLog.csv");
                csvFile = await FileIO.ReadLinesAsync(sampleFile);
                block.Text = "last recorded entry:";
                TextBlock lastEntry = new TextBlock();
                lastEntry.Text = csvFile.Last();
                theStackPanel.Children.Add(block);
                theStackPanel.Children.Add(lastEntry);
            }
            catch (Exception e)
            {
                block.Text = "No logged data yet.";
                theStackPanel.Children.Add(block);
                return;
            }
        }
    }

    internal class LogEntry
    {
        private readonly DateTime dateTime;
        private readonly BatteryStatus state;
        private readonly int remainingMwh;

        public LogEntry(DateTime dateTime, BatteryStatus state, int remainingMwh)
        {
            this.dateTime = dateTime;
            this.state = state;
            this.remainingMwh = remainingMwh;
        }
    }
}
