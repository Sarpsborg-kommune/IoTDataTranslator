/*
 * Every received message is translated to IoTMessage and sendt into the pipeline.
 *
 * Loriot: timeStamp is translated from ts (Linux Epoch)
 *         data is translated from original bytestring (data)
 *
*/

using System;

namespace Sarpsborgkommune.IoT.IoTDataTranslator
{
    class IoTMessage
    {
        public string id { get; set; }              // From received message
        public DateTime timeStamp { get; set; }     // From received message
        public string deviceType { get; set; }      // From device twin
        public string[] receivers { get; set; }     // From device twin
        public object data { get; set; }            // From received message

    }
}