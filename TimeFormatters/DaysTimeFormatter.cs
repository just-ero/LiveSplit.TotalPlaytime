using System;
using System.Globalization;

namespace LiveSplit.TimeFormatters
{
    public class DaysTimeFormatter : ITimeFormatter
    {
        public DaysTimeFormatter()
        {
        }

        public string Format(TimeSpan? time)
        {
            if (time.HasValue)
            {
                var formatted = string.Empty;
                if (time.Value.TotalDays >= 1)
                    formatted = (int)time.Value.TotalDays + "d ";

                if (time.Value.TotalHours >= 1)
                    formatted += time.Value.ToString(@"h\:mm\:ss", CultureInfo.InvariantCulture);
                else
                    formatted += time.Value.ToString(@"m\:ss", CultureInfo.InvariantCulture);

                return formatted;
            }
            return "0";
        }
    }
}
