namespace A417Sync.Client
{
    using System;

    public class DynamicTimeSpanFormatProvider : IFormatProvider, ICustomFormatter
    {
        private const string fileSizeFormat = "ts";

        public string Format(string format, object arg, IFormatProvider formatProvider)
        {
            if (format == null || !format.StartsWith(fileSizeFormat))
            {
                return defaultFormat(format, arg, formatProvider);
            }

            var span = (TimeSpan)arg;

            if (span.Days > 0)
            {
                return $"{span.TotalDays:N1} day(s)";
            }

            if (span.Hours > 0)
            {
                return $"{span.TotalHours:N1} hour(s)";
            }

            if (span.Minutes > 0)
            {
                return $"{span.TotalMinutes:N1} minute(s)";
            }

            return $"{span.TotalSeconds:N1} second(s)";
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