﻿using ClientCore;
using ClientGUI;
using DTAClient.Domain;
using Microsoft.Xna.Framework;
using Rampastring.Tools;
using Rampastring.XNAUI;
using Rampastring.XNAUI.XNAControls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DTAClient.DXGUI.Generic
{
    public class GameLoadingWindow : XNAWindow
    {
        const string SAVED_GAMES_DIRECTORY = "Saved Games";

        public GameLoadingWindow(WindowManager windowManager) : base(windowManager)
        {

        }

        XNAMultiColumnListBox lbSaveGameList;
        XNAClientButton btnLaunch;
        List<SavedGame> savedGames = new List<SavedGame>();

        public override void Initialize()
        {
            Name = "GameLoadingWindow";
            BackgroundTexture = AssetLoader.LoadTexture("loadmissionbg.png");

            ClientRectangle = new Rectangle(0, 0, 600, 380);
            CenterOnParent();

            lbSaveGameList = new XNAMultiColumnListBox(WindowManager);
            lbSaveGameList.Name = "lbSaveGameList";
            lbSaveGameList.AddColumn("SAVED GAME NAME", 400);
            lbSaveGameList.AddColumn("DATE / TIME", 174);
            lbSaveGameList.ClientRectangle = new Rectangle(13, 13, 574, 317);
            lbSaveGameList.BackgroundTexture = AssetLoader.CreateTexture(new Color(0, 0, 0, 128), 1, 1);
            lbSaveGameList.DrawMode = PanelBackgroundImageDrawMode.STRETCHED;
            lbSaveGameList.SelectedIndexChanged += ListBox_SelectedIndexChanged;

            btnLaunch = new XNAClientButton(WindowManager);
            btnLaunch.ClientRectangle = new Rectangle(161, 345, 133, 23);
            btnLaunch.Text = "Load";
            btnLaunch.AllowClick = false;
            btnLaunch.LeftClick += BtnLaunch_LeftClick;

            var btnCancel = new XNAClientButton(WindowManager);
            btnCancel.Name = "btnCancel";
            btnCancel.ClientRectangle = new Rectangle(304, btnLaunch.ClientRectangle.Y, 133, 23);
            btnCancel.Text = "Cancel";
            btnCancel.LeftClick += BtnCancel_LeftClick;

            AddChild(lbSaveGameList);
            AddChild(btnLaunch);
            AddChild(btnCancel);

            base.Initialize();

            ListSaves();
        }

        private void ListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lbSaveGameList.SelectedIndex == -1)
                btnLaunch.AllowClick = false;
            else
                btnLaunch.AllowClick = true;
        }

        private void BtnCancel_LeftClick(object sender, EventArgs e)
        {
            Enabled = false;
        }

        private void BtnLaunch_LeftClick(object sender, EventArgs e)
        {
            SavedGame sg = savedGames[lbSaveGameList.SelectedIndex];
            Logger.Log("Loading saved game " + sg.FileName);

            File.Delete(MainClientConstants.gamepath + ProgramConstants.SPAWNER_SETTINGS);
            StreamWriter sw = new StreamWriter(MainClientConstants.gamepath + ProgramConstants.SPAWNER_SETTINGS);
            sw.WriteLine("; generated by DTA Client");
            sw.WriteLine("[Settings]");
            sw.WriteLine("Scenario=spawnmap.ini");
            sw.WriteLine("SaveGameName=" + sg.FileName);
            sw.WriteLine("LoadSaveGame=Yes");
            sw.WriteLine("SidebarHack=" + MCDomainController.Instance.SidebarHack);
            sw.WriteLine("Firestorm=No");
            sw.WriteLine("GameSpeed=" + UserINISettings.Instance.GameSpeed);
            sw.WriteLine();
            sw.Close();

            File.Delete(ProgramConstants.GamePath + "spawnmap.ini");
            sw = new StreamWriter(ProgramConstants.GamePath + "spawnmap.ini");
            sw.WriteLine("[Map]");
            sw.WriteLine("Size=0,0,50,50");
            sw.WriteLine("LocalSize=0,0,50,50");
            sw.WriteLine();
            sw.Close();

            Logger.Log("About to launch main executable.");

            Enabled = false;
            SharedUILogic.StartGameProcess(0);
        }

        public void ListSaves()
        {
            savedGames.Clear();
            lbSaveGameList.ClearItems();

            if (!Directory.Exists(ProgramConstants.GamePath + SAVED_GAMES_DIRECTORY))
            {
                Logger.Log("Saved Games directory not found!");
                return;
            }

            string[] files = Directory.GetFiles(MainClientConstants.gamepath + SAVED_GAMES_DIRECTORY + "\\",
                "*.SAV", SearchOption.TopDirectoryOnly);

            foreach (string file in files)
            {
                ParseSaveGame(file);
            }

            savedGames = savedGames.OrderBy(sg => sg.LastModified.Ticks).ToList();
            savedGames.Reverse();

            foreach (SavedGame sg in savedGames)
            {
                string[] item = new string[] {
                    Renderer.GetSafeString(sg.GUIName, lbSaveGameList.FontIndex),
                    sg.LastModified.ToString() };
                lbSaveGameList.AddItem(item, true);
            }
        }

        private void ParseSaveGame(string fileName)
        {
            string shortName = Path.GetFileName(fileName);

            SavedGame sg = new SavedGame(shortName);
            if (sg.ParseInfo())
                savedGames.Add(sg);
        }
    }
}
