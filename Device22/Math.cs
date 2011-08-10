using System;
using System.Collections.Generic;
using System.Text;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Device22
{
    struct Earth
    {
        public static float Gravity = 9.81f;
    }

    public class Math2
    {
        private static Random rand = new Random();

        public static double randomNumber(double min, double max)
        {
            return (max - min) * rand.NextDouble() + min;
        }

        public static bool IsPowerOfTwo(ulong x)
        {
            return (x != 0) && ((x & (x - 1)) == 0);
        }

        public static bool IsPowerOfTwo(int x)
        {
            return (x != 0) && ((x & (x - 1)) == 0);
        }

        public static double degToRad(double degree)
        {
            return (double)((degree) * (Math.PI / 180.0d));
        }

        public static double radToDeg(double radian)
        {
            return (double)((radian) * (180.0d / Math.PI));
        }

        public static float normalize(float val, float minRange, float maxRange)
        {
            return ((val - minRange) / (maxRange - minRange));
        }
    }
}
