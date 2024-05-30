using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Devices.Power;
using Windows.Storage;
using Windows.System.Power;

namespace BackgroundTasks
{
    public sealed class LogAndUpdateTile : IBackgroundTask
    {
        public void Run(IBackgroundTaskInstance taskInstance)
        {
            BackgroundTaskDeferral deferral = taskInstance.GetDeferral();
            asyncStuff(taskInstance, deferral);
        }

        private async void asyncStuff(IBackgroundTaskInstance taskInstance, BackgroundTaskDeferral deferral)
        {
            string dateTime = DateTime.Now.ToString();

            BatteryReport batteryReport = Battery.AggregateBattery.GetReport();
            string state = batteryReport.Status.ToString();
            Nullable<int> remainingMwh = batteryReport.RemainingCapacityInMilliwattHours;
            string remaining;
            if (remainingMwh != null)
                remaining = remainingMwh.ToString();
            else
                remaining = "N/A";

            StringBuilder sb = new StringBuilder();
            sb.Append(dateTime);
            sb.Append(",");
            sb.Append(state);
            sb.Append(",");
            sb.Append(remaining);

            await WriteToFile("BatteryLog.csv", sb.ToString());
            deferral.Complete();
        }

        private async Task WriteToFile(string fileName, string content)
        {
            try
            {
                StorageFolder localFolder = ApplicationData.Current.LocalFolder;
                StorageFile file = await localFolder.CreateFileAsync(fileName, CreationCollisionOption.OpenIfExists);

                try
                {
                    await FileIO.AppendTextAsync(file, content);
                }
                catch (Exception e)
                {
                    StorageFile errorFile = await localFolder.CreateFileAsync("Err-" + fileName, CreationCollisionOption.OpenIfExists);
                    await FileIO.AppendTextAsync(errorFile, content);
                }
            }

            catch (Exception e)
            {
                //SendToast("Encountered fatal error" + e.Message);
            }
        }
    }
}
