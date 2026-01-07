namespace Billiards.Views;

public partial class StatsByDaysPage : ContentPage
{
    public StatsByDaysPage(ViewModels.StatsByDaysViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}