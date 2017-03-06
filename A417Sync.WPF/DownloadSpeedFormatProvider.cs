namespace A417Sync.Client
{
    using System;

    class DownloadSpeedFormatProvider : IFormatProvider, ICustomFormatter
    {
        private const string speedFormat = "sp";

        public string Format(string format, object arg, IFormatProvider formatProvider)
        {
            if (format == null || !format.StartsWith(speedFormat))
            {
                return defaultFormat(format, arg, formatProvider);
            }

            if (arg is string)
            {
                return defaultFormat(format, arg, formatProvider);
            }

            decimal rate;

            try
            {
                rate = Convert.ToDecimal(arg);
            }
            catch (InvalidCastException)
            {
                return defaultFormat(format, arg, formatProvider);
            }

            var ordinals = new[] { string.Empty, "K", "M", "G", "T", "P", "E" };

            var ordinal = 0;

            while (rate >= 1024)
            {
                rate /= 1024;
                ordinal++;
            }

            var suffix = ordinals[ordinal];

            string precision = format.Substring(2);
            if (string.IsNullOrEmpty(precision))
            {
                precision = "2";
            }

            return string.Format("{0:N" + precision + "}{1}B/s", rate, suffix);
        }

        public object GetFormat(Type formatType)
        {
            if (formatType == typeof(ICustomFormatter)) return this;
            return null;
        }

        private static string defaultFormat(string format, object arg, IFormatProvider formatProvider)
        {
            IFormattable formattableArg = arg as IFormattable;
            if (formattableArg != null)
            {
                return formattableArg.ToString(format, formatProvider);
            }

            return arg.ToString();
        }
    }
}