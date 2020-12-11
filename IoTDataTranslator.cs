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
        public async static Task Run([IoTHubTrigger("messages/events",
            Connection = "IoTHubEndpoint")] EventData[] messages,
            [EventHub("main", Connection = "EventHubEndpoint")] IAsyncCollector<string> output,
            ILogger log)
        {
            foreach (var message in messages)
            {
                string deviceId = message.SystemProperties["iothub-connection-device-id"].ToString();
                dynamic cacheEntry, twinTags;
                string sensorDecoder = null;
                int retryCount = 0;
                var iotData = JsonSerializer.Deserialize<LoriotMessageUplink>(Encoding.UTF8.GetString(message.Body.Array));
                IoTMessage iotmessage = new IoTMessage();

                if (iotData.cmd == "rx")
                {
                    while (true)
                    {
                        try
                        {
                            if (_memcache.TryGetValue(deviceId, out cacheEntry))
                            {
                                using (JsonDocument doc = JsonDocument.Parse(cacheEntry))
                                {
                                    sensorDecoder = doc.RootElement.GetProperty("deviceType").ToString();
                                }
                            }
                            else
                            {
                                twinTags = await GetTags(IotHubConnection, deviceId);
                                sensorDecoder = twinTags["deviceType"]?.ToString() ?? string.Empty;

                                if (!string.IsNullOrEmpty(sensorDecoder))
                                {
                                    string data = JsonSerializer.Serialize(twinTags);
                                    var cacheEntryOptions = new MemoryCacheEntryOptions()
                                        .SetSlidingExpiration(TimeSpan.FromMinutes(30));
                                    _memcache.Set(deviceId, data, cacheEntryOptions);
                                }
                                else
                                {
                                    throw new ArgumentNullException();
                                }

                            }
                            break;
                        }
                        catch (ArgumentNullException ex)
                        {
                            log.LogError($"deviceType missing from DeviceTwin for {deviceId}");
                            throw new Exception("deviceType missing from DeviceTwin", ex);
                        }
                        catch (Exception ex)
                        {
                            retryCount++;
                            if (retryCount > 4)
                                throw new Exception("Run failed (memorycache/IoTHub connection error).", ex);
                            await Task.Delay(random.Next(1000, 2000));
                        }
                    }

                    switch (sensorDecoder)
                    {
                        case "elsys":
                            iotmessage.id = iotData.EUI;
                            iotmessage.deviceType = sensorDecoder;
                            iotmessage.receivers = null;
                            iotmessage.timeStamp = DateTimeOffset.FromUnixTimeMilliseconds(iotData.ts).DateTime;
                            iotmessage.data = new ElsysMessage(helperfunctions.StringToByteArray(iotData.data));
                            log.LogInformation($"Json Data: {JsonSerializer.Serialize(iotmessage, new JsonSerializerOptions { IgnoreNullValues = true })}");

                            break;
                        default:
                            log.LogError($"Sensordecoder for deviceType {sensorDecoder} is not implemented");
                            break;
                    }
                }
                await output.AddAsync(JsonSerializer.Serialize(iotmessage, new JsonSerializerOptions { IgnoreNullValues = true }));

            }
            await output.FlushAsync();
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
