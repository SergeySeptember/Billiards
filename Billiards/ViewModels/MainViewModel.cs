using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Billiards.Core;

namespace Billiards.ViewModels;

public class MainViewModel : BaseViewModel
{
    private string _title = "Billiards";

    public string Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }

    // Коллекция страниц карусели
    public ObservableCollection<MainPageKind> Pages { get; } =
        new()
        {
            MainPageKind.Settings,
            MainPageKind.Match,
            MainPageKind.Stats
        };

    // Индекс текущей страницы (0 - настройки, 1 - матч, 2 - статистика)
    private int _currentIndex = 1;
    public int CurrentIndex
    {
        get => _currentIndex;
        set => SetProperty(ref _currentIndex, value);
    }
}