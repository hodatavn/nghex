namespace Nghex.Plugins.Abstractions.Models;

/// <summary>
/// Represents current and previous calendar ranges (with full-day time boundaries).
/// This model is shared across plugins to avoid duplicating calendar range logic.
/// </summary>
public class CalendarRangeModel
{
    public DateTime CurrentTime { get; set; } = DateTime.Now;
    /// <summary>
    /// Start of current period (inclusive, with time component).
    /// </summary>
    public DateTime CurrentFrom { get; set; }

    /// <summary>
    /// End of current period (inclusive, with time component).
    /// </summary>
    public DateTime CurrentTo { get; set; }

    /// <summary>
    /// Start of previous period (inclusive, with time component).
    /// </summary>
    public DateTime PreviousFrom { get; set; }

    /// <summary>
    /// End of previous period (inclusive, with time component).
    /// </summary>
    public DateTime PreviousTo { get; set; }
}




