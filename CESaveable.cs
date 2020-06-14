using CaptivityEvents.Brothel;
using CaptivityEvents.CampaignBehaviours;
using CaptivityEvents.Issues;
using CaptivityEvents.Notifications;
using System.Collections.Generic;
using TaleWorlds.SaveSystem;

namespace CaptivityEvents
{
    public class CESaveable : SaveableTypeDefiner
    {
        public CESaveable() : base(82185785)
        {
        }

        protected override void DefineClassTypes()
        {
            base.AddClassDefinition(typeof(CEWhereAreMyThingsIssueBehavior.CEWhereAreMyThingsIssue), 1);
            base.AddClassDefinition(typeof(CEWhereAreMyThingsIssueBehavior.CEWhereAreMyThingsIssueQuest), 2);
            base.AddClassDefinition(typeof(CECampaignBehavior.Pregnancy), 3);
            base.AddClassDefinition(typeof(CESkills), 4);
            base.AddClassDefinition(typeof(CECaptorMapNotification), 5);
            base.AddClassDefinition(typeof(CECaptorMapNotificationItemVM), 6);
            base.AddClassDefinition(typeof(CECampaignBehavior.ReturnEquipment), 7);
            base.AddClassDefinition(typeof(CEEventMapNotification), 8);
            base.AddClassDefinition(typeof(CEEventMapNotificationItemVM), 9);
            base.AddClassDefinition(typeof(CECampaignBehavior.ExtraVariables), 10);
            base.AddClassDefinition(typeof(CEBrothel), 11);
        }

        protected override void DefineContainerDefinitions()
        {
            ConstructContainerDefinition(typeof(List<CECampaignBehavior.Pregnancy>));
            ConstructContainerDefinition(typeof(List<CECampaignBehavior.ReturnEquipment>));
            ConstructContainerDefinition(typeof(List<CEBrothel>));
        }
    }
}