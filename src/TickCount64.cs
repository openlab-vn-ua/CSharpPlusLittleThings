namespace OpenLab.Plus
{
    /// <summary>
    /// Portable TickCount64 provider for all platforms
    /// (Uses TickCount64 if it available or fallback to TickCount if not)
    /// Returns only non-negatibe values, as you might expect from TickCount64
    /// </summary>
    /// <remarks>Open source software with MIT license</remarks>

    using System;
    using System.Reflection;

    public static partial class EnvironmentPlus
    {
        #if NETCOREAPP

        /// <summary>
        /// Number of miliseconds since system started (always >= 0) as long int
        /// </summary>
        public static long TickCount64 { get { return System.Environment.TickCount64; } }

        #else

        private static long CalcTickCount64ByInt32(int TickCount)
        {
            return (uint)(TickCount);
        }

        private static long GetTickCount64()
        {
            try
            {
                var TheType = typeof(Environment);
                var TheProp = TheType.GetProperty("TickCount64");

                if (TheProp == null)
                {
                    return CalcTickCount64ByInt32(Environment.TickCount); // Fallback
                }

                var TheValObj = TheProp.GetValue(null); // static
                long? TheVal = TheValObj as long?;

                if ((TheVal.HasValue) && (TheVal.Value >= 0))
                {
                    return TheVal.Value;
                }
                else
                {
                    return CalcTickCount64ByInt32(Environment.TickCount); // Fallback
                }
            }
            catch
            {
                return CalcTickCount64ByInt32(Environment.TickCount); // Fallback
            }
        }

        /// <summary>
        /// Number of miliseconds since system started (always >= 0) as long int.<br/>
        /// Internaly fallbacks to <c><see cref="Environment.TickCount" /></c> if TickCount64 is not available
        /// </summary>
        public static long TickCount64 { get { return GetTickCount64(); } }

        #endif
    }
}
