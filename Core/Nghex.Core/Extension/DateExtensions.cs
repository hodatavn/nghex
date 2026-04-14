using System.Globalization;
using Nghex.Core.Enum;

namespace Nghex.Core.Extension
{
    public static class DateExtension
    {
        /// <summary>
        /// Format DateTime to string
        /// </summary>
        /// <param name="dateTime">Date time</param>
        /// <param name="dateFormat">Date format</param>
        /// <returns>Formatted string</returns>
        public static string Format(this DateTime dateTime, 
                    DateFormat dateFormat = DateFormat.YearMonthDayHour24MinuteSecond)
        {
            return dateTime.ToString(dateFormat.GetFormat());
        }

        /// <summary>
        /// Convert string to DateTime
        /// </summary>
        /// <param name="dateTimeString">Date time string</param>
        /// <param name="dateFormat">Date format</param>
        /// <returns>DateTime</returns>
        public static DateTime ToDateTime(this string dateTimeString, 
                                DateFormat dateFormat = DateFormat.DayMonthYearHour24MinuteSecond)
        {
            return DateTime.ParseExact(
                dateTimeString, 
                dateFormat.GetFormat(),
                CultureInfo.InvariantCulture
            );
        }
        
    }
}