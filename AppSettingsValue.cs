namespace OpenLab.Plus.Configuration
{
    using System;
    using System.ComponentModel;
    using System.Configuration;
    using System.Globalization;

    /// <summary>
    /// Configuration value from ConfigurationManager.AppSettings
    /// </summary>
    /// <remarks>Open source software with MIT license</remarks>

    public class AppSettingsValue<ItemType>
    {
        public static T ParseString<T>(string input)
        {
            var Converter = TypeDescriptor.GetConverter(typeof(T));
            if (Converter == null) { throw new ArgumentException(); }
            return (T)Converter.ConvertFromString(input);
        }

        public static T ParseString<T>(string input, CultureInfo culture)
        {
            var Converter = TypeDescriptor.GetConverter(typeof(T));
            if (Converter == null) { throw new ArgumentException(); }
            try
            {
                return (T)Converter.ConvertFromString(null, culture, input);
            }
            catch
            {
                return (T)Converter.ConvertFromString(input); // faillback to call without culture
            }
        }

        private Lazy<ItemType> SetupValue;

        public AppSettingsValue(string ItemName, ItemType ItemDefault = default(ItemType), Func<ItemType, bool> IsValid = null)
        {
            this.SetupValue = new Lazy<ItemType>(() =>
            {
                if (ItemName == null) { return ItemDefault; }

                var setupStr = ConfigurationManager.AppSettings[ItemName];

                if (string.IsNullOrEmpty(setupStr)) { return ItemDefault; }

                try
                {
                    var Value = ParseString<ItemType>(setupStr, CultureInfo.InvariantCulture);
                    if ((IsValid != null) && (!IsValid(Value))) { return ItemDefault; }
                    return Value;
                }
                catch
                {
                    return ItemDefault;
                }
            });
        }

        public ItemType Value => SetupValue.Value;
    }
}
