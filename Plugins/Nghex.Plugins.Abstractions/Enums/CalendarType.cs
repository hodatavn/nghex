namespace Nghex.Plugins.Abstractions.Enums
{
    public enum CalendarType
    {
        Day = 0,
        Week = 1,
        Month = 2,
        Quarter = 3,
        HalfYear = 4,
        Year = 5,
        /// <summary>7 ngày gần nhất (từ compareDate trở về trước 7 ngày).</summary>
        Last7Days = 6,
        Unknown = 99
    }

    public static class CalendarTypeExtensions
    {
        public static string GetDisplayName(this CalendarType calendarType)
        {
            return calendarType switch
            {
                CalendarType.Day => "Day",
                CalendarType.Week => "Week",
                CalendarType.Month => "Month",
                CalendarType.Quarter => "Quarter",
                CalendarType.HalfYear => "Half Year",
                CalendarType.Year => "Year",
                CalendarType.Last7Days => "Last 7 Days",
                _ => "Unknown"
            };
        }

        public static string GetDisplayName(this CalendarType calendarType, string language = "en")
        {
            return language.ToLower() switch
            {
                "vi" => calendarType switch
                {
                    CalendarType.Day => "Ngày",
                    CalendarType.Week => "Tuần",
                    CalendarType.Month => "Tháng",
                    CalendarType.Quarter => "Quý",
                    CalendarType.HalfYear => "Bán niên",
                    CalendarType.Year => "Năm",
                    CalendarType.Last7Days => "7 ngày gần nhất",
                    _ => "Không xác định"
                },
                _ => calendarType.GetDisplayName(),
            };
        }

        public static CalendarType FromName(this string calendarTypeName)
        {
            return calendarTypeName.ToLower() switch
            {
                "day" => CalendarType.Day,
                "week" => CalendarType.Week,
                "month" => CalendarType.Month,
                "quarter" => CalendarType.Quarter,
                "half year" => CalendarType.HalfYear,
                "year" => CalendarType.Year,
                "last7days" => CalendarType.Last7Days,
                _ => CalendarType.Unknown
            };
        }

        public static CalendarType FromName(this string calendarTypeName, string language = "en")
        {
            return language.ToLower() switch
            {
                "vi" => calendarTypeName.ToLower() switch
                {
                    "ngày" => CalendarType.Day,
                    "tuần" => CalendarType.Week,
                    "month" => CalendarType.Month,
                    "quý" => CalendarType.Quarter,
                    "bán niên" => CalendarType.HalfYear,
                    "năm" => CalendarType.Year,
                    "7 ngày gần nhất" => CalendarType.Last7Days,
                    _ => CalendarType.Unknown
                },
                _ => calendarTypeName.FromName(),
            };
        }
    }
}



