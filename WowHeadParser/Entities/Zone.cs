/*
 * Created by Traesh (http://www.farahlon.com)
 */
using Newtonsoft.Json;
using Sql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace WowHeadParser.Entities
{
    struct ZoneTemplateParsing
    {
        public int id;
    }

    struct FishingLootParsing
    {
        public int id;
        public int classs;
        public int count;
        public dynamic modes;
        public int[] stack;

        public string percent;
    }

    class Zone : Entity
    {
        public Zone()
        {
            m_zoneTemplateData.id = 0;
        }

        public Zone(int id)
        {
            m_zoneTemplateData.id = id;
        }

        public override String GetWowheadUrl()
        {
            return GetWowheadBaseUrl() + "/zone=" + m_zoneTemplateData.id;
        }

        /*public override List<Entity> GetIdsFromZone(String zoneId, String zoneHtml)
        {
            String pattern = @"new Listview\({template: 'npc', id: 'npcs', name: LANG\.tab_npcs, tabs: tabsRelated, parent: 'lkljbjkb574', note: \$WH\.sprintf\(LANG\.lvnote_filterresults, '\/npcs\?filter=cr=6;crs=" + zoneId + @";crv=0'\), data: (.+)}\);";
            String creatureJSon = Tools.ExtractJsonFromWithPattern(zoneHtml, pattern);

            List<CreatureTemplateParsing> parsingArray = JsonConvert.DeserializeObject<List<CreatureTemplateParsing>>(creatureJSon);
            List<Entity> tempArray = new List<Entity>();
            foreach (CreatureTemplateParsing creatureTemplateStruct in parsingArray)
            {
                Creature creature = new Creature(creatureTemplateStruct.id);
                tempArray.Add(creature);
            }

            return tempArray;
        }*/

        public override bool ParseSingleJson(int id = 0)
        {
            if (m_zoneTemplateData.id == 0 && id == 0)
                return false;
            else if (m_zoneTemplateData.id == 0 && id != 0)
                m_zoneTemplateData.id = id;

            String zoneHtml = Tools.GetHtmlFromWowhead(GetWowheadUrl());

            if (zoneHtml.Contains("inputbox-error"))
                return false;

            String dataPattern = @"\$\.extend\(g_npcs\[" + m_creatureTemplateData.id + @"\], (.+)\);";
            String modelPattern = @"ModelViewer\.show\(\{ type: [0-9]+, typeId: " + m_creatureTemplateData.id + @", displayId: ([0-9]+)";
            String vendorPattern = @"new Listview\({template: 'item', id: 'sells', name: LANG.tab_sells, tabs: tabsRelated, parent: 'lkljbjkb574', extraCols: \[Listview\.extraCols\.cost(?:, _)*\], note: \$WH\.sprintf\(LANG.lvnote_filterresults, '\/items\?filter=cr=129;crs=0;crv=" + m_creatureTemplateData.id + @"'\), data: (.+)}\);";
            String creatureLootPattern = @"new Listview\({template: 'item', id: 'drops', name: LANG\.tab_drops, tabs: tabsRelated, parent: 'lkljbjkb574', extraCols: \[Listview\.extraCols\.count, Listview\.extraCols\.percent(?:, Listview.extraCols.mode)?\],  showLootSpecs: [0-9],sort:\['-percent', 'name'\], _totalCount: [0-9]+, computeDataFunc: Listview\.funcBox\.initLootTable, onAfterCreate: Listview\.funcBox\.addModeIndicator, data: (.+)}\);";
            String creatureSkinningPattern = @"new Listview\(\{template: 'item', id: 'skinning', name: LANG\.tab_skinning, tabs: tabsRelated, parent: 'lkljbjkb574', extraCols: \[Listview\.extraCols\.count, Listview\.extraCols\.percent\], sort:\['-percent', 'name'\], computeDataFunc: Listview\.funcBox\.initLootTable, note: \$WH\.sprintf\(LANG\.lvnote_npcskinning, [0-9]+\), _totalCount: ([0-9]+), data: (.+)}\);";

            String creatureTemplateDataJSon = Tools.ExtractJsonFromWithPattern(creatureHtml, dataPattern);
            CreatureTemplateParsing creatureTemplateData = JsonConvert.DeserializeObject<CreatureTemplateParsing>(creatureTemplateDataJSon);
            SetCreatureTemplateData(creatureTemplateData);

            String npcVendorJSon = Tools.ExtractJsonFromWithPattern(creatureHtml, vendorPattern);
            if (npcVendorJSon != null)
            {
                NpcVendorParsing[] npcVendorDatas = JsonConvert.DeserializeObject<NpcVendorParsing[]>(npcVendorJSon);
                SetNpcVendorData(npcVendorDatas);
            }

            String creatureLootJSon = Tools.ExtractJsonFromWithPattern(creatureHtml, creatureLootPattern);
            if (creatureLootJSon != null)
            {
                CreatureLootParsing[] creatureLootDatas = JsonConvert.DeserializeObject<CreatureLootParsing[]>(creatureLootJSon);
                SetCreatureLootData(creatureLootDatas);
            }

            String creatureSkinningCount = Tools.ExtractJsonFromWithPattern(creatureHtml, creatureSkinningPattern, 1);
            String creatureSkinningJSon = Tools.ExtractJsonFromWithPattern(creatureHtml, creatureSkinningPattern, 2);
            if (creatureSkinningJSon != null)
            {
                CreatureLootParsing[] creatureLootDatas = JsonConvert.DeserializeObject<CreatureLootParsing[]>(creatureSkinningJSon);
                SetCreatureSkinningData(creatureLootDatas, Int32.Parse(creatureSkinningCount));
            }

            String modelId = Tools.ExtractJsonFromWithPattern(creatureHtml, modelPattern);
            m_modelid = modelId != null ? Int32.Parse(modelId): 0;
            return true;
        }

        public void SetCreatureTemplateData(CreatureTemplateParsing creatureData)
        {
            m_creatureTemplateData = creatureData;

            m_isBoss = false;
            m_faction = GetFactionFromReact();

            if (m_creatureTemplateData.minlevel == 9999 || m_creatureTemplateData.maxlevel == 9999)
            {
                m_isBoss = true;
                m_creatureTemplateData.minlevel = 100;
                m_creatureTemplateData.maxlevel = 100;
            }

            m_subname = m_creatureTemplateData.tag ?? "";
        }

        public void SetNpcVendorData(NpcVendorParsing[] npcVendorDatas)
        {
            for (uint i = 0; i < npcVendorDatas.Length; ++i)
            {
                npcVendorDatas[i].avail = npcVendorDatas[i].avail == -1 ? 0 : npcVendorDatas[i].avail;
                npcVendorDatas[i].incrTime = npcVendorDatas[i].avail != 0 ? 3600 : 0;

                try
                {
                    int cost = Convert.ToInt32(npcVendorDatas[i].cost[0]);
                    npcVendorDatas[i].integerCost = cost;

                    List<Int32> itemId          = new List<Int32>();
                    List<Int32> itemCount       = new List<Int32>();

                    List<Int32> currencyId      = new List<Int32>();
                    List<Int32> currencyCount   = new List<Int32>();

                    foreach (JArray itemCost in npcVendorDatas[i].cost[2])
                    {
                        itemId.Add(Convert.ToInt32(itemCost[0]));
                        itemCount.Add(Convert.ToInt32(itemCost[1]));
                    }

                    foreach (JArray currencyCost in npcVendorDatas[i].cost[1])
                    {
                        currencyId.Add(Convert.ToInt32(currencyCost[0]));
                        currencyCount.Add(Convert.ToInt32(currencyCost[1]));
                    }

                    npcVendorDatas[i].integerExtendedCost = (int)Tools.GetExtendedCostId(itemId, itemCount, currencyId, currencyCount);
                }
                catch (Exception ex)
                {
                    npcVendorDatas[i].integerCost = 0;
                    npcVendorDatas[i].integerExtendedCost = 0;
                }
            }

            m_npcVendorDatas = npcVendorDatas;
        }

        public void SetCreatureLootData(CreatureLootParsing[] creatureLootDatas)
        {
            for (uint i = 0; i < creatureLootDatas.Length; ++i)
            {
                float count = (float)Convert.ToDouble(creatureLootDatas[i].modes["4"]["count"]);
                float outof = (float)Convert.ToDouble(creatureLootDatas[i].modes["4"]["outof"]);
                float percent = count * 100 / outof;

                if (creatureLootDatas[i].classs == 12)
                    percent *= -1;

                percent = Tools.NormalizeFloat(percent);

                creatureLootDatas[i].percent = percent.ToString().Replace(",", ".");
            }

            m_creatureLootDatas = creatureLootDatas;
        }

        public void SetCreatureSkinningData(CreatureLootParsing[] creatureSkinningDatas, int totalCount)
        {
            for (uint i = 0; i < creatureSkinningDatas.Length; ++i)
            {
                float percent = (float)creatureSkinningDatas[i].count * 100 / (float)totalCount;

                percent = Tools.NormalizeFloat(percent);

                creatureSkinningDatas[i].percent = percent.ToString().Replace(",", ".");
            }

            m_creatureSkinningDatas = creatureSkinningDatas;
        }

        private int GetFactionFromReact()
        {
            if (m_creatureTemplateData.react == null)
                return 14;

            if (m_creatureTemplateData.react[(int)reactOrder.ALLIANCE] == "1" && m_creatureTemplateData.react[(int)reactOrder.HORDE] == "1")
                return 35; // Ennemis
            else if (m_creatureTemplateData.react[(int)reactOrder.ALLIANCE] == "1" && m_creatureTemplateData.react[(int)reactOrder.HORDE] == "-1")
                return 11; // Hurlevent
            else if (m_creatureTemplateData.react[(int)reactOrder.ALLIANCE] == "-1" && m_creatureTemplateData.react[(int)reactOrder.HORDE] == "1")
                return 85; // Orgrimmar
            else if (m_creatureTemplateData.react[(int)reactOrder.ALLIANCE] == "0" && m_creatureTemplateData.react[(int)reactOrder.HORDE] == "0")
                return 2240; // Neutral

            return 14;
        }

        public override String GetSQLRequest()
        {
            String returnSql = "";

            if (m_creatureTemplateData.id == 0 || isError)
                return returnSql;

            // Creature Template
            if (IsCheckboxChecked("template"))
            {
                m_creatureTemplateBuilder = new SqlBuilder("creature_template", "entry");
                m_creatureTemplateBuilder.SetFieldsNames("minlevel", "maxlevel", "name", "subname", "modelid1", "rank", "type", "family");

                m_creatureTemplateBuilder.AppendFieldsValue(m_creatureTemplateData.id, m_creatureTemplateData.minlevel, m_creatureTemplateData.maxlevel, m_creatureTemplateData.name, m_subname ?? "", m_modelid, m_isBoss ? "3" : "0", m_creatureTemplateData.type, m_creatureTemplateData.family);
                returnSql += m_creatureTemplateBuilder.ToString() + "\n";
            }

            // Locales
            if (IsCheckboxChecked("locale"))
            {
                int localeIndex = Properties.Settings.Default.localIndex;

                if (localeIndex != 0)
                {
                    m_creatureLocalesBuilder = new SqlBuilder("locales_creature", "entry");
                    m_creatureLocalesBuilder.SetFieldsNames("name_loc" + localeIndex, "subname_loc" + localeIndex);

                    m_creatureLocalesBuilder.AppendFieldsValue(m_creatureTemplateData.id, m_creatureTemplateData.name, m_subname ?? "");
                    returnSql += m_creatureLocalesBuilder.ToString() + "\n";
                }
                else
                {
                    m_creatureLocalesBuilder = new SqlBuilder("creature_template", "entry");
                    m_creatureLocalesBuilder.SetFieldsNames("name", "subname");

                    m_creatureLocalesBuilder.AppendFieldsValue(m_creatureTemplateData.id, m_creatureTemplateData.name, m_subname ?? "");
                    returnSql += m_creatureLocalesBuilder.ToString() + "\n";
                }
            }

            if (IsCheckboxChecked("vendor") && m_npcVendorDatas != null)
            {
                m_npcVendorBuilder = new SqlBuilder("npc_vendor", "entry", SqlQueryType.DeleteInsert);
                m_npcVendorBuilder.SetFieldsNames("slot", "item", "maxcount", "incrtime", "ExtendedCost", "type", "PlayerConditionID");

                foreach (NpcVendorParsing npcVendorData in m_npcVendorDatas)
                    m_npcVendorBuilder.AppendFieldsValue(m_creatureTemplateData.id, npcVendorData.slot, npcVendorData.id, npcVendorData.avail, npcVendorData.incrTime, npcVendorData.integerExtendedCost, 1, 0);

                returnSql += m_npcVendorBuilder.ToString() + "\n";
            }

            if (IsCheckboxChecked("loot") && m_creatureLootDatas != null)
            {
                m_creatureLootBuilder = new SqlBuilder("creature_loot_template", "entry", SqlQueryType.InsertIgnore);
                m_creatureLootBuilder.SetFieldsNames("item", "ChanceOrQuestChance", "lootmode", "groupid", "mincountOrRef", "maxcount", "itemBonuses");

                returnSql += "UPDATE creature_template SET lootid = " + m_creatureTemplateData.id + " WHERE entry = " + m_creatureTemplateData.id + " AND lootid = 0;\n";
                foreach (CreatureLootParsing creatureLootData in m_creatureLootDatas)
                    m_creatureLootBuilder.AppendFieldsValue(m_creatureTemplateData.id, creatureLootData.id, creatureLootData.percent, 1, 0, creatureLootData.stack[0], creatureLootData.stack[1], "");

                returnSql += m_creatureLootBuilder.ToString() + "\n";
            }

            if (IsCheckboxChecked("skinning") && m_creatureSkinningDatas != null)
            {
                m_creatureSkinningBuilder = new SqlBuilder("skinning_loot_template", "entry", SqlQueryType.InsertIgnore);
                m_creatureSkinningBuilder.SetFieldsNames("item", "ChanceOrQuestChance", "lootmode", "groupid", "mincountOrRef", "maxcount", "itemBonuses");

                returnSql += "UPDATE creature_template SET skinloot = " + m_creatureTemplateData.id + " WHERE entry = " + m_creatureTemplateData.id + " AND skinloot = 0;\n";
                foreach (CreatureLootParsing creatureSkinningData in m_creatureSkinningDatas)
                    m_creatureSkinningBuilder.AppendFieldsValue(m_creatureTemplateData.id, creatureSkinningData.id, creatureSkinningData.percent, 1, 0, creatureSkinningData.stack[0], creatureSkinningData.stack[1], "");

                returnSql += m_creatureSkinningBuilder.ToString() + "\n";
            }

            return returnSql;
        }

        private int m_faction;
        private bool m_isBoss;
        private int m_modelid;
        private String m_subname;

        protected ZoneTemplateParsing m_zoneTemplateData;
        protected NpcVendorParsing[] m_npcVendorDatas;
        protected CreatureLootParsing[] m_creatureLootDatas;
        protected CreatureLootParsing[] m_creatureSkinningDatas;

        protected SqlBuilder m_creatureTemplateBuilder;
        protected SqlBuilder m_creatureLocalesBuilder;
        protected SqlBuilder m_npcVendorBuilder;
        protected SqlBuilder m_creatureLootBuilder;
        protected SqlBuilder m_creatureSkinningBuilder;
    }
}
