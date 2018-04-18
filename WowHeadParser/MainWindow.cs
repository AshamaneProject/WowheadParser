/*
 * * Created by Traesh for AshamaneProject (https://github.com/AshamaneProject)
 */
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using WowHeadParser.Entities;

namespace WowHeadParser
{
    public partial class MainWindow : Form
    {
        public enum LocaleConstant
        {
            enUS = 0,
            koKR = 1,
            frFR = 2,
            deDE = 3,
            zhCN = 4,
            zhTW = 5,
            esES = 6,
            esMX = 7,
            ruRU = 8,
            ptPT = 9,
            itIT = 10,
        };

        public MainWindow()
        {
            InitializeComponent();

            comboBoxChoice.Items.Add("Single");
            comboBoxChoice.Items.Add("Zone");
            comboBoxChoice.Items.Add("Range");

            comboBoxEntity.Items.Add("Creature");
            comboBoxEntity.Items.Add("Gameobject");
            comboBoxEntity.Items.Add("Quest");
            comboBoxEntity.Items.Add("Item");
            comboBoxEntity.Items.Add("Zone");
            comboBoxEntity.Items.Add("BlackMarket");

            comboBoxLocale.Items.Add("www");
            comboBoxLocale.Items.Add("fr");
            comboBoxLocale.Items.Add("es");
            comboBoxLocale.Items.Add("de");
            comboBoxLocale.Items.Add("it");
            comboBoxLocale.Items.Add("pt");
            comboBoxLocale.Items.Add("ru");

            comboBoxChoice.SelectedIndex = 0;

            HideToTextbox(true);

            ids = new List<String>();
            currentId = 0;
        }

        private void MainWindow_Load(object sender, EventArgs e)
        {
            for (int i = 0; i < comboBoxLocale.Items.Count; ++i)
                if (Properties.Settings.Default.wowheadLocale == (String)comboBoxLocale.Items[i])
                    comboBoxLocale.SelectedIndex = i;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            String selectedText = comboBoxLocale.Items[comboBoxLocale.SelectedIndex].ToString();
            Properties.Settings.Default.wowheadLocale = selectedText;
            Properties.Settings.Default.selectedEntity = comboBoxEntity.SelectedIndex;
            Entity.ReloadWowheadBaseUrl();
            UpdateCheckboxSettings(); // Must be done before Properties.Settings.Default.Save

            switch (selectedText)
            {
                case "www": Properties.Settings.Default.localIndex = (int)LocaleConstant.enUS;  break;
                case "fr":  Properties.Settings.Default.localIndex = (int)LocaleConstant.frFR;  break;
                case "es":  Properties.Settings.Default.localIndex = (int)LocaleConstant.esES;  break;
                case "de":  Properties.Settings.Default.localIndex = (int)LocaleConstant.deDE;  break;
                case "it":  Properties.Settings.Default.localIndex = (int)LocaleConstant.itIT;  break;
                case "pt":  Properties.Settings.Default.localIndex = (int)LocaleConstant.ptPT;  break;
                case "ru":  Properties.Settings.Default.localIndex = (int)LocaleConstant.ruRU;  break;
                default:    Properties.Settings.Default.localIndex = (int)LocaleConstant.frFR;  break;
            }

            Properties.Settings.Default.Save();

            ids = new List<String>(textBoxDe.Text.Split(' '));
            m_fileName = Tools.GetFileNameForCurrentTime();
            StartParsing();

            SetStartButtonEnableState(false);
        }

        public void StartParsing()
        {
            switch (comboBoxChoice.SelectedIndex)
            {
                case 0:
                {
                    int firstId = Int32.Parse(ids[currentId]);

                    Range range = new Range(this, m_fileName);
                    range.StartParsing(firstId, firstId);

                    break;
                }
                case 1:
                {
                    Zone zone = new Zone(this);
                    zone.StartParsing(ids[currentId]);
                    break;
                }
                case 2:
                {
                    int firstId = Int32.Parse(textBoxDe.Text);
                    int lastId = Int32.Parse(textBoxA.Text);

                    Range range = new Range(this, m_fileName);
                    range.StartParsing(firstId, lastId);

                    break;
                }
            }
        }

        public void SetStartButtonEnableState(bool state)
        {
            button1.Enabled = state;
        }

        public void setProgressBar(int progress)
        {
            this.progressBar1.Value = progress;
            this.ProgressBarValue.Text = progress + "%";
        }

        public void SetTimeLeft(int seconds)
        {
            int hours = seconds / 3600;
            seconds %= 3600;

            int minutes = seconds / 60;
            seconds %= 60;

            timeLeftLabel.Text = hours.ToString("00") + "h" + minutes.ToString("00") + "m" + seconds.ToString("00") + "s";
        }

        public void SetWorkDone()
        {
            if (ids.Count > ++currentId)
            {
                StartParsing();

                setProgressBar(100);
                timeLeftLabel.Text = "Terminé (" + (currentId + 1) + "/" + ids.Count + ")";
            }
            else
            {
                setProgressBar(100);
                timeLeftLabel.Text = "Terminé";
                SetStartButtonEnableState(true);
                currentId = 0;
            }
        }

        private void comboBoxChoice_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBoxChoice.SelectedIndex < 2)
                HideToTextbox(true);
            else
                HideToTextbox(false);
        }

        private void HideToTextbox(bool hide)
        {
            if (hide)
            {
                textBoxA.Hide();
                labelA.Hide();
            }
            else
            {
                textBoxA.Show();
                labelA.Show();
            }
        }

        private void comboBoxEntity_SelectedIndexChanged(object sender, EventArgs e)
        {
            selectList.Clear();

            switch (comboBoxEntity.SelectedIndex)
            {
                // Creature
                case 0:
                {
                    selectList.Items.Add("Is Dungeon/Raid Boss");
                    selectList.Items.Add("template");
                    selectList.Items.Add("health modifier");
                    selectList.Items.Add("locale");
                    selectList.Items.Add("vendor");
                    selectList.Items.Add("loot");
                    selectList.Items.Add("skinning");
                    selectList.Items.Add("trainer");
                    selectList.Items.Add("quest starter");
                    selectList.Items.Add("quest ender");
                    selectList.Items.Add("simple faction");
                    selectList.Items.Add("money");
                    break;
                }
                // Gameobject
                case 1:
                {
                    selectList.Items.Add("locale");
                    selectList.Items.Add("loot");
                    selectList.Items.Add("herbalism");
                    selectList.Items.Add("mining");
                    break;
                }
                // Quest
                case 2:
                {
                    selectList.Items.Add("starter/ender");
                    selectList.Items.Add("serie");
                    selectList.Items.Add("team");
                    selectList.Items.Add("class");
                    break;
                }
                // Item
                case 3:
                {
                    selectList.Items.Add("create item");
                    selectList.Items.Add("loot");
                    selectList.Items.Add("dropped by");
                    selectList.Items.Add("export pvp");
                    break;
                }
                // Zone
                case 4:
                {
                    selectList.Items.Add("Fishing");
                    break;
                }
                // Marché Noir
                case 5:
                {
                    selectList.Items.Add("Débug");
                    break;
                }
            }
        }

        public void UpdateCheckboxSettings()
        {
            if (Properties.Settings.Default.checkedList == null)
                Properties.Settings.Default.checkedList = new System.Collections.Specialized.StringCollection();
            else
                Properties.Settings.Default.checkedList.Clear();

            for (int i = 0; i < selectList.Items.Count; ++i)
                if (selectList.Items[i].Checked)
                    Properties.Settings.Default.checkedList.Add(selectList.Items[i].Text);
        }

        public Entity CreateNeededEntity(int id = 0)
        {
            switch (Properties.Settings.Default.selectedEntity)
            {
                case 0: return new Creature(id);
                case 1: return new Gameobject(id);
                case 2: return new Quest(id);
                case 3: return new Item(id);
                case 4: return new ZoneEntity(id);
                case 5: return new BlackMarket(id);
            }

            return null;
        }

        private int currentId;
        private List<String> ids;
        private String m_fileName;
    }
}
