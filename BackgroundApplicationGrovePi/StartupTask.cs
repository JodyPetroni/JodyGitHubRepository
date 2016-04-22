using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Http;
using Windows.ApplicationModel.Background;
using System.Threading.Tasks;
using GrovePi;
using System.Net.Http.Headers;
using GrovePi.Sensors;
using ppatierno.AzureSBLite.Messaging;
using Newtonsoft.Json;

// The Background Application template is documented at http://go.microsoft.com/fwlink/?LinkID=533884&clcid=0x409

namespace BackgroundApplicationGrovePi
{
    public sealed class TelementryEvent
    {
        public string guid { get; set; }
        public string organisation { get; set; }
        public string displayname { get; set; }
        public string location { get; set; }
        public string measurename { get; set; }
        public string unitormeasure { get; set; }
        public float value { get; set; }
        public string timecreated { get; set; }
        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }

    }
    public sealed class StartupTask : IBackgroundTask
    {
        BackgroundTaskDeferral deferral;
        SensorStatus s;

        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            // 
            // TODO: Insert code to perform background work
            //
            // If you start any asynchronous methods here, prevent the task
            // from closing prematurely by using BackgroundTaskDeferral as
            // described in http://aka.ms/backgroundtaskdeferral
            //

            //
            // Create the deferral by requesting it from the task instance.
            //
            deferral = taskInstance.GetDeferral();
            taskInstance.Canceled += TaskInstance_Canceled;
            //
            // Call asynchronous method(s) using the await keyword.
            //
            
            await ExampleMethodAsync();
            //ExampleMethod();
            //
            // Once the asynchronous method(s) are done, close the deferral.
            //
            deferral.Complete();
            

        }
        
        private void TaskInstance_Canceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            DeviceFactory.Build.RgbLcdDisplay().SetText("Stopped collecting");
            deferral.Complete();
        }

        private async Task ExampleMethodAsync()
        {
            var eh = EventHubClient.CreateFromConnectionString("Endpoint=sb://jodypetroni.servicebus.windows.net/;SharedAccessKeyName=Send;SharedAccessKey=KqToa8sf7OovLCCZxc/vRxEKlNOLlPJe6FlWb1ym+hQ=;EntityPath=jpeventhub3");
            //Endpoint=sb://jodypetroni.servicebus.windows.net/;SharedAccessKeyName=Send;SharedAccessKey=uFWqVT8W9faX5MSOvCqyt3yjBnBe2qJ0xhOxsOXLOPs=;EntityPath=jpeventhub
            while (true)
            {
                

                var light = DeviceFactory.Build.LightSensor(Pin.AnalogPin2).SensorValue();
                var temp = DeviceFactory.Build.TemperatureAndHumiditySensor(Pin.DigitalPin2, GrovePi.Sensors.TemperatureAndHumiditySensorModel.DHT11).TemperatureAndHumidity().Temperature;
                var humid = DeviceFactory.Build.TemperatureAndHumiditySensor(Pin.DigitalPin2, GrovePi.Sensors.TemperatureAndHumiditySensorModel.DHT11).TemperatureAndHumidity().Humidity;


                DeviceFactory.Build.RgbLcdDisplay().SetText(string.Format("Current Temperature is: {0}", temp.ToString()));
                var reading = new TelementryEvent { guid = Guid.NewGuid().ToString(), displayname = "Level 2 Software" , location = "210 Kings Way", measurename = "Temperature", organisation = "Datacom", unitormeasure="celcius", value=temp, timecreated=DateTime.Now.ToString() }.ToJson();
                eh.Send(new EventData(Encoding.UTF8.GetBytes(reading)));
                await Task.Delay(1000);

                DeviceFactory.Build.RgbLcdDisplay().SetText(string.Format("Current Humidity is: {0}", humid.ToString())).SetBacklightRgb(0, 255, 255);
                var reading1 = new TelementryEvent { guid = Guid.NewGuid().ToString(), displayname = "Level 2 Software", location = "210 Kings Way", measurename = "Humidity", organisation = "Datacom", unitormeasure = "percent", value = humid, timecreated = DateTime.Now.ToString() }.ToJson();
                eh.Send(new EventData(Encoding.UTF8.GetBytes(reading1)));
                await Task.Delay(1000);

                DeviceFactory.Build.RgbLcdDisplay().SetText(string.Format("Current Light is: {0}", light.ToString())).SetBacklightRgb(0, 255, 255);
                var reading2 = new TelementryEvent { guid = Guid.NewGuid().ToString(), displayname = "Level 2 Software", location = "210 Kings Way", measurename = "Light", organisation = "Datacom", unitormeasure = "brightness", value = light, timecreated = DateTime.Now.ToString() }.ToJson();
                eh.Send(new EventData(Encoding.UTF8.GetBytes(reading2)));
                await Task.Delay(1000);


                
            }
        }       

    }
}
