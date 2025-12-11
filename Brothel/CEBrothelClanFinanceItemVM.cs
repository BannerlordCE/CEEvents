using CaptivityEvents.Custom;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Workshops;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.CampaignSystem.ViewModelCollection.ClanManagement;
using TaleWorlds.CampaignSystem.ViewModelCollection.ClanManagement.ClanFinance;
using TaleWorlds.Core;
using TaleWorlds.Core.ImageIdentifiers;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Core.ViewModelCollection.Selector;
using TaleWorlds.Library;
using TaleWorlds.Localization;

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
            //base.RefreshValues();
            this.StoreOutputPercentageText = "N/A";
            this.UseWarehouseAsInputText = "N/A";
            this.WarehouseCapacityText = new TextObject("{=CEBROTHEL1103}Prostitute Capacity", null).ToString();

            var count = _brothel?.CaptiveProstitutes?.Count() ?? 0;
            this.WarehouseCapacityValue = GameTexts.FindText("str_LEFT_over_RIGHT", null).SetTextVariable("LEFT", count).SetTextVariable("RIGHT", count < 10 ? 10 : count).ToString();


            // WORKAROUND IN 1.5.9
            _brothel ??= new CEBrothel(Workshop.Settlement);

            Name = _brothel.Name.ToString();
            WorkshopType workshopType = WorkshopType.Find("brewery");
            WorkshopTypeId = workshopType.StringId;
            Location = _brothel.Settlement.Name.ToString();
            Income = (int)(Math.Max(0, _brothel.ProfitMade) / Campaign.Current.Models.ClanFinanceModel.RevenueSmoothenFraction()) * (_brothel.Level + 1);

            IncomeValueText = DetermineIncomeText(Income);
            InputsText = new TextObject("{=CEBROTHEL1104}Regular Prostitutes").ToString();
            OutputsText = new TextObject("{=CEBROTHEL0994}Notable Prostitutes").ToString();

            ItemProperties.Clear();
            PopulateActionList();
            PopulateStatsList();
        }

        public new void ExecuteManageWorkshop()
        {
            TextObject title = new("{=CEBROTHEL0975}Manage Brothel", null);
            ClanCardSelectionInfo obj = new(title, GetManageWorkshopItems(), new Action<List<object>, Action>(OnManageWorkshopDone), false);
            Action<ClanCardSelectionInfo> openCardSelectionPopup = _openCardSelectionPopup;
            openCardSelectionPopup?.Invoke(obj);
        }

        private IEnumerable<ClanCardSelectionItemInfo> GetManageWorkshopItems()
        {
            int sellingCost = _brothel.Capital;
            TextObject disabledReason = TextObject.GetEmpty();
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



            TextObject disabledReason2 = Hero.MainHero.Gold < costToStart && !isCurrentlyActive ? disabledTextObject : TextObject.GetEmpty();
            TextObject textObject2 = isCurrentlyActive ? new TextObject("{=CEBROTHEL0995}Stop Operations") : new TextObject("{=CEBROTHEL0996}Start Operations");
            CharacterObject townswoman = CharacterObject.CreateFrom(_brothel.Settlement.Culture.TavernWench);
            townswoman.Age = MBRandom.RandomInt(25, Campaign.Current.Models.AgeModel.BecomeOldAge);
            CharacterImageIdentifier image2 = new(CampaignUIHelper.GetCharacterCode(townswoman, true));

            yield return new ClanCardSelectionItemInfo("operations", textObject2, image2, CardSelectionItemSpriteType.None, null, null, GetText(GetBrothelRunningHintText(_brothel.IsRunning, _brothel.Expense)), !flag2, disabledReason2, ClanCardSelectionItemPropertyInfo.CreateActionGoldChangeText(isCurrentlyActive ? 0 : -costToStart));
        }

        private IEnumerable<ClanCardSelectionItemPropertyInfo> GetText(TextObject textObject)
        {
            yield return new ClanCardSelectionItemPropertyInfo(textObject);
        }

        private void OnManageWorkshopDone(List<object> selectedItems, Action closePopup)
        {
            try
            {
                closePopup?.Invoke();
                if (selectedItems.Count == 1)
                {
                    if (selectedItems[0]?.ToString() == "operations")
                    {
                        ExecuteToggleBrothel(selectedItems);
                    }
                    else
                    {
                        ExecuteSellBrothel(selectedItems);
                    }
                }
            }
            catch (Exception e)
            {
                CECustomHandler.ForceLogToFile("OnManageWorkshopDone : " + e);
            }
        }

        protected override void PopulateStatsList()
        {
            _brothel ??= new CEBrothel(Workshop.Settlement);

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

            InputProducts = "";
            OutputProducts = string.Join(",", [.. _brothel.CaptiveProstitutes.Where(c => c.IsHero).Select(c => c.HeroObject.Name.ToString())]);

            WarehouseInputAmount = _brothel.CaptiveProstitutes.Where(c => !c.IsHero).Count();
            WarehouseOutputAmount = _brothel.CaptiveProstitutes.Where(c => c.IsHero).Count();
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

        private CEBrothel _brothel;

        [DataSourceProperty]
        public new HintViewModel UseWarehouseAsInputHint
        {
            get
            {
                return _useWarehouseAsInputHint;
            }
            set
            {
                if (value != _useWarehouseAsInputHint)
                {
                    _useWarehouseAsInputHint = value;
                    OnPropertyChangedWithValue(value, "UseWarehouseAsInputHint");
                }
            }
        }

        [DataSourceProperty]
        public new HintViewModel StoreOutputPercentageHint
        {
            get
            {
                return _storeOutputPercentageHint;
            }
            set
            {
                if (value != _storeOutputPercentageHint)
                {
                    _storeOutputPercentageHint = value;
                    OnPropertyChangedWithValue(value, "StoreOutputPercentageHint");
                }
            }
        }

        [DataSourceProperty]
        public new HintViewModel ManageWorkshopHint
        {
            get
            {
                return _manageWorkshopHint;
            }
            set
            {
                if (value != _manageWorkshopHint)
                {
                    _manageWorkshopHint = value;
                    OnPropertyChangedWithValue(value, "ManageWorkshopHint");
                }
            }
        }

        [DataSourceProperty]
        public new BasicTooltipViewModel InputWarehouseCountsTooltip
        {
            get
            {
                return _inputWarehouseCountsTooltip;
            }
            set
            {
                if (value != _inputWarehouseCountsTooltip)
                {
                    _inputWarehouseCountsTooltip = value;
                    OnPropertyChangedWithValue(value, "InputWarehouseCountsTooltip");
                }
            }
        }

        [DataSourceProperty]
        public new BasicTooltipViewModel OutputWarehouseCountsTooltip
        {
            get
            {
                return _outputWarehouseCountsTooltip;
            }
            set
            {
                if (value != _outputWarehouseCountsTooltip)
                {
                    _outputWarehouseCountsTooltip = value;
                    OnPropertyChangedWithValue(value, "OutputWarehouseCountsTooltip");
                }
            }
        }

        public new string WorkshopTypeId
        {
            get
            {
                return _workshopTypeId;
            }
            set
            {
                if (value != _workshopTypeId)
                {
                    _workshopTypeId = value;
                    OnPropertyChangedWithValue(value, "WorkshopTypeId");
                }
            }
        }

        public new string InputsText
        {
            get
            {
                return _inputsText;
            }
            set
            {
                if (value != _inputsText)
                {
                    _inputsText = value;
                    OnPropertyChangedWithValue(value, "InputsText");
                }
            }
        }

        public new string OutputsText
        {
            get
            {
                return _outputsText;
            }
            set
            {
                if (value != _outputsText)
                {
                    _outputsText = value;
                    OnPropertyChangedWithValue(value, "OutputsText");
                }
            }
        }

        public new string InputProducts
        {
            get
            {
                return _inputProducts;
            }
            set
            {
                if (value != _inputProducts)
                {
                    _inputProducts = value;
                    OnPropertyChangedWithValue(value, "InputProducts");
                }
            }
        }

        public new string OutputProducts
        {
            get
            {
                return _outputProducts;
            }
            set
            {
                if (value != _outputProducts)
                {
                    _outputProducts = value;
                    OnPropertyChangedWithValue(value, "OutputProducts");
                }
            }
        }

        public new string UseWarehouseAsInputText
        {
            get
            {
                return _useWarehouseAsInputText;
            }
            set
            {
                if (value != _useWarehouseAsInputText)
                {
                    _useWarehouseAsInputText = value;
                    OnPropertyChangedWithValue(value, "UseWarehouseAsInputText");
                }
            }
        }

        public new string StoreOutputPercentageText
        {
            get
            {
                return _storeOutputPercentageText;
            }
            set
            {
                if (value != _storeOutputPercentageText)
                {
                    _storeOutputPercentageText = value;
                    OnPropertyChangedWithValue(value, "StoreOutputPercentageText");
                }
            }
        }

        public new string WarehouseCapacityText
        {
            get
            {
                return _warehouseCapacityText;
            }
            set
            {
                if (value != _warehouseCapacityText)
                {
                    _warehouseCapacityText = value;
                    OnPropertyChangedWithValue(value, "WarehouseCapacityText");
                }
            }
        }

        public new string WarehouseCapacityValue
        {
            get
            {
                return _warehouseCapacityValue;
            }
            set
            {
                if (value != _warehouseCapacityValue)
                {
                    _warehouseCapacityValue = value;
                    OnPropertyChangedWithValue(value, "WarehouseCapacityValue");
                }
            }
        }

        public new bool ReceiveInputFromWarehouse
        {
            get
            {
                return _receiveInputFromWarehouse;
            }
            set
            {
                if (value != _receiveInputFromWarehouse)
                {
                    _receiveInputFromWarehouse = value;
                    OnPropertyChangedWithValue(value, "ReceiveInputFromWarehouse");
                }
            }
        }

        public new int WarehouseInputAmount
        {
            get
            {
                return _warehouseInputAmount;
            }
            set
            {
                if (value != _warehouseInputAmount)
                {
                    _warehouseInputAmount = value;
                    OnPropertyChangedWithValue(value, "WarehouseInputAmount");
                }
            }
        }

        public new int WarehouseOutputAmount
        {
            get
            {
                return _warehouseOutputAmount;
            }
            set
            {
                if (value != _warehouseOutputAmount)
                {
                    _warehouseOutputAmount = value;
                    OnPropertyChangedWithValue(value, "WarehouseOutputAmount");
                }
            }
        }

        public new SelectorVM<WorkshopPercentageSelectorItemVM> WarehousePercentageSelector
        {
            get
            {
                return _warehousePercentageSelector;
            }
            set
            {
                if (value != _warehousePercentageSelector)
                {
                    _warehousePercentageSelector = value;
                    OnPropertyChangedWithValue(value, "WarehousePercentageSelector");
                }
            }
        }

        private readonly TextObject _runningText = new("{=iuKvbKJ7}Running", null);

        private readonly TextObject _haltedText = new("{=zgnEagTJ}Halted", null);

        private readonly TextObject _noRawMaterialsText = new("{=JRKC4ed4}This workshop has not been producing for {DAY} {?PLURAL_DAYS}days{?}day{\\?} due to lack of raw materials in the town market.", null);

        private readonly TextObject _noProfitText = new("{=no0chrAH}This workshop has not been running for {DAY} {?PLURAL_DAYS}days{?}day{\\?} because the production has not been profitable", null);

        private readonly IWorkshopWarehouseCampaignBehavior _workshopWarehouseBehavior;

        private readonly WorkshopModel _workshopModel;

        private readonly Action<ClanCardSelectionInfo> _openCardSelectionPopup;

        private readonly Action<ClanFinanceWorkshopItemVM> _onSelectionT;

        private ExplainedNumber _inputDetails;

        private ExplainedNumber _outputDetails;

        private HintViewModel _useWarehouseAsInputHint;

        private HintViewModel _storeOutputPercentageHint;

        private HintViewModel _manageWorkshopHint;

        private BasicTooltipViewModel _inputWarehouseCountsTooltip;

        private BasicTooltipViewModel _outputWarehouseCountsTooltip;

        private string _workshopTypeId;

        private string _inputsText;

        private string _outputsText;

        private string _inputProducts;

        private string _outputProducts;

        private string _useWarehouseAsInputText;

        private string _storeOutputPercentageText;

        private string _warehouseCapacityText;

        private string _warehouseCapacityValue;

        private bool _receiveInputFromWarehouse;

        private int _warehouseInputAmount;

        private int _warehouseOutputAmount;

        private SelectorVM<WorkshopPercentageSelectorItemVM> _warehousePercentageSelector;

    }
}