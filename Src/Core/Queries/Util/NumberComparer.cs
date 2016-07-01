
namespace Alachisoft.NoSQL.Core.Storage.Queries.Util
{
    public static class NumberComparer
    {
        //int
        public static int Compare(int num1, int num2)
        {
            var result = num1 - num2;

            if (result == 0)
                return 0;

            if (result < 0)
                return -1;
            
            return 1;
        }

        public static int Compare(int num1, long num2)
        {
            var result = num1 - num2;

            if (result == 0)
                return 0;

            if (result < 0)
                return -1;

            return 1;
        }

        public static int Compare(int num1, double num2)
        {
            var result = num1 - num2;

            if (result == 0)
                return 0;

            if (result < 0)
                return -1;

            return 1;
        }

        public static int Compare(int num1, decimal num2)
        {
            var result = num1 - num2;

            if (result == 0)
                return 0;

            if (result < 0)
                return -1;

            return 1;
        }
       
        public static int Compare(int num1, float num2)
        {
            var result = num1 - num2;

            if (result == 0)
                return 0;

            if (result < 0)
                return -1;

            return 1;
        }

        public static int Compare(int num1, short num2)
        {
            var result = num1 - num2;

            if (result == 0)
                return 0;

            if (result < 0)
                return -1;

            return 1;
        }

        public static int Compare(int num1, byte num2)
        {
            var result = num1 - num2;

            if (result == 0)
                return 0;

            if (result < 0)
                return -1;

            return 1;
        }

        public static int Compare(int num1, sbyte num2)
        {
            var result = num1 - num2;

            if (result == 0)
                return 0;

            if (result < 0)
                return -1;

            return 1;
        }

        public static int Compare(int num1, uint num2)
        {
            var result = num1 - num2;

            if (result == 0)
                return 0;

            if (result < 0)
                return -1;

            return 1;
        }

        public static int Compare(int num1, ulong num2)
        {
            var result = num1 - (decimal)num2;

            if (result == 0)
                return 0;

            if (result < 0)
                return -1;

            return 1;
        }

        public static int Compare(int num1, ushort num2)
        {
            var result = num1 - num2;

            if (result == 0)
                return 0;

            if (result < 0)
                return -1;

            return 1;
        }

        //long
        public static int Compare(long num1, int num2)
        {
            var result = num1 - num2;

            if (result == 0)
                return 0;

            if (result < 0)
                return -1;

            return 1;
        }

        public static int Compare(long num1, long num2)
        {
            var result = num1 - num2;

            if (result == 0)
                return 0;

            if (result < 0)
                return -1;

            return 1;
        }

        public static int Compare(long num1, double num2)
        {
            var result = num1 - num2;

            if (result == 0)
                return 0;

            if (result < 0)
                return -1;

            return 1;
        }

        public static int Compare(long num1, decimal num2)
        {
            var result = num1 - num2;

            if (result == 0)
                return 0;

            if (result < 0)
                return -1;

            return 1;
        }

        public static int Compare(long num1, float num2)
        {
            var result = num1 - num2;

            if (result == 0)
                return 0;

            if (result < 0)
                return -1;

            return 1;
        }

        public static int Compare(long num1, short num2)
        {
            var result = num1 - num2;

            if (result == 0)
                return 0;

            if (result < 0)
                return -1;

            return 1;
        }

        public static int Compare(long num1, byte num2)
        {
            var result = num1 - num2;

            if (result == 0)
                return 0;

            if (result < 0)
                return -1;

            return 1;
        }

        public static int Compare(long num1, sbyte num2)
        {
            var result = num1 - num2;

            if (result == 0)
                return 0;

            if (result < 0)
                return -1;

            return 1;
        }

        public static int Compare(long num1, uint num2)
        {
            var result = num1 - num2;

            if (result == 0)
                return 0;

            if (result < 0)
                return -1;

            return 1;
        }

        public static int Compare(long num1, ulong num2)
        {
            var result = num1 - (long)num2;

            if (result == 0)
                return 0;

            if (result < 0)
                return -1;

            return 1;
        }

        public static int Compare(long num1, ushort num2)
        {
            var result = num1 - num2;

            if (result == 0)
                return 0;

            if (result < 0)
                return -1;

            return 1;
        }

        //double
        public static int Compare(double num1, int num2)
        {
            var result = num1 - num2;

            if (result == 0)
                return 0;

            if (result < 0)
                return -1;

            return 1;
        }

        public static int Compare(double num1, long num2)
        {
            var result = num1 - num2;

            if (result == 0)
                return 0;

            if (result < 0)
                return -1;

            return 1;
        }

        public static int Compare(double num1, double num2)
        {
            var result = num1 - num2;

            if (result == 0)
                return 0;

            if (result < 0)
                return -1;

            return 1;
        }

        public static int Compare(double num1, decimal num2)
        {
            var result = (decimal)num1 - num2;

            if (result == 0)
                return 0;

            if (result < 0)
                return -1;

            return 1;
        }

        public static int Compare(double num1, float num2)
        {
            var result = num1 - num2;

            if (result == 0)
                return 0;

            if (result < 0)
                return -1;

            return 1;
        }

        public static int Compare(double num1, short num2)
        {
            var result = num1 - num2;

            if (result == 0)
                return 0;

            if (result < 0)
                return -1;

            return 1;
        }

        public static int Compare(double num1, byte num2)
        {
            var result = num1 - num2;

            if (result == 0)
                return 0;

            if (result < 0)
                return -1;

            return 1;
        }

        public static int Compare(double num1, sbyte num2)
        {
            var result = num1 - num2;

            if (result == 0)
                return 0;

            if (result < 0)
                return -1;

            return 1;
        }

        public static int Compare(double num1, uint num2)
        {
            var result = num1 - num2;

            if (result == 0)
                return 0;

            if (result < 0)
                return -1;

            return 1;
        }

        public static int Compare(double num1, ulong num2)
        {
            var result = num1 - (long)num2;

            if (result == 0)
                return 0;

            if (result < 0)
                return -1;

            return 1;
        }

        public static int Compare(double num1, ushort num2)
        {
            var result = num1 - num2;

            if (result == 0)
                return 0;

            if (result < 0)
                return -1;

            return 1;
        }

        //decimal
        public static int Compare(decimal num1, int num2)
        {
            var result = num1 - num2;

            if (result == 0)
                return 0;

            if (result < 0)
                return -1;

            return 1;
        }

        public static int Compare(decimal num1, long num2)
        {
            var result = num1 - num2;

            if (result == 0)
                return 0;

            if (result < 0)
                return -1;

            return 1;
        }

        public static int Compare(decimal num1, double num2)
        {
            var result = num1 - (decimal)num2;

            if (result == 0)
                return 0;

            if (result < 0)
                return -1;

            return 1;
        }

        public static int Compare(decimal num1, decimal num2)
        {
            var result = (decimal)num1 - num2;

            if (result == 0)
                return 0;

            if (result < 0)
                return -1;

            return 1;
        }

        public static int Compare(decimal num1, float num2)
        {
            var result = num1 - (decimal)num2;

            if (result == 0)
                return 0;

            if (result < 0)
                return -1;

            return 1;
        }

        public static int Compare(decimal num1, short num2)
        {
            var result = num1 - num2;

            if (result == 0)
                return 0;

            if (result < 0)
                return -1;

            return 1;
        }

        public static int Compare(decimal num1, byte num2)
        {
            var result = num1 - num2;

            if (result == 0)
                return 0;

            if (result < 0)
                return -1;

            return 1;
        }

        public static int Compare(decimal num1, sbyte num2)
        {
            var result = num1 - num2;

            if (result == 0)
                return 0;

            if (result < 0)
                return -1;

            return 1;
        }

        public static int Compare(decimal num1, uint num2)
        {
            var result = num1 - num2;

            if (result == 0)
                return 0;

            if (result < 0)
                return -1;

            return 1;
        }

        public static int Compare(decimal num1, ulong num2)
        {
            var result = num1 - (long)num2;

            if (result == 0)
                return 0;

            if (result < 0)
                return -1;

            return 1;
        }

        public static int Compare(decimal num1, ushort num2)
        {
            var result = num1 - num2;

            if (result == 0)
                return 0;

            if (result < 0)
                return -1;

            return 1;
        }

        //float
        public static int Compare(float num1, int num2)
        {
            var result = num1 - num2;

            if (result == 0)
                return 0;

            if (result < 0)
                return -1;

            return 1;
        }

        public static int Compare(float num1, long num2)
        {
            var result = num1 - num2;

            if (result == 0)
                return 0;

            if (result < 0)
                return -1;

            return 1;
        }

        public static int Compare(float num1, double num2)
        {
            var result = num1 -num2;

            if (result == 0)
                return 0;

            if (result < 0)
                return -1;

            return 1;
        }

        public static int Compare(float num1, decimal num2)
        {
            var result = (decimal)num1 - num2;

            if (result == 0)
                return 0;

            if (result < 0)
                return -1;

            return 1;
        }

        public static int Compare(float num1, float num2)
        {
            var result = num1 - num2;

            if (result == 0)
                return 0;

            if (result < 0)
                return -1;

            return 1;
        }

        public static int Compare(float num1, short num2)
        {
            var result = num1 - num2;

            if (result == 0)
                return 0;

            if (result < 0)
                return -1;

            return 1;
        }

        public static int Compare(float num1, byte num2)
        {
            var result = num1 - num2;

            if (result == 0)
                return 0;

            if (result < 0)
                return -1;

            return 1;
        }

        public static int Compare(float num1, sbyte num2)
        {
            var result = num1 - num2;

            if (result == 0)
                return 0;

            if (result < 0)
                return -1;

            return 1;
        }

        public static int Compare(float num1, uint num2)
        {
            var result = num1 - num2;

            if (result == 0)
                return 0;

            if (result < 0)
                return -1;

            return 1;
        }

        public static int Compare(float num1, ulong num2)
        {
            var result = num1 - (long)num2;

            if (result == 0)
                return 0;

            if (result < 0)
                return -1;

            return 1;
        }

        public static int Compare(float num1, ushort num2)
        {
            var result = num1 - num2;

            if (result == 0)
                return 0;

            if (result < 0)
                return -1;

            return 1;
        }

        //short
        public static int Compare(short num1, int num2)
        {
            var result = num1 - num2;

            if (result == 0)
                return 0;

            if (result < 0)
                return -1;

            return 1;
        }

        public static int Compare(short num1, long num2)
        {
            var result = num1 - num2;

            if (result == 0)
                return 0;

            if (result < 0)
                return -1;

            return 1;
        }

        public static int Compare(short num1, double num2)
        {
            var result = num1 - num2;

            if (result == 0)
                return 0;

            if (result < 0)
                return -1;

            return 1;
        }

        public static int Compare(short num1, decimal num2)
        {
            var result = (decimal)num1 - num2;

            if (result == 0)
                return 0;

            if (result < 0)
                return -1;

            return 1;
        }

        public static int Compare(short num1, float num2)
        {
            var result = num1 - num2;

            if (result == 0)
                return 0;

            if (result < 0)
                return -1;

            return 1;
        }

        public static int Compare(short num1, short num2)
        {
            var result = num1 - num2;

            if (result == 0)
                return 0;

            if (result < 0)
                return -1;

            return 1;
        }

        public static int Compare(short num1, byte num2)
        {
            var result = num1 - num2;

            if (result == 0)
                return 0;

            if (result < 0)
                return -1;

            return 1;
        }

        public static int Compare(short num1, sbyte num2)
        {
            var result = num1 - num2;

            if (result == 0)
                return 0;

            if (result < 0)
                return -1;

            return 1;
        }

        public static int Compare(short num1, uint num2)
        {
            var result = num1 - num2;

            if (result == 0)
                return 0;

            if (result < 0)
                return -1;

            return 1;
        }

        public static int Compare(short num1, ulong num2)
        {
            var result = num1 - (long)num2;

            if (result == 0)
                return 0;

            if (result < 0)
                return -1;

            return 1;
        }

        public static int Compare(short num1, ushort num2)
        {
            var result = num1 - num2;

            if (result == 0)
                return 0;

            if (result < 0)
                return -1;

            return 1;
        }

        //byte
        public static int Compare(byte num1, int num2)
        {
            var result = num1 - num2;

            if (result == 0)
                return 0;

            if (result < 0)
                return -1;

            return 1;
        }

        public static int Compare(byte num1, long num2)
        {
            var result = num1 - num2;

            if (result == 0)
                return 0;

            if (result < 0)
                return -1;

            return 1;
        }

        public static int Compare(byte num1, double num2)
        {
            var result = num1 - num2;

            if (result == 0)
                return 0;

            if (result < 0)
                return -1;

            return 1;
        }

        public static int Compare(byte num1, decimal num2)
        {
            var result = (decimal)num1 - num2;

            if (result == 0)
                return 0;

            if (result < 0)
                return -1;

            return 1;
        }

        public static int Compare(byte num1, float num2)
        {
            var result = num1 - num2;

            if (result == 0)
                return 0;

            if (result < 0)
                return -1;

            return 1;
        }

        public static int Compare(byte num1, short num2)
        {
            var result = num1 - num2;

            if (result == 0)
                return 0;

            if (result < 0)
                return -1;

            return 1;
        }

        public static int Compare(byte num1, byte num2)
        {
            var result = num1 - num2;

            if (result == 0)
                return 0;

            if (result < 0)
                return -1;

            return 1;
        }

        public static int Compare(byte num1, sbyte num2)
        {
            var result = num1 - num2;

            if (result == 0)
                return 0;

            if (result < 0)
                return -1;

            return 1;
        }

        public static int Compare(byte num1, uint num2)
        {
            var result = num1 - num2;

            if (result == 0)
                return 0;

            if (result < 0)
                return -1;

            return 1;
        }

        public static int Compare(byte num1, ulong num2)
        {
            var result = num1 - (long)num2;

            if (result == 0)
                return 0;

            if (result < 0)
                return -1;

            return 1;
        }

        public static int Compare(byte num1, ushort num2)
        {
            var result = num1 - num2;

            if (result == 0)
                return 0;

            if (result < 0)
                return -1;

            return 1;
        }


        //sbyte
        public static int Compare(sbyte num1, int num2)
        {
            var result = num1 - num2;

            if (result == 0)
                return 0;

            if (result < 0)
                return -1;

            return 1;
        }

        public static int Compare(sbyte num1, long num2)
        {
            var result = num1 - num2;

            if (result == 0)
                return 0;

            if (result < 0)
                return -1;

            return 1;
        }

        public static int Compare(sbyte num1, double num2)
        {
            var result = num1 - num2;

            if (result == 0)
                return 0;

            if (result < 0)
                return -1;

            return 1;
        }

        public static int Compare(sbyte num1, decimal num2)
        {
            var result = (decimal)num1 - num2;

            if (result == 0)
                return 0;

            if (result < 0)
                return -1;

            return 1;
        }

        public static int Compare(sbyte num1, float num2)
        {
            var result = num1 - num2;

            if (result == 0)
                return 0;

            if (result < 0)
                return -1;

            return 1;
        }

        public static int Compare(sbyte num1, short num2)
        {
            var result = num1 - num2;

            if (result == 0)
                return 0;

            if (result < 0)
                return -1;

            return 1;
        }

        public static int Compare(sbyte num1, byte num2)
        {
            var result = num1 - num2;

            if (result == 0)
                return 0;

            if (result < 0)
                return -1;

            return 1;
        }

        public static int Compare(sbyte num1, sbyte num2)
        {
            var result = num1 - num2;

            if (result == 0)
                return 0;

            if (result < 0)
                return -1;

            return 1;
        }

        public static int Compare(sbyte num1, uint num2)
        {
            var result = num1 - num2;

            if (result == 0)
                return 0;

            if (result < 0)
                return -1;

            return 1;
        }

        public static int Compare(sbyte num1, ulong num2)
        {
            var result = num1 - (long)num2;

            if (result == 0)
                return 0;

            if (result < 0)
                return -1;

            return 1;
        }

        public static int Compare(sbyte num1, ushort num2)
        {
            var result = num1 - num2;

            if (result == 0)
                return 0;

            if (result < 0)
                return -1;

            return 1;
        }

        //uint
        public static int Compare(uint num1, int num2)
        {
            var result = num1 - num2;

            if (result == 0)
                return 0;

            if (result < 0)
                return -1;

            return 1;
        }

        public static int Compare(uint num1, long num2)
        {
            var result = num1 - num2;

            if (result == 0)
                return 0;

            if (result < 0)
                return -1;

            return 1;
        }

        public static int Compare(uint num1, double num2)
        {
            var result = num1 - num2;

            if (result == 0)
                return 0;

            if (result < 0)
                return -1;

            return 1;
        }

        public static int Compare(uint num1, decimal num2)
        {
            var result = (decimal)num1 - num2;

            if (result == 0)
                return 0;

            if (result < 0)
                return -1;

            return 1;
        }

        public static int Compare(uint num1, float num2)
        {
            var result = num1 - num2;

            if (result == 0)
                return 0;

            if (result < 0)
                return -1;

            return 1;
        }

        public static int Compare(uint num1, short num2)
        {
            var result = num1 - num2;

            if (result == 0)
                return 0;

            if (result < 0)
                return -1;

            return 1;
        }

        public static int Compare(uint num1, byte num2)
        {
            var result = num1 - num2;

            if (result == 0)
                return 0;

            if (result < 0)
                return -1;

            return 1;
        }

        public static int Compare(uint num1, sbyte num2)
        {
            var result = num1 - num2;

            if (result == 0)
                return 0;

            if (result < 0)
                return -1;

            return 1;
        }

        public static int Compare(uint num1, uint num2)
        {
            var result = num1 - num2;

            if (result == 0)
                return 0;

            if (result < 0)
                return -1;

            return 1;
        }

        public static int Compare(uint num1, ulong num2)
        {
            var result = num1 - (long)num2;

            if (result == 0)
                return 0;

            if (result < 0)
                return -1;

            return 1;
        }

        public static int Compare(uint num1, ushort num2)
        {
            var result = num1 - num2;

            if (result == 0)
                return 0;

            if (result < 0)
                return -1;

            return 1;
        }


        //ulong
        public static int Compare(ulong num1, int num2)
        {
            var result = (decimal)num1 - num2;

            if (result == 0)
                return 0;

            if (result < 0)
                return -1;

            return 1;
        }

        public static int Compare(ulong num1, long num2)
        {
            var result = (decimal)num1 - num2;

            if (result == 0)
                return 0;

            if (result < 0)
                return -1;

            return 1;
        }

        public static int Compare(ulong num1, double num2)
        {
            var result = num1 - num2;

            if (result == 0)
                return 0;

            if (result < 0)
                return -1;

            return 1;
        }

        public static int Compare(ulong num1, decimal num2)
        {
            var result = (decimal)num1 - num2;

            if (result == 0)
                return 0;

            if (result < 0)
                return -1;

            return 1;
        }

        public static int Compare(ulong num1, float num2)
        {
            var result = num1 - num2;

            if (result == 0)
                return 0;

            if (result < 0)
                return -1;

            return 1;
        }

        public static int Compare(ulong num1, short num2)
        {
            var result = (decimal)num1 - num2;

            if (result == 0)
                return 0;

            if (result < 0)
                return -1;

            return 1;
        }

        public static int Compare(ulong num1, byte num2)
        {
            var result = num1 - num2;

            if (result == 0)
                return 0;

            if (result < 0)
                return -1;

            return 1;
        }

        public static int Compare(ulong num1, sbyte num2)
        {
            var result = (decimal)num1 - num2;

            if (result == 0)
                return 0;

            if (result < 0)
                return -1;

            return 1;
        }

        public static int Compare(ulong num1, uint num2)
        {
            var result = num1 - num2;

            if (result == 0)
                return 0;

            if (result < 0)
                return -1;

            return 1;
        }

        public static int Compare(ulong num1, ulong num2)
        {
            var result = num1 - num2;

            if (result == 0)
                return 0;

            if (result < 0)
                return -1;

            return 1;
        }

        public static int Compare(ulong num1, ushort num2)
        {
            var result = num1 - num2;

            if (result == 0)
                return 0;

            if (result < 0)
                return -1;

            return 1;
        }

        //ushort
        public static int Compare(ushort num1, int num2)
        {
            var result = num1 - num2;

            if (result == 0)
                return 0;

            if (result < 0)
                return -1;

            return 1;
        }

        public static int Compare(ushort num1, long num2)
        {
            var result = num1 - num2;

            if (result == 0)
                return 0;

            if (result < 0)
                return -1;

            return 1;
        }

        public static int Compare(ushort num1, double num2)
        {
            var result = num1 - num2;

            if (result == 0)
                return 0;

            if (result < 0)
                return -1;

            return 1;
        }

        public static int Compare(ushort num1, decimal num2)
        {
            var result = (decimal)num1 - num2;

            if (result == 0)
                return 0;

            if (result < 0)
                return -1;

            return 1;
        }

        public static int Compare(ushort num1, float num2)
        {
            var result = num1 - num2;

            if (result == 0)
                return 0;

            if (result < 0)
                return -1;

            return 1;
        }

        public static int Compare(ushort num1, short num2)
        {
            var result = num1 - num2;

            if (result == 0)
                return 0;

            if (result < 0)
                return -1;

            return 1;
        }

        public static int Compare(ushort num1, byte num2)
        {
            var result = num1 - num2;

            if (result == 0)
                return 0;

            if (result < 0)
                return -1;

            return 1;
        }

        public static int Compare(ushort num1, sbyte num2)
        {
            var result = num1 - num2;

            if (result == 0)
                return 0;

            if (result < 0)
                return -1;

            return 1;
        }

        public static int Compare(ushort num1, uint num2)
        {
            var result = num1 - num2;

            if (result == 0)
                return 0;

            if (result < 0)
                return -1;

            return 1;
        }

        public static int Compare(ushort num1, ulong num2)
        {
            var result = num1 - (long)num2;

            if (result == 0)
                return 0;

            if (result < 0)
                return -1;

            return 1;
        }

        public static int Compare(ushort num1, ushort num2)
        {
            var result = num1 - num2;

            if (result == 0)
                return 0;

            if (result < 0)
                return -1;

            return 1;
        }
    }
}
