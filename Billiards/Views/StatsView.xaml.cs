using Billiards.ViewModels;

namespace Billiards.Views;

public partial class StatsView : ContentView
{
    public StatsView()
    {
        InitializeComponent();
        BindingContext = new StatsViewModel();
    }
}