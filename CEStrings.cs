using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace CaptivityEvents
{
    internal class CEStrings
    {
        public static TextObject FetchTraitString(string trait) => GameTexts.FindText("str_CE_traits", trait);
    }
}