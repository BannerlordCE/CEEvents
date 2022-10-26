#define V100

using System;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.CampaignSystem.ViewModelCollection.ClanManagement;
using TaleWorlds.CampaignSystem.ViewModelCollection.ClanManagement.ClanFinance;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Workshops;
using TaleWorlds.Library;
using TaleWorlds.Core.ViewModelCollection.Information;
using System.Collections.Generic;

using TaleWorlds.Core.ViewModelCollection.Generic;

namespace CaptivityEvents.Brothel
{
    // TaleWorlds.CampaignSystem.ViewModelCollection.ClanManagement.ClanFinance ClanFinanceWorkshopItemVM
    internal class CEBrothelClanFinanceItemVM : ClanFinanceWorkshopItemVM
    {
        public CEBrothelClanFinanceItemVM(CEBrothel brothel, Workshop workshop, Action<ClanFinanceIncomeItemBaseVM> onSelection, Action onRefresh, Action<ClanCardSelectionInfo> openCardSelectionPopup) : base(workshop, onSelection, onRefresh, openCardSelectionPopup)
        {
            _brothel = brothel;

            IncomeTypeAsEnum = IncomeTypes.Workshop;
            _onSelection = new Action<ClanFinanceIncomeItemBaseVM>(TempOnSelection);
            _onSelectionT = onSelection;
            _openCardSelectionPopup = openCardSelectionPopup;
            SettlementComponent component = _brothel.Settlement.SettlementComponent;
            ImageName = component != null ? component.WaitMeshName : "";
            ManageWorkshopHint = new HintViewModel(new TextObject("{=CEBROTHEL0975}Manage Brothel", null), null);

            RefreshValues();
        }

        private void TempOnSelection(ClanFinanceIncomeItemBaseVM temp)
        {
            _onSelectionT(this);
        }

        public override void RefreshValues()
        {
            base.RefreshValues();

            // WORKAROUND IN 1.5.9
            if (_brothel == null) _brothel = new CEBrothel(Workshop.Settlement);

            Name = _brothel.Name.ToString();
            WorkshopType workshopType = WorkshopType.Find("brewery");
            WorkshopTypeId = workshopType.StringId;
            Location = _brothel.Settlement.Name.ToString();
            Income = (int)(Math.Max(0, _brothel.ProfitMade) / Campaign.Current.Models.ClanFinanceModel.RevenueSmoothenFraction()) * (_brothel.Level + 1);

            IncomeValueText = DetermineIncomeText(Income);
            InputsText = new TextObject("{=CEBROTHEL0985}Description").ToString();
            OutputsText = new TextObject("{=CEBROTHEL0994}Notable Prostitutes").ToString();

            ItemProperties.Clear();
            PopulateActionList();
            PopulateStatsList();
        }



        public new void ExecuteManageWorkshop()
        {
            TextObject title = new("{=CEBROTHEL0975}Manage Brothel", null);
            ClanCardSelectionInfo obj = new(title, GetManageWorkshopItems(), new Action<List<object>, Action>(OnManageWorkshopDone), false);
            Action<ClanCardSelectionInfo> openCardSelectionPopup = this._openCardSelectionPopup;
            openCardSelectionPopup?.Invoke(obj);
        }

        private IEnumerable<ClanCardSelectionItemInfo> GetManageWorkshopItems()
        {
            int sellingCost = _brothel.Capital;
            TextObject disabledReason = TextObject.Empty;
            bool flag = true;
            TextObject textObject = new("{=CEBROTHEL0974}Sell this Brothel for {GOLD_AMOUNT}{GOLD_ICON}", null);
            textObject.SetTextVariable("GOLD_AMOUNT", sellingCost);
            textObject.SetTextVariable("GOLD_ICON", "{=!}<img src=\"General\\Icons\\Coin@2x\" extend=\"8\">");

            yield return new ClanCardSelectionItemInfo(textObject, !flag, disabledReason, ClanCardSelectionItemPropertyInfo.CreateActionGoldChangeText(sellingCost));


            bool isCurrentlyActive = _brothel.IsRunning;
            int costToStart = _brothel.Expense;

            bool flag2 = isCurrentlyActive || Hero.MainHero.Gold >= costToStart;
            TextObject disabledTextObject = new("You will need {AMOUNT} denars to begin operations again{\\?}.");
            textObject.SetTextVariable("AMOUNT", costToStart);

   

            TextObject disabledReason2 = Hero.MainHero.Gold < costToStart && !isCurrentlyActive ? disabledTextObject : TextObject.Empty;
            TextObject textObject2 = isCurrentlyActive  ? new TextObject("{=CEBROTHEL0995}Stop Operations") : new TextObject("{=CEBROTHEL0996}Start Operations");
            CharacterObject townswoman = CharacterObject.CreateFrom(_brothel.Settlement.Culture.TavernWench);
            townswoman.Age = MBRandom.RandomInt(25, Campaign.Current.Models.AgeModel.BecomeOldAge);
            ImageIdentifier image2 = new(CampaignUIHelper.GetCharacterCode(townswoman, true));

            yield return new ClanCardSelectionItemInfo("operations", textObject2, image2, CardSelectionItemSpriteType.None, null, null, GetText(GetBrothelRunningHintText(_brothel.IsRunning, _brothel.Expense)), !flag2, disabledReason2, ClanCardSelectionItemPropertyInfo.CreateActionGoldChangeText(isCurrentlyActive ? 0 : -costToStart));
        }

        private IEnumerable<ClanCardSelectionItemPropertyInfo> GetText(TextObject textObject)
        {
            yield return new ClanCardSelectionItemPropertyInfo(textObject);
        }

        private void OnManageWorkshopDone(List<object> selectedItems, Action closePopup)
        {
            closePopup?.Invoke();
            if (selectedItems.Count == 1)
            {
                if (selectedItems[0].ToString() == "operations") {
                    ExecuteToggleBrothel(selectedItems);
                } else {
                    ExecuteSellBrothel(selectedItems);
                }
            }
        }

        protected override void PopulateStatsList()
        {
            if (_brothel == null) _brothel = new CEBrothel(Workshop.Settlement);

            ItemProperties.Add(new SelectableItemPropertyVM(new TextObject("{=CEBROTHEL0976}Level").ToString(), _brothel.Level.ToString()));
            ItemProperties.Add(new SelectableItemPropertyVM(new TextObject("{=CEBROTHEL0988}State").ToString(), _brothel.IsRunning
                                                                    ? new TextObject("{=CEBROTHEL0992}Normal").ToString()
                                                                    : new TextObject("{=CEBROTHEL0991}Closed").ToString()));
            ItemProperties.Add(new SelectableItemPropertyVM(new TextObject("{=CEBROTHEL0977}Initial Capital").ToString(), _brothel.InitialCapital.ToString()));
            ItemProperties.Add(new SelectableItemPropertyVM(new TextObject("{=CEBROTHEL0990}Capital").ToString(), _brothel.Capital.ToString()));
            ItemProperties.Add(new SelectableItemPropertyVM(new TextObject("{=CEBROTHEL0989}Daily Wages").ToString(), _brothel.Expense.ToString()));

            if (_brothel.NotRunnedDays > 0)
            {
                TextObject textObject = new("{=*}{DAYS} days ago");
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
            if (_brothel != null) InformationManager.ShowTooltip(typeof(CEBrothel), _brothel);
        }

        private new void ExecuteEndHint() => MBInformationManager.HideInformations();

        private static TextObject GetBrothelRunningHintText(bool isRunning, int costToStart)
        {
            TextObject textObject = new("The brothel is currently {?ISRUNNING}open{?}closed, you will need {GOLD_AMOUNT}{GOLD_ICON} to begin operations again{\\?}.");

            textObject.SetTextVariable("ISRUNNING", isRunning ? 1 : 0);
            if (!isRunning)
            {
                textObject.SetTextVariable("GOLD_AMOUNT", costToStart);
                textObject.SetTextVariable("GOLD_ICON", "{=!}<img src=\"General\\Icons\\Coin@2x\" extend=\"8\">");
            }

            return textObject;
        }

        private void ExecuteToggleBrothel(object identifier)
        {
            if (_brothel == null) return;
            if (!_brothel.IsRunning) GiveGoldAction.ApplyBetweenCharacters(Hero.MainHero, null, _brothel.Expense);
            _brothel.IsRunning = !_brothel.IsRunning;
            Action onRefresh = _onRefresh;
            onRefresh?.Invoke();
        }

        private static TextObject GetBrothelSellHintText(int sellCost)
        {
            TextObject textObject = new("{=CEBROTHEL1000}You can sell this brothel for {AMOUNT} denars.");
            textObject.SetTextVariable("AMOUNT", sellCost);

            return textObject;
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
                OnPropertyChangedWithValue(value, "WorkshopTypeId");
            }
        }

        public new string InputsText
        {
            get => _inputsText;
            set
            {
                if (value == _inputsText) return;
                _inputsText = value;
                OnPropertyChangedWithValue(value, "InputsText");
            }
        }

        public new string OutputsText
        {
            get => _outputsText;
            set
            {
                if (value == _outputsText) return;
                _outputsText = value;
                OnPropertyChangedWithValue(value, "OutputsText");
            }
        }

        public new string InputProducts
        {
            get => _inputProducts;
            set
            {
                if (value == _inputProducts) return;
                _inputProducts = value;
                OnPropertyChangedWithValue(value, "InputProducts");
            }
        }

        public new string OutputProducts
        {
            get => _outputProducts;
            set
            {
                if (value == _outputProducts) return;
                _outputProducts = value;
                OnPropertyChangedWithValue(value, "OutputProducts");
            }
        }

        private CEBrothel _brothel;

        private string _workshopTypeId;

        private string _inputsText;

        private string _outputsText;

        private string _inputProducts;

        private string _outputProducts;

        private readonly Action<ClanCardSelectionInfo> _openCardSelectionPopup;

        private Action<ClanFinanceWorkshopItemVM> _onSelectionT;

    }
}