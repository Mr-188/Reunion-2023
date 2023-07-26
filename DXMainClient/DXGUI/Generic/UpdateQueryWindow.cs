using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using ClientCore;
using ClientGUI;
using ClientUpdater;
using Localization;
using Microsoft.Xna.Framework;
using Rampastring.XNAUI;
using Rampastring.XNAUI.XNAControls;

namespace DTAClient.DXGUI.Generic
{
    /// <summary>
    /// A window that asks the user whether they want to update their game.
    /// </summary>
    public class UpdateQueryWindow : XNAWindow
    {
        public delegate void UpdateAcceptedEventHandler(object sender, EventArgs e);
        public event UpdateAcceptedEventHandler UpdateAccepted;

        public delegate void UpdateDeclinedEventHandler(object sender, EventArgs e);
        public event UpdateDeclinedEventHandler UpdateDeclined;

        public UpdateQueryWindow(WindowManager windowManager) : base(windowManager) { }

        private XNALabel lblDescription;
        private XNALabel lblUpdateSize;
        private XNAListBox updaterlog;
        private string changelogUrl;

        public override void Initialize()
        {
            changelogUrl = ClientConfiguration.Instance.ChangelogURL;

            Name = "UpdateQueryWindow";
            ClientRectangle = new Rectangle(0, 0, 251, 350);
            BackgroundTexture = AssetLoader.LoadTexture("updatequerybg.png");

            lblDescription = new XNALabel(WindowManager);
            lblDescription.ClientRectangle = new Rectangle(12, 9, 0, 0);
            lblDescription.Text = String.Empty;
            lblDescription.Name = nameof(lblDescription);

            var lblChangelogLink = new XNALinkLabel(WindowManager);
            lblChangelogLink.ClientRectangle = new Rectangle(12, 50, 0, 0);
            lblChangelogLink.Text = "View Changelog".L10N("UI:Main:ViewChangeLog");
            lblChangelogLink.IdleColor = Color.Goldenrod;
            lblChangelogLink.Name = nameof(lblChangelogLink);
            lblChangelogLink.LeftClick += LblChangelogLink_LeftClick;

            updaterlog = new XNAListBox(WindowManager);
            updaterlog.Name = nameof(updaterlog);
            updaterlog.ClientRectangle = new Rectangle(lblChangelogLink.X, lblChangelogLink.Y + 60, 225, 200);
            
            lblUpdateSize = new XNALabel(WindowManager);
            lblUpdateSize.ClientRectangle = new Rectangle(12, 80, 0, 0);
            lblUpdateSize.Text = String.Empty;
            lblUpdateSize.Name = nameof(lblUpdateSize);

            var btnYes = new XNAClientButton(WindowManager);
            btnYes.ClientRectangle = new Rectangle(12, 320, 75, 23);
            btnYes.Text = "Yes".L10N("UI:Main:ButtonYes");
            btnYes.LeftClick += BtnYes_LeftClick;
            btnYes.Name = nameof(btnYes);

            var btnNo = new XNAClientButton(WindowManager);
            btnNo.ClientRectangle = new Rectangle(164, 320, 75, 23);
            btnNo.Text = "No".L10N("UI:Main:ButtonNo");
            btnNo.LeftClick += BtnNo_LeftClick;
            btnNo.Name = nameof(btnNo);

            AddChild(lblDescription);
            AddChild(lblChangelogLink);
            AddChild(lblUpdateSize);
            AddChild(btnYes);
            AddChild(btnNo);
            AddChild(updaterlog);

            base.Initialize();

            CenterOnParent();
        }

        public async Task GetUpdateContentsAsync(string currentVersion, string latestVersion)
        {
            Dictionary<string, string> updateContents = new Dictionary<string, string>();
             
            // 读取INI文件
            string iniFilePath = "updater.ini"; // 替换为你的INI文件路径

            // 下载INI文件
            string iniFileUrl = "http://8.130.134.157/Updater/updater.ini";
            string iniContent;
            using (WebClient client = new WebClient())
            {
                try
                {
                    iniContent = await client.DownloadStringTaskAsync(iniFileUrl);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"无法下载INI文件: {ex.Message}");
                    return;
                }
            }

            // 解析INI文件内容
            using (StringReader reader = new StringReader(iniContent))
            {
                string line;
                string currentSection = string.Empty;
                List<string> currentContent = new List<string>();

                while ((line = reader.ReadLine()) != null)
                {
                    line = line.Trim();
                    if (line.StartsWith("[") && line.EndsWith("]"))
                    {
                        if (line.LastIndexOf(currentVersion) == -1)
                            continue;
                        else
                            break;
                    }

                    currentSection = line;
                    updaterlog.AddItem(currentSection);
                }
            }
        }




        private void LblChangelogLink_LeftClick(object sender, EventArgs e)
        {
            ProcessLauncher.StartShellProcess(changelogUrl);
        }

        private void BtnYes_LeftClick(object sender, EventArgs e)
        {
            UpdateAccepted?.Invoke(this, e);
        }

        private void BtnNo_LeftClick(object sender, EventArgs e)
        {
            UpdateDeclined?.Invoke(this, e);
        }

        public void SetInfo(string version, int updateSize)
        {
            lblDescription.Text = string.Format(("Version {0} is available for download." + Environment.NewLine + "Do you wish to install it?").L10N("UI:Main:VersionAvailable"), version);
            if (updateSize >= 1000)
                lblUpdateSize.Text = string.Format("The size of the update is {0} MB.".L10N("UI:Main:UpdateSizeMB"), updateSize / 1000);
            else
                lblUpdateSize.Text = string.Format("The size of the update is {0} KB.".L10N("UI:Main:UpdateSizeKB"), updateSize);
        }
    }
}
