using Billiards.Abstractions;
using Billiards.ViewModels;

namespace Billiards.Views;

public partial class StatsByDaysPage : ContentPage
{
    private readonly StatsByDaysViewModel _vm;
    private readonly IStatsDatePickerService _datePickerService;

    public StatsByDaysPage(StatsByDaysViewModel vm, IStatsDatePickerService datePickerService)
    {
        InitializeComponent();
        BindingContext = vm;
        _vm = vm;
        _datePickerService = datePickerService;
    }

    private void OnPickDateClicked(object? sender, EventArgs e)
    {
        _datePickerService.Show(_vm.SelectedDate, _vm.DatesWithMatches, selectedDate => _vm.SelectedDate = selectedDate);
    }

    private async void OnBackClicked(object? sender, EventArgs e)
    {
        if (Navigation.NavigationStack.Count > 1)
        {
            await Navigation.PopAsync();
        }
    }
}
