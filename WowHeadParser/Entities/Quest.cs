/*
 * * Created by Traesh for AshamaneProject (https://github.com/AshamaneProject)
 */
using Newtonsoft.Json;
using Sql;
using System;
using System.Collections.Generic;
using System.Xml;

namespace WowHeadParser.Entities
{
    class Quest : Entity
    {
        struct QuestTemplateParsing
        {
            public int id;
            public int category;
            public int category2;
            public int[] currencyrewards;
            public int money;
            public string name;
            public int reqlevel;
            public int side;
            public int type;
        }

        public Quest() : base()
        {
            m_builderStarter = new SqlBuilder("creature_queststarter", "id");
            m_builderStarter.SetFieldsNames("quest");

            m_builderEnder = new SqlBuilder("creature_questender", "id");
            m_builderEnder.SetFieldsNames("quest");

            m_builderSerie = new SqlBuilder("quest_template_addon", "id", SqlQueryType.Update);
            m_builderSerie.SetFieldsNames("PrevQuestID");

            m_builderRequiredTeam = new SqlBuilder("quest_template", "id", SqlQueryType.Update);
            m_builderRequiredTeam.SetFieldsNames("requiredTeam");
        }

        public Quest(int id) : this()
        {
            m_data.id = id;
        }

        public override String GetWowheadUrl()
        {
            return GetWowheadBaseUrl() + "/quest=" + m_data.id;
        }

        public override List<Entity> GetIdsFromZone(String zoneId, String zoneHtml)
        {
            String pattern = @"new Listview\({template: 'quest', id: 'quests', name: LANG\.tab_quests, tabs: tabsRelated, parent: 'lkljbjkb574', computeDataFunc: Listview\.funcBox\.initQuestFilter, onAfterCreate: Listview\.funcBox\.addQuestIndicator,(?: note: \$WH\.sprintf\(LANG\.lvnote_zonequests, [0-9]+, " + zoneId + @", '[a-zA-ZÉèéêÎ’'\- ]+', " + zoneId + @"\),)? data: (.+)}\);";
            String creatureJSon = Tools.ExtractJsonFromWithPattern(zoneHtml, pattern);

            List<CreatureTemplateParsing> parsingArray = JsonConvert.DeserializeObject<List<CreatureTemplateParsing>>(creatureJSon);
            List<Entity> tempArray = new List<Entity>();
            foreach (CreatureTemplateParsing creatureTemplateStruct in parsingArray)
            {
                Quest questTemplate = new Quest(creatureTemplateStruct.id);
                tempArray.Add(questTemplate);
            }

            return tempArray;
        }

        public override bool ParseSingleJson(int id = 0)
        {
            if (m_data.id == 0 && id == 0)
                return false;
            else if (m_data.id == 0 && id != 0)
                m_data.id = id;

            String questHtml = Tools.GetHtmlFromWowhead(GetWowheadUrl());

            if (questHtml.Contains("inputbox-error"))
                return false;

            String dataPattern = @"var myMapper = new Mapper\((.+)\)";
            String seriePattern = "(<table class=\"series\">.+?</table>)";

            String questDataJSon = Tools.ExtractJsonFromWithPattern(questHtml, dataPattern);
            String questSerieXml = Tools.ExtractJsonFromWithPattern(questHtml, seriePattern);

            bool isAlliance = questHtml.Contains(@"Faction\x20\x3A\x20\x5Bspan\x20class\x3Dicon\x2Dalliance\x5DAlliance");
            bool isHorde    = questHtml.Contains(@"Faction\x20\x3A\x20\x5Bspan\x20class\x3Dicon\x2Dhorde\x5DHorde");

            if (questDataJSon != null)
            {
                dynamic data = JsonConvert.DeserializeObject<dynamic>(questDataJSon);
                SetData(data);
            }

            if (questSerieXml != null)
            {
                SetSerie(questSerieXml);
            }

            SetTeam(isAlliance, isHorde);

            return true;
        }

        public void SetData(dynamic questData)
        {
            foreach (dynamic objective in questData.objectives)
            {
                foreach (dynamic zone in objective)
                {
                    foreach (dynamic test1 in zone.levels)
                    {
                        foreach (dynamic objectiveData in test1)
                        {
                            if (objectiveData.point == "start")
                                m_builderStarter.AppendFieldsValue(objectiveData.id, m_data.id);

                            if (objectiveData.point == "end")
                                m_builderEnder.AppendFieldsValue(objectiveData.id, m_data.id);
                        }
                    }
                }
            }
        }

        public void SetSerie(String serieXml)
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(serieXml);

                XmlNodeList trs = doc.DocumentElement.SelectNodes("tr");
                List<String> questInSerie = new List<String>();

                foreach (XmlNode tr in trs)
                {
                    XmlNode td = tr.SelectSingleNode("td");

                    if (td == null)
                        continue;

                    XmlNode div = td.SelectSingleNode("div");

                    if (div == null)
                        continue;

                    XmlNode a = div.SelectSingleNode("a");

                    if (a == null)
                    {
                        if (div.SelectSingleNode("b") != null)
                            questInSerie.Add(m_data.id.ToString());

                        continue;
                    }

                    XmlNode hrefAttr = a.Attributes.GetNamedItem("href");

                    if (hrefAttr == null)
                        continue;

                    String href = hrefAttr.Value;
                    String questId = href.Substring(7);
                    questInSerie.Add(questId);
                }

                if (questInSerie.Count < 2)
                    return;

                for (int i = questInSerie.Count - 1; i > 0; --i)
                {
                    String currentQuest = questInSerie[i];
                    String previousQuest = questInSerie[i - 1];

                    m_builderSerie.AppendFieldsValue(currentQuest, previousQuest);
                }
            }
            catch (Exception ex)
            { }
        }

        public void SetTeam(bool isAlliance, bool isHorde)
        {
            Int32 team = isAlliance ? 0 : isHorde ? 1 : -1;

            m_builderRequiredTeam.AppendFieldsValue(m_data.id, team);
        }

        public override String GetSQLRequest()
        {
            String sqlRequest = "";

            if (IsCheckboxChecked("starter/ender"))
            {
                sqlRequest += m_builderStarter.ToString() + m_builderEnder.ToString();
            }

            if (IsCheckboxChecked("serie"))
            {
                sqlRequest += m_builderSerie.ToString();
            }

            if (IsCheckboxChecked("team"))
            {
                sqlRequest += m_builderRequiredTeam.ToString();
            }

            return sqlRequest;
        }

        private QuestTemplateParsing m_data;

        protected SqlBuilder m_builderStarter;
        protected SqlBuilder m_builderEnder;
        protected SqlBuilder m_builderSerie;
        protected SqlBuilder m_builderRequiredTeam;
    }
}
