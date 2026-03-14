using Billiards.ViewModels;

namespace Billiards.Views;

public partial class StatsByPlayersPage : ContentPage
{
    private readonly StatsByPlayersViewModel _vm;

    public StatsByPlayersPage(StatsByPlayersViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
        _vm = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.LoadAsync();
    }

    private async void OnBackClicked(object? sender, EventArgs e)
    {
        if (Navigation.NavigationStack.Count > 1)
        {
            await Navigation.PopAsync();
        }
    }
}
