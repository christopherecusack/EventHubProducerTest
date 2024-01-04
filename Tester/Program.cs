using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;
using Newtonsoft.Json;
using System.Xml;

namespace Tester
{
    internal class Program
    {
        public static async Task Main()
        {
            var conn = "Endpoint=sb://<EventHubNamespace>.servicebus.windows.net/;SharedAccessKeyName=<AccessKeyName>;SharedAccessKey=<SharedAccessKey>";
            var hub = "<event hub name>";

            var clientOptions = new EventHubProducerClientOptions
            {
                RetryOptions = new EventHubsRetryOptions
                {
                    MaximumRetries = 10,
                    Delay = TimeSpan.FromSeconds(5),
                    MaximumDelay = TimeSpan.FromSeconds(10),
                    Mode = EventHubsRetryMode.Fixed,
                    TryTimeout = TimeSpan.FromSeconds(1)
                }
            };

            // Create the client; this will not establish the service connection since it is lazy.
            await using var producerClient = new EventHubProducerClient(conn, hub, clientOptions);

            // Force the service connection to be established without opening the publishing link.
            (await producerClient.GetEventHubPropertiesAsync()).Dump();

            // Create the batch; the first invocation will establish the publishing link to determine
            // the maximum message size.
            using var eventBatch = await producerClient.CreateBatchAsync();

            eventBatch.Dump();
            "Done".Dump();
        }        
    }

    public static class Ext
    {
        public static void Dump<T>(this T obj, bool indent = false)
        {
            var dump = JsonConvert.SerializeObject(obj, indent ? Newtonsoft.Json.Formatting.Indented : Newtonsoft.Json.Formatting.None,
                new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });

            Console.WriteLine(dump);
        }
    }   
}