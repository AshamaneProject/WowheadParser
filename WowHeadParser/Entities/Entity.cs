/*
 * * Created by Traesh for AshamaneProject (https://github.com/AshamaneProject)
 */
using Sql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace WowHeadParser.Entities
{
    public class Entity
    {
        public Entity()
        {
            m_hasZoneData = false;
            isError = false;
            webClient = null;
        }

        static public String GetWowheadBaseUrl()
        {
            if (m_baseWowheadUrl == "")
                ReloadWowheadBaseUrl();

            return m_baseWowheadUrl;
        }

        static public void ReloadWowheadBaseUrl()
        {
            m_baseWowheadUrl = "https://" + Properties.Settings.Default.wowheadLocale + ".wowhead.com";
        }

        public virtual String GetWowheadUrl() { return ""; }

        public virtual List<Entity> GetIdsFromZone(String zoneId, String zoneHtml) { return new List<Entity>(); }

        public virtual bool ParseSingleJson(int id = 0) { return false; }

        public virtual String GetSQLRequest() { return ""; }

        public void SetIsError() { isError = true; }

        public bool IsCheckboxChecked(String checkboxName)
        {
            for (int i = 0; i < Properties.Settings.Default.checkedList.Count; ++i)
                if (Properties.Settings.Default.checkedList[i] == checkboxName)
                    return true;

            return false;
        }

        protected bool isError;
        protected bool m_hasZoneData;

        static private String m_baseWowheadUrl = "";

        public HttpClient webClient;
    }
}
