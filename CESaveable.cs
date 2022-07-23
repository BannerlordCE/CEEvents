using CaptivityEvents.Brothel;
using CaptivityEvents.CampaignBehaviors;
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
            AddClassDefinition(typeof(CEWhereAreMyThingsIssueBehavior.CEWhereAreMyThingsIssue), 1, null);
            AddClassDefinition(typeof(CEWhereAreMyThingsIssueBehavior.CEWhereAreMyThingsIssueQuest), 2, null);
            AddClassDefinition(typeof(CECampaignBehavior.Pregnancy), 3, null);
            AddClassDefinition(typeof(CESkills), 4, null);
            AddClassDefinition(typeof(CECaptorMapNotification), 5, null);
            // VM ONCE SAVEDATA CRASHES REFER TO TALEWORLD'S SAVE DEFINER ISSUE
            AddClassDefinition(typeof(CECaptorMapNotificationItemVM), 6, null);
            AddClassDefinition(typeof(CECampaignBehavior.ReturnEquipment), 7, null);
            AddClassDefinition(typeof(CEEventMapNotification), 8, null);
            // VM ONCE SAVEDATA CRASHES REFER TO TALEWORLD'S SAVE DEFINER ISSUE
            AddClassDefinition(typeof(CEEventMapNotificationItemVM), 9, null);
            AddClassDefinition(typeof(CECampaignBehavior.ExtraVariables), 10, null);
            AddClassDefinition(typeof(CEBrothel), 11, null);
        }

        protected override void DefineContainerDefinitions()
        {
            ConstructContainerDefinition(typeof(List<CECampaignBehavior.Pregnancy>));
            ConstructContainerDefinition(typeof(List<CECampaignBehavior.ReturnEquipment>));
            ConstructContainerDefinition(typeof(List<CEBrothel>));
        }
    }
}