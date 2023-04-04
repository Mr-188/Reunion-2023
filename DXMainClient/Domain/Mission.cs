using Localization;
using Rampastring.Tools;
using System;
using System.Reflection;
using System.Text.RegularExpressions;

namespace DTAClient.Domain
{
    /// <summary>
    /// A Tiberian Sun mission listed in Battle(E).ini.
    /// </summary>
    public class Mission
    {
        public Mission(IniFile iniFile, string sectionName, int index)
        {
            this.sectionName = sectionName;
            Index = index;
            CD = iniFile.GetIntValue(sectionName, nameof(CD), 0);
            Side = iniFile.GetIntValue(sectionName, nameof(Side), 0);
            Scenario = iniFile.GetStringValue(sectionName, nameof(Scenario), string.Empty);
            GUIName = iniFile.GetStringValue(sectionName, "Description", "Undefined mission");
            IconPath = iniFile.GetStringValue(sectionName, "SideName", string.Empty);
            GUIDescription = iniFile.GetStringValue(sectionName, "LongDescription", string.Empty).L10N("UI:MissionText:" + sectionName);
            FinalMovie = iniFile.GetStringValue(sectionName, nameof(FinalMovie), "none");
            RequiredAddon = iniFile.GetBooleanValue(sectionName, nameof(RequiredAddon), false);
            Enabled = iniFile.GetBooleanValue(sectionName, nameof(Enabled), true);
            BuildOffAlly = iniFile.GetBooleanValue(sectionName, nameof(BuildOffAlly), false);
            PlayerAlwaysOnNormalDifficulty = iniFile.GetBooleanValue(sectionName, nameof(PlayerAlwaysOnNormalDifficulty), false);
            Mod = iniFile.GetStringValue(sectionName, "Mod", string.Empty);
            defaultMod = iniFile.GetIntValue(sectionName, "defaultMod", 0);
            Attached = iniFile.GetStringValue(sectionName, "Attached", string.Empty);
            difficulty = iniFile.GetStringValue(sectionName, "difficulty", "一般"); //难度筛选用

            // GUIDescription = GUIDescription.Replace("@", Environment.NewLine);

            if (HasChinese(GUIDescription))
            {

                string description = string.Empty;
                string s1;
                foreach (string s in GUIDescription.Split('@'))
                {
                    s1 = s+'@';
                    if (s1.Length > 31)
                    {
                        // Logger.Log(s1);
                        s1 = InsertFormat(s1, 31, "@");
                    }
                    description += s1;
                }

                GUIDescription = description;
            }
                GUIDescription = GUIDescription.Replace("@", Environment.NewLine);
        }

        public bool HasChinese(string str)
        {
            return Regex.IsMatch(str, @"[\u4e00-\u9fa5]");
        }

        public int Index { get; }
        public int CD { get; }
        public int Side { get; }
        public string Scenario { get; }
        public string GUIName { get; }
        public string IconPath { get; }
        public string GUIDescription { get; }
        public string FinalMovie { get; }
        public bool RequiredAddon { get; }
        public bool Enabled { get; }
        public bool BuildOffAlly { get; }
        public bool PlayerAlwaysOnNormalDifficulty { get; }
        public string Mod { get; }
        public int defaultMod { get; }
        public string sectionName { get; }

        public string Attached { get; }
        public string difficulty { get; }
        private  string InsertFormat(string input, int interval, string value)
        {
            for (int i = interval; i < input.Length; i += interval + 1)
                input = input.Insert(i, value);
            return input;
        }
    }
}
