using System;
using System.Collections.Generic;
using System.Text;

namespace HWFilters
{
    public static class MathUtils
    {
        public static int Clamp(int value, int min, int max)
        {
            if (value < min)
            {
                return min;
            }
            else if (value > max)
            {
                return max;
            }
            else
            {
                return value;
            }
        }
        public static float Clamp(float value, float min, float max)
        {
            if (value < min)
            {
                return min;
            }
            else if (value > max)
            {
                return max;
            }
            else
            {
                return value;
            }
        }
        public static double Clamp(double value, double min, double max)
        {
            if (value < min)
            {
                return min;
            }
            else if (value > max)
            {
                return max;
            }
            else
            {
                return value;
            }
        }

        public static float Lerp(float lowerBound, float upperBound, float t)
        {
            t = Clamp(t, 0, 1);
            return upperBound * t + lowerBound * (1 - t);
        }
        public static int Lerp(int lowerBound, int upperBound, float t)
        {
            t = Clamp(t, 0, 1);
            return (int)(upperBound * t) + (int)(lowerBound * (1 - t));
        }
        public static int Lerp(int lowerBound, int upperBound, double t)
        {
            t = Clamp(t, 0, 1);
            return (int)(upperBound * t) + (int)(lowerBound * (1 - t));
        }
        public static double Lerp(double lowerBound, double upperBound, double t)
        {
            t = Clamp(t, 0, 1);
            return (int)(upperBound * t) + (int)(lowerBound * (1 - t));
        }

        public static bool IsBetween(int lowerBound, int upperBound, int value)
        {
            return value > lowerBound && value < upperBound;
        }
        public static bool IsBetweenOrEqual(int lowerBound, int upperBound, int value)
        {
            return value >= lowerBound && value <= upperBound;
        }
    }
}
