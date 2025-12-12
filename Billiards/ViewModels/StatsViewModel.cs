namespace Billiards.ViewModels;

public class StatsViewModel : BaseViewModel
{
    private string _debugText = "Здесь позже будет статистика матчей.";

    public string DebugText
    {
        get => _debugText;
        set => SetProperty(ref _debugText, value);
    }
}