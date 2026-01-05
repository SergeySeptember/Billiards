using Billiards.ViewModels;

namespace Billiards.Views;

public class MainCarouselTemplateSelector : DataTemplateSelector
{
    public DataTemplate SettingsTemplate { get; set; } = null!;
    public DataTemplate MatchTemplate { get; set; } = null!;
    public DataTemplate StatsTemplate { get; set; } = null!;

    protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        => item switch
        {
            SettingsViewModel => SettingsTemplate,
            MatchViewModel => MatchTemplate,
            StatsViewModel => StatsTemplate,
            _ => MatchTemplate
        };
}