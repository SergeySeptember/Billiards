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
}