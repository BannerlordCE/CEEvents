#define V180

using TaleWorlds.Localization;
#if V172
using TaleWorlds.Core.ViewModelCollection;
#else
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Library;
#endif

namespace CaptivityEvents.Brothel
{
    internal static class CEBrothelToolTip
    {
#if V172
        public static void BrothelTypeTooltipAction(this TooltipVM tooltipVM, object[] args) => UpdateTooltip(tooltipVM, args[0] as CEBrothel);

        public static void UpdateTooltip(this TooltipVM tooltipVM, CEBrothel brothel)
        {
            tooltipVM.Mode = 1;
            tooltipVM.AddProperty("", new TextObject("{=CEEVENTS1099}Brothel").ToString(), 0, TooltipProperty.TooltipPropertyFlags.Title);
            tooltipVM.AddProperty(new TextObject("{=qRqnrtdX}Owner").ToString(), brothel.Owner.Name.ToString());
            tooltipVM.AddProperty(new TextObject("{=CEBROTHEL0994}Notable Prostitutes").ToString(), "None");
        }
#else
        public static void BrothelTypeTooltipAction(this PropertyBasedTooltipVM tooltipVM, object[] args) => UpdateTooltip(tooltipVM, args[0] as CEBrothel);

        public static void UpdateTooltip(this PropertyBasedTooltipVM tooltipVM, CEBrothel brothel)
        {
            tooltipVM.Mode = 1;
            tooltipVM.AddProperty("", new TextObject("{=CEEVENTS1099}Brothel").ToString(), 0, TooltipProperty.TooltipPropertyFlags.Title);
            tooltipVM.AddProperty(new TextObject("{=qRqnrtdX}Owner").ToString(), brothel.Owner.Name.ToString());
            tooltipVM.AddProperty(new TextObject("{=CEBROTHEL0994}Notable Prostitutes").ToString(), "None");
        }
#endif
    }
}