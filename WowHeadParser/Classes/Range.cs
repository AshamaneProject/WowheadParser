/*
 * * Created by Traesh for AshamaneProject (https://github.com/AshamaneProject)
 */
using System;
using System.ComponentModel;
using System.IO;
using System.Net.Http;
using WowHeadParser.Entities;

namespace WowHeadParser
{
    class Range
    {
        const int MAX_WORKER = 20;

        public Range(MainWindow view, String fileName)
        {
            m_view = view;
            m_index = 0;
            m_parsedEntitiesCount = 0;
            m_getRangeListBackgroundWorker = new BackgroundWorker[MAX_WORKER];
            m_webClients = new HttpClient[MAX_WORKER];

            m_fileName = fileName;
            m_lastEstimateTime = 0;
        }

        public void StartParsing(int from, int to)
        {
            if (from > to)
                return;

            m_timestamp = Tools.GetUnixTimestamp();

            m_from  = from;
            m_to    = to;
            m_entityTodoCount = to - from + 1; // + 1 car le premier est compris

            StartSnifByEntity();
        }

        void StartSnifByEntity()
        {
            m_index = 0;
            m_parsedEntitiesCount = 0;

            int maxWorkers = (m_to - m_from + 1) > MAX_WORKER ? MAX_WORKER : m_to - m_from + 1;

            for (int i = 0; i < maxWorkers; ++i)
            {
                m_webClients[i] = Tools.InitHttpClient();

                m_getRangeListBackgroundWorker[i] = new BackgroundWorker();
                m_getRangeListBackgroundWorker[i].DoWork += new DoWorkEventHandler(BackgroundWorkerProcessEntitiesList);
                m_getRangeListBackgroundWorker[i].RunWorkerCompleted += new RunWorkerCompletedEventHandler(BackgroundWorkerProcessEntitiesCompleted);
                m_getRangeListBackgroundWorker[i].RunWorkerAsync(i);
            }
        }

        private void BackgroundWorkerProcessEntitiesList(object sender, DoWorkEventArgs e)
        {
            if (m_index >= m_entityTodoCount)
                return;

            int tempIndex = m_index++;
            try
            {
                e.Result = e.Argument;
                Entity entity = m_view.CreateNeededEntity(m_from + tempIndex);
                entity.webClient = m_webClients[(int)e.Result];
                entity.ParseSingleJson();
                String requestText = "\n\n" + entity.GetSQLRequest();
                requestText += requestText != "" ? "\n" : "";
                File.AppendAllText(m_fileName, entity.GetSQLRequest());
            }
            catch (Exception ex)
            {
                if (ex.Message.IndexOf("404") != -1)
                    Console.WriteLine("Introuvable");
                else
                    Console.WriteLine("Erreur");
            }
            ++m_parsedEntitiesCount;
        }

        private void BackgroundWorkerProcessEntitiesCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (m_parsedEntitiesCount > m_entityTodoCount)
                return;

            Console.WriteLine("Nombre effectué : " + m_parsedEntitiesCount);

            float percent = ((float)m_index / (float)m_entityTodoCount) * 100;

            if (m_view != null)
            {
                m_view.setProgressBar((int)percent);
                EstimateSecondsTimeLeft();
            }

            if (m_parsedEntitiesCount == m_entityTodoCount)
            {
                m_view.SetWorkDone();
                return;
            }

            if (m_index >= m_entityTodoCount)
                return;

            int workerIndex = (int)e.Result;
            m_getRangeListBackgroundWorker[workerIndex].RunWorkerAsync(workerIndex);
        }

        private void EstimateSecondsTimeLeft()
        {
            Int32 unixTimestamp = Tools.GetUnixTimestamp();

            if ((m_lastEstimateTime + 1) >= unixTimestamp)
                return;

            m_lastEstimateTime = unixTimestamp;

            float elapsedSeconds = unixTimestamp - m_timestamp;

            float entityCount = m_to - m_from;
            float timeByEntity = (float)elapsedSeconds / (float)m_parsedEntitiesCount;

            float estimatedSecondsLeft = timeByEntity * (entityCount - m_parsedEntitiesCount);

            m_view.SetTimeLeft((Int32)estimatedSecondsLeft);
        }

        private MainWindow m_view;

        private String m_fileName;

        private int m_from;
        private int m_to;
        private int m_entityTodoCount;
        private int m_index;
        private int m_parsedEntitiesCount;

        private BackgroundWorker[] m_getRangeListBackgroundWorker;
        private HttpClient[] m_webClients;

        // Test
        private int m_timestamp;
        private int m_lastEstimateTime;
    }
}
