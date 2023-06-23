using Localization;
using ClientCore;
using ClientGUI;
using Microsoft.Xna.Framework;
using Rampastring.Tools;
using Rampastring.XNAUI;
using Rampastring.XNAUI.XNAControls;
using System;
using System.Collections.Generic;
using System.IO;
using ClientUpdater;
using System.Threading.Tasks;
using System.Net.Http;
using HtmlAgilityPack;
using IniParser;
using IniParser.Model;
using System.Text;
using System.IO.Compression;
using System.Net;
using System.Diagnostics;
using static System.Collections.Specialized.BitVector32;
using System.Linq;
using Newtonsoft.Json.Linq;
using ClientCore.CnCNet5;
using Localization.Tools;

namespace DTAConfig.OptionPanels
{
    class ComponentsPanel : XNAOptionsPanel
    {
        public ComponentsPanel(WindowManager windowManager, UserINISettings iniSettings)
            : base(windowManager, iniSettings)
        {
        }

        public int i = 0;

        List<XNAClientButton> installationButtons = new List<XNAClientButton>();

        bool downloadCancelled = false;

        private string baseUrl = "http://ra2wx.online/RU/Components/";

        string componentNamePath = Path.Combine(ProgramConstants.GamePath, "Resources", "components");

        FileIniDataParser iniParser;
        IniData iniData;
        public override void Initialize()
        {
            base.Initialize();

            //Logger.Log(tools.GetDirectUrl("https://mr-liu.lanzoum.com/iqHTj0prhdoh"));

            Name = "ComponentsPanel";

            
           iniParser = new FileIniDataParser();
           iniData = iniParser.ReadFile(componentNamePath);



            GetComponentsDataAsync();
           


            
        }
        public static bool AreKeyDataCollectionsEqual(KeyDataCollection collection1, KeyDataCollection collection2)
        {
            Dictionary<string, string> dict1 = collection1.ToDictionary(keyData => keyData.KeyName, keyData => keyData.Value);
            Dictionary<string, string> dict2 = collection2.ToDictionary(keyData => keyData.KeyName, keyData => keyData.Value);

            return dict1.SequenceEqual(dict2);
        }

        private async void GetComponentsDataAsync()
        {
         
            string filePath = Path.Combine(ProgramConstants.GamePath, "Components");

            var client = new HttpClient();
            try
            {
                var content = await client.GetStringAsync(baseUrl + "Components");
            
            Encoding fileEncoding = Encoding.GetEncoding("GBK");
            // 将内容写入临时文件
            File.WriteAllText(filePath, content, fileEncoding);

            var parser = new FileIniDataParser();
            IniData urldata = parser.ReadFile(filePath, fileEncoding); // 指定文件的实际编码方式
            IniData localdata = parser.ReadFile(Path.Combine(ProgramConstants.GamePath, "Resources", "components"), fileEncoding);

            int componentIndex = 0;
           
            foreach (SectionData section in urldata.Sections)
            {
                string buttonText = "Not Available";
                string name = section.Keys["name"];

                if (!string.IsNullOrEmpty(name))
                {
                    buttonText = "安装";
                }

                foreach (SectionData loaclsection in localdata.Sections)
                {
                    if (section.SectionName == loaclsection.SectionName) {
                        buttonText = "更新";
                        if(AreKeyDataCollectionsEqual(section.Keys, loaclsection.Keys))
                        {
                            buttonText = "卸载";
                        }
                        break;
                    }
                }

                XNAClientButton btn = new XNAClientButton(WindowManager);
                btn.Name = "btn" + name;
                btn.ClientRectangle = new Rectangle(Width - 145,
                    12 + componentIndex * 35, UIDesignConstants.BUTTON_WIDTH_133, UIDesignConstants.BUTTON_HEIGHT);
                btn.Text = buttonText;
                btn.Tag = section;
                btn.LeftClick += Btn_LeftClick;

                XNALabel lbl = new XNALabel(WindowManager);
                lbl.Name = "lbl" + name;
                lbl.ClientRectangle = new Rectangle(12, btn.Y + 2, 0, 0);
                lbl.Text = name;

                AddChild(btn);
                AddChild(lbl);

                installationButtons.Add(btn);

                componentIndex++;

            }
            }
            catch (Exception ex) { return; }
        }


        static async Task GetFileList(string url)
        {
            HttpClient client = new HttpClient();
            string html = await client.GetStringAsync(url);

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);

            foreach (HtmlNode link in doc.DocumentNode.SelectNodes("//a[@href]"))
            {
                HtmlAttribute att = link.Attributes["href"];
                Logger.Log(att.Value);
            }
        }
        public override void Load()
        {
            base.Load();

            // UpdateInstallationButtons();
        }

        private async Task DownloadFilesAsync(XNAClientButton button)
        {
        
           
            XNAProgressBar progressBar = new XNAProgressBar(WindowManager);
            progressBar.Name = nameof(progressBar);
            progressBar.Maximum = 100;
            progressBar.ClientRectangle = button.ClientRectangle;
            progressBar.Value = 0;

            AddChild(progressBar);


            string componentName = ((SectionData)button.Tag).SectionName;


            iniData.Sections.AddSection(componentName);
            iniData[componentName].AddKey("name", ((SectionData)button.Tag).Keys["name"]);
            string downloadUrl = string.Empty;
            string downloadPath = string.Empty;
            bool Special = false;
            foreach (KeyData keyData in ((SectionData)button.Tag).Keys)
            {
                
                if (keyData.KeyName == "name")
                    continue;

                if (keyData.KeyName == "Special")
                {
                    Special = true;
                    iniData[componentName].AddKey("Special", keyData.Value);
                    continue;
                }
                if(keyData.KeyName == "Unload")
                {
                    iniData[componentName].AddKey("Unload", keyData.Value);
                    continue;
                }
                if (Special)
                {
                    downloadUrl = ((SectionData)button.Tag).Keys["Special"] + keyData.KeyName;
                    downloadPath = Path.GetFileName(keyData.KeyName);
                }
                else
                {
                    downloadUrl = Path.Combine(baseUrl, ((SectionData)button.Tag).SectionName, keyData.KeyName);

                    downloadPath = Path.Combine(ProgramConstants.GamePath, keyData.KeyName);
                }

                string directory = Path.GetDirectoryName(downloadPath);


                if (!Directory.Exists(directory)&&!Special)
                {
                    Directory.CreateDirectory(directory);
                }

             
                using (WebClient webClient = new WebClient())
                {

                    webClient.DownloadProgressChanged += (s, evt) =>
                    {
                        progressBar.Value = evt.ProgressPercentage;
                    };
                    
                    await webClient.DownloadFileTaskAsync(new Uri(downloadUrl), downloadPath);
                }

                i = 1;
                iniData[componentName].AddKey(keyData.KeyName, keyData.Value);
            }

            if (Special)
            {

                await Task.Run(() => 
                ZIP.Unpack(ProgramConstants.GamePath + Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(downloadUrl)) + ".7z.001")
                );

            }

            progressBar.Visible = false;
            button.Visible = true;
        }

            private async void Btn_LeftClick(object sender, EventArgs e)
        {
            XNAClientButton button = (XNAClientButton)sender;

            bool Special = false;
            if (button.Text == "安装")
            {
                button.Visible = false;
                await DownloadFilesAsync(button);
                // 更新按钮文本
                button.Text = "卸载";
                
            }
            else if (button.Text == "卸载")
            {
                try
                {
                    foreach (KeyData keyData in ((SectionData)button.Tag).Keys)
                    {
                        if (keyData.KeyName == "name")
                            continue;
                    if (keyData.KeyName == "Special")
                    {
                        Special = true;
                        continue;
                    }
                        if ((keyData.KeyName == "Unload"))
                            continue;
                    if(!Special)
                        File.Delete(Path.Combine(ProgramConstants.GamePath, keyData.KeyName));
                    }
                    if(Special)
                {
                    foreach (string fiename in ((SectionData)button.Tag).Keys["Unload"].Split(','))
                        File.Delete(fiename);

                }

                    iniData.Sections.RemoveSection(((SectionData)button.Tag).SectionName);
                    button.Text = "安装";
            }
                catch {
               return;
            }
        }
            else
            {
                button.Text = "卸载";
                button.OnLeftClick();
                button.Text = "安装";
                button.OnLeftClick();
            }
            iniParser.WriteFile(componentNamePath, iniData);
        }
  

            //var btn = (XNAClientButton)sender;

            //var cc = (CustomComponent)btn.Tag;

            //if (cc.IsBeingDownloaded)
            //    return;

            //FileInfo localFileInfo = SafePath.GetFile(ProgramConstants.GamePath, cc.LocalPath);

            //if (localFileInfo.Exists)
            //{
            //    if (cc.LocalIdentifier == cc.RemoteIdentifier)
            //    {
            //        localFileInfo.Delete();
            //        btn.Text = "Install".L10N("UI:DTAConfig:Install") + $" ({GetSizeString(cc.RemoteSize)})";
            //        return;
            //    }

            //    btn.AllowClick = false;

            //    cc.DownloadFinished += cc_DownloadFinished;
            //    cc.DownloadProgressChanged += cc_DownloadProgressChanged;
            //    cc.DownloadComponent();
            //}
            //else
            //{
            //    var msgBox = new XNAMessageBox(WindowManager, "Confirmation Required".L10N("UI:DTAConfig:UpdateConfirmRequiredTitle"),
            //        string.Format(("To enable {0} the Client will download the necessary files to your game directory." +
            //        Environment.NewLine + Environment.NewLine + "This will take an additional {1} of disk space (size of the download is {2}), and the download may last" +
            //        Environment.NewLine +
            //        "from a few minutes to multiple hours depending on your Internet connection speed." +
            //        Environment.NewLine + Environment.NewLine +
            //        "You will not be able to play during the download. Do you want to continue?").L10N("UI:DTAConfig:UpdateConfirmRequiredText"),
            //        cc.GUIName, GetSizeString(cc.RemoteSize), GetSizeString(cc.Archived ? cc.RemoteArchiveSize : cc.RemoteSize)
            //        ), XNAMessageBoxButtons.YesNo);

            //    msgBox.Tag = btn;
            //    msgBox.Show();
            //    msgBox.YesClickedAction = MsgBox_YesClicked;
            //}



            private void MsgBox_YesClicked(XNAMessageBox messageBox)
        {
            var btn = (XNAClientButton)messageBox.Tag;
            btn.AllowClick = false;

            var cc = (CustomComponent)btn.Tag;

            cc.DownloadFinished += cc_DownloadFinished;
            cc.DownloadProgressChanged += cc_DownloadProgressChanged;
            cc.DownloadComponent();
        }

        public void InstallComponent(int id)
        {
            var btn = installationButtons[id];
            btn.AllowClick = false;

            var cc = (CustomComponent)btn.Tag;

            cc.DownloadFinished += cc_DownloadFinished;
            cc.DownloadProgressChanged += cc_DownloadProgressChanged;
            cc.DownloadComponent();
        }

        /// <summary>
        /// Called whenever a custom component download's progress is changed.
        /// </summary>
        /// <param name="c">The CustomComponent object.</param>
        /// <param name="percentage">The current download progress percentage.</param>
        private void cc_DownloadProgressChanged(CustomComponent c, int percentage)
        {
            WindowManager.AddCallback(new Action<CustomComponent, int>(HandleDownloadProgressChanged), c, percentage);
        }

        private void HandleDownloadProgressChanged(CustomComponent cc, int percentage)
        {
            percentage = Math.Min(percentage, 100);

            var btn = installationButtons.Find(b => object.ReferenceEquals(b.Tag, cc));

            if (cc.Archived && percentage == 100)
                btn.Text = "Unpacking...".L10N("UI:DTAConfig:Unpacking");
            else
                btn.Text = "Downloading...".L10N("UI:DTAConfig:Downloading") + " " + percentage + "%";
        }

        /// <summary>
        /// Called whenever a custom component download is finished.
        /// </summary>
        /// <param name="c">The CustomComponent object.</param>
        /// <param name="success">True if the download succeeded, otherwise false.</param>
        private void cc_DownloadFinished(CustomComponent c, bool success)
        {
            WindowManager.AddCallback(new Action<CustomComponent, bool>(HandleDownloadFinished), c, success);
        }

        private void HandleDownloadFinished(CustomComponent cc, bool success)
        {
            cc.DownloadFinished -= cc_DownloadFinished;
            cc.DownloadProgressChanged -= cc_DownloadProgressChanged;

            var btn = installationButtons.Find(b => object.ReferenceEquals(b.Tag, cc));
            btn.AllowClick = true;

            if (!success)
            {
                if (!downloadCancelled)
                {
                    XNAMessageBox.Show(WindowManager, "Optional Component Download Failed".L10N("UI:DTAConfig:OptionalComponentDownloadFailedTitle"),
                        string.Format(("Download of optional component {0} failed." + Environment.NewLine +
                        "See client.log for details." + Environment.NewLine + Environment.NewLine +
                        "If this problem continues, please contact your mod's authors for support.").L10N("UI:DTAConfig:OptionalComponentDownloadFailedText"),
                        cc.GUIName));
                }

                btn.Text = "Install".L10N("UI:DTAConfig:Install") + $" ({GetSizeString(cc.RemoteSize)})";

                if (SafePath.GetFile(ProgramConstants.GamePath, cc.LocalPath).Exists)
                    btn.Text = "Update".L10N("UI:DTAConfig:Update") + $" ({GetSizeString(cc.RemoteSize)})";
            }
            else
            {
                XNAMessageBox.Show(WindowManager, "Download Completed".L10N("UI:DTAConfig:DownloadCompleteTitle"),
                    string.Format("Download of optional component {0} completed succesfully.".L10N("UI:DTAConfig:DownloadCompleteText"), cc.GUIName));
                btn.Text = "Uninstall".L10N("UI:DTAConfig:Uninstall");
            }
        }

        public override bool Save()
        {
         //   CampaignSelector campaignSelector = new CampaignSelector();
         
            return true;
        }

        public void CancelAllDownloads()
        {
            Logger.Log("Cancelling all custom component downloads.");

            downloadCancelled = true;

            if (Updater.CustomComponents == null)
                return;

            foreach (CustomComponent cc in Updater.CustomComponents)
            {
                if (cc.IsBeingDownloaded)
                    cc.StopDownload();
            }
        }

        public void Open()
        {
            downloadCancelled = false;
        }

        private string GetSizeString(long size)
        {
            if (size < 1048576)
            {
                return (size / 1024) + " KB";
            }
            else
            {
                return (size / 1048576) + " MB";
            }
        }

        public static explicit operator ComponentsPanel(bool v)
        {
            throw new NotImplementedException();
        }
    }
}
