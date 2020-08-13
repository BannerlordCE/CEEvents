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
        public CESaveable() : base(82185785) { }

        protected override void DefineClassTypes()
        {
            AddClassDefinition(typeof(CEWhereAreMyThingsIssueBehavior.CEWhereAreMyThingsIssue), 1);
            AddClassDefinition(typeof(CEWhereAreMyThingsIssueBehavior.CEWhereAreMyThingsIssueQuest), 2);
            AddClassDefinition(typeof(CECampaignBehavior.Pregnancy), 3);
            AddClassDefinition(typeof(CESkills), 4);
            AddClassDefinition(typeof(CECaptorMapNotification), 5);
            // REMOVE VM ONCE SAVEDATA CRASHES IN BETA REFER TO TALEWORLD'S SAVE DEFINER ISSUE
            AddClassDefinition(typeof(CECaptorMapNotificationItemVM), 6);
            AddClassDefinition(typeof(CECampaignBehavior.ReturnEquipment), 7);
            AddClassDefinition(typeof(CEEventMapNotification), 8);
            // REMOVE VM ONCE SAVEDATA CRASHES IN BETA REFER TO TALEWORLD'S SAVE DEFINER ISSUE
            AddClassDefinition(typeof(CEEventMapNotificationItemVM), 9);
            AddClassDefinition(typeof(CECampaignBehavior.ExtraVariables), 10);
            AddClassDefinition(typeof(CEBrothel), 11);
        }

        protected override void DefineContainerDefinitions()
        {
            ConstructContainerDefinition(typeof(List<CECampaignBehavior.Pregnancy>));
            ConstructContainerDefinition(typeof(List<CECampaignBehavior.ReturnEquipment>));
            ConstructContainerDefinition(typeof(List<CEBrothel>));
        }
    }
}