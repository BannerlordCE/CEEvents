using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Core.ViewModelCollection;
using TaleWorlds.Localization;

namespace CaptivityEvents.Brothel
{
    static class CEBrothelToolTip
    {
        public static void WorkshopTypeTooltipAction(this TooltipVM tooltipVM, object[] args)
        {
            UpdateTooltip(tooltipVM, args[0] as CEBrothel);
        }
        public static void UpdateTooltip(this TooltipVM tooltipVM, CEBrothel brothel)
        {
            tooltipVM.Mode = 1;
            tooltipVM.AddProperty("", new TextObject("{=CEEVENTS1099}Brothel").ToString(), 0, TooltipProperty.TooltipPropertyFlags.Title);
            tooltipVM.AddProperty(new TextObject("{=qRqnrtdX}Owner", null).ToString(), brothel.Owner.Name.ToString(), 0, TooltipProperty.TooltipPropertyFlags.None);
            tooltipVM.AddProperty(new TextObject("Captives To Be Added", null).ToString(), brothel.Owner.Name.ToString(), 0, TooltipProperty.TooltipPropertyFlags.None);
            tooltipVM.AddProperty(string.Empty, string.Empty, 0, TooltipProperty.TooltipPropertyFlags.None);
        }
    }
}
