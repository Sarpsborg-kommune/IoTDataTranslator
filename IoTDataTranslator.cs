using System;
using System.Text;
using System.Text.Json;
using System.Net.Http;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure.EventHubs;
using Microsoft.Azure.Devices;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;


using IoTHubTrigger = Microsoft.Azure.WebJobs.EventHubTriggerAttribute;


namespace sarpsborgkommune.iot
{
    public static class IoTRouter
    {
        private static HttpClient client = new HttpClient();
        private static IMemoryCache _memcache = new MemoryCache(new MemoryCacheOptions());
        private static Random random = new Random();

        private static string IotHubConnection = Environment.GetEnvironmentVariable("IoTHubConnection");

        public async static Task<dynamic> GetTags(string ConnectionString, string id)
        {
            RegistryManager registryManager = RegistryManager.CreateFromConnectionString(ConnectionString);
            var twin = await registryManager.GetTwinAsync(id);
            
            return JsonSerializer.Deserialize<Dictionary<string, object>>(twin.Tags.ToJson());
        }

        [FunctionName("IoTRouter")]
        public async static Task Run([IoTHubTrigger("messages/events", Connection = "IoTHubEndpoint")]EventData[] messages, Microsoft.Extensions.Logging.ILogger log)
        {
            foreach (var message in messages)
            {
                string deviceId = message.SystemProperties["iothub-connection-device-id"].ToString();
                dynamic cacheEntry, twinTags;
                string sensorDecoder = null;
                var iotData = JsonSerializer.Deserialize<Dictionary<string, object>>(Encoding.UTF8.GetString(message.Body.Array));
                int retryCount = 0;
                

                log.LogInformation($"Received IoT Message:");
                log.LogInformation(Encoding.UTF8.GetString(message.Body.Array));
                if (iotData["cmd"].ToString() == "rx")
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
    }

    public abstract class MessageDecoder
    {
        


        public abstract Dictionary<string, object> Decode(byte[] data);
    }

    public class IoTMessage
    {
        // Example output from: loriot
        // {"cmd":"rx","seqno":948423,"EUI":"A81758FFFE03F796","ts":1603875210504,"fcnt":159521,
        // "port":5,"freq":867500000,"rssi":-72,"snr":8,"toa":66,"dr":"SF7 BW125 4/5","ack":false,
        // "bat":254,"offline":false,"data":"0100ea022804017b0500060198070e40"}
        // See https://docs.loriot.io/display/LNS/API+Data+Format
        public string cmd { get; set;  }        // Identifies type of message, always 'tx' for downlink messages
        public int seqno { get; set;  }         // Must correspond to the latest FCnt reported by the 'txtd' message
        public string eui { get; set; }         // Device EUI, 16 hex digits (without dashes)
        public string ts { get; set; }          // Server timestamp as a number of milliseconds from Linux epoch OR timestamp (gateway internal counter)
        public string fcnt { get; set; }        // Frame counter, a 32-bit integer number
        public int port { get; set; }           // Port number (1 to 223)
        public int freq { get; set; }           // radio frequency at which the frame was received, in Hz
        public int rssi { get; set; }           // radio rssi, in dBm
        public int snr { get; set; }            // radio snr, in dB, singe decimal digit precision
        public int toa { get; set;  }           
        public string dr { get; set; }          // radio data rate - spreading factor, bandwidth and coding rate, e.g. SF12 BW125 4/5
        public bool ack { get; set; }           // acknowledgment flag as set by device
        public int bat { get; set;  }           // device battery status
        public bool offline { get; set; }
        public bool confirmed { get; set; }
        public string time { get; set; }        // timestamp of packet reception by gateway , in ISO 8601 format, UTC, up to nanosecond precision for GPS-enabled gateway, otherwise micro or milliseconds
        public int tmms { get; set; }           // timestamp of packet reception by gateways, in GPS time, number of milliseconds since Jan. 6th 1980
        public string gweui { get; set; }       // gateway extended EUI as EUI.RADIO, eg. 1122334455667788.0
        public int lat { get; set; }            // gateway latiude, fractional degrees
        public int lon { get; set; }            // gateway longtiude, fractional degrees
        public string data { get; set; }        // data payload (to be encrypted by server) as a hexadecimal string, min. 2 hex char (= 1 byte)
    }
}