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

    /// <summary>
    /// Exte
    /// </summary>

    public static partial class EnvironmentPlus
    {
        #if NETCOREAPP

        /// <summary>
        /// Number of miliseconds since system started (always >= 0)
        /// </summary>
        public static long TickCount64 { get { return System.Environment.TickCount64; } }

        #else

        private static long GetTickCount64()
        {
            try
            {
                var TheType = typeof(Environment);
                var TheProp = TheType.GetProperty("TickCount64");
                var TheValObj = TheProp.GetValue(null); // static
                long? TheVal = TheValObj as long?;
                if ((TheVal.HasValue) && (TheVal.Value >= 0))
                {
                    return TheVal.Value;
                }
                else
                {
                    return (Environment.TickCount & Int32.MaxValue); // remove sign
                }
            }
            catch
            {
                return (Environment.TickCount & Int32.MaxValue); // remove sign
            }
        }

        /// <summary>
        /// Number of miliseconds since system started (always >= 0)
        /// </summary>
        public static long TickCount64 { get { return GetTickCount64(); } }

        #endif
    }
}
