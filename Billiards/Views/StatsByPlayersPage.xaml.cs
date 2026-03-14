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
        NavigationPage.SetHasNavigationBar(this, true);
        NavigationPage.SetHasBackButton(this, true);
        await _vm.LoadAsync();
    }
}
