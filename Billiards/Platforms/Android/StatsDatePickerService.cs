using Android.Content;
using Android.Content.Res;
using Android.OS;
using AndroidX.Fragment.App;
using Billiards.Abstractions;
using Google.Android.Material.DatePicker;
using Java.Lang;

namespace Billiards.Platforms.Android;

public sealed class StatsDatePickerService : IStatsDatePickerService
{
    public void Show(DateTime selectedDate, IReadOnlyCollection<DateTime> datesWithMatches, Action<DateTime> onDateSelected)
    {
        if (Platform.CurrentActivity is not FragmentActivity activity)
        {
            return;
        }

        var builder = MaterialDatePicker.Builder.DatePicker();
        builder.SetSelection(Long.ValueOf(ToUtcMilliseconds(selectedDate)));

        if (datesWithMatches.Count > 0)
        {
            builder.SetDayViewDecorator(new MatchDatesDayDecorator(datesWithMatches));
        }

        var picker = builder.Build();
        picker.AddOnPositiveButtonClickListener(new PositiveButtonClickListener(selection =>
        {
            if (selection is not Long selectedValue)
            {
                return;
            }

            var pickedDate = DateTimeOffset
                .FromUnixTimeMilliseconds(selectedValue.LongValue())
                .UtcDateTime
                .Date;

            MainThread.BeginInvokeOnMainThread(() => onDateSelected(pickedDate));
        }));

        picker.Show(activity.SupportFragmentManager, "stats_by_days_picker");
    }

    private static long ToUtcMilliseconds(DateTime date) =>
        new DateTimeOffset(DateTime.SpecifyKind(date.Date, DateTimeKind.Utc)).ToUnixTimeMilliseconds();

    private sealed class PositiveButtonClickListener(Action<Java.Lang.Object?> onClick)
        : Java.Lang.Object, IMaterialPickerOnPositiveButtonClickListener
    {
        public void OnPositiveButtonClick(Java.Lang.Object? value) => onClick(value);
    }

    private sealed class MatchDatesDayDecorator(IEnumerable<DateTime> dates) : DayViewDecorator
    {
        private readonly HashSet<DateOnly> _dates = dates.Select(d => DateOnly.FromDateTime(d.Date)).ToHashSet();
        private static readonly ColorStateList HighlightBackground = ColorStateList.ValueOf(global::Android.Graphics.Color.ParseColor("#DCE9FF"));
        private static readonly ColorStateList HighlightText = ColorStateList.ValueOf(global::Android.Graphics.Color.ParseColor("#174EA6"));

        public override void Initialize(Context context)
        {
        }

        public override int DescribeContents() => 0;

        public override void WriteToParcel(Parcel? dest, ParcelableWriteFlags flags)
        {
        }

        public override ColorStateList? GetBackgroundColor(Context context, int year, int month, int day, bool valid, bool selected)
        {
            if (!valid || selected || !HasMatch(year, month, day))
            {
                return null;
            }

            return HighlightBackground;
        }

        public override ColorStateList? GetTextColor(Context context, int year, int month, int day, bool valid, bool selected)
        {
            if (!valid || selected || !HasMatch(year, month, day))
            {
                return null;
            }

            return HighlightText;
        }

        public override ICharSequence GetContentDescriptionFormatted(
            Context context,
            int year,
            int month,
            int day,
            bool valid,
            bool selected,
            ICharSequence? originalContentDescription)
        {
            if (!HasMatch(year, month, day))
            {
                return originalContentDescription ?? new Java.Lang.String(string.Empty);
            }

            return new Java.Lang.String($"{originalContentDescription} \u0415\u0441\u0442\u044C \u043C\u0430\u0442\u0447\u0438");
        }

        private bool HasMatch(int year, int month, int day)
        {
            try
            {
                return _dates.Contains(new(year, month + 1, day));
            }
            catch
            {
                return false;
            }
        }
    }
}
