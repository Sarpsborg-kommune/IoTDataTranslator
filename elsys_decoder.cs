using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;

namespace sarpsborgkommune.iot
{
    // See https://www.elsys.se/en/elsys-payload/
    public enum ElsysMeasurementType : byte
    {
        TEMP = 0x01,   // Temp 2 bytes -3276.8°C --> 3276.7°C
        RH = 0x02,   // Humidity 1 byte 0-100%
        ACC = 0x03,   // Acceleration 3 bytes X,Y,Z -127 --> 127 +/-63=1G
        LIGHT = 0x04,   // Light 2 bytes 0-->65535 Lux
        MOTION = 0x05,   // No of motion 1 byte 0-255
        CO2 = 0x06,   // CO₂ 2 bytes 0-65535 ppm
        VDD = 0x07,   // VDD(Battery Level) 2 bytes 0-65535mV
        ANALOG1 = 0x08,   // VDD 2 bytes 0-65535mV
        GPS = 0x09,   // 3 bytes lat 3 bytes long binary
        PULSE1 = 0x0A,   // 2 bytes relative pulse count
        PULSE1_ABS = 0x0B,   // 4 bytes no 0->0xFFFFFFFF
        EXT_TEMP1 = 0x0C,   // 2 bytes -3276.5°C-->3276.5°C
        EXT_DIGITAL = 0x0D,   // 1 bytes value 1 or 0
        EXT_DISTANCE = 0x0E,   // 2 bytes distance in mm
        ACC_MOTION = 0x0F,   // 1 byte number of vibration/motion
        IR_TEMP = 0x10,   // 2 bytes internal temp 2 bytes external temp -3276.5°C-->3276.5°C
        OCCUPANCY = 0x11,   // 1 byte data
        WATERLEAK = 0x12,   // 1 byte data 0-255
        GRIDEYE = 0x13,   // 65 byte temperature data 1 byte ref+64byte external temp
        PRESSURE = 0x14,   // 4 byte pressure data (hPa)
        SOUND = 0x15,   // 2 byte sound data (peak/avg)
        PULSE2 = 0x16,   // 2 bytes 0-->0xFFFF
        PULSE2_ABS = 0x17,   // 4 bytes no 0->0xFFFFFFFF
        ANALOG2 = 0x18,   // 2 bytes voltage in mV
        EXT_TEMP2 = 0x19,   // 2 bytes -3276.5°C-->3275.5°C
        EXT_DIGITAL2 = 0x1A,   // 1 bytes value 1 or 0
        EXT_ANALOG_UV = 0x1B,   // 4 bytes signd int (uV)
        DEBUG = 0x3D    // 4 bytes debug
    }

    public struct Acc
    {
        public int? x;
        public int? y;
        public int? z; 
    }

    public struct Gps
    {
        public int? lat;
        public int? lon; 
    }

    public class ElsysMessage
    {
        public float? temp { get; set; }
        public int? rh { get; set; }
        public Acc acc;
        public int? light { get; set; }
        public int? motion { get; set; }
        public int? co2 { get; set; }
        public int? vdd { get; set; }
        public int? analog1 { get; set; }
        public Gps gps { get; set; }
        public int? pulse { get; set; }
        public int? pulse1_abs { get; set; }
        public float? ext_temp1 { get; set; }
        public bool? ext_digital { get; set; }
        public int? ext_distance { get; set; }
        public int? acc_motion { get; set; }
        public float? ir_temp { get; set; }
        public int? occupancy { get; set; }
        public int? waterleak { get; set; }
        public byte[] grideye { get; set; }
        public int? pressure { get; set; }
        public int? sound { get; set; }
        public int? pulse2 { get; set; }
        public int? pulse2_abs { get; set; }
        public int? analog2 { get; set; }
        public float? ext_temp2 { get; set; }
        public bool? ext_digital2 { get; set; }
        public int? ext_analog_uv { get; set; }
        public byte[] debug { get; set; }

        public ElsysMessage(byte[] data)
        {
            for (int counter = 0; counter < data.Length; counter ++)
            {
                switch(data[counter])
                {
                    case (byte)ElsysMeasurementType.TEMP:
                        int temp = helperfunctions.Bin16Dec(data[counter + 1], data[counter + 2]);
                        this.temp = ((float)temp / 10);
                        counter += 2;
                        break;
                    case (byte)ElsysMeasurementType.RH:
                        this.rh = ((int)data[counter + 1]);
                        counter += 1;
                        break;
                    case (byte)ElsysMeasurementType.ACC:
                        this.acc.x = helperfunctions.Bin8Dec(data[counter + 1]);
                        this.acc.y = helperfunctions.Bin8Dec(data[counter + 2]);
                        this.acc.z = helperfunctions.Bin8Dec(data[counter + 3]);
                        counter += 3;
                        break;
                    case (byte)ElsysMeasurementType.LIGHT:
                        this.light = helperfunctions.Bin16Dec(data[counter + 1], data[counter + 2]);
                        counter += 2;
                        break;
                    case (byte)ElsysMeasurementType.MOTION:
                        this.motion = helperfunctions.Bin8Dec(data[counter + 1]);
                        counter += 1;
                        break;
                    case (byte)ElsysMeasurementType.CO2:
                        this.co2 = helperfunctions.Bin16Dec(data[counter + 1], data[counter + 2]);
                        counter += 2;
                        break;
                    case (byte)ElsysMeasurementType.VDD:
                        this.vdd = helperfunctions.Bin16Dec(data[counter + 1], data[counter + 2]);
                        counter += 2;
                        break;
                    case (byte)ElsysMeasurementType.ANALOG1:
                        this.analog1 = helperfunctions.Bin16Dec(data[counter + 1], data[counter + 2]);
                        counter += 2;
                        break;
                    case (byte)ElsysMeasurementType.GPS:
                        break;
                    case (byte)ElsysMeasurementType.PULSE1:
                        break;
                    case (byte)ElsysMeasurementType.PULSE1_ABS:
                        break;
                    case (byte)ElsysMeasurementType.EXT_TEMP1:
                        break;
                    case (byte)ElsysMeasurementType.EXT_DIGITAL:
                        break;
                    case (byte)ElsysMeasurementType.EXT_DISTANCE:
                        break;
                    case (byte)ElsysMeasurementType.ACC_MOTION:
                        break;
                    case (byte)ElsysMeasurementType.IR_TEMP:
                        break;
                    case (byte)ElsysMeasurementType.OCCUPANCY:
                        break;
                    case (byte)ElsysMeasurementType.WATERLEAK:
                        break;
                    case (byte)ElsysMeasurementType.GRIDEYE:
                        break;
                    case (byte)ElsysMeasurementType.PRESSURE:
                        break;
                    case (byte)ElsysMeasurementType.SOUND:
                        break;
                    case (byte)ElsysMeasurementType.PULSE2:
                        break;
                    case (byte)ElsysMeasurementType.PULSE2_ABS:
                        break;
                    case (byte)ElsysMeasurementType.ANALOG2:
                        break;
                    case (byte)ElsysMeasurementType.EXT_TEMP2:
                        break;
                    case (byte)ElsysMeasurementType.EXT_DIGITAL2:
                        break;
                    default:
                        break;
                }
            }
        }

        public override string ToString()
        {
            string output = "";

            if (!(this.temp is null)) output += $"Temperature: {this.temp}°C\n";
            if (!(this.rh is null)) output += $"Humidity: {this.rh}%\n";
            if (!(this.acc.x is null | this.acc.y is null | this.acc.x is null)) output += $"Acceleration: {this.acc.x},{this.acc.y},{this.acc.z}\n";
            if (!(this.light is null)) output += $"Light: {this.light} Lux\n";
            if (!(this.motion is null)) output += $"Motion: {this.motion}\n";
            if (!(this.co2 is null)) output += $"CO₂: {this.co2} ppm\n";
            if (!(this.vdd is null)) output += $"Battery Level: {this.vdd} mV\n";
            if (!(this.analog1 is null)) output += $"Analog1: {this.vdd} mV\n";
            return output;
        }
    }
}
