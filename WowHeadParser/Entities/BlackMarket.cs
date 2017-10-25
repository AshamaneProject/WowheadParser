/*
 * * Created by Traesh for AshamaneProject (https://github.com/AshamaneProject)
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
using System.IO;

namespace WowHeadParser.Entities
{
    class BlackMarket : Entity
    {
        struct BlackMarketItem
        {
            public int id;
            public int level;
        }

        private int m_id;

        public BlackMarket(int id = 0)
        {
            m_id = id;
        }

        public override List<Entity> GetIdsFromZone(String zoneId, String zoneHtml)
        {
            String blackMarketHtml = Tools.GetHtmlFromWowhead("http://www.wowhead.com/items?filter=cr=181;crs=1;crv=0#700-2");

            String blackMarketItemsPattern = @"var listviewitems = (\[.+\]);";
            String allBlackMarketItemJson = Tools.ExtractJsonFromWithPattern(blackMarketHtml, blackMarketItemsPattern);
            BlackMarketItem[] allBlackMarketItemsParsing = JsonConvert.DeserializeObject<BlackMarketItem[]>(allBlackMarketItemJson);

            List<Entity> tempArray = new List<Entity>();
            foreach (BlackMarketItem blackMarketItemsParsing in allBlackMarketItemsParsing)
            {
                if (blackMarketItemsParsing.level > UInt32.Parse(zoneId))
                    continue;

                BlackMarket bm = new BlackMarket(blackMarketItemsParsing.id);
                tempArray.Add(bm);
            }

            return tempArray;
        }

        public override bool ParseSingleJson(int id = 0)
        {
            if (id == 0 && m_id != 0)
                id = m_id;
            else
                return false;

            return true;
        }

        public override String GetSQLRequest()
        {
            String returnSql = "INSERT INTO blackmarket_template (id, itemEntry, itemCount, seller, startBid, duration, chance) VALUES (0, " + m_id + ", 1, 83867, 50000000, 43200, 10);\n";
            return returnSql;
        }
    }
}
