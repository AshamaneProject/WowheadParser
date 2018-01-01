/*
 * * Created by Traesh for AshamaneProject (https://github.com/AshamaneProject)
 */
using Newtonsoft.Json;
using Sql;
using System;

namespace WowHeadParser.Entities
{
    class ZoneEntity : Entity
    {
        public struct ZoneParsing
        {
            public int id;
            public string name;
        }

        public struct FishingParsing
        {
            public int id;
            public int count;
        }

        public ZoneEntity()
        {
            m_data.id = 0;
            m_itemMaxCount = 0;
        }

        public ZoneEntity(int id)
        {
            m_data.id = id;
            m_itemMaxCount = 0;
        }

        public override String GetWowheadUrl()
        {
            return GetWowheadBaseUrl() + "/zone=" + m_data.id;
        }

        public override bool ParseSingleJson(int id = 0)
        {
            if (m_data.id == 0 && id == 0)
                return false;
            else if (m_data.id == 0 && id != 0)
                m_data.id = id;

            String zoneHTML = Tools.GetHtmlFromWowhead(GetWowheadUrl());

            String fishingPattern = @"new Listview\(\{template: 'item', id: 'fishing', name: LANG\.tab_fishing, tabs: tabsRelated, parent: 'lkljbjkb574', extraCols: \[Listview\.extraCols\.count, Listview\.extraCols.percent\], sort:\['-percent', 'name'\], computeDataFunc: Listview\.funcBox\.initLootTable, note: \$WH\.sprintf\(LANG\.lvnote_zonefishing, [0-9]+\), _totalCount: ([0-9]+), data: (.+)\}\);";

            m_itemMaxCount = Int32.Parse(Tools.ExtractJsonFromWithPattern(zoneHTML, fishingPattern, 0));
            String fishingJSon = Tools.ExtractJsonFromWithPattern(zoneHTML, fishingPattern, 1);
            if (fishingJSon != null)
            {
                m_fishingDatas = JsonConvert.DeserializeObject<FishingParsing[]>(fishingJSon);
            }

            return true;
        }

        public override String GetSQLRequest()
        {
            String returnSql = "";

            if (m_data.id == 0 || isError)
                return returnSql;

            if (IsCheckboxChecked("Fishing") && m_fishingDatas != null)
            {
                m_FishingLootTemplateBuilder = new SqlBuilder("fishing_loot_template", "entry", SqlQueryType.InsertIgnore);
                m_FishingLootTemplateBuilder.SetFieldsNames("item", "ChanceOrQuestChance", "lootmode", "groupid", "mincountOrRef", "maxcount", "itemBonuses");

                foreach (FishingParsing fishingLootdata in m_fishingDatas)
                {
                    String percent = ((float)fishingLootdata.count / (float)m_itemMaxCount * 100).ToString().Replace(",", ".");
                    m_FishingLootTemplateBuilder.AppendFieldsValue(m_data.id, fishingLootdata.id, percent, 1, 0, "1", "1", "");
                }

                returnSql += m_FishingLootTemplateBuilder.ToString() + "\n";
            }

            return returnSql;
        }

        public ZoneParsing m_data;

        protected FishingParsing[] m_fishingDatas;
        protected int m_itemMaxCount;

        protected SqlBuilder m_spellLootTemplateBuilder;
        protected SqlBuilder m_FishingLootTemplateBuilder;
    }
}
