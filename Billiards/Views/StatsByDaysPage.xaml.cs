using Billiards.ViewModels;

#if ANDROID
using Color = Android.Graphics.Color;
using Android.Content;
using Android.Content.Res;
using Android.OS;
using AndroidX.Fragment.App;
using Google.Android.Material.DatePicker;
using Java.Lang;
#endif

namespace Billiards.Views;

public partial class StatsByDaysPage : ContentPage
{
    private readonly StatsByDaysViewModel _vm;

    public StatsByDaysPage(StatsByDaysViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
        _vm = vm;
    }

    private void OnPickDateClicked(object? sender, EventArgs e)
    {
#if ANDROID
        ShowAndroidDatePicker();
#endif
    }

#if ANDROID
    private void ShowAndroidDatePicker()
    {
        if (Platform.CurrentActivity is not FragmentActivity activity)
        {
            return;
        }

        var builder = MaterialDatePicker.Builder.DatePicker();
        builder.SetTitleText("Выберите день");
        builder.SetSelection(new Java.Lang.Long(ToUtcMilliseconds(_vm.SelectedDate)));

        var datesWithMatches = _vm.DatesWithMatches;
        if (datesWithMatches.Count > 0)
        {
            builder.SetDayViewDecorator(new MatchDatesDayDecorator(datesWithMatches));
        }

        var picker = builder.Build();
        picker.AddOnPositiveButtonClickListener(new PositiveButtonClickListener(selection =>
        {
            if (selection is Java.Lang.Long selectedValue)
            {
                var selectedDate = DateTimeOffset
                    .FromUnixTimeMilliseconds(selectedValue.LongValue())
                    .UtcDateTime
                    .Date;

                MainThread.BeginInvokeOnMainThread(() => _vm.SelectedDate = selectedDate);
            }
        }));

        picker.Show(activity.SupportFragmentManager, "stats_by_days_picker");
    }

    private static long ToUtcMilliseconds(DateTime date) =>
        new DateTimeOffset(DateTime.SpecifyKind(date.Date, DateTimeKind.Utc)).ToUnixTimeMilliseconds();

    private sealed class PositiveButtonClickListener(Action<Java.Lang.Object> onClick)
        : Java.Lang.Object, IMaterialPickerOnPositiveButtonClickListener
    {
        public void OnPositiveButtonClick(Java.Lang.Object p0) => onClick(p0);
    }

    private sealed class MatchDatesDayDecorator(IEnumerable<DateTime> dates) : DayViewDecorator
    {
        private readonly HashSet<DateOnly> _dates = dates.Select(d => DateOnly.FromDateTime(d.Date)).ToHashSet();
        private static readonly ColorStateList HighlightBackground = ColorStateList.ValueOf(Color.ParseColor("#DCE9FF"));
        private static readonly ColorStateList HighlightText = ColorStateList.ValueOf(Color.ParseColor("#174EA6"));

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
            ICharSequence originalContentDescription)
        {
            if (!HasMatch(year, month, day))
            {
                return originalContentDescription;
            }

            return new Java.Lang.String($"{originalContentDescription} Есть матчи");
        }

        private bool HasMatch(int year, int month, int day)
        {
            try
            {
                return _dates.Contains(new DateOnly(year, month + 1, day));
            }
            catch
            {
                return false;
            }
        }
    }
#endif
}
