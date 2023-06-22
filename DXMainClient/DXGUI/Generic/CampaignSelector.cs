﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ClientCore;
using ClientGUI;
using ClientUpdater;
using DTAClient.Domain;
using Localization;
using Microsoft.Xna.Framework;
using Rampastring.Tools;
using Rampastring.Tools.INIProperties;
using Rampastring.XNAUI;
using Rampastring.XNAUI.XNAControls;
//using SharpDX.DXGI;
//using EllipticCurve.Utils;

//using static System.Windows.Forms.VisualStyles.VisualStyleElement.Menu;

namespace DTAClient.DXGUI.Generic
{
    public class CampaignSelector : XNAWindow
    {
        private const int DEFAULT_WIDTH = 650;
        private const int DEFAULT_HEIGHT = 600;

        private static string[] DifficultyNames = new string[] { "Easy", "Medium", "Hard" };

        private static string[] DifficultyIniPaths = new string[]
        {
            "INI/MapCode/Difficulty Easy.ini",
            "INI/MapCode/Difficulty Medium.ini",
            "INI/MapCode/Difficulty Hard.ini"
        };

        public CampaignSelector(WindowManager windowManager, DiscordHandler discordHandler) : base(windowManager)
        {
            this.discordHandler = discordHandler;
        }

        private DiscordHandler discordHandler;

        private List<Mission> Missions = new List<Mission>();
        private XNAListBox lbCampaignList;
        private XNALabel lblScreen;
        private XNADropDown dddifficulty;
        private XNADropDown ddside;
        private XNAClientButton btnLaunch;
        private XNATextBlock tbMissionDescription;
        private XNATrackbar trbDifficultySelector;

        private XNALabel lbGameSpeed;
        private XNADropDown ddGameSpeed;
        private XNALabel lbGameMod;
        private XNADropDown ddGameMod;



        private CheaterWindow cheaterWindow;

        List<string> difficultyList = new List<string>();
        List<string> sideList = new List<string>();
        Dictionary<string, string[]> Mod = new Dictionary<string, string[]>();
        Dictionary<string, List<string[]>> ModParent = new Dictionary<string, List<string[]>>();
        private string[] filesToCheck = new string[]
        {
            "INI/AI.ini",
            "INI/AIE.ini",
            "INI/Art.ini",
            "INI/ArtE.ini",
            "INI/Enhance.ini",
            "INI/Rules.ini",
            "INI/MapCode/Difficulty Hard.ini",
            "INI/MapCode/Difficulty Medium.ini",
            "INI/MapCode/Difficulty Easy.ini"
        };

        private Mission missionToLaunch;
        private XNAContextMenu mapContextMenu;
        private XNAContextMenuItem toggleFavoriteItem;

        public override void Initialize()
        {
            BackgroundTexture = AssetLoader.LoadTexture("missionselectorbg.png");
            ClientRectangle = new Rectangle(0, 0, DEFAULT_WIDTH, DEFAULT_HEIGHT);
            BorderColor = UISettings.ActiveSettings.PanelBorderColor;

            Name = "CampaignSelector";

            var lblSelectCampaign = new XNALabel(WindowManager);
            lblSelectCampaign.Name = "lblSelectCampaign";
            lblSelectCampaign.FontIndex = 1;
            lblSelectCampaign.ClientRectangle = new Rectangle(12, 12, 0, 0);
            lblSelectCampaign.Text = "MISSIONS:".L10N("UI:Main:Missions");

            lbCampaignList = new XNAListBox(WindowManager);
            lbCampaignList.Name = "lbCampaignList";
            lbCampaignList.BackgroundTexture = AssetLoader.CreateTexture(new Color(0, 0, 0, 128), 2, 2);
            lbCampaignList.PanelBackgroundDrawMode = PanelBackgroundImageDrawMode.STRETCHED;
            lbCampaignList.ClientRectangle = new Rectangle(12,
                lblSelectCampaign.Bottom + 36, 300, 480);
            lbCampaignList.LineHeight = 20;
            lbCampaignList.SelectedIndexChanged += LbCampaignList_SelectedIndexChanged;
            lbCampaignList.RightClick += lbCampaignList_RightClick;

            lblScreen = new XNALabel(WindowManager);
            lblScreen.Name = "lblScreen";
            lblScreen.Text = "Screen:".L10N("UI:Campaign:Screen");
            lblScreen.ClientRectangle = new Rectangle(10, 35, 0, 0);

            dddifficulty = new XNADropDown(WindowManager);
            dddifficulty.Name = nameof(dddifficulty);
            dddifficulty.ClientRectangle = new Rectangle(10, 55, 100, 25);


            ddside = new XNADropDown(WindowManager);
            ddside.Name = nameof(ddside);
            ddside.ClientRectangle = new Rectangle(dddifficulty.X + dddifficulty.Width + 5, dddifficulty.Y, dddifficulty.Width, dddifficulty.Height);

            var lblMissionDescriptionHeader = new XNALabel(WindowManager);
            lblMissionDescriptionHeader.Name = "lblMissionDescriptionHeader";
            lblMissionDescriptionHeader.FontIndex = 1;
            lblMissionDescriptionHeader.ClientRectangle = new Rectangle(
                lbCampaignList.Right + 12,
                lblSelectCampaign.Y, 0, 0);
            lblMissionDescriptionHeader.Text = "MISSION DESCRIPTION:".L10N("UI:Main:MissionDescription");

            tbMissionDescription = new XNATextBlock(WindowManager);
            tbMissionDescription.Name = "tbMissionDescription";
            tbMissionDescription.ClientRectangle = new Rectangle(
                lblMissionDescriptionHeader.X,
                lblMissionDescriptionHeader.Bottom + 6,
                467, 350);
            tbMissionDescription.PanelBackgroundDrawMode = PanelBackgroundImageDrawMode.STRETCHED;
            tbMissionDescription.Alpha = 1.0f;
            tbMissionDescription.FontIndex = 1;
            tbMissionDescription.BackgroundTexture = AssetLoader.CreateTexture(AssetLoader.GetColorFromString(ClientConfiguration.Instance.AltUIBackgroundColor),
                tbMissionDescription.Width, tbMissionDescription.Height);

            var lblDifficultyLevel = new XNALabel(WindowManager);
            lblDifficultyLevel.Name = "lblDifficultyLevel";
            lblDifficultyLevel.Text = "DIFFICULTY LEVEL".L10N("UI:Main:DifficultyLevel");
            lblDifficultyLevel.FontIndex = 1;
            Vector2 textSize = Renderer.GetTextDimensions(lblDifficultyLevel.Text, lblDifficultyLevel.FontIndex);
            lblDifficultyLevel.ClientRectangle = new Rectangle(
                tbMissionDescription.X + (tbMissionDescription.Width - (int)textSize.X) / 2,
                tbMissionDescription.Bottom + 12, (int)textSize.X, (int)textSize.Y);

            trbDifficultySelector = new XNATrackbar(WindowManager);
            trbDifficultySelector.Name = "trbDifficultySelector";
            trbDifficultySelector.ClientRectangle = new Rectangle(
                tbMissionDescription.X, lblDifficultyLevel.Bottom + 6,
                tbMissionDescription.Width - 130, 30);
            trbDifficultySelector.MinValue = 0;
            trbDifficultySelector.MaxValue = 2;
            trbDifficultySelector.BackgroundTexture = AssetLoader.CreateTexture(
                new Color(0, 0, 0, 128), 2, 2);
            trbDifficultySelector.ButtonTexture = AssetLoader.LoadTextureUncached(
                "trackbarButton_difficulty.png");

           
            

            lbGameMod = new XNALabel(WindowManager);
            lbGameMod.Name = "lbGameMod";
            lbGameMod.Text = "Mod：".L10N("UI:Main:GameMod");
            lbGameMod.FontIndex = 1;
            lbGameMod.ClientRectangle = new Rectangle(trbDifficultySelector.X + 220, trbDifficultySelector.Y - 15, 0, 0);

            ddGameMod = new XNADropDown(WindowManager);
            ddGameMod.Name = "ddGameMod";
            ddGameMod.ClientRectangle = new Rectangle(lbGameMod.X + 60, lbGameMod.Y, 160, 40);

            ddGameSpeed = new XNADropDown(WindowManager);
            ddGameSpeed.Name = "ddGameSpeed";
            ddGameSpeed.ClientRectangle = new Rectangle(lbGameMod.X - 100, lbGameMod.Y, 80, 40);
            ddGameSpeed.Visible = false;


            mapContextMenu = new XNAContextMenu(WindowManager);
            mapContextMenu.Name = nameof(mapContextMenu);
            mapContextMenu.Width = 100;
            //mapContextMenu.AddItem("删除这组任务");
            toggleFavoriteItem = new XNAContextMenuItem
            {
                Text = "删除这组任务",
                SelectAction = DelConf
            };
            mapContextMenu.AddItem(toggleFavoriteItem);
            AddChild(mapContextMenu);

            for (int i = 6; i >= 0; i--)
            {
                ddGameSpeed.AddItem(i.ToString());
            }

            ddGameSpeed.SelectedIndex = 6 - UserINISettings.Instance.CampaignDefaultGameSpeed.Value;

            lbGameSpeed = new XNALabel(WindowManager);
            lbGameSpeed.Name = "lbGameSpeed";
            lbGameSpeed.Text = "Game Speed:".L10N("UI:Main:GameSpeed");
            lbGameSpeed.FontIndex = 1;
            lbGameSpeed.ClientRectangle = new Rectangle(ddGameSpeed.X - 100, lbGameMod.Y, 0, 0);
            lbGameSpeed.Visible = false;

            var lblEasy = new XNALabel(WindowManager);
            lblEasy.Name = "lblEasy";
            lblEasy.FontIndex = 1;
            lblEasy.Text = "EASY".L10N("UI:Main:DifficultyEasy");
            lblEasy.ClientRectangle = new Rectangle(trbDifficultySelector.X,
                trbDifficultySelector.Bottom + 6, 1, 1);

            var lblNormal = new XNALabel(WindowManager);
            lblNormal.Name = "lblNormal";
            lblNormal.FontIndex = 1;
            lblNormal.Text = "NORMAL".L10N("UI:Main:DifficultyNormal");
            textSize = Renderer.GetTextDimensions(lblNormal.Text, lblNormal.FontIndex);
            lblNormal.ClientRectangle = new Rectangle(
                tbMissionDescription.X + (tbMissionDescription.Width - (int)textSize.X) / 2,
                lblEasy.Y, (int)textSize.X, (int)textSize.Y);

            var lblHard = new XNALabel(WindowManager);
            lblHard.Name = "lblHard";
            lblHard.FontIndex = 1;
            lblHard.Text = "HARD".L10N("UI:Main:DifficultyHard");
            lblHard.ClientRectangle = new Rectangle(
                tbMissionDescription.Right - lblHard.Width,
                lblEasy.Y, 1, 1);

            btnLaunch = new XNAClientButton(WindowManager);
            btnLaunch.Name = "btnLaunch";
            btnLaunch.ClientRectangle = new Rectangle(12, Height - 35, UIDesignConstants.BUTTON_WIDTH_133, UIDesignConstants.BUTTON_HEIGHT);
            btnLaunch.Text = "Launch".L10N("UI:Main:ButtonLaunch");
            btnLaunch.AllowClick = false;
            btnLaunch.LeftClick += BtnLaunch_LeftClick;

            var btnCancel = new XNAClientButton(WindowManager);
            btnCancel.Name = "btnCancel";
            btnCancel.ClientRectangle = new Rectangle(Width - 145,
                btnLaunch.Y, UIDesignConstants.BUTTON_WIDTH_133, UIDesignConstants.BUTTON_HEIGHT);
            btnCancel.Text = "Cancel".L10N("UI:Main:ButtonCancel");
            btnCancel.LeftClick += BtnCancel_LeftClick;

            AddChild(lblSelectCampaign);
            AddChild(lblMissionDescriptionHeader);
            AddChild(lbCampaignList);
            AddChild(lblScreen);
            AddChild(dddifficulty);
            AddChild(ddside);
            AddChild(tbMissionDescription);
            AddChild(lblDifficultyLevel);
            AddChild(btnLaunch);
            AddChild(btnCancel);
            AddChild(trbDifficultySelector);
            AddChild(lblEasy);
            AddChild(lblNormal);
            AddChild(lblHard);
            AddChild(lbGameMod);
            AddChild(ddGameMod);
            AddChild(lbGameSpeed);
            AddChild(ddGameSpeed);
            // Set control attributes from INI file
            base.Initialize();

            // Center on screen
            CenterOnParent();

            trbDifficultySelector.Value = UserINISettings.Instance.Difficulty;

            XNADropDownItem allitem = new XNADropDownItem();
            allitem.Text = "All".L10N("UI:Main:All");
            allitem.Tag = "All";

            dddifficulty.AddItem(allitem);
            ddside.AddItem(allitem);
            ddside.SelectedIndex = 0;
            dddifficulty.SelectedIndex = 0;
            ReadMissionList();


            foreach (string diff in difficultyList)
            {
                XNADropDownItem item = new XNADropDownItem();
                item.Text = diff.L10N("UI:Campaign:" + diff);
                item.Tag = diff;
                dddifficulty.AddItem(item);
            }

            foreach (string side in sideList)
            {
                XNADropDownItem item = new XNADropDownItem();
                item.Text = side.L10N("UI:Campaign:" + side);
                item.Tag = side;
                ddside.AddItem(item);
            }



            ddside.SelectedIndexChanged += Dddifficulty_SelectedIndexChanged;
            dddifficulty.SelectedIndexChanged += Dddifficulty_SelectedIndexChanged;

            cheaterWindow = new CheaterWindow(WindowManager);
            var dp = new DarkeningPanel(WindowManager);
            dp.AddChild(cheaterWindow);
            AddChild(dp);
            dp.CenterOnParent();
            cheaterWindow.CenterOnParent();
            cheaterWindow.YesClicked += CheaterWindow_YesClicked;
            cheaterWindow.Disable();

        }

        protected virtual void DelConf()
        {
            XNAMessageBox messageBox = new XNAMessageBox(WindowManager, "删除确认", "你真的要删除这组任务吗？", XNAMessageBoxButtons.YesNo);
            messageBox.Show();
            messageBox.YesClickedAction += DelConf_YesClicked;
        }

        private void DelConf_YesClicked(XNAMessageBox messageBox)
        {
            ////表层删除
            //_ = lbCampaignList.Items.Remove(lbCampaignList.SelectedItem);

            //Console.WriteLine(lbCampaignList.SelectedItem.Tag);
            //lbCampaignList.Items.RemoveAll(i => (string)i.Tag == Missions[lbCampaignList.SelectedIndex].Attached);

            //底层删除
          //  Console.WriteLine(lbCampaignList.SelectedItem.Tag);
            string path = @"INI/";

            var files = Directory.GetFiles(path, "Battle*.ini");
            bool f = false;
            foreach (var file in files)
            {
                IniFile iniFile = new IniFile(file);
                if (iniFile.KeyExists("Battles", (string)lbCampaignList.SelectedItem.Tag))
                {
                    iniFile.SetStringValue("Battles", (string)lbCampaignList.SelectedItem.Tag, string.Empty);
                    iniFile.RemoveSection((string)lbCampaignList.SelectedItem.Tag);
                    List<string> sectionsToRemove = new List<string>();
                    foreach (string section in iniFile.GetSections())
                    {
                        if (iniFile.GetStringValue(section, "Attached", string.Empty) == (string)lbCampaignList.SelectedItem.Tag
                            || iniFile.GetStringValue(section, "Mod", string.Empty).IndexOf((string)lbCampaignList.SelectedItem.Tag) != -1)
                        {
                            f = true;
                            iniFile.RemoveSection(section);
                            //   iniFile.
                            iniFile.SetStringValue("Battles", section, string.Empty);
                        }
                    }

                    if (f)
                    {

                        iniFile.WriteIniFile();
                        break;
                    }


                }
                // iniFile.WriteIniFile();
            }

            ReadMissionList();


        }

        private void lbCampaignList_RightClick(object sender, EventArgs e)
        {
            if (lbCampaignList.HoveredIndex < 0 || lbCampaignList.HoveredIndex >= lbCampaignList.Items.Count)
                return;

            if (string.IsNullOrEmpty(Missions[lbCampaignList.HoveredIndex].Scenario))
            {

                lbCampaignList.SelectedIndex = lbCampaignList.HoveredIndex;

                //if (!lbCampaignList.Items.Any(i => i.VisibilityChecker == null || i.VisibilityChecker()))
                //    return;

                //    toggleFavoriteItem.Text = GameModeMap.IsFavorite ? "Remove Favorite".L10N("UI:Main:RemoveFavorite") : "Add Favorite".L10N("UI:Main:AddFavorite");


                mapContextMenu.Open(GetCursorPoint());
            }
        }

        private void Dddifficulty_SelectedIndexChanged(object sender, EventArgs e)
        {
            ReadMissionList();
           

        }


        private void LbCampaignList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lbCampaignList.SelectedIndex == -1)
            {
                tbMissionDescription.Text = string.Empty;
                btnLaunch.AllowClick = false;
                return;
            }

            //改变

            Mission mission = Missions[lbCampaignList.SelectedIndex];


            ddGameMod.Items.Clear();

            foreach (string s in mission.Mod)
            {
                if (s != "")
                {
                    XNADropDownItem item = new XNADropDownItem();
                    item.Text = Mod[s][1];
                    item.Tag = new string[]{Mod[s][0],Mod[s][2], Mod[s][3]};

                    if (ddGameMod.Items.Find(i => i.Text == item.Text && i.Tag == item.Tag) ==null)
                        ddGameMod.AddItem(item);

                    if (ModParent.ContainsKey(s)&&ModParent[s].Count!=0)
                        
                        for(int i=0; i < ModParent[s].Count;i++) 
                        {
                            XNADropDownItem item1 = new XNADropDownItem();
                            item1.Text = ModParent[s][i][1];
                            item1.Tag = new string[] { ModParent[s][i][0], ModParent[s][i][2], ModParent[s][i][3] };

                            if (ddGameMod.Items.Find(i => i.Text == item1.Text) == null)
                                ddGameMod.AddItem(item1);
                        }

                   
                    
                }
            }


            ddGameMod.SelectedIndex = ddGameMod.Items.FindIndex(i => ((object[])i.Tag)[0].ToString() == mission.defaultMod);

            if (string.IsNullOrEmpty(mission.Scenario))
            {
                tbMissionDescription.Text = string.Empty;
                btnLaunch.AllowClick = false;
                return;
            }

            tbMissionDescription.Text = mission.GUIDescription;

            if (!mission.Enabled)
            {
                btnLaunch.AllowClick = false;
                return;
            }

            btnLaunch.AllowClick = true;

        }

        private void BtnCancel_LeftClick(object sender, EventArgs e)
        {
            Enabled = false;
        }

        private void BtnLaunch_LeftClick(object sender, EventArgs e)
        {
            int selectedMissionId = lbCampaignList.SelectedIndex;

            Mission mission = Missions[selectedMissionId];

            if (!ClientConfiguration.Instance.ModMode &&
                (!Updater.IsFileNonexistantOrOriginal(mission.Scenario) || AreFilesModified()))
            {
                // Confront the user by showing the cheater screen
                missionToLaunch = mission;
                cheaterWindow.Enable();
                return;
            }


            LaunchMission(mission);
        }

        public void DelFile(List<string> deleteFile)
        {
            //  string resultDirectory = Environment.CurrentDirectory;//目录

            if (deleteFile != null)
            {
                for (int i = 0; i < deleteFile.Count; i++)
                {
                    try
                    {
                        File.Delete(deleteFile[i]);
                    }
                    catch
                    {
                        continue;
                    }
                }
            }
        }
        public void CopyDirectory(string sourceDirPath, string saveDirPath)
        {

            if (sourceDirPath != null && sourceDirPath != "")
            {

                if (!Directory.Exists(saveDirPath))
                {
                    Directory.CreateDirectory(saveDirPath);
                }
                string[] files = Directory.GetFiles(sourceDirPath);
                foreach (string file in files)
                {
                    string pFilePath = saveDirPath + "\\" + Path.GetFileName(file);

                    File.Copy(file, pFilePath, true);
                }
            }
        }

        protected List<string> GetDeleteFile(string oldGame)
        {
            if (oldGame == null || oldGame == "")
                return null;

            List<string> deleteFile = new List<string>();

            foreach (string file in Directory.GetFiles(oldGame))
            {
                deleteFile.Add(Path.GetFileName(file));

            }

            return deleteFile;
        }


        private bool AreFilesModified()
        {
            foreach (string filePath in filesToCheck)
            {
                if (!Updater.IsFileNonexistantOrOriginal(filePath))
                    return true;
            }

            return false;
        }
       
        /// <summary>
        /// Called when the user wants to proceed to the mission despite having
        /// being called a cheater.
        /// </summary>
        private void CheaterWindow_YesClicked(object sender, EventArgs e)
        {
            LaunchMission(missionToLaunch);
        }



        /// <summary>
        /// Starts a singleplayer mission.
        /// </summary>
        /// 

        private void LaunchMission(Mission mission)
        {
            bool copyMapsToSpawnmapINI = ClientConfiguration.Instance.CopyMissionsToSpawnmapINI;

            Logger.Log("About to write spawn.ini.");

            IniFile spawnReader = new IniFile(SafePath.CombineFilePath(ProgramConstants.GamePath, "spawn.ini"));

            string oldMain = spawnReader.GetStringValue("Settings", "Main", string.Empty);
          
        
            string newMain = ((object[])ddGameMod.SelectedItem.Tag)[2].ToString();

            string oldGame = spawnReader.GetStringValue("Settings", "Game", string.Empty);
            string newGame = ((object[])ddGameMod.SelectedItem.Tag)[1].ToString();
            string oldAttached = spawnReader.GetStringValue("Settings", "Attached", string.Empty);
            string newAttached = mission.Attached;
            string oldAi = spawnReader.GetStringValue("Settings", "AI", string.Empty);
            string newAi = "INI\\GameOptions\\AI\\Other";

            if (UserINISettings.Instance.Mod_cath)
            {
                if (oldMain != newMain)
                {
                    if(oldMain!=string.Empty)
                    DelFile(GetDeleteFile("INI/GameOptions/Game/" + oldMain));
                    if (newMain != string.Empty)
                        CopyDirectory("INI/GameOptions/Game/" + newMain, "./");
                }

                //如果和前一次使用的游戏不一样
                if (oldGame != newGame)
                {
                    DelFile(GetDeleteFile(oldGame));
                    CopyDirectory(newGame, "./");
                }

                if (oldAi != newAi)
                {
                    DelFile(GetDeleteFile(oldAi));
                    CopyDirectory(newAi, "./");
                }

                if (oldAttached != newAttached)
                {
                    DelFile(GetDeleteFile(oldAttached));
                    CopyDirectory(newAttached, "./");
                }
            }
            else
            {
                DelFile(GetDeleteFile(oldGame));
                DelFile(GetDeleteFile(oldAi));
                DelFile(GetDeleteFile(oldAttached));
                CopyDirectory(newGame, "./");
                CopyDirectory(newAi, "./");
                CopyDirectory(newAttached, "./");
            }
            using var spawnStreamWriter = new StreamWriter(SafePath.CombineFilePath(ProgramConstants.GamePath, "spawn.ini"));

            spawnStreamWriter.WriteLine("; Generated by DTA Client");
            //spawnStreamWriter.WriteLine("[Actions]");
            //spawnStreamWriter.WriteLine("01000022=1,16,0,0,0,0,0,0,A");
            //spawnStreamWriter.WriteLine("[Events]");
            //spawnStreamWriter.WriteLine("01000022 = 1,8,0,0");
            //spawnStreamWriter.WriteLine("[Tags]");
            //spawnStreamWriter.WriteLine("01000023=0,New Trigger 1,01000022");
            //spawnStreamWriter.WriteLine("[Triggers]");
            //spawnStreamWriter.WriteLine("01000022=Americans,<none>,New Trigger,0,1,1,1,0");

            spawnStreamWriter.WriteLine("[Settings]");
            if (copyMapsToSpawnmapINI)
                spawnStreamWriter.WriteLine("Scenario=spawnmap.ini");
            else
                spawnStreamWriter.WriteLine("Scenario=" + mission.Scenario);

            // No one wants to play missions on Fastest, so we'll change it to Faster
            if (UserINISettings.Instance.GameSpeed == 0)
                UserINISettings.Instance.GameSpeed.Value = 1;

            spawnStreamWriter.WriteLine("Main=" + newMain);
            //写入当前游戏
            spawnStreamWriter.WriteLine("Game=" + newGame);
            spawnStreamWriter.WriteLine("AI=" + newAi);

            spawnStreamWriter.WriteLine("Attached=" + newAttached);
          
            spawnStreamWriter.WriteLine("CampaignID=" + mission.Index);
            spawnStreamWriter.WriteLine("GameSpeed=" + UserINISettings.Instance.GameSpeed);
            spawnStreamWriter.WriteLine("Firestorm=" + mission.RequiredAddon);
            spawnStreamWriter.WriteLine("CustomLoadScreen=" + LoadingScreenController.GetLoadScreenName(mission.Side.ToString()));
            spawnStreamWriter.WriteLine("IsSinglePlayer=Yes");
            spawnStreamWriter.WriteLine("SidebarHack=" + ClientConfiguration.Instance.SidebarHack);
            spawnStreamWriter.WriteLine("Side=" + mission.Side);
            spawnStreamWriter.WriteLine("BuildOffAlly=" + mission.BuildOffAlly);

            UserINISettings.Instance.Difficulty.Value = trbDifficultySelector.Value;

            spawnStreamWriter.WriteLine("DifficultyModeHuman=" + (mission.PlayerAlwaysOnNormalDifficulty ? "1" : trbDifficultySelector.Value.ToString()));
            spawnStreamWriter.WriteLine("DifficultyModeComputer=" + GetComputerDifficulty());

            var difficultyIni = new IniFile(SafePath.CombineFilePath(ProgramConstants.GamePath, DifficultyIniPaths[trbDifficultySelector.Value]));
            string difficultyName = DifficultyNames[trbDifficultySelector.Value];

            spawnStreamWriter.WriteLine();
            spawnStreamWriter.WriteLine();
            spawnStreamWriter.WriteLine();

            if (copyMapsToSpawnmapINI)
            {

                var mapIni = new IniFile(SafePath.CombineFilePath(ProgramConstants.GamePath, mission.Scenario));
                IniFile.ConsolidateIniFiles(mapIni, difficultyIni);
                mapIni.WriteIniFile(SafePath.CombineFilePath(ProgramConstants.GamePath, "spawnmap.ini"));
            }

            UserINISettings.Instance.CampaignDefaultGameSpeed.Value = 6 - ddGameSpeed.SelectedIndex;
            UserINISettings.Instance.Difficulty.Value = trbDifficultySelector.Value;
            UserINISettings.Instance.SaveSettings();

            ((MainMenuDarkeningPanel)Parent).Hide();

            discordHandler.UpdatePresence(mission.GUIName, difficultyName, mission.IconPath, true);
            GameProcessLogic.GameProcessExited += GameProcessExited_Callback;

            GameProcessLogic.StartGameProcess(WindowManager);
        }

        private int GetComputerDifficulty() =>
            Math.Abs(trbDifficultySelector.Value - 2);

        private void GameProcessExited_Callback()
        {

            WindowManager.AddCallback(new Action(GameProcessExited), null);
        }

        protected virtual void GameProcessExited()
        {
            GameProcessLogic.GameProcessExited -= GameProcessExited_Callback;
            // Logger.Log("GameProcessExited: Updating Discord Presence.");
            discordHandler.UpdatePresence();
        }

        public void ReadMissionList()
        {
            lbCampaignList.Clear();
            Missions.Clear();
            Mod.Clear();
            string path = @"INI/";

            var files = Directory.GetFiles(path, "Battle*.ini");

            foreach (var file in files)
            {
                // Logger.Log(file);
                ParseBattleIni(file);
            }

            if (Missions.Count == 0)
                ParseBattleIni("INI/" + ClientConfiguration.Instance.BattleFSFileName);

            difficultyList = difficultyList.ToArray().GroupBy(p => p).Select(p => p.Key).ToList();
            sideList = sideList.ToArray().GroupBy(p => p).Select(p => p.Key).ToList();
        }

        /// <summary>
        /// Parses a Battle(E).ini file. Returns true if succesful (file found), otherwise false.
        /// </summary>
        /// <param name="path">The path of the file, relative to the game directory.</param>
        /// <returns>True if succesful, otherwise false.</returns>
        private bool ParseBattleIni(string path)
        {

            Logger.Log("Attempting to parse " + path + " to populate mission list.");

            FileInfo battleIniFileInfo = SafePath.GetFile(ProgramConstants.GamePath, path);
            if (!battleIniFileInfo.Exists)
            {
                Logger.Log("File " + path + " not found. Ignoring.");
                return false;
            }

            //if (Missions.Count > 0)
            //{
            //    throw new InvalidOperationException("Loading multiple Battle*.ini files is not supported anymore.");
            //}



            var battleIni = new IniFile(battleIniFileInfo.FullName);
            var files = Directory.GetFiles("INI/", "Mod&AI*.ini");
            List<string> modKeys = new List<string>();
            foreach (var file in files)
            {
                var mod_aiINI = new IniFile(file);
                foreach (var modkey in mod_aiINI.GetSectionKeys("Game"))
                {
                    modKeys.Add(modkey);
                    string modSection = mod_aiINI.GetStringValue("Game", modkey, "NOT FOUND");
                    if(Mod.ContainsKey(modSection)) { continue; }
                    //键为mod注册名，值为数组:mod名，mod文件路径，mod继承自哪个
                    Mod.Add(modSection, new string[] {
                        modSection,
                        mod_aiINI.GetStringValue(modSection, "Text", modSection).L10N("UI:ModName:" + modSection),
                        mod_aiINI.GetStringValue(modSection, "File", $"INI\\GameOptions\\Game\\{modSection}"),
                        mod_aiINI.GetStringValue(modSection,"Main",string.Empty)});

                    String p = mod_aiINI.GetStringValue(modSection, "Parent", string.Empty);
                    if (p != string.Empty)
                    {
                        if (!ModParent.ContainsKey(p))
                            ModParent[p] = new List<string[]>();
                        ModParent[p].Add(new string[] {
                            modSection,
                            mod_aiINI.GetStringValue(modSection, "Text", modSection).L10N("UI:ModName:" + modSection),
                            mod_aiINI.GetStringValue(modSection, "File", $"INI\\GameOptions\\Game\\{modSection}"),
                            mod_aiINI.GetStringValue(modSection,"Main",string.Empty)
                        });
                    }
                }
            }



            List<string> battleKeys = battleIni.GetSectionKeys("Battles");

            if (battleKeys == null)
                return false; // File exists but [Battles] doesn't


            for (int i = 0; i < battleKeys.Count; i++)
            {
                string battleEntry = battleKeys[i];
                string battleSection = battleIni.GetStringValue("Battles", battleEntry, "NOT FOUND");

                if (!battleIni.SectionExists(battleSection))
                    continue;

                var mission = new Mission(battleIni, battleSection, i);


                if (dddifficulty.SelectedIndex != 0 && mission.difficulty != (string)dddifficulty.SelectedItem.Tag)
                    continue;

                if (ddside.SelectedIndex != 0 && mission.IconPath != (string)ddside.SelectedItem.Tag)
                    continue;

                if (mission.difficulty != string.Empty)
                    difficultyList.Add(mission.difficulty);
                if (mission.IconPath != string.Empty)
                    sideList.Add(mission.IconPath);

                Missions.Add(mission);

                var item = new XNAListBoxItem();
                item.Text = mission.GUIName.L10N("UI:MissionName:" + mission.sectionName);
                item.Tag = mission.sectionName;
                if (!mission.Enabled)
                {
                    item.TextColor = UISettings.ActiveSettings.DisabledItemColor;
                }
                else if (string.IsNullOrEmpty(mission.Scenario))
                {
                    item.TextColor = AssetLoader.GetColorFromString(
                        ClientConfiguration.Instance.ListBoxHeaderColor);
                    item.IsHeader = true;
                    item.Selectable = false;
                }
                else
                {
                    item.TextColor = lbCampaignList.DefaultItemColor;
                }

                if (!string.IsNullOrEmpty(mission.IconPath))
                    item.Texture = AssetLoader.LoadTexture(mission.IconPath + "icon.png");

                lbCampaignList.AddItem(item);

            }



            Logger.Log("Finished parsing " + path + ".");
            return true;

        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
        }
    }
}
