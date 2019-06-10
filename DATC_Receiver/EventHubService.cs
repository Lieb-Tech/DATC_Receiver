using Microsoft.Azure.EventHubs;
using Microsoft.Azure.EventHubs.Processor;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DATC_Receiver
{
    /// <summary>
    ///  Event hub manager 
    /// </summary>
    class EventHubService
    {
        private string StorageConnectionString = "";
        private configSettings config = null;
        public EventHubService()
        {
            // get settings from file
            var json = File.ReadAllText("eventHub.json");            
            config = JsonConvert.DeserializeObject<configSettings>(json);

            // build connection string from settings
            StorageConnectionString = string.Format("DefaultEndpointsProtocol=https;AccountName={0};AccountKey={1}", config.storageAccountName, config.storageAccountKey);
        }

        public void StartReceivers()
        {
            // start up processor, registering EventHubReceiver to handle messages
            var eventProcessorHost = new EventProcessorHost(
                    config.eventHubName,
                    PartitionReceiver.DefaultConsumerGroupName,
                    config.eventHubConnectionString,
                    StorageConnectionString,
                    config.storageContainerName);

            // Registers the Event Processor Host and starts receiving messages
            eventProcessorHost.RegisterEventProcessorAsync<EventHubReceiver>().Wait();
        }

        // Event hub settings
        public class configSettings
        {
            public string eventHubConnectionString { get; set; }
            public string eventHubName { get; set; }

            public string storageContainerName { get; set; }
            public string storageAccountName { get; set; }
            public string storageAccountKey { get; set; }
        }
    }

}
