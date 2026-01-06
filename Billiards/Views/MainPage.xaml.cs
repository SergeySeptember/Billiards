using Billiards.Abstractions;
using Billiards.ViewModels;

namespace Billiards;

public partial class MainPage : ContentPage
{
    public MainPage(MainViewModel vm, IPlayersStore players, IMatchesStore matches)
    {
        InitializeComponent();
        BindingContext = vm;

        Loaded += async (_, _) =>
        {
            await players.ReloadAsync();
            await matches.ReloadAsync();
        };
    }
}