using Billiards.ViewModels;

namespace Billiards;

public partial class MainPage
{
    public MainPage()
    {
        InitializeComponent();
        BindingContext = new MainViewModel();
    }
}