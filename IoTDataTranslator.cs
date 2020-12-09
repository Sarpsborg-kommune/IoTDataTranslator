using IoTHubTrigger = Microsoft.Azure.WebJobs.EventHubTriggerAttribute;

using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure.EventHubs;
using Microsoft.Azure.Devices;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;


namespace Sarpsborgkommune.IoT.IoTDataTranslator
{
    public static class IoTDataTranslator
    {
        private static HttpClient client = new HttpClient();
        private static IMemoryCache _memcache = new MemoryCache(new MemoryCacheOptions());
        private static Random random = new Random();
        private static string IotHubConnection = Environment.GetEnvironmentVariable("IoTHubConnection");

        [FunctionName("IoTDataTranslator")]
        public async static Task Run([IoTHubTrigger("messages/events", Connection = "IoTHubEndpoint")] EventData[] messages, ILogger log)
        {
            foreach (var message in messages)
            {
                string deviceId = message.SystemProperties["iothub-connection-device-id"].ToString();
                dynamic cacheEntry, twinTags;
                string sensorDecoder = null;
                // var iotData = JsonSerializer.Deserialize<Dictionary<string, object>>(Encoding.UTF8.GetString(message.Body.Array));
                var iotData = JsonSerializer.Deserialize<LoriotMessageUplink>(Encoding.UTF8.GetString(message.Body.Array));
                int retryCount = 0;
                log.LogWarning($"ID: {iotData.EUI}");
                if (iotData.cmd == "rx")
                {
                    while (true)
                    {
                        try
                        {
                            log.LogInformation($"IoT Message is RX and will be processed");

                            if (_memcache.TryGetValue(deviceId, out cacheEntry))
                            {
                                log.LogInformation("Cache HIT (Twin)");
                                using (JsonDocument doc = JsonDocument.Parse(cacheEntry))
                                {
                                    sensorDecoder = doc.RootElement.GetProperty("deviceType").ToString();
                                }
                                //log.LogInformation($"Cache: {twinTags}");
                                // sensorDecoder = twinTags["deviceType"]?.ToString() ?? string.Empty;  // This fails for big time test http://zetcode.com/csharp/json/
                                log.LogInformation($"Decoder: {sensorDecoder}");
                                log.LogInformation($"{JsonSerializer.Serialize(iotData)}");

                            }
                            else
                            {
                                twinTags = await GetTags(IotHubConnection, deviceId);
                                sensorDecoder = twinTags["deviceType"]?.ToString() ?? string.Empty;
                                log.LogInformation($"Decoder: {sensorDecoder}");

                                if (!string.IsNullOrEmpty(sensorDecoder))
                                {
                                    string data = JsonSerializer.Serialize(twinTags);
                                    var cacheEntryOptions = new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(10));
                                    _memcache.Set(deviceId, data, cacheEntryOptions);
                                    log.LogInformation("Cache MISS (Twin): Caching Twin Data:");
                                    log.LogInformation($"{deviceId} : {data}");
                                    log.LogInformation($"{JsonSerializer.Serialize(iotData)}");
                                }

                            }
                            break;
                        }
                        catch (Exception ex)
                        {
                            retryCount++;
                            if (retryCount > 4)
                                throw new Exception("Run failed (memorycache/IoTHub connection error).", ex);
                            await Task.Delay(random.Next(1000, 2000));
                        }
                    }
                }
            }
        }

        public async static Task<dynamic> GetTags(string ConnectionString, string id)
        {
            RegistryManager registryManager = RegistryManager.CreateFromConnectionString(ConnectionString);
            var twin = await registryManager.GetTwinAsync(id);

            return JsonSerializer.Deserialize<Dictionary<string, object>>(twin.Tags.ToJson());
        }
    }

    public abstract class MessageDecoder
    {

        public abstract Dictionary<string, object> Decode(byte[] data);
    }
}
