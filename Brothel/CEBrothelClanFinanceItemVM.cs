#define BETA // 1.5.9
using System;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.CampaignSystem.ViewModelCollection.ClanManagement;
using TaleWorlds.CampaignSystem.ViewModelCollection.ClanManagement.ClanFinance;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection;
using TaleWorlds.Localization;

namespace CaptivityEvents.Brothel
{
    // TaleWorlds.CampaignSystem.ViewModelCollection.ClanManagement.ClanFinance ClanFinanceWorkshopItemVM
#if BETA
    internal class CEBrothelClanFinanceItemVM : ClanFinanceWorkshopItemVM
#else
    internal class CEBrothelClanFinanceItemVM : ClanFinanceIncomeItemBaseVM
#endif
    {
        public CEBrothelClanFinanceItemVM(CEBrothel brothel, Workshop workshop, Action<ClanFinanceIncomeItemBaseVM> onSelection, Action onRefresh) : base(workshop, onSelection, onRefresh)
        {
            _brothel = brothel;

            IncomeTypeAsEnum = IncomeTypes.Workshop;
            SettlementComponent component = _brothel.Settlement.GetComponent<SettlementComponent>();
            ImageName = component != null ? component.WaitMeshName : "";
            RefreshValues();

        }

        public override void RefreshValues()
        {
            base.RefreshValues();

            if (_brothel == null) _brothel = new CEBrothel(Workshop.Settlement);

            Name = _brothel.Name.ToString();
            WorkshopType workshopType = WorkshopType.Find("pottery_shop");
            WorkshopTypeId = workshopType.StringId;
            Location = _brothel.Settlement.Name.ToString();
            Income = (int)(Math.Max(0, _brothel.ProfitMade) / Campaign.Current.Models.ClanFinanceModel.RevenueSmoothenFraction()) * (_brothel.Level + 1);

            IncomeValueText = DetermineIncomeText(Income);
            InputsText = new TextObject("{=CEBROTHEL0985}Description").ToString();
            OutputsText = new TextObject("{=CEBROTHEL0994}Notable Prostitutes").ToString();
            ActionList.Clear();
            ItemProperties.Clear();
            PopulateActionList();
            PopulateStatsList();
        }

        protected override void PopulateActionList()
        {
            if (_brothel == null) _brothel = new CEBrothel(base.Workshop.Settlement);

            int sellingCost = _brothel.Capital;
#if BETA || STABLE
            TextObject hint = GetBrothelSellHintText(sellingCost);
#else
            string hint = GetBrothelSellHintText(sellingCost);     
#endif
            ActionList.Add(new StringItemWithEnabledAndHintVM(ExecuteSellBrothel, new TextObject("{=PHkC8Gia}Sell").ToString(), true, null, hint));

            bool isCurrentlyActive = _brothel.IsRunning;
            int costToStart = _brothel.Expense;

#if BETA || STABLE
            TextObject hint2 = GetBrothelSellHintText(sellingCost);
#else
            string hint2 = GetBrothelRunningHintText(isCurrentlyActive, costToStart);
#endif

            ActionList.Add(isCurrentlyActive
                               ? new StringItemWithEnabledAndHintVM(ExecuteToggleBrothel, new TextObject("{=CEBROTHEL0995}Stop Operations").ToString(), true, null, hint2)
                               : new StringItemWithEnabledAndHintVM(ExecuteToggleBrothel, new TextObject("{=CEBROTHEL0996}Start Operations").ToString(), Hero.MainHero.Gold >= costToStart, null, hint2));
        }

        protected override void PopulateStatsList()
        {
            ItemProperties.Add(new SelectableItemPropertyVM(new TextObject("{=CEBROTHEL0976}Level").ToString(), _brothel.Level.ToString()));
            ItemProperties.Add(new SelectableItemPropertyVM(new TextObject("{=CEBROTHEL0988}State").ToString(), _brothel.IsRunning
                                                                    ? new TextObject("{=CEBROTHEL0992}Normal").ToString()
                                                                    : new TextObject("{=CEBROTHEL0991}Closed").ToString()));
            ItemProperties.Add(new SelectableItemPropertyVM(new TextObject("{=CEBROTHEL0977}Initial Capital").ToString(), _brothel.InitialCapital.ToString()));
            ItemProperties.Add(new SelectableItemPropertyVM(new TextObject("{=CEBROTHEL0990}Capital").ToString(), _brothel.Capital.ToString()));
            ItemProperties.Add(new SelectableItemPropertyVM(new TextObject("{=CEBROTHEL0989}Daily Wages").ToString(), _brothel.Expense.ToString()));

            if (_brothel.NotRunnedDays > 0)
            {
                TextObject textObject = new TextObject("{=*}{DAYS} days ago");
                textObject.SetTextVariable("DAYS", _brothel.NotRunnedDays);
                ItemProperties.Add(new SelectableItemPropertyVM(new TextObject("{=*}Last Run").ToString(), textObject.ToString()));
            }

            InputProducts = GameTexts.FindText("str_CE_brothel_description", _brothel.IsRunning
                                                   ? null
                                                   : "inactive").ToString();
            OutputProducts = string.Join(",", _brothel.CaptiveProstitutes.Where(c => c.IsHero).Select(c => c.HeroObject.Name.ToString()).ToArray());
        }

        private new void ExecuteBeginWorkshopHint()
        {
            if (_brothel != null) InformationManager.AddTooltipInformation(typeof(CEBrothel), _brothel);
        }

        private new void ExecuteEndHint() => InformationManager.HideInformations();

#if BETA || STABLE
        private static TextObject GetBrothelRunningHintText(bool isRunning, int costToStart)
#else
        private static string GetBrothelRunningHintText(bool isRunning, int costToStart)
#endif
        {
            TextObject textObject = new TextObject("The brothel is currently {?ISRUNNING}open{?}closed, you will need {AMOUNT} denars to begin operations again{\\?}.");

            textObject.SetTextVariable("ISRUNNING", isRunning ? 1 : 0);
            if (!isRunning) textObject.SetTextVariable("AMOUNT", costToStart);

#if BETA || STABLE
            return textObject;
#else
            return textObject.ToString();
#endif
        }

    private void ExecuteToggleBrothel(object identifier)
        {
            if (_brothel == null) return;
            if (!_brothel.IsRunning) GiveGoldAction.ApplyBetweenCharacters(Hero.MainHero, null, _brothel.Expense);
            _brothel.IsRunning = !_brothel.IsRunning;
            Action onRefresh = _onRefresh;

            onRefresh?.Invoke();
        }

#if BETA || STABLE
        private static TextObject GetBrothelSellHintText(int sellCost)
#else
        private static string GetBrothelSellHintText(int sellCost)
#endif
        {
            TextObject textObject = new TextObject("{=CEBROTHEL1000}You can sell this brothel for {AMOUNT} denars.");
            textObject.SetTextVariable("AMOUNT", sellCost);
#if BETA || STABLE
            return textObject;
#else
            return textObject.ToString();
#endif
        }

        private void ExecuteSellBrothel(object identifier)
        {
            if (_brothel == null) return;
            GiveGoldAction.ApplyBetweenCharacters(null, Hero.MainHero, _brothel.Capital);
            CEBrothelBehavior.BrothelInteraction(_brothel.Settlement, false);

            Action onRefresh = _onRefresh;

            onRefresh?.Invoke();
        }


        public new string WorkshopTypeId
        {
            get => _workshopTypeId;
            set
            {
                if (value == _workshopTypeId) return;
                _workshopTypeId = value;
#if BETA
                OnPropertyChangedWithValue(value, "WorkshopTypeId");
#else
                OnPropertyChanged();
#endif
            }
        }

        public new string InputsText
        {
            get => _inputsText;
            set
            {
                if (value == _inputsText) return;
                _inputsText = value;
#if BETA
                OnPropertyChangedWithValue(value, "InputsText");
#else
                OnPropertyChanged();
#endif
            }
        }

        public new string OutputsText
        {
            get => _outputsText;
            set
            {
                if (value == _outputsText) return;
                _outputsText = value;
#if BETA
                OnPropertyChangedWithValue(value, "OutputsText");
#else
                OnPropertyChanged();
#endif
            }
        }

        public new string InputProducts
        {
            get => _inputProducts;
            set
            {
                if (value == _inputProducts) return;
                _inputProducts = value;
#if BETA
                OnPropertyChangedWithValue(value, "InputProducts");
#else
                OnPropertyChanged();
#endif
            }
        }

        public new string OutputProducts
        {
            get => _outputProducts;
            set
            {
                if (value == _outputProducts) return;
                _outputProducts = value;
#if BETA
                OnPropertyChangedWithValue(value, "OutputProducts");
#else
                OnPropertyChanged();
#endif
            }
        }

        private CEBrothel _brothel;

        private string _workshopTypeId;

        private string _inputsText;

        private string _outputsText;

        private string _inputProducts;

        private string _outputProducts;
    }
}