namespace Service.Diagnoses.Domain.Enums;

public enum DosageIntervals
{
    EveryHour = 1,
    EveryThreeHours = 2,
    EverySixHours = 3,
    EveryTwelveHours = 4,
    OnePerDay = 5
}

public static class DosageIntervalsExtensions
{
    public static TimeSpan ToTimeSpan(this DosageIntervals interval)
    {
        return interval switch
        {
            DosageIntervals.EveryHour => TimeSpan.FromHours(1),
            DosageIntervals.EveryThreeHours => TimeSpan.FromHours(3),
            DosageIntervals.EverySixHours => TimeSpan.FromHours(6),
            DosageIntervals.EveryTwelveHours => TimeSpan.FromHours(12),
            DosageIntervals.OnePerDay => TimeSpan.FromHours(24),
            _ => throw new ArgumentOutOfRangeException(nameof(interval), interval, null)
        };
    }
}