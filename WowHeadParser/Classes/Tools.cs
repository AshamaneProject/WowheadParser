/*
 * Created by Traesh for AshamaneProject (https://github.com/AshamaneProject)
 */
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;

namespace WowHeadParser
{
    public struct ItemExtendedCostEntry
    {
        public UInt32 ID;
        public List<UInt32> RequiredItem;               // required item id
        public List<UInt32> RequiredCurrencyCount;      // required curency count
        public List<UInt16> RequiredItemCount;          // required count of 1st item
        public UInt16 RequiredPersonalArenaRating;      // required personal arena rating
        public List<UInt16> RequiredCurrency;           // required curency id
        public Byte RequiredArenaSlot;                  // arena slot restrictions (min slot value)
        public Byte RequiredFactionId;
        public Byte RequiredFactionStanding;
        public Byte RequirementFlags;
        public Byte RequiredAchievement;
    };

    public struct PlayerConditionEntry
    {
        public Int32 ID;                                                      // 0
        public Int32 Flags;                                                   // 1
        public Int32 MinLevel;                                                // 2
        public Int32 MaxLevel;                                                // 3
        public Int32 RaceMask;                                                // 4
        public Int32 ClassMask;                                               // 5
        public Int32 Gender;                                                  // 6
        public Int32 NativeGender;                                            // 7
        public List<Int32> SkillID;                                           // 8-11
        public List<Int32> MinSkill;                                          // 12-15
        public List<Int32> MaxSkill;                                          // 16-19
        public Int32 SkillLogic;                                              // 20
        public Int32 LanguageID;                                              // 21
        public Int32 MinLanguage;                                             // 22
        public Int32 MaxLanguage;                                             // 23
        public List<Int32> MinFactionID;                                      // 24-26
        public Int32 MaxFactionID;                                            // 27
        public List<Int32> MinReputation;                                     // 28-30
        public Int32 MaxReputation;                                           // 31
        public Int32 ReputationLogic;                                         // 32
        public Int32 MinPVPRank;                                              // 33
        public Int32 MaxPVPRank;                                              // 34
        public Int32 PvpMedal;                                                // 35
        public Int32 PrevQuestLogic;                                          // 36
        public List<Int32> PrevQuestID;                                       // 37-40
        public Int32 CurrQuestLogic;                                          // 41
        public List<Int32> CurrQuestID;                                       // 42-45
        public Int32 CurrentCompletedQuestLogic;                              // 46
        public List<Int32> CurrentCompletedQuestID;                           // 47-50
        public Int32 SpellLogic;                                              // 51
        public List<Int32> SpellID;                                           // 52-55
        public Int32 ItemLogic;                                               // 56
        public List<Int32> ItemID;                                            // 57-60
        public List<Int32> ItemCount;                                         // 61-64
        public Int32 ItemFlags;                                               // 65
        public List<Int32> Explored;                                          // 66-67
        public List<Int32> Time;                                              // 68-69
        public Int32 AuraSpellLogic;                                          // 70
        public List<Int32> AuraSpellID;                                       // 71-74
        public Int32 WorldStateExpressionID;                                  // 75
        public Int32 WeatherID;                                               // 76
        public Int32 PartyStatus;                                             // 77
        public Int32 LifetimeMaxPVPRank;                                      // 78
        public Int32 AchievementLogic;                                        // 79
        public List<Int32> Achievement;                                       // 80-83
        public Int32 LfgLogic;                                                // 84
        public List<Int32> LfgStatus;                                         // 85-88
        public List<Int32> LfgCompare;                                        // 89-92
        public List<Int32> LfgValue;                                          // 93-96
        public Int32 AreaLogic;                                               // 97
        public List<Int32> AreaID;                                            // 98-101
        public Int32 CurrencyLogic;                                           // 102
        public List<Int32> CurrencyID;                                        // 103-106
        public List<Int32> CurrencyCount;                                     // 107-110
        public Int32 QuestKillID;                                             // 111
        public Int32 QuestKillLogic;                                          // 112
        public List<Int32> QuestKillMonster;                                  // 113-116
        public Int32 MinExpansionLevel;                                       // 117
        public Int32 MaxExpansionLevel;                                       // 118
        public Int32 MinExpansionTier;                                        // 119
        public Int32 MaxExpansionTier;                                        // 120
        public Int32 MinGuildLevel;                                           // 121
        public Int32 MaxGuildLevel;                                           // 122
        public Int32 PhaseUseFlags;                                           // 123
        public Int32 PhaseID;                                                 // 124
        public Int32 PhaseGroupID;                                            // 125
        public Int32 MinAvgItemLevel;                                         // 126
        public Int32 MaxAvgItemLevel;                                         // 127
        public Int32 MinAvgEquippedItemLevel;                                 // 128
        public Int32 MaxAvgEquippedItemLevel;                                 // 129
        public Int32 ChrSpecializationIndex;                                  // 130
        public Int32 ChrSpecializationRole;                                   // 131
        public String FailureDescriptionLang;                                 // 132
        public Int32 PowerType;                                               // 133
        public Int32 PowerTypeComp;                                           // 134
        public Int32 PowerTypeValue;                                          // 135
    };

    enum CurrencyFlags
    {
        CURRENCY_FLAG_TRADEABLE             = 0x01,
        CURRENCY_FLAG_HIGH_PRECISION        = 0x08,
        CURRENCY_FLAG_ARCHAEOLOGY_FRAGMENT  = 0x20,
        CURRENCY_FLAG_HAS_SEASON_COUNT      = 0x80
    };

    struct CurrencyTypesEntry
    {
        public UInt32 ID;
        public String Name;
        public UInt32 MaxQty;
        public UInt32 MaxEarnablePerWeek;
        public UInt32 Flags;
        public String Description;
        public Byte CategoryID;
        public Byte SpellCategory;
        public Byte Quality;
        public UInt32 InventoryIconFileDataID;
        public UInt32 SpellWeight;

        public bool HasPrecision()   { return (Flags & (int)CurrencyFlags.CURRENCY_FLAG_HIGH_PRECISION) != 0; }
        public bool HasSeasonCount() { return (Flags & (int)CurrencyFlags.CURRENCY_FLAG_HAS_SEASON_COUNT) != 0; }
        public float GetPrecision()  { return HasPrecision() ? 100.0f : 1.0f; }
    };

    public enum UnitClass
    {
        UNIT_CLASS_WARRIOR  = 1,
        UNIT_CLASS_PALADIN  = 2,
        UNIT_CLASS_ROGUE    = 4,
        UNIT_CLASS_MAGE     = 8
    };

    class Tools
    {
        public static HttpClient InitHttpClient()
        {
            ServicePointManager.ServerCertificateValidationCallback += ValidateRemoteCertificate;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

            HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/62.0.3202.94 Safari/537.36");
            return httpClient;
        }

        private static bool ValidateRemoteCertificate(object sender, X509Certificate cert, X509Chain chain, SslPolicyErrors error)
        {
            // If the certificate is a valid, signed certificate, return true.
            if (error == System.Net.Security.SslPolicyErrors.None)
            {
                return true;
            }

            Console.WriteLine("X509Certificate [{0}] Policy Error: '{1}'",
                cert.Subject,
                error.ToString());

            return false;
        }

        public static String GetHtmlFromWowhead(String url, HttpClient webClient = null)
        {
            if (webClient == null)
                webClient = InitHttpClient();

            try
            {
                using (HttpResponseMessage response = webClient.GetAsync(url).Result)
                {
                    using (HttpContent content = response.Content)
                    {
                        return content.ReadAsStringAsync().Result;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Write(ex);
            }

            return "";
        }

        public static String GetWowheadUrl(String type, String id)
        {
            if (type != "")
                return "http://fr.wowhead.com/" + type + "=" + id;
            else
                return "http://fr.wowhead.com/" + id;
        }

        public static String GetFileNameForCurrentTime()
        {
            return DateTime.Now.ToString("yyyy-MM-dd_hh-mm-ss") + ".sql";
        }

        public static List<String> ExtractListJsonFromWithPattern(String input, String pattern)
        {
            Regex parseJSonRegex = new Regex(pattern);
            MatchCollection jSonMatch = parseJSonRegex.Matches(input);

            List<String> returnList = new List<String>();
            foreach (Match match in jSonMatch)
            {
                if (match.Success != true)
                    continue;

                short i = 0;
                foreach (Group group in match.Groups)
                {
                    if (i++ != 0)
                        returnList.Add(group.Value);
                }
            }

            return returnList;
        }

        public static String ExtractJsonFromWithPattern(String input, String pattern, int groupIndex = 0)
        {
            List<String> extractedValues = ExtractListJsonFromWithPattern(input, pattern);

            if (extractedValues.Count <= groupIndex)
                return null;

            String jsonString = extractedValues[groupIndex];

            jsonString = jsonString.Replace("undefined",    "\"undefined\"");
            jsonString = jsonString.Replace("[,1]",         "[0,1]");
            jsonString = jsonString.Replace("[1,]",         "[1,0]");
            jsonString = jsonString.Replace("[,0]",         "[0,0]");
            jsonString = jsonString.Replace("[0,]",         "[0,0]");
            jsonString = jsonString.Replace("[,-1]",        "[0,-1]");
            jsonString = jsonString.Replace("[-1,]",        "[-1,0]");

            jsonString = jsonString.Replace("'{",           "{");
            jsonString = jsonString.Replace("}'",           "}");

            // Npc loot specific
            jsonString = jsonString.Replace("modes:",       "\"modes\":");
            jsonString = jsonString.Replace("count:",       "\"count\":");
            jsonString = jsonString.Replace("pctstack:",    "\"pctstack\":");

            // Npc vendor specific
            jsonString = jsonString.Replace("standing:",    "\"standing\":");
            jsonString = jsonString.Replace("react:",       "\"react\":");
            jsonString = jsonString.Replace("stack:",       "\"stack\":");
            jsonString = jsonString.Replace("avail:",       "\"avail\":");
            jsonString = jsonString.Replace("cost:",        "\"cost\":");

            return jsonString;
        }

        public static String NormalizeFloat(float value)
        {
            float returnFloat = value;

            if (Math.Floor(value) > (value - 0.10f))
                returnFloat = (float)Math.Round(value);

            if (Math.Ceiling(value) < (value + 0.10f))
                returnFloat = (float)Math.Round(value);

            if (Math.Round(value) == 0 && value != 0)
                returnFloat = value;

            return returnFloat.ToString("F99").TrimEnd("0".ToCharArray()).Replace(",", ".").TrimEnd(".".ToCharArray());
        }

        public static Int32 GetUnixTimestamp()
        {
            return (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
        }

        public static void LoadCurrencyTemplatesCSV()
        {
            if (m_currencyTemplate != null)
                return;

            m_currencyTemplate = new Dictionary<UInt32, CurrencyTypesEntry>();

            List<String> allLines = new List<String>(File.ReadAllLines("Ressources/CurrencyTypes.db2.csv"));

            allLines.RemoveAt(0);

            foreach (String line in allLines)
            {
                CurrencyTypesEntry currencyTemplate = new CurrencyTypesEntry();
                String[] values = line.Split(',');

                int index = 0;
                currencyTemplate.ID                         = Convert.ToUInt32(values[index++]);
                currencyTemplate.Name                       = values[index++];
                currencyTemplate.MaxQty                     = Convert.ToUInt32(values[index++]);
                currencyTemplate.MaxEarnablePerWeek         = Convert.ToUInt32(values[index++]);
                currencyTemplate.Flags                      = Convert.ToUInt32(values[index++]);
                currencyTemplate.Description                = values[index++];
                currencyTemplate.CategoryID                 = Convert.ToByte(values[index++]);
                currencyTemplate.SpellCategory              = Convert.ToByte(values[index++]);
                currencyTemplate.Quality                    = Convert.ToByte(values[index++]);
                currencyTemplate.InventoryIconFileDataID    = Convert.ToUInt32(values[index++]);
                //currencyTemplate.SpellWeight                = Convert.ToUInt32(values[index++]);

                m_currencyTemplate.Add(currencyTemplate.ID, currencyTemplate);
            }
        }

        public static void LoadItemExtendedCostDb2CSV()
        {
            if (m_itemExtendedCost != null)
                return;

            m_itemExtendedCost = new List<ItemExtendedCostEntry>();

            List<String> allLines = new List<String>(File.ReadAllLines("Ressources/ItemExtendedCost.db2.csv"));

            foreach (String line in allLines)
            {
                ItemExtendedCostEntry extendedCost = new ItemExtendedCostEntry();
                String[] values = line.Split(',');
                List<UInt32> intValues = new List<UInt32>();

                foreach (String value in values)
                    intValues.Add(Convert.ToUInt32(value));

                extendedCost.RequiredItem                   = new List<UInt32>();
                extendedCost.RequiredItemCount              = new List<UInt16>();
                extendedCost.RequiredCurrency               = new List<UInt16>();
                extendedCost.RequiredCurrencyCount          = new List<UInt32>();

                extendedCost.ID                             = intValues[0];

                for (int i = 0; i < 5; ++i)
                    extendedCost.RequiredItem.Add(intValues[1 + i]);

                for (int i = 0; i < 5; ++i)
                    extendedCost.RequiredCurrencyCount.Add(intValues[6 + i]);

                for (int i = 0; i < 5; ++i)
                    extendedCost.RequiredItemCount.Add((UInt16)intValues[11 + i]);

                extendedCost.RequiredPersonalArenaRating    = (UInt16)intValues[16];

                for (int i = 0; i < 5; ++i)
                    extendedCost.RequiredCurrency.Add((UInt16)intValues[17 + i]);

                extendedCost.RequiredArenaSlot          = (byte)intValues[22];
                extendedCost.RequiredFactionId          = (byte)intValues[23];
                extendedCost.RequiredFactionStanding    = (byte)intValues[24];
                extendedCost.RequirementFlags           = (byte)intValues[25];
                extendedCost.RequiredAchievement        = (byte)intValues[26];

                m_itemExtendedCost.Add(extendedCost);
            }
        }

        public static UInt32 GetExtendedCostId(List<Int32> itemId, List<Int32> itemCount, List<Int32> currencyId, List<Int32> currencyCount)
        {
            if (itemId.Count != itemCount.Count)
                return 0;

            if (currencyId.Count != currencyCount.Count)
                return 0;

            if (itemId.Count == 0 && currencyId.Count == 0)
                return 0;

            LoadItemExtendedCostDb2CSV();
            LoadCurrencyTemplatesCSV();

            foreach (ItemExtendedCostEntry extendedCostEntry in m_itemExtendedCost)
            {
                bool notMatch = false;

                for (int i = 0; i < 5; ++i)
                {
                    if (itemId.Count < (i + 1))
                        break;

                    if (extendedCostEntry.RequiredItem[i] != itemId[i])
                    {
                        notMatch = true;
                        break;
                    }

                    if (extendedCostEntry.RequiredItemCount[i] != itemCount[i])
                    {
                        notMatch = true;
                        break;
                    }
                }

                if (notMatch)
                    continue;

                for (int i = 0; i < 5; ++i)
                {
                    if (currencyId.Count < (i + 1))
                        break;

                    if (extendedCostEntry.RequiredCurrency[i] != currencyId[i])
                    {
                        notMatch = true;
                        break;
                    }

                    int precision = (int)m_currencyTemplate[extendedCostEntry.RequiredCurrency[i]].GetPrecision();
                    if (extendedCostEntry.RequiredCurrencyCount[i] != (currencyCount[i] * precision))
                    {
                        notMatch = true;
                        break;
                    }
                }

                if (!notMatch)
                    return extendedCostEntry.ID;
            }

            return 0;
        }

        public static void LoadPlayerConditionDb2CSV()
        {
            if (m_playerConditions != null)
                return;

            m_playerConditions = new List<PlayerConditionEntry>();

            List<String> allLines = new List<String>(File.ReadAllLines("Ressources/PlayerCondition.db2.csv"));

            allLines.RemoveAt(0);

            foreach (String line in allLines)
            {
                PlayerConditionEntry playerCondition = new PlayerConditionEntry();
                String[] values = line.Split(',');

                playerCondition.PrevQuestID = new List<Int32>();

                playerCondition.ID              = Convert.ToInt32(values[0]);
                playerCondition.PrevQuestLogic  = Convert.ToInt32(values[36]);

                for (int i = 0; i < 4; ++i)
                    playerCondition.PrevQuestID.Add(Convert.ToInt32(values[37 + i]));

                m_playerConditions.Add(playerCondition);
            }
        }

        enum PrevQuestLogicFlags
        {
            Unk1                = 0x00001,
            TrackingQuestId1    = 0x10000,
            TrackingQuestId2    = 0x20000
        };

        public static Int32 GetPlayerConditionForTreasure(UInt32 questId)
        {
            LoadPlayerConditionDb2CSV();

            foreach (PlayerConditionEntry playerCondition in m_playerConditions)
            {
                if ((playerCondition.PrevQuestLogic & (int)PrevQuestLogicFlags.TrackingQuestId1) != 0)
                    if (playerCondition.PrevQuestID[0] == questId)
                        return playerCondition.ID;

                if ((playerCondition.PrevQuestLogic & (int)PrevQuestLogicFlags.TrackingQuestId2) != 0)
                    if (playerCondition.PrevQuestID[1] == questId)
                        return playerCondition.ID;
            }

            return 0;
        }

        public static UInt32 GetClassMaskFromClassId(String strClassId)
        {
            UInt32 classId = UInt32.Parse(strClassId);
            return Convert.ToUInt32(Math.Pow(2, classId - 1));
        }

        public static void LoadBaseHps()
        {
            if (m_baseHpForLevelAndClass != null)
                return;

            List<String> BaseHpGts = new List<String>()
            {
                "NpcTotalHp.txt",
                "NpcTotalHpExp1.txt",
                "NpcTotalHpExp2.txt",
                "NpcTotalHpExp3.txt",
                "NpcTotalHpExp4.txt",
                "NpcTotalHpExp5.txt",
                "NpcTotalHpExp6.txt",
            };

            m_baseHpForLevelAndClass = new Dictionary<int, Dictionary<int, Dictionary<int, float>>>();

            for (int i = 0; i < BaseHpGts.Count; ++i)
            {
                List<String> allLines = new List<String>(File.ReadAllLines("Ressources/" + BaseHpGts[i]));

                allLines.RemoveAt(0);
                m_baseHpForLevelAndClass.Add(i, new Dictionary<int, Dictionary<int, float>>());

                for (int rowClassIndex = 0; rowClassIndex < allLines.Count; ++rowClassIndex)
                {
                    String[] values = allLines[rowClassIndex].Split('\t');
                    int level = int.Parse(values[0]);

                    m_baseHpForLevelAndClass[i].Add(level, new Dictionary<int, float>());
                    m_baseHpForLevelAndClass[i][level].Add((int)UnitClass.UNIT_CLASS_ROGUE,  float.Parse(values[1].Replace(".", ",")));
                    m_baseHpForLevelAndClass[i][level].Add((int)UnitClass.UNIT_CLASS_MAGE,   float.Parse(values[4].Replace(".", ",")));
                    m_baseHpForLevelAndClass[i][level].Add((int)UnitClass.UNIT_CLASS_PALADIN,float.Parse(values[5].Replace(".", ",")));
                    m_baseHpForLevelAndClass[i][level].Add((int)UnitClass.UNIT_CLASS_WARRIOR,float.Parse(values[9].Replace(".", ",")));
                }
            }
        }

        public static String GetHealthModifier(float currentHealth, int exp, int level, int classIndex)
        {
            LoadBaseHps();

            float baseHp = m_baseHpForLevelAndClass[exp][level][classIndex];

            return NormalizeFloat(currentHealth / baseHp);
        }

        private static List<ItemExtendedCostEntry> m_itemExtendedCost = null;
        private static List<PlayerConditionEntry> m_playerConditions = null;
        private static Dictionary<UInt32, CurrencyTypesEntry> m_currencyTemplate = null;
        //                        Exp             Level           Class
        private static Dictionary<int, Dictionary<int, Dictionary<int, float>>> m_baseHpForLevelAndClass;
    }
}
