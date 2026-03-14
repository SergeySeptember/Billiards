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

    public int CurrentIndex
    {
        get;
        set => SetProperty(ref field, value);
    } = 1;
}