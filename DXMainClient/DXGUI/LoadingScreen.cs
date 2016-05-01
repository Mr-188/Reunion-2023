﻿using ClientCore;
using ClientGUI;
using DTAClient.domain;
using DTAClient.domain.CnCNet;
using Microsoft.Xna.Framework;
using Rampastring.XNAUI;
using Rampastring.XNAUI.DXControls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Updater;

namespace DTAClient.DXGUI
{
    public class LoadingScreen : DXWindow
    {
        public LoadingScreen(Game game, WindowManager windowManager) : base(game, windowManager)
        {

        }

        private static readonly object locker = new object();

        bool updaterReady = false;
        bool mapsReady = false;

        //DXProgressBar progressBar;

        public override void Initialize()
        {
            ClientRectangle = new Rectangle(0, 0, 800, 600);
            Name = "LoadingScreen";

            BackgroundTexture = AssetLoader.LoadTexture("loadingscreen.png");

            CenterOnParent();

            base.Initialize();
        }

        private void CUpdater_OnLocalFileVersionsChecked()
        {
            lock (locker)
            {
                updaterReady = true;

                if (mapsReady)
                    AddCallback(new Action(Finish), null);
            }
        }

        public void Start()
        {
            MapLoader mapLoader = new MapLoader();
            mapLoader.MapLoadingComplete += MapLoader_MapLoadingComplete;

            mapLoader.LoadMapsAsync();

            if (!MCDomainController.Instance.GetModModeStatus())
            {
                CUpdater.OnLocalFileVersionsChecked += CUpdater_OnLocalFileVersionsChecked;

                Thread thread = new Thread(CUpdater.CheckLocalFileVersions);
                thread.Start();
            }
            else
            {
                updaterReady = true;
            }
        }

        private void MapLoader_MapLoadingComplete(object sender, EventArgs e)
        {
            lock (locker)
            {
                mapsReady = true;

                if (updaterReady)
                    AddCallback(new Action(Finish), null);
            }
        }

        private void Finish()
        {
            MainMenu mm = new MainMenu(Game, WindowManager);
            CUpdater.OnLocalFileVersionsChecked -= CUpdater_OnLocalFileVersionsChecked;
            WindowManager.Instance.AddAndInitializeControl(mm);
            WindowManager.Instance.RemoveControl(this);
            mm.PostInit();
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
        }
    }
}