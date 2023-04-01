using System;
using Rampastring.Tools;
using Rampastring.XNAUI;
using Rampastring.XNAUI.XNAControls;
using ClientGUI;
using DTAClient.Domain.Multiplayer;
using Microsoft.Xna.Framework.Graphics;
using Localization;
using System.Linq;
using System.Collections.Generic;

namespace DTAClient.DXGUI.Multiplayer.GameLobby
{
    /// <summary>
    /// A game option drop-down for the game lobby.
    /// </summary>
    public class GameLobbyDropDown : XNAClientDropDown
    {
        public GameLobbyDropDown(WindowManager windowManager) : base(windowManager) { }

        public string OptionName { get; private set; }

        public int HostSelectedIndex { get; set; }

        public int UserSelectedIndex { get; set; }

        private DropDownDataWriteMode dataWriteMode = DropDownDataWriteMode.BOOLEAN;

        private string spawnIniOption = string.Empty;

        private int defaultIndex;

        public string[] Sides;
        public string[] RandomSelectors;
        string[] RandomSides;
        string[] RandomSidesIndex;
    
        public string[] Mod;
        public string[] DisallowedSideIndiex;
        public string[] DisallowedSide;

        public List<string> ControlName;

        public List<string> ControlIndex;
        public override void Initialize()
        {
            // Find the game lobby that this control belongs to and register ourselves as a game option.

            XNAControl parent = Parent;
            while (true)
            {
                if (parent == null)
                    break;

                // oh no, we have a circular class reference here!
                if (parent is GameLobbyBase gameLobby)
                {
                    gameLobby.DropDowns.Add(this);
                    break;
                }

                parent = parent.Parent;
            }

            base.Initialize();
        }

        public override void ParseAttributeFromINI(IniFile iniFile, string key, string value)
        {
           
            switch (key)
            {
                case "Items":

                    string[] itemlabels = iniFile.GetStringValue(Name, "ItemLabels", "").Split(',');
                    string[] items = value.Split(',');
                    if (itemlabels.Length == 0)
                    {
                        items = value.L10N("UI:Main:" + OptionName).Split(',');
                    }
                    else
                    {
                        itemlabels = iniFile.GetStringValue(Name, "ItemLabels", "").Split(',');
                    }


                    Mod = iniFile.GetStringValue(Name, "Mod", "").Split(',');

                    if (iniFile.GetStringValue(Name, "DisallowedSideIndex", "") != "")
                    {
                        DisallowedSideIndiex = iniFile.GetStringValue(Name, "DisallowedSideIndex", "").Split(',');
                    }

                    if (iniFile.GetStringValue(Name, "Sides", "") != "")
                    {
                        
                        Sides = iniFile.GetStringValue(Name, "Sides", "").Split('|');
                    }
                


                    if (iniFile.GetStringValue(Name, "RandomSides", "") != "")
                    {
                        RandomSelectors = iniFile.GetStringValue(Name, "RandomSides", "").Split('|');
                        RandomSidesIndex = iniFile.GetStringValue(Name, "RandomSidesIndex", "").Split('|');
                    }
                    for (int i = 0; i < items.Length; i++)
                    {
                        XNADropDownItem item = new XNADropDownItem();
                        if (itemlabels.Length > i && !String.IsNullOrEmpty(itemlabels[i]))
                        {
                            item.Text = itemlabels[i].L10N("UI:GameOption:"+ itemlabels[i]);

                            if (items.Length==Mod.Length)
                                item.Tag = new string[2] { items[i], Mod[i] };
                            else
                                item.Tag = new string[2] { items[i], "" };
                        }
                        else item.Text = items[i];
                        AddItem(item);
                    }
                    return;
                case "DataWriteMode":
                    if (value.ToUpper() == "INDEX")
                        dataWriteMode = DropDownDataWriteMode.INDEX;
                    else if (value.ToUpper() == "BOOLEAN")
                        dataWriteMode = DropDownDataWriteMode.BOOLEAN;
                    else if (value.ToUpper() == "MAPCODE")
                        dataWriteMode = DropDownDataWriteMode.MAPCODE;
                    else
                        dataWriteMode = DropDownDataWriteMode.STRING;
                    return;
                case "SpawnIniOption":
                    spawnIniOption = value;
                    return;
                case "DefaultIndex":
                    SelectedIndex = int.Parse(value);
                    defaultIndex = SelectedIndex;
                    HostSelectedIndex = SelectedIndex;
                    UserSelectedIndex = SelectedIndex;
                    return;
                case "OptionName":
                    OptionName = value;
                    return;
                case "ControlName":
                    ControlName = value.Split(',').ToList();
                    return;
                case "ControlIndex":
                    ControlIndex = value.Split(',').ToList();
                    return;
            }

            base.ParseAttributeFromINI(iniFile, key, value);
        }

        /// <summary>
        /// Applies the drop down's associated code to spawn.ini.
        /// </summary>
        /// <param name="spawnIni">The spawn INI file.</param>
        public void ApplySpawnIniCode(IniFile spawnIni)
        {
            if (dataWriteMode == DropDownDataWriteMode.MAPCODE || SelectedIndex < 0 || SelectedIndex >= Items.Count)
                return;

            if (String.IsNullOrEmpty(spawnIniOption))
            {
                Logger.Log("GameLobbyDropDown.WriteSpawnIniCode: " + Name + " has no associated spawn INI option!");
                return;
            }

            switch (dataWriteMode)
            {
                case DropDownDataWriteMode.BOOLEAN:
                    spawnIni.SetBooleanValue("Settings", spawnIniOption, SelectedIndex > 0);
                    break;
                case DropDownDataWriteMode.INDEX:
                    spawnIni.SetIntValue("Settings", spawnIniOption, SelectedIndex);
                    break;
                default:
                case DropDownDataWriteMode.STRING:
                    if (Items[SelectedIndex].Tag != null)
                    {
                        spawnIni.SetStringValue("Settings", spawnIniOption, ((string[])Items[SelectedIndex].Tag)[0]);
                    }
                    else
                    {
                        spawnIni.SetStringValue("Settings", spawnIniOption, Items[SelectedIndex].Text);
                    }
                    break;
            }

        }
            /// <summary>
            /// Applies the drop down's associated code to the map INI file.
            /// </summary>
            /// <param name="mapIni">The map INI file.</param>
            /// <param name="gameMode">Currently selected gamemode, if set.</param>
            public void ApplyMapCode(IniFile mapIni, GameMode gameMode)
            {
                if (dataWriteMode != DropDownDataWriteMode.MAPCODE || SelectedIndex < 0 || SelectedIndex >= Items.Count) return;

                string customIniPath;
                if (Items[SelectedIndex].Tag != null) customIniPath = ((string[])Items[SelectedIndex].Tag)[0];
                else customIniPath = Items[SelectedIndex].Text;

                MapCodeHelper.ApplyMapCode(mapIni, customIniPath, gameMode);
            }

        public override void OnLeftClick()
        {
            if (!AllowDropDown)
                return;

            base.OnLeftClick();
            UserSelectedIndex = SelectedIndex;
        }
    

    public void ApplyDisallowedSideIndex(bool[] disallowedArray)
    {

        if (DisallowedSideIndiex == null || DisallowedSideIndiex.Length == 0 || SelectedIndex >= DisallowedSideIndiex.Length)
            return;
        int[] sideNotAllowed;
        DisallowedSide = DisallowedSideIndiex[SelectedIndex].Split('-');

        if (DisallowedSide.Length != 0)
        {

            sideNotAllowed = Array.ConvertAll(DisallowedSide, int.Parse);
            for (int j = 0; j < DisallowedSide.Length; j++)
                disallowedArray[sideNotAllowed[j]] = true;
        }
    }
    public string[] SetSides()
    {
        if (Sides != null && Sides.Length > SelectedIndex && Sides[SelectedIndex] != "")
        {
            return Sides[SelectedIndex].Split(',');
        }
        else
            return null;
    }

    public string[,] SetRandomSelectors()
    {
        if (RandomSelectors != null && RandomSelectors.Length > SelectedIndex)
        {

            RandomSides = RandomSelectors[SelectedIndex].Split(',');

        }
        if (RandomSides != null && RandomSelectors.Length > SelectedIndex)
        {

            string[,] list = new string[RandomSides.Length, 2];
            for (int i = 0; i < RandomSides.Length; i++)
            {
                list[i, 0] = RandomSides[i];

                if (RandomSidesIndex != null && RandomSidesIndex.Length > SelectedIndex)
                    list[i, 1] = RandomSidesIndex[SelectedIndex].Split('&')[i];
                else
                    list[i, 1] = "";

            }
            return list;
        }
        else return null;
    }

   
}

}
