/*
 * * Created by Traesh for AshamaneProject (https://github.com/AshamaneProject)
 */
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using WowHeadParser.Entities;

namespace WowHeadParser
{
    class Zone
    {
        const int MAX_WORKER = 50;

        public Zone(MainWindow view)
        {
            m_view = view;
            m_zoneId = "0";
            m_index = 0;
            m_parsedEntitiesCount = 0;
            m_getZoneListBackgroundWorker = new BackgroundWorker[MAX_WORKER];

            m_fileName  = "";
            m_array     = new List<Entity>();
        }

        private void ResetZone()
        {
            if (m_view != null)
                m_view.setProgressBar(0);

            m_index = 0;
            m_array.Clear();
            m_parsedEntitiesCount = 0;
            m_timestamp = Tools.GetUnixTimestamp();
            m_fileName = Tools.GetFileNameForCurrentTime();
            m_timestamp = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
        }

        public void StartParsing(String zone)
        {
            Entity askedEntity = m_view.CreateNeededEntity();

            if (askedEntity == null)
                return;

            ResetZone();
            m_zoneId = zone;

            if (askedEntity.GetType() != typeof(BlackMarket))
                m_zoneHtml = GetZoneHtmlFromWowhead(m_zoneId);

            ParseZoneJson();
            StartSnifByEntity();
        }

        public String GetZoneHtmlFromWowhead(String zone)
        {
            return Tools.GetHtmlFromWowhead(Tools.GetWowheadUrl("zone", zone));
        }

        public void ParseZoneJson()
        {
            Entity askedEntity = m_view.CreateNeededEntity();

            if (askedEntity == null)
                return;

            List<Entity> entityList = m_view.CreateNeededEntity().GetIdsFromZone(m_zoneId, m_zoneHtml);

            if (entityList != null)
                m_array.AddRange(entityList);
        }

        void StartSnifByEntity()
        {
            m_index = 0;
            m_parsedEntitiesCount = 0;

            for (int i = 0; i < MAX_WORKER; ++i)
            {
                m_getZoneListBackgroundWorker[i] = new BackgroundWorker();
                m_getZoneListBackgroundWorker[i].DoWork += new DoWorkEventHandler(BackgroundWorkerProcessEntitiesList);
                m_getZoneListBackgroundWorker[i].RunWorkerCompleted += new RunWorkerCompletedEventHandler(BackgroundWorkerProcessEntitiesCompleted);
                m_getZoneListBackgroundWorker[i].WorkerReportsProgress = true;
                m_getZoneListBackgroundWorker[i].WorkerSupportsCancellation = true;
                m_getZoneListBackgroundWorker[i].RunWorkerAsync(i);
            }
        }

        private void BackgroundWorkerProcessEntitiesList(object sender, DoWorkEventArgs e)
        {
            if (m_index >= m_array.Count)
                return;

            int tempIndex = m_index++;
            try
            {
                e.Result = e.Argument;
                bool parseReturn = m_array[tempIndex].ParseSingleJson();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erreur");
            }
            ++m_parsedEntitiesCount;
        }

        private void BackgroundWorkerProcessEntitiesCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (m_parsedEntitiesCount > m_array.Count)
                return;

            Console.WriteLine("Nombre effectué : " + m_parsedEntitiesCount);

            float percent = ((float)m_index / (float)m_array.Count) * 100;

            if (m_view != null)
            {
                m_view.setProgressBar((int)percent);
                EstimateSecondsTimeLeft();
            }

            if (m_parsedEntitiesCount == m_array.Count)
            {
                AppendAllEntitiesToSql();
                m_view.SetWorkDone();
                return;
            }

            if (m_index >= m_array.Count)
                return;

            int workerIndex = (int)e.Result;

            if (!m_getZoneListBackgroundWorker[workerIndex].IsBusy)
                m_getZoneListBackgroundWorker[workerIndex].RunWorkerAsync(workerIndex);
        }

        void AppendAllEntitiesToSql()
        {
            int tempCount = 0;
            foreach (Entity entity in m_array)
            {
                Console.WriteLine("Doing entity n°" + tempCount++);
                String requestText = entity.GetSQLRequest();
                requestText += requestText != "" ? "\n" : "";
                File.AppendAllText(m_fileName, entity.GetSQLRequest());
            }

            Console.WriteLine("Elapsed Time : " + ((Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds - m_timestamp));
        }

        private void EstimateSecondsTimeLeft()
        {
            Int32 unixTimestamp = Tools.GetUnixTimestamp();

            if ((m_lastEstimateTime + 1) >= unixTimestamp)
                return;

            m_lastEstimateTime = unixTimestamp;

            float elapsedSeconds = unixTimestamp - m_timestamp;

            float entityCount = m_array.Count();
            float timeByEntity = (float)elapsedSeconds / (float)m_parsedEntitiesCount;

            float estimatedSecondsLeft = timeByEntity * (entityCount - m_parsedEntitiesCount);

            m_view.SetTimeLeft((Int32)estimatedSecondsLeft);
        }

        private MainWindow m_view;

        private String m_zoneId;
        private String m_zoneHtml;

        private String m_fileName;

        private List<Entity> m_array;
        private int m_index;
        private int m_parsedEntitiesCount;

        private BackgroundWorker[] m_getZoneListBackgroundWorker;

        private int m_timestamp;
        private int m_lastEstimateTime;
    }
}
