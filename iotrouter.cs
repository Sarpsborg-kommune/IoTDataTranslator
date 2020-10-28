using IoTHubTrigger = Microsoft.Azure.WebJobs.EventHubTriggerAttribute;

using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure.EventHubs;
using System.Text;
using System.Net.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Build.Framework;

namespace sarpsborgkommune.iot
{
    public static class IoTRouter
    {
        private static HttpClient client = new HttpClient();

        [FunctionName("IoTRouter")]
        public static void Run([IoTHubTrigger("messages/events", Connection = "IoTHubConnection")]EventData message, ILogger log)
        {
            log.LogInformation($"C# IoT Hub trigger function processed a message: {Encoding.UTF8.GetString(message.Body.Array)}");
        }
    }

    public abstract class MessageDecoder
    {
        protected static int Bin8Dec(byte b1)
        {
            int number = b1;
            if (number > 128) number = -(256 - number);
            else if (number == 128) number = 0;

            return number;
        }

        protected static int Bin16Dec(byte b1, byte b2)
        {
            int number = (b1 * 256) + b2;
            if (number > 32768) number = -(65535 - number);
            else if (number == 32768) number = 0;

            return number;
        }

        protected static int GetHexVal(char hex)
        {
            int value = (int)hex;
            return value - (value < 58 ? 48 : 55);
        }


        public abstract string Decode();
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