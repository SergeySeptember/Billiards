using System.Collections.ObjectModel;
using Billiards.ModelAndDto;
using Billiards.Views;

namespace Billiards.ViewModels;

public class MainViewModel : BaseViewModel
{
    public ObservableCollection<MainCarouselItem> Pages { get; }

    public MainViewModel(
        SettingsViewModel settingsVm,
        MatchViewModel matchVm,
        StatsViewModel statsVm,
        SettingsView settingsView,
        MatchView matchView,
        StatsView statsView)
    {
        settingsView.BindingContext = settingsVm;
        matchView.BindingContext = matchVm;
        statsView.BindingContext = statsVm;

        Pages = new()
        {
            new(settingsView),
            new(matchView),
            new(statsView)
        };
    }

    private int _currentIndex = 1;

    public int CurrentIndex
    {
        get => _currentIndex;
        set => SetProperty(ref _currentIndex, value);
    }
}
