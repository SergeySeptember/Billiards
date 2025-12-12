using Billiards.ViewModels;

namespace Billiards.Views;

public partial class SettingsView : ContentView
{
    public SettingsView()
    {
        InitializeComponent();
        BindingContext = new SettingsViewModel();
    }
}