namespace Billiards.Abstractions;

public interface IStatsDatePickerService
{
    void Show(DateTime selectedDate, IReadOnlyCollection<DateTime> datesWithMatches, Action<DateTime> onDateSelected);
}
