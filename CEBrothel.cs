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
        public CEBrothel(Settlement settlement)
        {
            Settlement = settlement;
            Expense = 200;
            InitialCapital = 5000;
            Capital = 5000;
        }

        public void ChangeGold(int amount)
        {
            Capital = MBMath.ClampInt(Capital + amount, 1, 10000);
        }

        public int ProfitMade => Math.Max(Capital - InitialCapital, 0);

        [SaveableField(1)]
        public Settlement Settlement;

        [SaveableField(2)]
        public List<CharacterObject> CaptiveProstitutes;

        [SaveableField(3)]
        public Hero Owner = null;

        [SaveableField(5)]
        public bool IsRunning = true;

        [SaveableField(6)]
        public int NotRunnedDays = 0;

        [SaveableField(7)]
        public TextObject Name = new TextObject("{=CEEVENTS1099}Brothel");

        [SaveableField(8)]
        public int Capital = 5000;

        [SaveableField(9)]
        public int Expense = 200;

        [SaveableField(10)]
        public int InitialCapital = 5000;


    }
}
