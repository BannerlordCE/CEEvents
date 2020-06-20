using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.SaveSystem;

namespace CaptivityEvents.Brothel
{
    internal class CEBrothel
    {
        [SaveableField(1)]
        public Settlement Settlement;

        [SaveableField(2)]
        public List<CharacterObject> CaptiveProstitutes;

        [SaveableField(3)]
        public Hero Owner;

        [SaveableField(5)]
        public bool IsRunning;

        [SaveableField(6)]
        public int NotRunnedDays;

        [SaveableField(7)]
        public readonly TextObject Name;

        [SaveableField(8)]
        public int Capital;

        [SaveableField(9)]
        public readonly int Expense;

        [SaveableField(10)]
        public readonly int InitialCapital;

        public int ProfitMade => Math.Max(Capital - InitialCapital, 0);

        public CEBrothel(Settlement settlement)
        {
            Settlement = settlement;
            Expense = 200;
            InitialCapital = 5000;
            Capital = 5000;
            Name = new TextObject("{=CEEVENTS1099}Brothel");
            NotRunnedDays = 0;
            IsRunning = true;
            Owner = null;
            CaptiveProstitutes = new List<CharacterObject>();
        }

        public void ChangeGold(int amount)
        {
            Capital = MBMath.ClampInt(Capital + amount, 1, 10000);
        }
    }
}