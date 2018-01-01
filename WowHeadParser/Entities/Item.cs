/*
 * * Created by Traesh for AshamaneProject (https://github.com/AshamaneProject)
 */
using Newtonsoft.Json;
using Sql;
using System;
using System.Collections.Generic;

namespace WowHeadParser.Entities
{
    class Item : Entity
    {
        struct ItemParsing
        {
            public int id;
            public string name;
            public string namedesc;
            public string description;
            public List<int> specs;
            public int level;
            public int classs;
            public int subclass;
            public int quality;
        }

        public struct ItemSpellParsing
        {
            public int id;
        }

        public struct ItemCreateItemParsing
        {
            public int id;
        }

        public struct ItemLootTemplateParsing
        {
            public int id;
            public int count;
            public int[] stack;
        }

        public struct ItemDroppedByTemplateParsing
        {
            public int id;
            public int count;
            public int outof;
        }

        public Item()
        {
            m_data.id = 0;
        }

        public Item(int id)
        {
            m_data.id = id;
        }

        public override String GetWowheadUrl()
        {
            return GetWowheadBaseUrl() + "/item=" + m_data.id + "&bonus=524";
        }

        public override bool ParseSingleJson(int id = 0)
        {
            if (m_data.id == 0 && id == 0)
                return false;
            else if (m_data.id == 0 && id != 0)
                m_data.id = id;

            String itemHtml = Tools.GetHtmlFromWowhead(GetWowheadUrl());

            String dataPattern = @"\$\.extend\(g_items\[" + m_data.id + @"\], (.+)\);";
            String qualityPattern = @"_\[" + m_data.id + @"\]" + "={\"name_frfr\":\"(?:.+?)\",\"quality\":([0-9])";
            String itemSpellPattern = @"new Listview\(\{template: 'spell', id: 'reagent-for', name: LANG\.tab_reagentfor, tabs: tabsRelated, parent: 'lkljbjkb574',.*(?:\n)?.*data: (.+)\}\);";
            String itemCreatePattern = @"new Listview\(\{template: 'item', id: 'creates', name: LANG\.tab_creates, tabs: tabsRelated, parent: 'lkljbjkb574', sort:\['name'\],.*(?:\n)?.*data: (.+)}\);";
            String itemLootTemplatePattern = @"new Listview\(\{template: 'item', id: 'contains', name: LANG\.tab_contains, tabs: tabsRelated, parent: 'lkljbjkb574',\n* *extraCols: \[Listview\.extraCols\.count, Listview.extraCols.percent\], sort:\['-percent', 'name'\],\n* *computeDataFunc: Listview\.funcBox\.initLootTable, note: WH\.sprintf\(LANG\.lvnote_itemopening, [0-9]+\),\n* *_totalCount: ([0-9]+), data: (.+)\}\);";
            String itemDroppedByPattern = @"new Listview\(\{template: 'npc', id: 'dropped-by', name: LANG\.tab_droppedby, tabs: tabsRelated, parent: 'lkljbjkb574',\n* *hiddenCols: \['type'\], extraCols: \[Listview.extraCols.count, Listview.extraCols.percent\], sort:\['-percent', '-count', 'name'\],\n* *computeDataFunc: Listview.funcBox.initLootTable, data: (.+)}\);";

            String itemDataJSon = Tools.ExtractJsonFromWithPattern(itemHtml, dataPattern);
            if (itemDataJSon != null)
            {
                m_data = JsonConvert.DeserializeObject<ItemParsing>(itemDataJSon);
            }

            String itemQuality = Tools.ExtractJsonFromWithPattern(itemHtml, qualityPattern);
            if (itemQuality != null)
            {
                Int32.TryParse(itemQuality, out m_data.quality);
            }

            String itemSpellJSon = Tools.ExtractJsonFromWithPattern(itemHtml, itemSpellPattern);
            if (itemSpellJSon != null)
            {
                m_itemSpellDatas = JsonConvert.DeserializeObject<ItemSpellParsing[]>(itemSpellJSon);
            }

            String itemCreateJSon = Tools.ExtractJsonFromWithPattern(itemHtml, itemCreatePattern);
            if (itemCreateJSon != null)
            {
                m_itemCreateItemDatas = JsonConvert.DeserializeObject<ItemCreateItemParsing[]>(itemCreateJSon);
            }

            String lootMaxCountStr      = Tools.ExtractJsonFromWithPattern(itemHtml, itemLootTemplatePattern, 0);
            m_lootMaxCount              = lootMaxCountStr != null ? Int32.Parse(lootMaxCountStr): 0;
            String itemLootTemplateJSon = Tools.ExtractJsonFromWithPattern(itemHtml, itemLootTemplatePattern, 1);
            if (itemLootTemplateJSon != null)
            {
                m_itemLootTemplateDatas = JsonConvert.DeserializeObject<ItemLootTemplateParsing[]>(itemLootTemplateJSon);
            }

            String itemDroppedByJson = Tools.ExtractJsonFromWithPattern(itemHtml, itemDroppedByPattern);
            if (itemDroppedByJson != null)
            {
                m_itemDroppedByDatas = JsonConvert.DeserializeObject<ItemDroppedByTemplateParsing[]>(itemDroppedByJson);
            }

            return true;
        }

        public override String GetSQLRequest()
        {
            String returnSql = "";

            if (m_data.id == 0 || isError)
                return returnSql;

            if (IsCheckboxChecked("locale"))
            {
                int localeIndex = Properties.Settings.Default.localIndex;

                if (localeIndex >= 1 && localeIndex <= 10)
                {
                    SqlBuilder m_itemLocalesBuilder = new SqlBuilder("item_sparse_locale", "ID", SqlQueryType.InsertOrUpdate);

                    String locale = "";

                    switch (localeIndex)
                    {
                        case 1:  locale = "koKR"; break;
                        case 2:  locale = "frFR"; break;
                        case 3:  locale = "deDE"; break;
                        case 4:  locale = "zhCN"; break;
                        case 5:  locale = "zhTW"; break;
                        case 6:  locale = "esES"; break;
                        case 7:  locale = "esMX"; break;
                        case 8:  locale = "ruRU"; break;
                        case 9:  locale = "ptPT"; break;
                        case 10: locale = "itIT"; break;
                    }

                    m_itemLocalesBuilder.SetFieldsNames("Name_" + locale);
                    m_itemLocalesBuilder.AppendFieldsValue(m_data.id, m_data.name.Substring(1) ?? "");
                    returnSql += m_itemLocalesBuilder.ToString() + "\n";
                }
            }

            if (IsCheckboxChecked("create item") && m_itemCreateItemDatas != null)
            {
                m_spellLootTemplateBuilder = new SqlBuilder("spell_loot_template", "entry", SqlQueryType.InsertIgnore);
                m_spellLootTemplateBuilder.SetFieldsNames("Item", "Reference", "Chance", "QuestRequired", "LootMode", "GroupId", "MinCount", "MaxCount", "Comment");

                foreach (ItemCreateItemParsing itemLootData in m_itemCreateItemDatas)
                    m_spellLootTemplateBuilder.AppendFieldsValue(m_itemSpellDatas[0].id, // Entry
                                                                 itemLootData.id, // Item
                                                                 0, // Reference
                                                                 "100", // Chance
                                                                 0, // QuestRequired
                                                                 1, // LootMode
                                                                 0, // GroupId
                                                                 "1", // MinCount
                                                                 "1", // MaxCount
                                                                 ""); // Comment

                returnSql += m_spellLootTemplateBuilder.ToString() + "\n";
            }

            if (IsCheckboxChecked("loot") && m_itemLootTemplateDatas != null)
            {
                m_itemLootTemplateBuilder = new SqlBuilder("item_loot_template", "entry", SqlQueryType.InsertIgnore);
                m_itemLootTemplateBuilder.SetFieldsNames("Item", "Reference", "Chance", "QuestRequired", "LootMode", "GroupId", "MinCount", "MaxCount", "Comment");

                foreach (ItemLootTemplateParsing itemLootData in m_itemLootTemplateDatas)
                {
                    String percent = ((float)itemLootData.count / (float)m_lootMaxCount * 100).ToString().Replace(",", ".");

                    int minLootCount = itemLootData.stack.Length >= 1 ? itemLootData.stack[0] : 1;
                    int maxLootCount = itemLootData.stack.Length >= 2 ? itemLootData.stack[1] : minLootCount;

                    m_itemLootTemplateBuilder.AppendFieldsValue(m_data.id, // Entry
                                                                itemLootData.id, // Item
                                                                0, // Reference
                                                                percent, // Chance
                                                                0, // QuestRequired
                                                                1, // LootMode
                                                                0, // GroupId
                                                                minLootCount, // MinCount
                                                                maxLootCount, // MaxCount
                                                                ""); // Comment
                }

                returnSql += m_itemLootTemplateBuilder.ToString() + "\n";
            }

            if (IsCheckboxChecked("dropped by") && m_itemDroppedByDatas != null)
            {
                m_itemDroppedByBuilder = new SqlBuilder("creature_loot_template", "entry", SqlQueryType.InsertIgnore);
                m_itemDroppedByBuilder.SetFieldsNames("item", "ChanceOrQuestChance", "lootmode", "groupid", "mincountOrRef", "maxcount", "itemBonuses");

                foreach (ItemDroppedByTemplateParsing itemDroppedByData in m_itemDroppedByDatas)
                {
                    float percent = ((float)itemDroppedByData.count / (float)itemDroppedByData.outof) * 100.0f;
                    String percentStr = Tools.NormalizeFloat(percent);

                    m_itemDroppedByBuilder.AppendFieldsValue(itemDroppedByData.id, m_data.id, percentStr, 1, 0, "1", "1", "");
                }

                returnSql += "DELETE FROM creature_loot_template WHERE item = " + m_data.id + ";\n";
                returnSql += m_itemDroppedByBuilder.ToString() + "\n";
            }

            if (IsCheckboxChecked("export pvp"))
            {
                if (m_data.level != 620 && m_data.level != 660 && // PVP
                    m_data.level != 630 && m_data.level != 655)   // PVE
                    return "";

                if ((m_data.level == 620 || m_data.level == 630) && m_data.quality != 3)
                    return "";

                if ((m_data.level == 660 || m_data.level == 655) && m_data.quality != 4)
                    return "";

                if (m_data.classs != 2 && m_data.classs != 4)
                    return "";

                if ((m_data.level == 620 || m_data.level == 660) && m_data.namedesc == "Saison 1 de Warlords")
                {
                    returnSql += "INSERT INTO item_wod (id, ilevel, pvp, spec) VALUES (" + m_data.id + ", " + m_data.level + ", 1, '" + string.Join(" ", m_data.specs) + "');";
                }

                if ((m_data.level == 630 || m_data.level == 655) && m_data.namedesc != "Saison 1 de Warlords")
                {
                    returnSql += "INSERT INTO item_wod (id, ilevel, pvp, spec) VALUES (" + m_data.id + ", " + m_data.level + ", 0, '" + string.Join(" ", m_data.specs) + "');";
                }
            }

            return returnSql;
        }

        protected int m_lootMaxCount;

        private ItemParsing m_data;
        protected ItemSpellParsing[] m_itemSpellDatas;
        protected ItemCreateItemParsing[] m_itemCreateItemDatas;
        protected ItemLootTemplateParsing[] m_itemLootTemplateDatas;
        protected ItemDroppedByTemplateParsing[] m_itemDroppedByDatas;

        protected SqlBuilder m_spellLootTemplateBuilder;
        protected SqlBuilder m_itemLootTemplateBuilder;
        protected SqlBuilder m_itemDroppedByBuilder;
    }
}
