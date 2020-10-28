using System;
using System.Collections.Generic;
using System.Text;

namespace sarpsborgkommune.iot
{
    class ElsysDecoder : MessageDecoder
    {
        // See https://www.elsys.se/en/elsys-payload/
        private enum ElsysMeasurementType : byte
        {
            TEMP =          0x01,   // Temp 2 bytes -3276.8°C --> 3276.7°C
            RH =            0x02,   // Humidity 1 byte 0-100%
            ACC =           0x03,   // Acceleration 3 bytes X,Y,Z -127 --> 127 +/-63=1G
            LIGHT =         0x04,   // Light 2 bytes 0-->65535 Lux
            MOTION =        0x05,   // No of motion 1 byte 0-255
            CO2 =           0x06,   // CO₂ 2 bytes 0-65535 ppm
            VDD =           0x07,   // VDD 2 bytes 0-65535mV
            ANALOG1 =       0x08,   // VDD 2 bytes 0-65535mV
            GPS =           0x09,   // 3 bytes lat 3 bytes long binary
            PULSE1 =        0x0A,   // 2 bytes relative pulse count
            PULSE1_ABS =    0x0B,   // 4 bytes no 0->0xFFFFFFFF
            EXT_TEMP1 =     0x0C,   // 2 bytes -3276.5°C-->3276.5°C
            EXT_DIGITAL =   0x0D,   // 1 bytes value 1 or 0
            EXT_DISTANCE =  0x0E,   // 2 bytes distance in mm
            ACC_MOTION =    0x0F,   // 1 byte number of vibration/motion
            IR_TEMP =       0x10,   // 2 bytes internal temp 2 bytes external temp -3276.5°C-->3276.5°C
            OCCUPANCY =     0x11,   // 1 byte data
            WATERLEAK =     0x12,   // 1 byte data 0-255
            GRIDEYE =       0x13,   // 65 byte temperature data 1 byte ref+64byte external temp
            PRESSURE =      0x14,   // 4 byte pressure data (hPa)
            SOUND =         0x15,   // 2 byte sound data (peak/avg)
            PULSE2 =        0x16,   // 2 bytes 0-->0xFFFF
            PULSE2_ABS =    0x17,   // 4 bytes no 0->0xFFFFFFFF
            ANALOG2 =       0x18,   // 2 bytes voltage in mV
            EXT_TEMP2 =     0x19,   // 2 bytes -3276.5°C-->3275.5°C
            EXT_DIGITAL2 =  0x1A,   // 1 bytes value 1 or 0
            EXT_ANALOG_UV = 0x1B,   // 4 bytes signd int (uV)
            DEBUG =         0x3D    // 4 bytes debug
        }

        public override string Decode()
        {
            return "";
        }
    }
}
