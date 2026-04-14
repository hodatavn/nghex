using Nghex.Plugins.Abstractions.Enums;
using Nghex.Plugins.Abstractions.Models;

namespace Nghex.Plugins.Abstractions.Helpers;

/// <summary>
/// Shared helper for calculating current and previous calendar ranges
/// based on a compare date and calendar type value.
/// 
/// This lives in Plugin.Base so that multiple plugins (e.g. Master, Inpatient)
/// can reuse the same logic without creating cross-service dependencies.
/// 
/// The calendarType parameter uses the shared CalendarType enum.
/// </summary>
public static class CalendarRangeHelper
{
    public static CalendarRangeModel GetCurrentAndPreviousRanges(DateTime compareDate, CalendarType calendarType)
    {
        var date = compareDate.Date;
        var ranges = calendarType switch
        {
            CalendarType.Week => GetWeekRanges(date),
            CalendarType.Month => GetMonthRanges(date),
            CalendarType.Quarter => GetQuarterRanges(date),
            CalendarType.HalfYear => GetHalfYearRanges(date),
            CalendarType.Year => GetYearRanges(date),
            CalendarType.Last7Days => GetLast7DaysRanges(date),
            _ => GetDayRanges(date),
        };

        // If the compare date is today, return CurrentTo "up to now" instead of end-of-day/end-of-period.
        // This supports dashboards that need real-time "to-date" numbers for the current period.
        if (date == DateTime.Now.Date)
        {
            ranges.CurrentTime = DateTime.Now;
        }

        return ranges;
    }

    public static CalendarRangeModel GetDaysRanges(DateTime date, int days)
    {
        var currentFrom = date.AddDays(-days + 1).Date;
        var currentTo = date.AddDays(1).AddSeconds(-1);
        var previousFrom = currentFrom.AddDays(-days);
        var previousTo = currentFrom.AddSeconds(-1);
        return new CalendarRangeModel{
            CurrentFrom = currentFrom, 
            CurrentTo = currentTo,
            PreviousFrom = previousFrom,
            PreviousTo = previousTo
        };
    }

    private static CalendarRangeModel GetLast7DaysRanges(DateTime date)
    {
        var currentFrom = date.AddDays(-6).Date;
        var currentTo = date.AddDays(1).AddSeconds(-1);
        return new CalendarRangeModel
        {
            CurrentFrom = currentFrom,
            CurrentTo = currentTo,
            PreviousFrom = currentFrom.AddDays(-7),
            PreviousTo = currentFrom.AddSeconds(-1)
        };
    }

    private static CalendarRangeModel GetDayRanges(DateTime date)
    {
        var currentFrom = date.Date; // 00:00:00
        var currentTo = currentFrom.AddDays(1).AddSeconds(-1); // 23:59:59

        var previousFrom = currentFrom.AddDays(-1); // previous day 00:00:00
        var previousTo = currentFrom.AddSeconds(-1); // previous day 23:59:59

        return new CalendarRangeModel
        {
            CurrentFrom = currentFrom,
            CurrentTo = currentTo,
            PreviousFrom = previousFrom,
            PreviousTo = previousTo
        };
    }

    private static CalendarRangeModel GetWeekRanges(DateTime date)
    {
        // ISO week-style: week starts on Monday
        var diff = (7 + (int)date.DayOfWeek - (int)DayOfWeek.Monday) % 7;
        var currentFrom = date.AddDays(-diff).Date; // Monday 00:00:00
        var currentTo = currentFrom.AddDays(7).AddSeconds(-1); // Sunday 23:59:59

        var previousFrom = currentFrom.AddDays(-7); // previous Monday 00:00:00
        var previousTo = currentFrom.AddSeconds(-1); // previous Sunday 23:59:59

        return new CalendarRangeModel
        {
            CurrentFrom = currentFrom,
            CurrentTo = currentTo,
            PreviousFrom = previousFrom,
            PreviousTo = previousTo
        };
    }

    private static CalendarRangeModel GetMonthRanges(DateTime date)
    {
        var currentFrom = new DateTime(date.Year, date.Month, 1); // 00:00:00
        var currentTo = currentFrom.AddMonths(1).AddSeconds(-1); // last day 23:59:59

        var previousFrom = currentFrom.AddMonths(-1); // previous month first day 00:00:00
        var previousTo = currentFrom.AddSeconds(-1); // previous month last day 23:59:59

        return new CalendarRangeModel
        {
            CurrentFrom = currentFrom,
            CurrentTo = currentTo,
            PreviousFrom = previousFrom,
            PreviousTo = previousTo
        };
    }

    private static CalendarRangeModel GetQuarterRanges(DateTime date)
    {
        var quarter = (date.Month - 1) / 3; // 0-based quarter index
        var quarterStartMonth = quarter * 3 + 1;
        var currentFrom = new DateTime(date.Year, quarterStartMonth, 1); // 00:00:00
        var currentTo = currentFrom.AddMonths(3).AddSeconds(-1); // end of quarter 23:59:59

        var previousFrom = currentFrom.AddMonths(-3); // start of previous quarter 00:00:00
        var previousTo = currentFrom.AddSeconds(-1); // end of previous quarter 23:59:59

        return new CalendarRangeModel
        {
            CurrentFrom = currentFrom,
            CurrentTo = currentTo,
            PreviousFrom = previousFrom,
            PreviousTo = previousTo
        };
    }

    private static CalendarRangeModel GetHalfYearRanges(DateTime date)
    {
        // Half 1: Jan 1 - Jun 30, Half 2: Jul 1 - Dec 31
        var isFirstHalf = date.Month <= 6;
        var currentFrom = new DateTime(date.Year, isFirstHalf ? 1 : 7, 1); // 00:00:00
        var currentTo = currentFrom.AddMonths(6).AddSeconds(-1); // end of half-year 23:59:59

        var previousFrom = currentFrom.AddMonths(-6); // start of previous half-year 00:00:00
        var previousTo = currentFrom.AddSeconds(-1); // end of previous half-year 23:59:59

        return new CalendarRangeModel
        {
            CurrentFrom = currentFrom,
            CurrentTo = currentTo,
            PreviousFrom = previousFrom,
            PreviousTo = previousTo
        };
    }

    private static CalendarRangeModel GetYearRanges(DateTime date)
    {
        var currentFrom = new DateTime(date.Year, 1, 1); // 00:00:00
        var currentTo = new DateTime(date.Year + 1, 1, 1).AddSeconds(-1); // Dec 31 23:59:59

        var previousFrom = currentFrom.AddYears(-1); // Jan 1 of previous year 00:00:00
        var previousTo = currentFrom.AddSeconds(-1); // Dec 31 of previous year 23:59:59

        return new CalendarRangeModel
        {
            CurrentFrom = currentFrom,
            CurrentTo = currentTo,
            PreviousFrom = previousFrom,
            PreviousTo = previousTo
        };
    }
}


