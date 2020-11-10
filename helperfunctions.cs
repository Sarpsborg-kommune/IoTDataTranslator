using System.Linq.Expressions;

namespace sarpsborgkommune.iot
{
    public static class helperfunctions
    {
        public static int Bin8Dec(byte b1)
        {
            int number = b1;
            if (number > 128) number = -(256 - number);
            else if (number == 128) number = 0;

            return number;
        }

        public static int Bin16Dec(byte b1, byte b2)
        {
            int number = (b1 * 256) + b2;
            if (number > 32768) number = -(65535 - number);
            else if (number == 32768) number = 0;

            return number;
        }

        public static byte[] StringToByteArray(string hex)
        {
            if (hex.Length % 2 == 1)
                throw new System.Exception("The output string cannot have an odd number of characters.");

            byte[] arr = new byte[hex.Length >> 1];

            for (int i = 0; i < hex.Length >> 1; ++i)
                arr[i] = (byte)((GetHexVal(hex[i << 1]) << 4) + (GetHexVal(hex[(i << 1) + 1])));

            return arr;
        }

        public static int GetHexVal(char hex)
        {
            int val = (int)hex;

            // Uppercase A-F letters: return val - (val < 58 ? 48 : 55);
            // Lowercase a-f letters: return val - (val < 58 ? 48 : 87);
            return val - (val < 58 ? 48 : (val < 97 ? 55 : 87));
        }
    }

}