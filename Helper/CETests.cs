using System;
using TaleWorlds.Library;

namespace CaptivityEvents.Helper
{
    internal class CETests
    {
        public static string RunTestOne()
        {
            try
            {
                string unused = "\n" + CommandLineFunctionality.CallFunction("campaign.fill_party", "10", out bool success);
                unused += "\n" + CommandLineFunctionality.CallFunction("campaign.add_prisoner", "5", out success);
                unused += "\n" + CommandLineFunctionality.CallFunction("captivity.fire_event", "CE_captor_male_escape", out success);

                return success ? "Test One: Success" : "Test One: Failed\n" + unused;
            }
            catch (Exception e)
            {
                return "Test One: Failed - " + e;
            }

        }
        public static string RunTestThree()
        {
            try
            {
                string unused = "\n" + CommandLineFunctionality.CallFunction("campaign.fill_party", "10", out bool success);
                unused += "\n" + CommandLineFunctionality.CallFunction("campaign.add_prisoner", "5 sword_sister", out success);
                unused += "\n" + CommandLineFunctionality.CallFunction("campaign.add_gold_to_hero", "10000", out success);

                return success ? "Test Three: Success" : "Test Three: Failed\n" + unused;
            }
            catch (Exception e)
            {
                return "Test One: Failed - " + e;
            }

        }

        public static string RunTestTwo()
        {
            try
            {
                return "Test Two: Success";
            }
            catch (Exception e)
            {
                return "Test Two: Failed - " + e;
            }
        }
    }
}