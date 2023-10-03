using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using ClientCore;
using ClientCore.Settings;
using ClientGUI;
using ClientUpdater;
using HtmlAgilityPack;
using IniParser;
using IniParser.Model;
using Localization;
using Localization.Tools;
using Microsoft.Xna.Framework;
using Rampastring.Tools;
using Rampastring.XNAUI;
using Rampastring.XNAUI.XNAControls;

namespace DTAConfig.OptionPanels
{
    class ComponentsPanel : XNAOptionsPanel
    {
        public ComponentsPanel(WindowManager windowManager, UserINISettings iniSettings)
            : base(windowManager, iniSettings)
        {
        }

        public int i = 0;

        private XNAClientButton mainbutton;

        List<XNAClientButton> installationButtons = new List<XNAClientButton>();

        private XNAProgressBar progressBar;

        bool downloadCancelled = false;


        private string baseUrl;

        string componentNamePath = Path.Combine(ProgramConstants.GamePath, "Resources", "Components");

        FileIniDataParser iniParser;
        IniData iniData;

        private XNAMultiColumnListBox CompList;

        private List<XNAProgressBar> progressBars = new List<XNAProgressBar>();


        private List<SectionData> buttons = new List<SectionData>();
        public override void Initialize()
        {
            base.Initialize();

            Name = "ComponentsPanel";

            CompList = new XNAMultiColumnListBox(WindowManager);
            CompList.Name = nameof(CompList);
            CompList.ClientRectangle = new Rectangle(20, 10, Width - 40, Height - 60);
            CompList.SelectedIndexChanged += CompList_SelectedChanged;
            CompList.LineHeight = 30;
            //CompList.LineHeight = 25; //行间距扩大
            CompList.FontIndex = 1;

            CompList.AddColumn("组件", CompList.Width - 100);
            CompList.AddColumn("状态", CompList.Width - 50);

            AddChild(CompList);

            iniParser = new FileIniDataParser();

            if (NetWorkINISettings.Instance != null)
            {

                try
                {
                    iniData = iniParser.ReadFile(componentNamePath);
                    baseUrl = NetWorkINISettings.Instance.ComponentsMirrors;

                    mainbutton = new XNAClientButton(WindowManager);
                    mainbutton.Name = nameof(mainbutton);
                    mainbutton.ClientRectangle = new Rectangle(Width / 2 - 60, Height - 40, UIDesignConstants.BUTTON_WIDTH_133, UIDesignConstants.BUTTON_HEIGHT);
                    mainbutton.LeftClick += Btn_LeftClick;
                    AddChild(mainbutton);

                    GetComponentsDataAsync();

                    if (CompList.ItemCount > 0)
                        CompList.SelectedIndex = 1;
                }
                catch (Exception ex) 
                {
                    Logger.Log("组件初始化出错：" + ex);
                }
            }
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

               // Logger.Log(content);

                Encoding fileEncoding = Encoding.GetEncoding("UTF-8");
                // 将内容写入临时文件
                File.WriteAllText(filePath, content, fileEncoding);

                var parser = new FileIniDataParser();
                IniData urldata = parser.ReadFile(filePath, fileEncoding); // 指定文件的实际编码方式
                IniData localdata = parser.ReadFile(Path.Combine(ProgramConstants.GamePath, "Resources", "components"), fileEncoding);

                int componentIndex = 0;

                foreach (SectionData section in urldata.Sections)
                {
                    string buttonText = "不可用";
                    string name = section.Keys["name"];
                    Color color = Color.Red;
                    if (!string.IsNullOrEmpty(name))
                    {
                        
                        buttonText = "未安装";
                        color = Color.Blue;
                    }

                    foreach (SectionData loaclsection in localdata.Sections)
                    {
                        if (section.SectionName == loaclsection.SectionName)
                        {
                            buttonText = "可以更新";
                            if (AreKeyDataCollectionsEqual(section.Keys, loaclsection.Keys))
                            {
                                buttonText = "已安装";
                                color = Color.Green;
                            }
                            break;
                        }
                    }

                 

                    XNAListBoxItem lbl = new XNAListBoxItem
                    {
                        Text = name
                    };

                    //  lbGameModeMapList.AddItem(mapInfoArray);

                    XNAListBoxItem btn = new XNAListBoxItem
                    {
                       // Name = name,
                       // ClientRectangle = new Rectangle(Width - 145,
                       // 12 + componentIndex * 35, UIDesignConstants.BUTTON_WIDTH_133, UIDesignConstants.BUTTON_HEIGHT),
                        Text = buttonText,
                        TextColor = color,
                        Tag = section
                    };

                    buttons.Add(section);

                    XNAListBoxItem[] mapInfoArray = {
                    lbl,
                    btn,
                };


                    CompList.AddItem(mapInfoArray);

                // btn.LeftClick += Btn_LeftClick;

                // buttons.Add(btn);



                XNAProgressBar progressBar = new XNAProgressBar(WindowManager);
                    progressBar.Name = section.SectionName;
                    progressBar.Maximum = 100;
                    progressBar.ClientRectangle = mainbutton.ClientRectangle;
                    progressBar.Value = 0;
                    progressBar.Tag = false;
                    progressBar.Visible = false;
                    progressBars.Add(progressBar);
                    AddChild(progressBar);

                    //AddChild(btn);
                    //AddChild(lbl);

                    //installationButtons.Add(btn);

                    componentIndex++;

                }
            }
            catch { return; }
        }

        private void CompList_SelectedChanged(object sender, EventArgs e)
        {
            if (CompList.SelectedIndex < 0 || CompList.SelectedIndex > CompList.ItemCount)
                return;

            XNAListBoxItem s = CompList.GetItem(1, CompList.SelectedIndex);
            if (progressBar != null)
            {
                progressBar.Visible = false;
            }

            progressBar = progressBars.Find(p => p.Name == ((SectionData)s.Tag).SectionName);
            

          

            if ((bool)progressBar.Tag)
            {
                progressBar.Visible = true;
                mainbutton.Visible = false;
            }
            else
            {
                progressBar.Visible = false;
                mainbutton.Visible = true;
               
            }
            if (s.Text == "未安装")
                mainbutton.Text = "安装";
            else if (s.Text == "已安装")
                mainbutton.Text = "卸载";
            else { mainbutton.Text = "更新"; }

            mainbutton.Tag = buttons[CompList.SelectedIndex];
           
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

        private async Task DownloadFilesAsync(SectionData button)
        {

           XNAProgressBar progressBar1 = progressBars.Find(p => p.Name == button.SectionName);

            progressBar1.Visible = true;

            progressBar1.Tag = true;

            string componentName = button.SectionName;

            CompList.GetItem(1, buttons.FindIndex(s => s == button)).Text = "安装中";
            CompList.GetItem(1, buttons.FindIndex(s => s == button)).TextColor = Color.White;
            iniData.Sections.AddSection(componentName);

            string downloadUrl = string.Empty;
            string downloadPath = string.Empty;
            bool Special = false;
            try
            {

                foreach (KeyData keyData in button.Keys)
                {

                    iniData[componentName].AddKey(keyData.KeyName, keyData.Value);

                    if (keyData.KeyName == "name")
                    {
                        // iniData[componentName].AddKey("name", ((SectionData)button.Tag).Keys["name"]);
                        continue;
                    }
                    if (keyData.KeyName == "Repel")
                    {

                        continue;
                    }
                    if (keyData.KeyName == "Special")
                    {
                        Special = true;
                        //iniData[componentName].AddKey("Special", keyData.Value);
                        continue;
                    }
                    if (keyData.KeyName == "Unload")
                    {
                        // iniData[componentName].AddKey("Unload", keyData.Value);
                        continue;
                    }
                    if (Special)
                    {
                        downloadUrl = keyData.KeyName;
                        downloadPath = Path.GetFileName(componentName + ".7z");
                    }
                    else
                    {
                        downloadUrl = Path.Combine(baseUrl, button.SectionName, keyData.KeyName);

                        downloadPath = Path.Combine(ProgramConstants.GamePath, keyData.KeyName);
                    }

                    string directory = Path.GetDirectoryName(downloadPath);


                    if (!Directory.Exists(directory) && !Special)
                    {
                        Directory.CreateDirectory(directory);
                    }


                    using (WebClient webClient = new WebClient())
                    {

                        webClient.DownloadProgressChanged += (s, evt) =>
                        {
                            progressBar1.Value = evt.ProgressPercentage;
                        };



                        await webClient.DownloadFileTaskAsync(new Uri(downloadUrl), downloadPath);
                    }

                    i = 1;
                    iniData[componentName].AddKey(keyData.KeyName, keyData.Value);
                }
            }
            catch (Exception ex)
            {
                XNAMessageBox.Show(WindowManager, "错误", $"下载时出错{ex}");
                CompList.GetItem(1, buttons.FindIndex(s => s == button)).Text = "未安装";
                CompList.GetItem(1, buttons.FindIndex(s => s == button)).TextColor = Color.Blue;
                progressBar1.Visible = false;
                progressBar1.Tag = false;
                return;
            }

            if (Special)
            {

                await Task.Run(() =>
                ZIP.Unpack(downloadPath)
                );

            }

            CompList.GetItem(1, buttons.FindIndex(s => s == button)).Text = "已安装";
            CompList.GetItem(1, buttons.FindIndex(s => s == button)).TextColor = Color.Green;
            if ((SectionData)mainbutton.Tag == button)
                mainbutton.Text = "卸载";
            progressBar1.Visible = false;
            progressBar1.Tag = false;
        
        }

        private async void Btn_LeftClick(object sender, EventArgs e)
        {
            XNAClientButton button = (XNAClientButton)sender;

            XNAProgressBar progressBar = progressBars.Find(p => p.Name == ((SectionData)button.Tag).SectionName);

            bool canDownload = false;
            if (button.Text == "安装")
            {
                List<SectionData> repellist = new List<SectionData>();


                if (((SectionData)button.Tag).Keys.ContainsKey("Repel"))
                {
                    foreach (string name in ((SectionData)button.Tag).Keys["Repel"].Split(','))
                    {
                        if (buttons.FindIndex(b => b.SectionName == name) != -1)
                        {
                            var repelButton = buttons.Find((b => b.SectionName == name));
                            repellist.Add(repelButton);

                        }
                    }
                    if (repellist.Count != 0)
                    {


                        XNAMessageBox xNAMessageBox = new XNAMessageBox(WindowManager,
                                   "信息",
                                   $"这个组件和以下组件不兼容：\n\n{string.Join("\n", repellist.Select(button => button.SectionName))}\n\n如果安装将会卸载以上组件。",
                                   XNAMessageBoxButtons.YesNo);

                        xNAMessageBox.YesClickedAction += async (e) =>
                        {
                            foreach (SectionData button3 in repellist)
                            {
                                //if (!button3.Visible)
                                //{
                                //    XNAMessageBox.Show(WindowManager, "信息", "互斥的组件正在下载中，请等待下载完成。");
                                //    return;
                                //}

                                unload(button3);
                            }
                                await DownloadFilesAsync((SectionData)button.Tag);

                        };
                        xNAMessageBox.CancelClickedAction += (e) => { return; };
                        xNAMessageBox.Show();

                    }
                    else
                    {
                       
                            await DownloadFilesAsync((SectionData)button.Tag);
                       
                    }

                }
                else
                {
                    
                        await DownloadFilesAsync((SectionData)button.Tag);
                    
                   
                }

            }
            else if (button.Text == "卸载")
            {
                unload((SectionData)button.Tag);
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


        private void unload(SectionData button)
        {
            try
            {
                bool Special = false;
            foreach (KeyData keyData in button.Keys)
            {
                if (keyData.KeyName == "name")
                    continue;
                if (keyData.KeyName == "Special")
                {
                    Special = true;
                    break;
                }
                if ((keyData.KeyName == "Unload"))
                    continue;
                if (!Special)
                    File.Delete(Path.Combine(ProgramConstants.GamePath, keyData.KeyName));
            }
            if (Special)
            {
                foreach (string fiename in button.Keys["Unload"].Split(','))
                {
                    if (File.Exists(fiename))
                    {
                        File.Delete(fiename);
                    }
                    else if (Directory.Exists(fiename))
                    {
                        Directory.Delete(fiename, true);
                    }
                }
            }

            iniData.Sections.RemoveSection(button.SectionName);

            mainbutton.Text = "安装";
            CompList.GetItem(1, buttons.FindIndex(s => s == button)).Text = "未安装";

            }
            catch
              {
                  return;
            }
        }



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

            return false;
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
