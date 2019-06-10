using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Akka.Actor;
using DATC_Receiver.DataStructures;
using Microsoft.Azure.EventHubs;
using Microsoft.Azure.EventHubs.Processor;
using Newtonsoft.Json;

namespace DATC_Receiver
{
    /// <summary>
    /// Process event hub events 
    /// </summary>
    internal class EventHubReceiver : IEventProcessor
    {
        public EventHubReceiver()
        {
        }

        public Task CloseAsync(PartitionContext context, CloseReason reason)
        {
            Console.WriteLine($"EH Processor Shutting Down. Partition '{context.PartitionId}', Reason: '{reason}'.");
            return Task.CompletedTask;
        }

        public Task OpenAsync(PartitionContext context)
        {
            Console.WriteLine($"EH initialized. Partition: '{context.PartitionId}'");
            return Task.CompletedTask;
        }

        public Task ProcessErrorAsync(PartitionContext context, Exception error)
        {
            Console.WriteLine($"Error on Partition: {context.PartitionId}, Error: {error.Message}");
            return Task.CompletedTask;
        }

        public Task ProcessEventsAsync(PartitionContext context, IEnumerable<EventData> messages)
        {
            // if messages come in a batch
            foreach (var eventData in messages)
            {
                // get the message body
                var data = Encoding.UTF8.GetString(eventData.Body.Array, eventData.Body.Offset, eventData.Body.Count);
                // convert to POCO
                var info = JsonConvert.DeserializeObject<DeviceReading>(data);

                // send off to sub coordinator to process
                Program.tower.Tell(info);

                Console.WriteLine($"partId: {context.PartitionId} - entries: {info.aircraft.Count}");
            }
            // update EvntHub pointer (where we left off in procesing)
            return context.CheckpointAsync();
        }
    }
}