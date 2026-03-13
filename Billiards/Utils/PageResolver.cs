namespace Billiards.Utils;

public static class PageResolver
{
    public static Page? CurrentPage => ResolveCurrentPage(Application.Current?.Windows.FirstOrDefault()?.Page);

    public static INavigation? Navigation => CurrentPage?.Navigation ?? Application.Current?.Windows.FirstOrDefault()?.Page?.Navigation;

    private static Page? ResolveCurrentPage(Page? page)
    {
        while (page is not null)
        {
            switch (page)
            {
                case NavigationPage navigationPage when navigationPage.CurrentPage is not null:
                    page = navigationPage.CurrentPage;
                    continue;
                case FlyoutPage flyoutPage when flyoutPage.Detail is not null:
                    page = flyoutPage.Detail;
                    continue;
                case TabbedPage tabbedPage when tabbedPage.CurrentPage is not null:
                    page = tabbedPage.CurrentPage;
                    continue;
                default:
                    return page;
            }
        }

        return null;
    }
}
