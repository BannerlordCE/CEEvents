#define V127

using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.SaveSystem;
using TaleWorlds.CampaignSystem.Settlements;

namespace CaptivityEvents.Brothel
{
    internal class CEBrothel(Settlement settlement)
    {
        public void ChangeGold(int amount) => Capital = MBMath.ClampInt(Capital + amount, 1, 10000);

        public int ProfitMade => Math.Max(Capital - InitialCapital, 0);

        [SaveableField(1)]
        public Settlement Settlement = settlement;

        [SaveableField(2)]
        public List<CharacterObject> CaptiveProstitutes = [];

        [SaveableField(3)]
        public Hero Owner = null;

        [SaveableField(5)]
        public bool IsRunning = true;

        [SaveableField(6)]
        public int NotRunnedDays = 0;

        [SaveableField(7)]
        public readonly TextObject Name = new("{=CEEVENTS1099}Brothel");

        [SaveableField(8)]
        public int Capital = 5000;

        [SaveableField(9)]
        public readonly int Expense = 200;

        [SaveableField(10)]
        public readonly int InitialCapital = 5000;

        [SaveableField(11)]
        public int Level = 0;
    }
}