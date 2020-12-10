using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Sarpsborgkommune.IoT.IoTDataTranslator
{
    // Documentation: https://docs.loriot.io/display/LNS/API+Data+Format
    class LoriotMessageUplink
    {
        // Commented out percieved unneeded class members to minimize object size this should
        // make this a lean, mean codingmachine :-)
        public string cmd { get; set; }         // string: identifies message type, always 'rx' for uplink data message
        // public int seqno { get; set; }          // number: must correspond to the latest FCnt reported by the 'txd' message
        public string EUI { get; set; }         // string: device EUI, 16 hex digits (without dashes)
        public long ts { get; set; }             // number: Server timestamp as number of milliseconds from Linux epoch
        // public int fcnt { get; set; }           // number: frame counter, a 32-bit integer number
        // public int port { get; set; }           // number: port number as sent by the end device
        // public int freq { get; set; }           // number: radio frequency at which the frame was received, in Hz
        // public int rssi { get; set; }           // number: frame rssi, in dBm, as integer number
        // public float snr { get; set; }          // number: frame snr , in dB, one decimal place
        // public int toa { get; set; }
        // public string dr { get; set; }          // string: radio data rate - spreading factor, bandwidth and coding rate (Example: 'SF8 BW125 4/5')
        // public bool ack { get; set; }           // boolean: acknowledgement flag set by device
        // public byte bat { get; set; }           // number: device battery status, response of the DevStatusReq LoRaWAN MAC command
        // public bool offline { get; set; }
        public string data { get; set; }        // string: decrypted data payload as hexadecimal string
    }

    class LoriotMessageDownlink
    {
        public string cmd { get; set; }         // string: identifies type of message, always 'tx' for downlink messages
        public string eui { get; set; }         // string: device EUI, 16 hex digits (without dashes)
        public int port { get; set; }           // number: port number (1 to 223)
        public bool confirmed { get; set; }     // boolean(optional): request confirmation (ACK) from end device
        public string data { get; set; }        // data payload (to be encrypted by our server) as a hexadecimal string, minimum two hex characters (equivalent of one byte)
    }

    class LoriotSendReqAck
    {
        public string cmd { get; set; }         // string: identifies type of message, always 'tx' for this type of message
        public string eui { get; set; }         // string: device EUI, 16 hex digits (without dashes)
        public string sucess { get; set; }      // string (optional): If command succeeded, it will contain a confirmation message
        public string error { get; set; }       // string (optional): If command failed, it will report the error description
        public string data { get; set; }        // string (optional): Data that was enqueued (either plaintext or ciphertext), only present on sucess.
    }

    class LoriotDownlinkConfirmation
    {
        public string cmd { get; set; }         // string: identifies type of message, always 'tx' for this type of message
        public string eui { get; set; }         // string: device EUI, 16 hex digits (without dashes)
        public int seqdn { get; set; }          // number: FCnt used for the downlink
        public int ts { get; set; }             // number: Unix timestamp, moment of the transfer to gateway
    }
}
