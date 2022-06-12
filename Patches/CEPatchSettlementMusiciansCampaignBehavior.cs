#define V180

using HarmonyLib;
using SandBox.CampaignBehaviors;
using SandBox.Objects;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.ObjectSystem;

namespace CaptivityEvents.Patches
{
    [HarmonyPatch(typeof(SettlementMusiciansCampaignBehavior))]
    internal class CEPatchSettlementMusiciansCampaignBehavior
    {
        [HarmonyPatch("CreateRandomPlayList")]
        [HarmonyPostfix]
        private static void CreateRandomPlayList(ref List<SettlementMusicData> __result, Settlement settlement)
        {
            if (CampaignMission.Current.Location.StringId == "brothel")
            {
                List<string> listOfLocationTags = new()
                {
                    "tavern"
                };
                Dictionary<CultureObject, float> dictionary = new();
                MBReadOnlyList<CultureObject> objectTypeList = MBObjectManager.Instance.GetObjectTypeList<CultureObject>();
                Town town = settlement.Town;
                float num;
                if (town == null)
                {
                    Village village = settlement.Village;
                    num = ((village != null) ? village.Bound.Town.Loyalty : 100f);
                }
                else
                {
                    num = town.Loyalty;
                }
                float num2 = num * 0.01f;
                float num3 = 0f;
                using (List<CultureObject>.Enumerator enumerator = objectTypeList.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        CultureObject c = enumerator.Current;
                        dictionary.Add(c, 0f);
                        float num4 = Kingdom.All.Sum(delegate (Kingdom k)
                        {
                            if (c != k.Culture)
                            {
                                return 0f;
                            }
                            return k.TotalStrength;
                        });
                        if (num4 > num3)
                        {
                            num3 = num4;
                        }
                    }
                }
                foreach (Kingdom kingdom in Kingdom.All)
                {
                    float num5 = (Campaign.MapDiagonal - Campaign.Current.Models.MapDistanceModel.GetDistance(kingdom.FactionMidSettlement, settlement.MapFaction.FactionMidSettlement)) / Campaign.MaximumDistanceBetweenTwoSettlements;
                    float num6 = num5 * num5 * num5 * 2f;
                    num6 += (settlement.MapFaction.IsAtWarWith(kingdom) ? 1f : 2f) * num2;
                    dictionary[kingdom.Culture] = MathF.Max(dictionary[kingdom.Culture], num6);
                }
                Dictionary<CultureObject, float> dictionary2;
                CultureObject culture;
                foreach (Kingdom kingdom2 in Kingdom.All)
                {
                    dictionary2 = dictionary;
                    culture = kingdom2.Culture;
                    dictionary2[culture] += kingdom2.TotalStrength / num3 * 0.5f;
                }
                foreach (Town town2 in Town.AllTowns)
                {
                    float num7 = (Campaign.MapDiagonal - Campaign.Current.Models.MapDistanceModel.GetDistance(settlement, town2.Settlement)) / Campaign.MapDiagonal;
                    float num8 = num7 * num7 * num7;
                    num8 *= MathF.Min(settlement.Prosperity, 5000f) * 0.0002f;
                    dictionary2 = dictionary;
                    culture = town2.Culture;
                    dictionary2[culture] += num8;
                }
                dictionary2 = dictionary;
                culture = settlement.Culture;
                dictionary2[culture] += 10f;
                dictionary2 = dictionary;
                culture = settlement.MapFaction.Culture;
                dictionary2[culture] += num2 * 5f;
                List<SettlementMusicData> settlementMusicDatas = (from x in MBObjectManager.Instance.GetObjectTypeList<SettlementMusicData>()
                                                                  where listOfLocationTags.Contains(x.LocationId)
                                                                  select x).ToList();
                KeyValuePair<CultureObject, float> maxWeightedCulture = dictionary.MaxBy((KeyValuePair<CultureObject, float> x) => x.Value);
                float num9 = (float)settlementMusicDatas.Count((SettlementMusicData x) => x.Culture == maxWeightedCulture.Key) / maxWeightedCulture.Value;
                List<SettlementMusicData> playList = new();
                foreach (KeyValuePair<CultureObject, float> keyValuePair in dictionary)
                {
                    int num10 = MBRandom.RoundRandomized(num9 * keyValuePair.Value);
                    if (num10 > 0)
                    {
                        List<SettlementMusicData> list = (from x in settlementMusicDatas
                                                          where x.Culture == keyValuePair.Key
                                                          select x).ToList<SettlementMusicData>();
                        list.Shuffle();
                        int num11 = 0;
                        while (num11 < num10 && num11 < list.Count)
                        {
                            playList.Add(list[num11]);
                            num11++;
                        }
                    }
                }
                if (playList.IsEmpty())
                {
                    playList = settlementMusicDatas;
                }
                playList.Shuffle();
                __result = playList;
            }
        }
    }
}