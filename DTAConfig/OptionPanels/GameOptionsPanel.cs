﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using ClientCore;
using ClientCore.CnCNet5;
using ClientGUI;
using DTAConfig.Settings;
using Localization;
using Microsoft.Xna.Framework;
using Rampastring.XNAUI;
using Rampastring.XNAUI.XNAControls;
//using static System.Windows.Forms.VisualStyles.VisualStyleElement.Menu;

namespace DTAConfig.OptionPanels
{
    class GameOptionsPanel : XNAOptionsPanel
    {

#if TS
        private const string TEXT_BACKGROUND_COLOR_TRANSPARENT = "0";
        private const string TEXT_BACKGROUND_COLOR_BLACK = "12";
#endif
        private const int MAX_SCROLL_RATE = 6;

        public GameOptionsPanel(WindowManager windowManager, UserINISettings iniSettings, XNAControl topBar)
            : base(windowManager, iniSettings)
        {
            this.topBar = topBar;
        }

        private XNALabel lblScrollRateValue;

        private XNATrackbar trbScrollRate;
        private XNAClientCheckBox chkTargetLines;
        private XNAClientCheckBox chkScrollCoasting;
        private XNAClientCheckBox chkTooltips;
#if TS
        private XNAClientCheckBox chkAltToUndeploy;
        private XNAClientCheckBox chkBlackChatBackground;
#else
        private XNAClientCheckBox chkShowHiddenObjects;
#endif

        private XNAClientCheckBox chkStartCap;

        private XNAControl topBar;

        private XNATextBox tbPlayerName;

        private HotkeyConfigurationWindow hotkeyConfigWindow;

        public override void Initialize()
        {
            base.Initialize();

            Name = "GameOptionsPanel";

            var lblScrollRate = new XNALabel(WindowManager);
            lblScrollRate.Name = "lblScrollRate";
            lblScrollRate.ClientRectangle = new Rectangle(12,
                14, 0, 0);
            lblScrollRate.Text = "Scroll Rate:".L10N("UI:DTAConfig:ScrollRate");

            lblScrollRateValue = new XNALabel(WindowManager);
            lblScrollRateValue.Name = "lblScrollRateValue";
            lblScrollRateValue.FontIndex = 1;
            lblScrollRateValue.Text = "3";
            lblScrollRateValue.ClientRectangle = new Rectangle(
                Width - lblScrollRateValue.Width - 12,
                lblScrollRate.Y, 0, 0);

            trbScrollRate = new XNATrackbar(WindowManager);
            trbScrollRate.Name = "trbClientVolume";
            trbScrollRate.ClientRectangle = new Rectangle(
                lblScrollRate.Right + 32,
                lblScrollRate.Y - 2,
                lblScrollRateValue.X - lblScrollRate.Right - 47,
                22);
            trbScrollRate.BackgroundTexture = AssetLoader.CreateTexture(new Color(0, 0, 0, 128), 2, 2);
            trbScrollRate.MinValue = 0;
            trbScrollRate.MaxValue = MAX_SCROLL_RATE;
            trbScrollRate.ValueChanged += TrbScrollRate_ValueChanged;

            chkScrollCoasting = new SettingCheckBox(WindowManager, true, UserINISettings.OPTIONS, "ScrollMethod", true, "0", "1");
            chkScrollCoasting.Name = "chkScrollCoasting";
            chkScrollCoasting.ClientRectangle = new Rectangle(
                lblScrollRate.X,
                trbScrollRate.Bottom + 20, 0, 0);
            chkScrollCoasting.Text = "Scroll Coasting".L10N("UI:DTAConfig:ScrollCoasting");

            //选择游戏
            var lblGameMod = new XNALabel(WindowManager);
            lblGameMod.Name = "lblGameMod";
            lblGameMod.ClientRectangle = new Rectangle(400, chkScrollCoasting.Y, 0, 0);
            lblGameMod.Text = "Mod:".L10N("UI:DTAConfig:Mod");

            chkStartCap = new XNAClientCheckBox(WindowManager);
            chkStartCap.Name = "chkStartCap";
            chkStartCap.ClientRectangle = new Rectangle(lblGameMod.X + 60, chkScrollCoasting.Y, 150, 20);
            chkStartCap.Text = "启动时是否检查任务包";

            //foreach (string s in UserINISettings.Instance.GameModName.Value.Split(','))
            //    chkStartCap.AddItem(s);

            chkTargetLines = new SettingCheckBox(WindowManager, true, UserINISettings.OPTIONS, "UnitActionLines");
            chkTargetLines.Name = "chkTargetLines";
            chkTargetLines.ClientRectangle = new Rectangle(
                lblScrollRate.X,
                chkScrollCoasting.Bottom + 24, 0, 0);
            chkTargetLines.Text = "Target Lines".L10N("UI:DTAConfig:TargetLines");

            chkTooltips = new SettingCheckBox(WindowManager, true, UserINISettings.OPTIONS, "ToolTips");
            chkTooltips.Name = "chkTooltips";
            chkTooltips.Text = "Tooltips".L10N("UI:DTAConfig:Tooltips");

            var lblPlayerName = new XNALabel(WindowManager);
            lblPlayerName.Name = "lblPlayerName";
            lblPlayerName.Text = "Player Name*:".L10N("UI:DTAConfig:PlayerName");

#if TS
            chkTooltips.ClientRectangle = new Rectangle(
                lblScrollRate.X,
                chkTargetLines.Bottom + 24, 0, 0);
#else
            chkShowHiddenObjects = new SettingCheckBox(WindowManager, true, UserINISettings.OPTIONS, "ShowHidden");
            chkShowHiddenObjects.Name = "chkShowHiddenObjects";
            chkShowHiddenObjects.ClientRectangle = new Rectangle(
                lblScrollRate.X,
                chkTargetLines.Bottom + 24, 0, 0);
            chkShowHiddenObjects.Text = "Show Hidden Objects".L10N("UI:DTAConfig:YRShowHidden");

            chkTooltips.ClientRectangle = new Rectangle(
                lblScrollRate.X,
                chkShowHiddenObjects.Bottom + 24, 0, 0);

            lblPlayerName.ClientRectangle = new Rectangle(
                lblScrollRate.X,
                chkTooltips.Bottom + 30, 0, 0);

            AddChild(chkShowHiddenObjects);
#endif

#if TS
            chkBlackChatBackground = new SettingCheckBox(WindowManager, false, UserINISettings.OPTIONS, "TextBackgroundColor", true, TEXT_BACKGROUND_COLOR_BLACK, TEXT_BACKGROUND_COLOR_TRANSPARENT);
            chkBlackChatBackground.Name = "chkBlackChatBackground";
            chkBlackChatBackground.ClientRectangle = new Rectangle(
                chkScrollCoasting.X,
                chkTooltips.Bottom + 24, 0, 0);
            chkBlackChatBackground.Text = "Use black background for in-game chat messages".L10N("UI:DTAConfig:TSUseBlackBackgroundChat");

            AddChild(chkBlackChatBackground);

            chkAltToUndeploy = new SettingCheckBox(WindowManager, true, UserINISettings.OPTIONS, "MoveToUndeploy");
            chkAltToUndeploy.Name = "chkAltToUndeploy";
            chkAltToUndeploy.ClientRectangle = new Rectangle(
                chkScrollCoasting.X,
                chkBlackChatBackground.Bottom + 24, 0, 0);
            chkAltToUndeploy.Text = "Undeploy units by holding Alt key instead of a regular move command".L10N("UI:DTAConfig:TsUndeployAltKey");

            AddChild(chkAltToUndeploy);

            lblPlayerName.ClientRectangle = new Rectangle(
                lblScrollRate.X,
                chkAltToUndeploy.Bottom + 30, 0, 0);
#endif

            tbPlayerName = new XNATextBox(WindowManager);
            tbPlayerName.Name = "tbPlayerName";
            tbPlayerName.MaximumTextLength = ClientConfiguration.Instance.MaxNameLength;
            tbPlayerName.ClientRectangle = new Rectangle(trbScrollRate.X,
                lblPlayerName.Y - 2, 200, 19);
            tbPlayerName.Text = ProgramConstants.PLAYERNAME;

            var lblNotice = new XNALabel(WindowManager);
            lblNotice.Name = "lblNotice";
            lblNotice.ClientRectangle = new Rectangle(lblPlayerName.X,
                lblPlayerName.Bottom + 30, 0, 0);
            lblNotice.Text = ("* If you are currently connected to CnCNet, you need to log out and reconnect" +
                Environment.NewLine + "for your new name to be applied.").L10N("UI:DTAConfig:ReconnectAfterRename");

            hotkeyConfigWindow = new HotkeyConfigurationWindow(WindowManager);
            DarkeningPanel.AddAndInitializeWithControl(WindowManager, hotkeyConfigWindow);
            hotkeyConfigWindow.Disable();

            var btnConfigureHotkeys = new XNAClientButton(WindowManager);
            btnConfigureHotkeys.Name = "btnConfigureHotkeys";
            btnConfigureHotkeys.ClientRectangle = new Rectangle(lblPlayerName.X, lblNotice.Bottom + 36, UIDesignConstants.BUTTON_WIDTH_160, UIDesignConstants.BUTTON_HEIGHT);
            btnConfigureHotkeys.Text = "Configure Hotkeys".L10N("UI:DTAConfig:ConfigureHotkeys");
            btnConfigureHotkeys.LeftClick += BtnConfigureHotkeys_LeftClick;

            var btnRecover = new XNAClientButton(WindowManager);
            btnRecover.Name = "btnRecover";
            btnRecover.ClientRectangle = new Rectangle(lblPlayerName.X, lblNotice.Bottom + 72, UIDesignConstants.BUTTON_WIDTH_160, UIDesignConstants.BUTTON_HEIGHT);
            btnRecover.Text = "清理游戏文件缓存";
            btnRecover.SetToolTipText("如果游戏出现问题，可以点击这个按钮尝试修复。");
            btnRecover.LeftClick += BtnRecover_LeftClick;

            AddChild(lblScrollRate);
            AddChild(lblScrollRateValue);
            AddChild(trbScrollRate);
            AddChild(chkScrollCoasting);
            AddChild(chkTargetLines);
            AddChild(chkTooltips);
            AddChild(lblPlayerName);
            AddChild(tbPlayerName);
            AddChild(lblNotice);
            AddChild(btnConfigureHotkeys);
            AddChild(btnRecover);
            // AddChild(lblGameMod);
            AddChild(chkStartCap);
        }

        private void BtnRecover_LeftClick(object sender, EventArgs e)
        {
            XNAMessageBox xNAMessageBox = new XNAMessageBox(WindowManager, "清理确认", "你确定要清理文件缓存吗？", XNAMessageBoxButtons.YesNo);
            xNAMessageBox.Show();
            xNAMessageBox.YesClickedAction += (e) => Recover();
        }

        private void Recover()
        {
            try
            {

                File.Delete("expandmd94.mix");
                File.Delete("expandmd95.mix");
                File.Delete("expandmd96.mix");
                File.Delete("expandmd97.mix");
                File.Delete("expandmd98.mix");
                File.Delete("movies01.mix");
                File.Delete("movies02.mix");
                File.Delete("movmd03.mix");
                File.Delete("spawn.ini");
                File.Delete("phobos.dll");
                File.Delete("Mars.dll");
                File.Delete("sidec03.mix");
                File.Delete("aimd.ini");
                File.Delete("Ai.tlb");
                File.Delete("spawner.xdp");
                File.Delete("spawnmap.ini");

                XNAMessageBox.Show(WindowManager, "清理文件", "清理成功！");
            }
            catch (Exception ex)
            {
                XNAMessageBox.Show(WindowManager, "错误", "清理失败，可能是某个文件被占用了");
            }

        }
        private void BtnConfigureHotkeys_LeftClick(object sender, EventArgs e)
        {
            hotkeyConfigWindow.Enable();

            if (topBar.Enabled)
            {
                topBar.Disable();
                hotkeyConfigWindow.EnabledChanged += HotkeyConfigWindow_EnabledChanged;
            }
        }

        private void HotkeyConfigWindow_EnabledChanged(object sender, EventArgs e)
        {
            hotkeyConfigWindow.EnabledChanged -= HotkeyConfigWindow_EnabledChanged;
            topBar.Enable();
        }

        private void TrbScrollRate_ValueChanged(object sender, EventArgs e)
        {
            lblScrollRateValue.Text = trbScrollRate.Value.ToString();
        }

        public override void Load()
        {
            base.Load();

            int scrollRate = ReverseScrollRate(IniSettings.ScrollRate);

            if (scrollRate >= trbScrollRate.MinValue && scrollRate <= trbScrollRate.MaxValue)
            {
                trbScrollRate.Value = scrollRate;
                lblScrollRateValue.Text = scrollRate.ToString();
            }

            chkStartCap.Checked = UserINISettings.Instance.StartCap;


            tbPlayerName.Text = UserINISettings.Instance.PlayerName;
        }

        public bool HasChinese(string str)
        {
            return Regex.IsMatch(str, @"[\u4e00-\u9fa5]");
        }

        public override bool Save()
        {
            bool restartRequired = base.Save();

            if (HasChinese(tbPlayerName.Text))
            {
                XNAMessageBox messageBox = new XNAMessageBox(WindowManager, "出错", "请不要使用中文作为游戏名。", XNAMessageBoxButtons.OK);
                messageBox.Show();
                return false;
            }

            IniSettings.ScrollRate.Value = ReverseScrollRate(trbScrollRate.Value);

            string playerName = NameValidator.GetValidOfflineName(tbPlayerName.Text);

            if (playerName.Length > 0)
                IniSettings.PlayerName.Value = playerName;

            //if (chkStartCap.SelectedIndex != IniSettings.GameModSelect) {
            //    restartRequired = true;

            //    List<string> deleteFile = new List<string>();
            //    foreach (string file in Directory.GetFiles(UserINISettings.Instance.GameModPath.Value.Split(',')[UserINISettings.Instance.GameModSelect.Value]))
            //        deleteFile.Add(Path.GetFileName(file));

            //    DelFile(deleteFile);
            //    CopyDirectory(UserINISettings.Instance.GameModPath.Value.Split(',')[chkStartCap.SelectedIndex],"./");

            IniSettings.StartCap.Value = chkStartCap.Enabled;
            // }

            return restartRequired;
        }

        private void CopyDirectory(string sourceDirPath, string saveDirPath)
        {

            if (sourceDirPath != null)
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
                string[] folders = System.IO.Directory.GetDirectories(sourceDirPath);
                foreach (string folder in folders)
                {
                    string name = System.IO.Path.GetFileName(folder);
                    string dest = System.IO.Path.Combine(saveDirPath, name);
                    CopyDirectory(folder, dest);//构建目标路径,递归复制文件
                }
            }
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
        private int ReverseScrollRate(int scrollRate)
        {
            return Math.Abs(scrollRate - MAX_SCROLL_RATE);
        }
    }
}
