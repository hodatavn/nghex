namespace Nghex.Core.Enum
{
    public enum DateFormat
    {
        /// <summary>
        /// Year-Month-Day
        /// </summary>
        YearMonthDay = 0,
        /// <summary>
        /// Year-Month-Day Hour24:Minute
        /// </summary>
        YearMonthDayHour24Minute = 1,
        /// <summary>
        /// Year-Month-Day Hour24:Minute:Second
        /// </summary>
        YearMonthDayHour24MinuteSecond = 2,
        /// <summary>
        /// Month-Day-Year
        /// </summary>
        MonthDayYear = 3,
        /// <summary>
        /// Month-Day-Year Hour24:Minute
        /// </summary>
        MonthDayYearHour24Minute = 4,
        /// <summary>
        /// Month-Day-Year Hour24:Minute:Second
        /// </summary>
        MonthDayYearHour24MinuteSecond = 5,
        /// <summary>
        /// Day-Month-Year
        /// </summary>
        DayMonthYear = 6,
        /// <summary>
        /// Day-Month-Year Hour24:Minute
        /// </summary>
        DayMonthYearHour24Minute = 7,
        /// <summary>
        /// Day-Month-Year Hour24:Minute:Second
        /// </summary>
        DayMonthYearHour24MinuteSecond = 8
    }

    public static class DateFormatExtensions
    {
        /// <summary>
        /// Get format string for date format
        /// </summary>
        /// <param name="dateFormat">Date format</param>
        /// <returns>Format string. Default is "yyyy-MM-dd HH24:mm:ss"</returns>
        public static string GetFormat(this DateFormat dateFormat)
        {
            return dateFormat switch
            {
                DateFormat.YearMonthDay => "yyyy-MM-dd",
                DateFormat.YearMonthDayHour24Minute => "yyyy-MM-dd HH:mm",
                DateFormat.YearMonthDayHour24MinuteSecond => "yyyy-MM-dd HH:mm:ss",
                DateFormat.MonthDayYear => "MM-dd-yyyy",
                DateFormat.MonthDayYearHour24Minute => "MM-dd-yyyy HH:mm",
                DateFormat.MonthDayYearHour24MinuteSecond => "MM-dd-yyyy HH:mm:ss",
                DateFormat.DayMonthYear => "dd-MM-yyyy",
                DateFormat.DayMonthYearHour24Minute => "dd-MM-yyyy HH:mm",
                DateFormat.DayMonthYearHour24MinuteSecond => "dd-MM-yyyy HH:mm:ss",
                _ => DateFormat.DayMonthYearHour24MinuteSecond.GetFormat()
            };
        }

        /// <summary>
        /// Get date format enum from format string
        /// </summary>
        /// <param name="format">Format string</param>
        /// <returns>Date format enum. Default is DateFormat.YearMonthDayHour24MinuteSecond</returns>
        public static DateFormat FromFormat(this string format)
        {
            return format switch
            {
                "yyyy-MM-dd" => DateFormat.YearMonthDay,
                "yyyy-MM-dd HH:mm" => DateFormat.YearMonthDayHour24Minute,
                "yyyy-MM-dd HH:mm:ss" => DateFormat.YearMonthDayHour24MinuteSecond,
                "MM-dd-yyyy" => DateFormat.MonthDayYear,
                "MM-dd-yyyy HH:mm" => DateFormat.MonthDayYearHour24Minute,
                "MM-dd-yyyy HH:mm:ss" => DateFormat.MonthDayYearHour24MinuteSecond,
                "dd-MM-yyyy" => DateFormat.DayMonthYear,
                "dd-MM-yyyy HH:mm" => DateFormat.DayMonthYearHour24Minute,
                "dd-MM-yyyy HH:mm:ss" => DateFormat.DayMonthYearHour24MinuteSecond,
                _ => DateFormat.DayMonthYearHour24MinuteSecond
            };
        }
    }
}