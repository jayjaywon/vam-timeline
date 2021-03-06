using System;
using System.Runtime.CompilerServices;

namespace VamTimeline
{
    public static class FloatExtensions
    {
        [MethodImpl(256)]
        public static bool IsSameFrame(this float value, float time)
        {
            return value > time - 0.0005f && value < time + 0.0005f;
        }

        [MethodImpl(256)]
        public static float Snap(this float value, float range = 0f)
        {
            value = (float)(Math.Round(value * 1000f) / 1000f);

            if (value < 0f)
                value = 0f;

            if (range > 0f)
            {
                var snapDelta = value % range;
                if (snapDelta != 0f)
                {
                    value -= snapDelta;
                    if (snapDelta > range / 2f)
                        value += range;
                }
            }

            return value;
        }

        [MethodImpl(256)]
        public static int ToMilliseconds(this float value)
        {
            return (int)(Math.Round(value * 1000f));
        }
    }
}
