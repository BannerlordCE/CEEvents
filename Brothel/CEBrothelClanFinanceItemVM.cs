using System;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.ViewModelCollection.ClanManagement;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection;
using TaleWorlds.Localization;

namespace CaptivityEvents.Brothel
{
    internal class CEBrothelClanFinanceItemVM : ClanFinanceIncomeItemBaseVM
    {
        public CEBrothelClanFinanceItemVM(CEBrothel brothel, Action<ClanFinanceIncomeItemBaseVM> onSelection, Action onRefresh) : base(onSelection, onRefresh)
        {
            _brothel = brothel;

            IncomeTypeAsEnum = IncomeTypes.None;
            SettlementComponent component = _brothel.Settlement.GetComponent<SettlementComponent>();
            WorkshopType workshopType = WorkshopType.Find("pottery_shop");
            WorkshopTypeId = workshopType.StringId;

            // 1.4.1
            /*
            Visual = ((_brothel.Owner.CharacterObject != null) ? new ImageIdentifierVM(CharacterCode.CreateFrom(_brothel.Owner.CharacterObject)) : new ImageIdentifierVM(ImageIdentifierType.Null));
            */
            // 1.4.2
            ImageName = component != null ? component.WaitMeshName : "";
            
        }

        public override void RefreshValues()
        {
            base.RefreshValues();

            // 1.4.1
            /*
            TextObject textObject = new TextObject("{=CEBROTHEL0984}The brothel of {SETTLEMENT}");
            textObject.SetTextVariable("SETTLEMENT", _brothel.Settlement.Name);
            Name = textObject.ToString();
            */
            // 1.4.2
            Name = _brothel.Name.ToString();
            Location = _brothel.Settlement.Name.ToString(); 
            Income = (int) (Math.Max(0, _brothel.ProfitMade) / Campaign.Current.Models.ClanFinanceModel.RevenueSmoothenFraction());
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
            int sellingCost = _brothel.Capital;
            string hint = GetBrothelSellHintText(sellingCost);
            ActionList.Add(new StringItemWithEnabledAndHintVM(ExecuteSellBrothel, new TextObject("{=PHkC8Gia}Sell").ToString(), true, null, hint));

            bool isCurrentlyActive = _brothel.IsRunning;
            int costToStart = _brothel.Expense;
            string hint2 = GetBrothelRunningHintText(isCurrentlyActive, costToStart);

            ActionList.Add(isCurrentlyActive
                               ? new StringItemWithEnabledAndHintVM(ExecuteToggleBrothel, new TextObject("{=CEBROTHEL0995}Stop Operations").ToString(), true, null, hint2)
                               : new StringItemWithEnabledAndHintVM(ExecuteToggleBrothel, new TextObject("{=CEBROTHEL0996}Start Operations").ToString(), Hero.MainHero.Gold >= costToStart, null, hint2));
        }

        protected override void PopulateStatsList()
        {
            ItemProperties.Add(new ClanSelectableItemPropertyVM(new TextObject("{=CEBROTHEL0976}Level").ToString(), _brothel.Level.ToString()));
            ItemProperties.Add(new ClanSelectableItemPropertyVM(new TextObject("{=CEBROTHEL0988}State").ToString(), _brothel.IsRunning
                                                                    ? new TextObject("{=CEBROTHEL0992}Normal").ToString()
                                                                    : new TextObject("{=CEBROTHEL0991}Closed").ToString()));
            ItemProperties.Add(new ClanSelectableItemPropertyVM(new TextObject("{=CEBROTHEL0977}Initial Capital").ToString(), _brothel.InitialCapital.ToString()));
            ItemProperties.Add(new ClanSelectableItemPropertyVM(new TextObject("{=CEBROTHEL0990}Capital").ToString(), _brothel.Capital.ToString()));
            ItemProperties.Add(new ClanSelectableItemPropertyVM(new TextObject("{=CEBROTHEL0989}Daily Wages").ToString(), _brothel.Expense.ToString()));

            if (_brothel.NotRunnedDays > 0)
            {
                TextObject textObject = new TextObject("{=*}{DAYS} days ago");
                textObject.SetTextVariable("DAYS", _brothel.NotRunnedDays);
                ItemProperties.Add(new ClanSelectableItemPropertyVM(new TextObject("{=*}Last Run").ToString(), textObject.ToString()));
            }

            InputProducts = GameTexts.FindText("str_CE_brothel_description", _brothel.IsRunning
                                                   ? null
                                                   : "inactive").ToString();
            OutputProducts = string.Join(",", _brothel.CaptiveProstitutes.Where(c => c.IsHero).Select(c => c.HeroObject.Name.ToString()).ToArray());
        }

        private void ExecuteBeginWorkshopHint()
        {
            if (_brothel != null) InformationManager.AddTooltipInformation(typeof(CEBrothel), _brothel);
        }

        private void ExecuteEndHint()
        {
            InformationManager.HideInformations();
        }

        private static string GetBrothelRunningHintText(bool isRunning, int costToStart)
        {
            TextObject textObject = new TextObject("The brothel is currently {?ISRUNNING}open{?}closed, you will need {AMOUNT} denars to begin operations again{\\?}.");

            textObject.SetTextVariable("ISRUNNING", isRunning
                                           ? 1
                                           : 0);
            if (!isRunning) textObject.SetTextVariable("AMOUNT", costToStart);

            return textObject.ToString();
        }

        private void ExecuteToggleBrothel(object identifier)
        {
            if (_brothel == null) return;
            if (!_brothel.IsRunning) GiveGoldAction.ApplyBetweenCharacters(Hero.MainHero, null, _brothel.Expense);
            _brothel.IsRunning = !_brothel.IsRunning;
            Action onRefresh = _onRefresh;

            onRefresh?.Invoke();
        }

        private static string GetBrothelSellHintText(int sellCost)
        {
            TextObject textObject = new TextObject("{=CEBROTHEL1000}You can sell this brothel for {AMOUNT} denars.");
            textObject.SetTextVariable("AMOUNT", sellCost);

            return textObject.ToString();
        }

        private void ExecuteSellBrothel(object identifier)
        {
            if (_brothel == null) return;
            GiveGoldAction.ApplyBetweenCharacters(null, Hero.MainHero, _brothel.Capital);
            CEBrothelBehavior.BrothelInteraction(_brothel.Settlement, false);

            Action onRefresh = _onRefresh;

            onRefresh?.Invoke();
        }


        public string WorkshopTypeId
        {
            get => _workshopTypeId;
            set
            {
                if (value == _workshopTypeId) return;
                _workshopTypeId = value;
                OnPropertyChanged();
            }
        }

        public string InputsText
        {
            get => _inputsText;
            set
            {
                if (value == _inputsText) return;
                _inputsText = value;
                OnPropertyChanged();
            }
        }

        public string OutputsText
        {
            get => _outputsText;
            set
            {
                if (value == _outputsText) return;
                _outputsText = value;
                OnPropertyChanged();
            }
        }

        public string InputProducts
        {
            get => _inputProducts;
            set
            {
                if (value == _inputProducts) return;
                _inputProducts = value;
                OnPropertyChanged();
            }
        }

        public string OutputProducts
        {
            get => _outputProducts;
            set
            {
                if (value == _outputProducts) return;
                _outputProducts = value;
                OnPropertyChanged();
            }
        }

        private readonly CEBrothel _brothel;

        private string _workshopTypeId;

        private string _inputsText;

        private string _outputsText;

        private string _inputProducts;

        private string _outputProducts;
    }
}