using System.Collections.ObjectModel;

namespace Billiards.ViewModels;

public class MainViewModel(SettingsViewModel settingsVm, MatchViewModel matchVm, StatsViewModel statsVm)
    : BaseViewModel
{
    public ObservableCollection<object> Pages { get; } = new()
    {
        settingsVm,
        matchVm,
        statsVm
    };

    private int _currentIndex = 1;
    public int CurrentIndex
    {
        get => _currentIndex;
        set => SetProperty(ref _currentIndex, value);
    }
}