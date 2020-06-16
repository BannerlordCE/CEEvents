using CaptivityEvents.Custom;
using System;
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
            // 1.4.1 
            //base.IncomeTypeAsEnum = IncomeTypes.None;
            //GameTexts.SetVariable("SHOPNAME", _brothel.Settlement.Name);
            //GameTexts.SetVariable("SHOPTYPE", new TextObject("{=CEEVENTS1099}Brothel"));
            //PopulateActionList();
            //PopulateStatsList();
            //base.Name = GameTexts.FindText("str_clan_finance_shop", null).ToString();
            //base.Income = (int)(Math.Max(0, brothel.Capital) / Campaign.Current.Models.ClanFinanceModel.RevenueSmoothenFraction());
            //base.Visual = ((CharacterObject.PlayerCharacter != null) ? new ImageIdentifierVM(CharacterCode.CreateFrom(CharacterObject.PlayerCharacter)) : new ImageIdentifierVM(ImageIdentifierType.Null));
            //base.IncomeValueText = base.DetermineIncomeText(base.Income);

            // 1.4.2
            base.IncomeTypeAsEnum = IncomeTypes.None;
            SettlementComponent component = this._brothel.Settlement.GetComponent<SettlementComponent>();
            WorkshopType workshopType = WorkshopType.Find("pottery_shop");
            this.WorkshopTypeId = workshopType.StringId;
            base.ImageName = ((component != null) ? component.WaitMeshName : "");
            //GameTexts.SetVariable("SHOPNAME", _brothel.Settlement.Name);
            //GameTexts.SetVariable("SHOPTYPE", new TextObject("{=CEEVENTS1099}Brothel"));
        }

        public override void RefreshValues()
        {
            base.RefreshValues();
            base.Name = this._brothel.Name.ToString();
            base.Location = this._brothel.Settlement.Name.ToString();
            base.Income = (int)(Math.Max(0, this._brothel.ProfitMade) / Campaign.Current.Models.ClanFinanceModel.RevenueSmoothenFraction());
            base.IncomeValueText = base.DetermineIncomeText(base.Income);
            base.ActionList.Clear();
            base.ItemProperties.Clear();
            this.PopulateActionList();
            this.PopulateStatsList();
        }

        public string WorkshopTypeId
        {
            get
            {
                return this._workshopTypeId;
            }
            set
            {
                if (value != this._workshopTypeId)
                {
                    this._workshopTypeId = value;
                    base.OnPropertyChanged("WorkshopTypeId");
                }
            }
        }

        protected override void PopulateActionList()
        {
            int sellingCost = _brothel.Capital * 3;
            string hint = CEBrothelClanFinanceItemVM.GetBrothelSellHintText(sellingCost);
            base.ActionList.Add(new StringItemWithEnabledAndHintVM(new Action<object>(ExecuteSellBrothel), new TextObject("{=PHkC8Gia}Sell", null).ToString(), true, null, hint));

            bool isCurrentlyActive = _brothel.IsRunning;
            int costToStart = _brothel.Expense;
            string hint2 = CEBrothelClanFinanceItemVM.GetBrothelRunningHintText(isCurrentlyActive, costToStart);
            if (isCurrentlyActive)
            {
                base.ActionList.Add(new StringItemWithEnabledAndHintVM(new Action<object>(ExecuteToggleBrothel), new TextObject("{=CEBROTHEL0995}Stop Operations", null).ToString(), true, null, hint2));
            }
            else
            {
                base.ActionList.Add(new StringItemWithEnabledAndHintVM(new Action<object>(ExecuteToggleBrothel), new TextObject("{=CEBROTHEL0996}Start Operations", null).ToString(), (Hero.MainHero.Gold >= costToStart), null, hint2));
            }
        }

        protected override void PopulateStatsList()
        {
            base.ItemProperties.Add(new ClanSelectableItemPropertyVM(_brothel.IsRunning ? new TextObject("{=nMcvafHY}Active", null).ToString() : new TextObject("{=bnrRzeiF}Not Active", null).ToString(), "", null));
            base.ItemProperties.Add(new ClanSelectableItemPropertyVM(new TextObject("{=Ra17aK4e}Capital:", null).ToString(), _brothel.Capital.ToString(), null));
            base.ItemProperties.Add(new ClanSelectableItemPropertyVM(new TextObject("{=CaRbMaZY}Daily Wage:", null).ToString(), _brothel.Expense.ToString(), null));
            if (this._brothel.NotRunnedDays > 0)
            {
                TextObject textObject = new TextObject("{=*}{DAYS} days ago", null);
                textObject.SetTextVariable("DAYS", this._brothel.NotRunnedDays);
                base.ItemProperties.Add(new ClanSelectableItemPropertyVM(new TextObject("{=*}Last Run", null).ToString(), textObject.ToString(), null));
            }
        }

        private void ExecuteBeginWorkshopHint()
        {
            if (_brothel != null)
            {
                InformationManager.AddTooltipInformation(typeof(CEBrothel), new object[]
                {
                    _brothel
                });
            }
        }

        private void ExecuteEndHint()
        {
            InformationManager.HideInformations();
        }

        private static string GetBrothelRunningHintText(bool isRunning, int costToStart)
        {
            TextObject textObject = new TextObject("The brothel is currently {?ISRUNNING}active{?}not active, you will need {AMOUNT} denars to begin operations again{\\?}.", null);
            textObject.SetTextVariable("ISRUNNING", isRunning ? 1 : 0);
            if (!isRunning) textObject.SetTextVariable("AMOUNT", costToStart);
            return textObject.ToString();
        }

        private void ExecuteToggleBrothel(object identifier)
        {
            if (_brothel != null)
            {
                if (!_brothel.IsRunning)
                {
                    GiveGoldAction.ApplyBetweenCharacters(Hero.MainHero, null, _brothel.Expense, false);
                }
                _brothel.IsRunning = !_brothel.IsRunning;
                Action onRefresh = _onRefresh;
                if (onRefresh == null)
                {
                    return;
                }
                onRefresh();
            }
        }

        private static string GetBrothelSellHintText(int sellCost)
        {
            TextObject textObject = new TextObject("{=CEBROTHEL1000}You can sell this brothel for {AMOUNT} denars.", null);
            textObject.SetTextVariable("AMOUNT", sellCost);
            return textObject.ToString();
        }

        private void ExecuteSellBrothel(object identifier)
        {
            if (_brothel != null)
            {
                CEBrothelBehaviour.BrothelInteraction(_brothel.Settlement, false);
                GiveGoldAction.ApplyBetweenCharacters(null, Hero.MainHero, _brothel.Capital * 3, false);

                Action onRefresh = _onRefresh;
                if (onRefresh == null)
                {
                    return;
                }
                onRefresh();
            }
        }

        private readonly CEBrothel _brothel;

        private string _workshopTypeId;

    }
}
