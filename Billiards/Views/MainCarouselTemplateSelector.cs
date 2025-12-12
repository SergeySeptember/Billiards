using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Billiards.Core;

namespace Billiards.Views
{
    public class MainCarouselTemplateSelector : DataTemplateSelector
    {
        public DataTemplate? SettingsTemplate { get; set; }
        public DataTemplate? MatchTemplate { get; set; }
        public DataTemplate? StatsTemplate { get; set; }

        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            if (item is not MainPageKind kind)
                return MatchTemplate ?? new DataTemplate();

            return kind switch
            {
                MainPageKind.Settings => SettingsTemplate ?? new DataTemplate(),
                MainPageKind.Match => MatchTemplate ?? new DataTemplate(),
                MainPageKind.Stats => StatsTemplate ?? new DataTemplate(),
                _ => MatchTemplate ?? new DataTemplate()
            };
        }
    }
}
