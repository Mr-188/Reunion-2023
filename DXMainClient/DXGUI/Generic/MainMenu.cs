using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ClientCore;
using ClientCore.Settings;
using ClientGUI;
using ClientUpdater;
using DTAClient.Domain;
using DTAClient.Domain.Multiplayer.CnCNet;
using DTAClient.DXGUI.Multiplayer;
using DTAClient.DXGUI.Multiplayer.CnCNet;
using DTAClient.DXGUI.Multiplayer.GameLobby;
using DTAClient.Online;
using DTAConfig;
using Localization;
using Localization.Tools;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Newtonsoft.Json.Linq;
using Rampastring.Tools;
using Rampastring.XNAUI;
using Rampastring.XNAUI.XNAControls;
using SendGrid.Helpers.Mail;
//using System.Windows.Forms;
//using Accord.Statistics.Kernels;

namespace DTAClient.DXGUI.Generic
{

    class GuideWindow : XNAWindow, ISwitchable
    {
        public GuideWindow(WindowManager windowManager) : base(windowManager)
        {
            //WindowManager windowManager
        }
        
        private XNALabel firstLabel; //引导语句

        public override void Initialize()
        {
            firstLabel = new XNALabel(WindowManager);
            firstLabel.ClientRectangle = new Rectangle(20, 10, 0, 0);
            firstLabel.Text = "看起来你是第一次运行游戏，先给自己取个名字吧！";

            base.Initialize();

            AddChild(firstLabel);

        }

        public string GetSwitchName()
        {
            throw new NotImplementedException();
        }

        public void SwitchOff()
        {
            throw new NotImplementedException();
        }

        public void SwitchOn()
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// The main menu of the client.
    /// </summary>
    class MainMenu : XNAWindow, ISwitchable
    {
        private const float MEDIA_PLAYER_VOLUME_FADE_STEP = 0.01f;
        private const float MEDIA_PLAYER_VOLUME_EXIT_FADE_STEP = 0.025f;
        private const double UPDATE_RE_CHECK_THRESHOLD = 30.0;

        /// <summary>
        /// Creates a new instance of the main menu.
        /// </summary>
        public MainMenu(
            WindowManager windowManager,
            SkirmishLobby skirmishLobby,
            LANLobby lanLobby,
            TopBar topBar,
            OptionsWindow optionsWindow,
            CnCNetLobby cncnetLobby,
            CnCNetManager connectionManager,
            DiscordHandler discordHandler,
            CnCNetGameLoadingLobby cnCNetGameLoadingLobby,
            CnCNetGameLobby cnCNetGameLobby,
            PrivateMessagingPanel privateMessagingPanel,
            PrivateMessagingWindow privateMessagingWindow,
            GameInProgressWindow gameInProgressWindow
        ) : base(windowManager)
        {
            this.lanLobby = lanLobby;
            this.topBar = topBar;
            this.connectionManager = connectionManager;
            this.optionsWindow = optionsWindow;
            this.cncnetLobby = cncnetLobby;
            this.discordHandler = discordHandler;
            this.skirmishLobby = skirmishLobby;
            this.cnCNetGameLoadingLobby = cnCNetGameLoadingLobby;
            this.cnCNetGameLobby = cnCNetGameLobby;
            this.privateMessagingPanel = privateMessagingPanel;
            this.privateMessagingWindow = privateMessagingWindow;
            this.gameInProgressWindow = gameInProgressWindow;
            this.cncnetLobby.UpdateCheck += CncnetLobby_UpdateCheck;
            isMediaPlayerAvailable = IsMediaPlayerAvailable();
        }

        private MainMenuDarkeningPanel innerPanel;

        private XNALabel lblCnCNetPlayerCount;
        private XNALinkLabel lblUpdateStatus;
        private XNALinkLabel lblVersion;

        private CnCNetLobby cncnetLobby;

        private SkirmishLobby skirmishLobby;

        private LANLobby lanLobby;

        private CnCNetManager connectionManager;

        private OptionsWindow optionsWindow;

        private DiscordHandler discordHandler;

        private TopBar topBar;
        private readonly CnCNetGameLoadingLobby cnCNetGameLoadingLobby;
        private readonly CnCNetGameLobby cnCNetGameLobby;
        private readonly PrivateMessagingPanel privateMessagingPanel;
        private readonly PrivateMessagingWindow privateMessagingWindow;
        private readonly GameInProgressWindow gameInProgressWindow;

        private XNAMessageBox firstRunMessageBox;

        private bool _updateInProgress;
        private bool UpdateInProgress
        {
            get { return _updateInProgress; }
            set
            {
                _updateInProgress = value;
                topBar.SetSwitchButtonsClickable(!_updateInProgress);
                topBar.SetOptionsButtonClickable(!_updateInProgress);
                SetButtonHotkeys(!_updateInProgress);
            }
        }

        private bool customComponentDialogQueued = false;

        private DateTime lastUpdateCheckTime;

        private Song themeSong;

        private static readonly object locker = new object();

        private bool isMusicFading = false;

        private readonly bool isMediaPlayerAvailable;

        private List<Keys> secretCodeSequence = new List<Keys> { Keys.Up, Keys.Up, Keys.Down, Keys.Down, Keys.Left, Keys.Right, Keys.Left, Keys.Right, Keys.B, Keys.A };
        private int secretCodeIndex = 0;

        private CancellationTokenSource cncnetPlayerCountCancellationSource;

        // Main Menu Buttons
        private XNAClientButton btnNewCampaign;
        private XNAClientButton btnLoadGame;
        private XNAClientButton btnSkirmish;
        private XNAClientButton btnCnCNet;
        private XNAClientButton btnLan;
        private XNAClientButton btnOptions;
        private XNAClientButton btnMapEditor;
        private XNAClientButton btnStatistics;
        private XNAClientButton btnCredits;
        private XNAClientButton btnExtras;
        private XNATextBlock lblannouncement;
        /// <summary>
        /// Initializes the main menu's controls.
        /// </summary>
        public override void Initialize()
        {
            Logger.Log("主菜单初始化");
            topBar.SetSecondarySwitch(cncnetLobby);
            GameProcessLogic.GameProcessExited += SharedUILogic_GameProcessExited;

            Name = nameof(MainMenu);
            BackgroundTexture = AssetLoader.LoadTexture("MainMenu/mainmenubg.png");
            ClientRectangle = new Rectangle(0, 0, BackgroundTexture.Width, BackgroundTexture.Height);

            WindowManager.CenterControlOnScreen(this);

            btnNewCampaign = new XNAClientButton(WindowManager);
            btnNewCampaign.Name = nameof(btnNewCampaign);
            btnNewCampaign.IdleTexture = AssetLoader.LoadTexture("MainMenu/campaign.png");
            btnNewCampaign.HoverTexture = AssetLoader.LoadTexture("MainMenu/campaign_c.png");
            btnNewCampaign.HoverSoundEffect = new EnhancedSoundEffect("MainMenu/button.wav");
            btnNewCampaign.LeftClick += BtnNewCampaign_LeftClick;
            btnNewCampaign.Text = "Campaign".L10N("UI:Main:Campaign");

            btnLoadGame = new XNAClientButton(WindowManager);
            btnLoadGame.Name = nameof(btnLoadGame);
            btnLoadGame.IdleTexture = AssetLoader.LoadTexture("MainMenu/loadmission.png");
            btnLoadGame.HoverTexture = AssetLoader.LoadTexture("MainMenu/loadmission_c.png");
            btnLoadGame.HoverSoundEffect = new EnhancedSoundEffect("MainMenu/button.wav");
            btnLoadGame.LeftClick += BtnLoadGame_LeftClick;
            btnLoadGame.Text = "Load Game".L10N("UI:Main:LoadGame");

            btnSkirmish = new XNAClientButton(WindowManager);
            btnSkirmish.Name = nameof(btnSkirmish);
            btnSkirmish.IdleTexture = AssetLoader.LoadTexture("MainMenu/skirmish.png");
            btnSkirmish.HoverTexture = AssetLoader.LoadTexture("MainMenu/skirmish_c.png");
            btnSkirmish.HoverSoundEffect = new EnhancedSoundEffect("MainMenu/button.wav");
            btnSkirmish.LeftClick += BtnSkirmish_LeftClick;
            btnSkirmish.Text = "Skirmish".L10N("UI:Main:SkirmishLobby");

            btnCnCNet = new XNAClientButton(WindowManager);
            btnCnCNet.Name = nameof(btnCnCNet);
            btnCnCNet.IdleTexture = AssetLoader.LoadTexture("MainMenu/cncnet.png");
            btnCnCNet.HoverTexture = AssetLoader.LoadTexture("MainMenu/cncnet_c.png");
            btnCnCNet.HoverSoundEffect = new EnhancedSoundEffect("MainMenu/button.wav");
            btnCnCNet.LeftClick += BtnCnCNet_LeftClick;
            btnCnCNet.Text = "CnCNet".L10N("UI:Main:CnCNetLobby");

            btnLan = new XNAClientButton(WindowManager);
            btnLan.Name = nameof(btnLan);
            btnLan.IdleTexture = AssetLoader.LoadTexture("MainMenu/lan.png");
            btnLan.HoverTexture = AssetLoader.LoadTexture("MainMenu/lan_c.png");
            btnLan.HoverSoundEffect = new EnhancedSoundEffect("MainMenu/button.wav");
            btnLan.Text = "LAN".L10N("UI:Main:LANGameLobby");
            btnLan.LeftClick += BtnLan_LeftClick;

            btnOptions = new XNAClientButton(WindowManager);
            btnOptions.Name = nameof(btnOptions);
            btnOptions.IdleTexture = AssetLoader.LoadTexture("MainMenu/options.png");
            btnOptions.HoverTexture = AssetLoader.LoadTexture("MainMenu/options_c.png");
            btnOptions.HoverSoundEffect = new EnhancedSoundEffect("MainMenu/button.wav");
            btnOptions.LeftClick += BtnOptions_LeftClick;
            btnOptions.Text = "Options".L10N("UI:Main:Options");

            btnMapEditor = new XNAClientButton(WindowManager);
            btnMapEditor.Name = nameof(btnMapEditor);
            btnMapEditor.IdleTexture = AssetLoader.LoadTexture("MainMenu/mapeditor.png");
            btnMapEditor.HoverTexture = AssetLoader.LoadTexture("MainMenu/mapeditor_c.png");
            btnMapEditor.HoverSoundEffect = new EnhancedSoundEffect("MainMenu/button.wav");
            btnMapEditor.LeftClick += BtnMapEditor_LeftClick;
            btnMapEditor.Text = "Map Editor".L10N("UI:Main:MapEditor");

            // btnMapEditor.Visible = false; //暂时禁用地编

            btnStatistics = new XNAClientButton(WindowManager);
            btnStatistics.Name = nameof(btnStatistics);
            btnStatistics.IdleTexture = AssetLoader.LoadTexture("MainMenu/statistics.png");
            btnStatistics.HoverTexture = AssetLoader.LoadTexture("MainMenu/statistics_c.png");
            btnStatistics.HoverSoundEffect = new EnhancedSoundEffect("MainMenu/button.wav");
            btnStatistics.LeftClick += BtnStatistics_LeftClick;
            btnStatistics.Text = "Statistics".L10N("UI:Main:Statistics");

            btnCredits = new XNAClientButton(WindowManager);
            btnCredits.Name = nameof(btnCredits);
            btnCredits.IdleTexture = AssetLoader.LoadTexture("MainMenu/credits.png");
            btnCredits.HoverTexture = AssetLoader.LoadTexture("MainMenu/credits_c.png");
            btnCredits.HoverSoundEffect = new EnhancedSoundEffect("MainMenu/button.wav");
            btnCredits.LeftClick += BtnCredits_LeftClick;
            btnCredits.Text = "View Credits".L10N("UI:MainMenu:Credits");

            btnExtras = new XNAClientButton(WindowManager);
            btnExtras.Name = nameof(btnExtras);
            btnExtras.IdleTexture = AssetLoader.LoadTexture("MainMenu/extras.png");
            btnExtras.HoverTexture = AssetLoader.LoadTexture("MainMenu/extras_c.png");
            btnExtras.HoverSoundEffect = new EnhancedSoundEffect("MainMenu/button.wav");
            btnExtras.LeftClick += BtnExtras_LeftClick;

            var btnExit = new XNAClientButton(WindowManager);
            btnExit.Name = nameof(btnExit);
            btnExit.IdleTexture = AssetLoader.LoadTexture("MainMenu/exitgame.png");
            btnExit.HoverTexture = AssetLoader.LoadTexture("MainMenu/exitgame_c.png");
            btnExit.HoverSoundEffect = new EnhancedSoundEffect("MainMenu/button.wav");
            btnExit.LeftClick += BtnExit_LeftClick;
            btnExit.Text = "Exit".L10N("UI:Main:Exit");

            XNALabel lblCnCNetStatus = new XNALabel(WindowManager);
            lblCnCNetStatus.Name = nameof(lblCnCNetStatus);
            lblCnCNetStatus.Text = "DTA players on CnCNet:".L10N("UI:Main:CnCNetOnlinePlayersCountText");
            lblCnCNetStatus.ClientRectangle = new Rectangle(12, 9, 0, 0);

            lblCnCNetPlayerCount = new XNALabel(WindowManager);
            lblCnCNetPlayerCount.Name = nameof(lblCnCNetPlayerCount);
            lblCnCNetPlayerCount.Text = "-";

            lblVersion = new XNALinkLabel(WindowManager);
            lblVersion.Name = nameof(lblVersion);
            lblVersion.LeftClick += LblVersion_LeftClick;

            lblannouncement = new XNATextBlock(WindowManager);
            lblannouncement.Name = nameof(lblannouncement);
            lblannouncement.ClientRectangle = new Rectangle(880,135,155,120);
            lblannouncement.TextColor = Color.White;

            lblUpdateStatus = new XNALinkLabel(WindowManager);
            lblUpdateStatus.Name = nameof(lblUpdateStatus);
            lblUpdateStatus.LeftClick += LblUpdateStatus_LeftClick;
            lblUpdateStatus.ClientRectangle = new Rectangle(0, 0, UIDesignConstants.BUTTON_WIDTH_160, 20);

            //lblannouncement.Text = "无法获取公告内容。";

            NetWorkINISettings.DownloadCompleted += UseDownloadedData;



            AddChild(lblannouncement);
            AddChild(btnNewCampaign);
            AddChild(btnLoadGame);
            AddChild(btnSkirmish);
            AddChild(btnCnCNet);
            AddChild(btnLan);
            AddChild(btnOptions);
            AddChild(btnMapEditor);
            AddChild(btnStatistics);
            AddChild(btnCredits);
            AddChild(btnExtras);
            AddChild(btnExit);
            AddChild(lblCnCNetStatus);
            AddChild(lblCnCNetPlayerCount);

            if (!ClientConfiguration.Instance.ModMode)
            {
                // ModMode disables version tracking and the updater if it's enabled

                AddChild(lblVersion);
                AddChild(lblUpdateStatus);

                Updater.FileIdentifiersUpdated += Updater_FileIdentifiersUpdated;
                Updater.OnCustomComponentsOutdated += Updater_OnCustomComponentsOutdated;
            }

            base.Initialize(); // Read control attributes from INI

            Keyboard.OnKeyPressed += Keyboard_OnKeyPressed;

            innerPanel = new MainMenuDarkeningPanel(WindowManager, discordHandler);
            innerPanel.ClientRectangle = new Rectangle(0, 0,
                Width,
                Height);
            innerPanel.DrawOrder = int.MaxValue;
            innerPanel.UpdateOrder = int.MaxValue;
            AddChild(innerPanel);
            innerPanel.Hide();

            lblVersion.Text = Updater.GameVersion;


            innerPanel.UpdateQueryWindow.UpdateDeclined += UpdateQueryWindow_UpdateDeclined;
            innerPanel.UpdateQueryWindow.UpdateAccepted += UpdateQueryWindow_UpdateAccepted;
            innerPanel.ManualUpdateQueryWindow.Closed += ManualUpdateQueryWindow_Closed;

            innerPanel.UpdateWindow.UpdateCompleted += UpdateWindow_UpdateCompleted;
            innerPanel.UpdateWindow.UpdateCancelled += UpdateWindow_UpdateCancelled;
            innerPanel.UpdateWindow.UpdateFailed += UpdateWindow_UpdateFailed;

            ClientRectangle = new Rectangle((WindowManager.RenderResolutionX - Width) / 2,
                (WindowManager.RenderResolutionY - Height) / 2,
                Width, Height);
            innerPanel.ClientRectangle = new Rectangle(0, 0,
                Math.Max(WindowManager.RenderResolutionX, Width),
                Math.Max(WindowManager.RenderResolutionY, Height));

            CnCNetPlayerCountTask.CnCNetGameCountUpdated += CnCNetInfoController_CnCNetGameCountUpdated;
            cncnetPlayerCountCancellationSource = new CancellationTokenSource();
            CnCNetPlayerCountTask.InitializeService(cncnetPlayerCountCancellationSource);

            WindowManager.GameClosing += WindowManager_GameClosing;

            skirmishLobby.Exited += SkirmishLobby_Exited;
            lanLobby.Exited += LanLobby_Exited;
            optionsWindow.EnabledChanged += OptionsWindow_EnabledChanged;

            optionsWindow.OnForceUpdate += (s, e) => ForceUpdate();

            GameProcessLogic.GameProcessStarted += SharedUILogic_GameProcessStarted;
            GameProcessLogic.GameProcessStarting += SharedUILogic_GameProcessStarting;

            UserINISettings.Instance.SettingsSaved += SettingsSaved;

            Updater.Restart += Updater_Restart;

            SetButtonHotkeys(true);
            Logger.Log("主菜单初始化完毕");
        }
      
        public void UseDownloadedData(object sender, EventArgs e)
        {
            Console.WriteLine("触发下载完成");
            lblannouncement.Text = GetContent(NetWorkINISettings.Instance.Announcement);
        }

        private string GetContent(string content)
        {
            string s1;
            string description = string.Empty;

            foreach (string s in content.Split('@'))
            {
                s1 = s + Environment.NewLine;
                if (s1.Length > 11)
                {

                    s1 = InsertFormat(s1, 11, Environment.NewLine);
                }

                description += s1;
            }

            return description;
        }

        private string InsertFormat(string input, int interval, string value)
        {
            for (int i = interval; i < input.Length; i += interval + 1)
                input = input.Insert(i, value);
            return input;
        }
        private void Keyboard_OnKeyPressed(object sender, Rampastring.XNAUI.Input.KeyPressEventArgs e)
        {
            var keyPressed = e.PressedKey;

            if (keyPressed == secretCodeSequence[secretCodeIndex])
            {
                secretCodeIndex++;

                if (secretCodeIndex == secretCodeSequence.Count)
                {
                    ActivateSecretFunction();
                    secretCodeIndex = 0;
                }
            }
            else
            {
                secretCodeIndex = 0;
            }
        }

        private void SetButtonHotkeys(bool enableHotkeys)
        {
            if (!Initialized)
                return;

            if (enableHotkeys)
            {
                btnNewCampaign.HotKey = Keys.C;
                btnLoadGame.HotKey = Keys.L;
                btnSkirmish.HotKey = Keys.S;
                btnCnCNet.HotKey = Keys.M;
                btnLan.HotKey = Keys.N;
                btnOptions.HotKey = Keys.O;
                btnMapEditor.HotKey = Keys.E;
                btnStatistics.HotKey = Keys.T;
                btnCredits.HotKey = Keys.R;
                btnExtras.HotKey = Keys.X;
            }
            else
            {
                btnNewCampaign.HotKey = Keys.None;
                btnLoadGame.HotKey = Keys.None;
                btnSkirmish.HotKey = Keys.None;
                btnCnCNet.HotKey = Keys.None;
                btnLan.HotKey = Keys.None;
                btnOptions.HotKey = Keys.None;
                btnMapEditor.HotKey = Keys.None;
                btnStatistics.HotKey = Keys.None;
                btnCredits.HotKey = Keys.None;
                btnExtras.HotKey = Keys.None;
            }
        }



        private void OptionsWindow_EnabledChanged(object sender, EventArgs e)
        {
            if (!optionsWindow.Enabled)
            {
                if (customComponentDialogQueued)
                    Updater_OnCustomComponentsOutdated();
            }
        }

        /// <summary>
        /// Refreshes settings. Called when the game process is starting.
        /// </summary>
        private void SharedUILogic_GameProcessStarting()
        {
            UserINISettings.Instance.ReloadSettings();

            try
            {
                optionsWindow.RefreshSettings();
            }
            catch (Exception ex)
            {
                Logger.Log("Refreshing settings failed! Exception message: " + ex.Message);
                // We don't want to show the dialog when starting a game
                //XNAMessageBox.Show(WindowManager, "Saving settings failed",
                //    "Saving settings failed! Error message: " + ex.Message);
            }
        }

        private void Updater_Restart(object sender, EventArgs e) =>
            WindowManager.AddCallback(new Action(ExitClient), null);

        /// <summary>
        /// Applies configuration changes (music playback and volume)
        /// when settings are saved.
        /// </summary>
        private void SettingsSaved(object sender, EventArgs e)
        {
            if (isMediaPlayerAvailable)
            {
                if (MediaPlayer.State == MediaState.Playing)
                {
                    if (!UserINISettings.Instance.PlayMainMenuMusic)
                        isMusicFading = true;
                }
                else if (topBar.GetTopMostPrimarySwitchable() == this &&
                    topBar.LastSwitchType == SwitchType.PRIMARY)
                {
                    PlayMusic();
                }
            }

            if (!connectionManager.IsConnected)
                ProgramConstants.PLAYERNAME = UserINISettings.Instance.PlayerName;

            if (UserINISettings.Instance.DiscordIntegration)
                discordHandler.Connect();
            else
                discordHandler.Disconnect();
        }

        private void ActivateSecretFunction()
        {
            List<string> strings = new List<string>
            {
                "三星大头兵可以借助自家部队拆除围墙!",
                "遥控坦克和驱逐舰对打会同归于尽",
                "黑鹰战机和入侵者战机的价格是一样的",
                "两个维修IFV互相维修仍会被大量蜘蛛击杀",
                "只要你手速够快，建筑和飞机也能使用路径点",
                "在PVP中，可以将尤里藏于建筑后面，让进攻的敌人摸不着头脑",
                "海豚可以直接攻击岸边的树",
                "冰天雪地中一共有338课树",
                "在原版中，闪电风暴有概率一次劈掉基地",
                "海豚是盟军训练的，而乌贼是苏军“心灵控制”的",
                "在某些需要工程师占领的任务中，也可以使用间谍进入来完成任务",
                "在游戏中躲在高架桥下可以躲避核弹轰炸。",
                "海豚也可以解除蜘蛛。",
                "飞机有可能扔出两发子弹"
            };

            XNAMessageBox.Show(WindowManager, "你知道吗", strings[new Random().Next(strings.Count)]);
        }

        private void CheckCampaign()
        {
            XNAMessageBox messageBox;

            if (File.Exists("mapsmd03.mix"))
                Mix.UnPackMix(ProgramConstants.GamePath, $"{ProgramConstants.GamePath}mapsmd03.mix");
            if (File.Exists("maps01.mix"))
                Mix.UnPackMix(ProgramConstants.GamePath, $"{ProgramConstants.GamePath}mapsmd01.mix");
            if (File.Exists("maps02.mix"))
                Mix.UnPackMix(ProgramConstants.GamePath, $"{ProgramConstants.GamePath}maps02.mix");

            string directoryPath = "./";
            var alls = Directory.GetFiles("./", "all*.map");
            var sovs = Directory.GetFiles("./", "sov*.map");

            if (alls.Length + sovs.Length != 0)
            {
                // 提示用户加载任务包
                messageBox = new XNAMessageBox(WindowManager, "检测到可能存在的任务包", "你需要加载此任务包吗？", XNAMessageBoxButtons.YesNo);
                messageBox.Show();
                messageBox.YesClickedAction += CheckCampaign_YesClicked;
                return;
            }

        }

        private bool IsTaskPackage(string mapFilePath)
        {
            try
            {
                IniFile ini = new IniFile(mapFilePath);
                if (ini.GetIntValue("Basic", "MultiplayerOnly", 1) == 0)
                {
                    return true;
                }
            }
            catch
            {
                return false;
            }
            return false;
        }
        private void CheckCampaign_YesClicked(XNAMessageBox messageBox)
        {

            try
            {

                var alls = Directory.GetFiles("./", "all*md.map");
                var sovs = Directory.GetFiles("./", "sov*md.map");
                var mod = true;


                string md = string.Empty;
                string contry = string.Empty;


                //string value = csf["LoadMsg:All01md"];
                //Console.WriteLine($"Value for key '{"LoadMsg:All01md"}': {value}");

                if (alls.Length != 0)
                {
                    md = "md";

                }
                else if (sovs.Length != 0)
                {
                    md = "md";

                }
                else
                {
                    alls = Directory.GetFiles("./", "all*.map");
                    sovs = Directory.GetFiles("./", "sov*.map");

                }
                bool goodcsf = true;
                JObject csf = new JObject();
                try
                {
                    if (File.Exists($"{ProgramConstants.GamePath}ra2{md}.csf"))
                        csf = CSFConverter.Csf($"{ProgramConstants.GamePath}ra2{md}.csf", $"{ProgramConstants.GamePath}Resources\\ra2{md}.json");
                    else
                        goodcsf = false;


                }
                catch (Exception ex)
                {
                    Logger.Log("csf解析失败");
                    goodcsf = false;
                }

                if (File.Exists($"{ProgramConstants.GamePath}rules{md}.ini") || File.Exists($"{ProgramConstants.GamePath}art{md}.ini"))
                    mod = true;

                else
                {
                    mod = false;
                }

                int j = new Random().Next(0, 10000);

                while (Directory.Exists($"INI/GameOptions/{(mod ? "Game" : "Mission")}/{j.ToString()}/"))
                {
                    j = new Random().Next(0, 10000);
                }

                IniFile iniFile = new IniFile($"INI/Battle{j}.ini");

                int i = new Random().Next(0, 10000);

                while (iniFile.KeyExists("Battles", i.ToString()))
                {
                    i = new Random().Next(0, 10000);
                }



                iniFile.SetStringValue("Battles", j.ToString(), j.ToString());
                iniFile.SetStringValue(j.ToString(), "Description", "—— 第三方任务 ——");



                Directory.CreateDirectory($"INI/GameOptions/{(mod ? "Game" : "Mission")}/{j}");



                List<string> mission = new List<string>();
                foreach (var file in alls)
                {
                    //if (!IsTaskPackage(file))
                    //    continue;
                    mission.Add(file);
                }
                foreach (var file in sovs)
                {
                    //if (!IsTaskPackage(file))
                    //    continue;
                    mission.Add(file);
                }

                int Count = 0;
                foreach (var file in mission)
                {

                    contry = Path.GetFileName(file).ToUpper().Substring(0, 3);
                    Count = Count + 1;
                    i = new Random().Next(0, 10000);
                    while (iniFile.KeyExists("Battles", i.ToString()))
                    {
                        i = new Random().Next(0, 10000);
                    }
                    iniFile.SetStringValue("Battles", i.ToString(), i.ToString());
                    iniFile.SetStringValue(i.ToString(), "Scenario", Path.GetFileName(file).ToUpper());
                    iniFile.SetStringValue(i.ToString(), "BuildOffAlly", "yes");
                    iniFile.SetBooleanValue(i.ToString(),"other",true);

                    iniFile.SetStringValue(i.ToString(), "Mod", mod ? j.ToString() : md != string.Empty ? "YR" : "RA2");
                    iniFile.SetStringValue(i.ToString(), "Description", "第" + Count.ToString() + "关");
                    //Console.WriteLine($"LoadMsg:All{Path.GetFileName(file).Substring(3, 2)}md");

                    if (contry == "ALL")
                        iniFile.SetStringValue(i.ToString(), "SideName", "Allied");
                    else if (contry == "SOV")
                        iniFile.SetStringValue(i.ToString(), "SideName", "Soviet");

                    if (goodcsf && (contry == "ALL" || contry == "SOV"))
                        iniFile.SetStringValue(i.ToString(), "LongDescription", csf.SelectToken($"BRIEF:{contry}{Path.GetFileName(file).Substring(3, 2)}{(md != string.Empty ? "MD" : "")}").ToString().Replace("\n", "@"));

                    if (!mod)
                    {

                        iniFile.SetStringValue(i.ToString(), "Attached", j.ToString());
                        File.Move(file, $"INI/GameOptions/Mission/{j}/{Path.GetFileName(file)}");
                    }
                    else
                    {

                        File.Move(file, $"INI/GameOptions/Game/{j}/{Path.GetFileName(file)}");
                    }

                    var lpal = Directory.GetFiles("./", "l*.pal");
                    var lshp = Directory.GetFiles("./", "l*.shp");

                    if (lpal.Length != 0)
                    {
                        foreach (var pal in lpal)
                        {
                            File.Move(pal, $"INI/GameOptions/{(mod ? "Game" : "Mission")}/{j}/{Path.GetFileName(pal)}");
                        }
                    }

                    if (lshp.Length != 0)
                    {
                        foreach (var shp in lshp)
                        {
                            File.Move(shp, $"INI/GameOptions/{(mod ? "Game" : "Mission")}/{j}/{Path.GetFileName(shp)}");
                        }
                    }
                }
                // File.Create($"INI/Mod&AI{j}.ini");
                IniFile modINI;


                if (mod)
                {
                    modINI = new IniFile($"INI/Mod&AI{j}.ini");
                    modINI.SetStringValue("Game", j.ToString(), j.ToString());
                    modINI.SetStringValue(section: j.ToString(), key: "Visible", value: "False");

                    if (md == string.Empty)
                        modINI.SetStringValue(section: j.ToString(), key: "Main", value: "RA2_Main");
                    else
                        modINI.SetStringValue(section: j.ToString(), key: "Main", value: "YR_Main");
                    modINI.WriteIniFile();
                }


                iniFile.WriteIniFile();

                if (File.Exists($"{ProgramConstants.GamePath}ra2{md}.csf"))
                    if (mod)
                        File.Move($"{ProgramConstants.GamePath}ra2{md}.csf", $"INI/GameOptions/Game/{j}/stringtable00.csf");
                    else
                        File.Move($"{ProgramConstants.GamePath}ra2{md}.csf", $"INI/GameOptions/Mission/{j}/stringtable00.csf");

                if (File.Exists($"{ProgramConstants.GamePath}rules{md}.ini"))
                    File.Move($"{ProgramConstants.GamePath}rules{md}.ini", $"INI/GameOptions/Game/{j}/rules{md}.ini");
                if (File.Exists($"{ProgramConstants.GamePath}art{md}.ini"))
                    File.Move($"{ProgramConstants.GamePath}art{md}.ini", $"INI/GameOptions/Game/{j}/art{md}.ini");
                if (File.Exists($"{ProgramConstants.GamePath}mission{md}.csf"))
                    File.Move($"{ProgramConstants.GamePath}mission{md}.csf", $"INI/GameOptions/Game/{j}/missionmd.ini");
                if (File.Exists($"{ProgramConstants.GamePath}battle{md}.ini"))
                    File.Move($"{ProgramConstants.GamePath}battle{md}.ini", $"INI/GameOptions/Game/{j}/battlemd.ini");
                if (File.Exists($"{ProgramConstants.GamePath}mapsel{md}.ini"))
                    File.Move($"{ProgramConstants.GamePath}mapsel{md}.ini", $"INI/GameOptions/Game/{j}/mapselmd.ini");
                if (File.Exists($"{ProgramConstants.GamePath}ai{md}.ini"))
                    File.Move($"{ProgramConstants.GamePath}ai{md}.ini", $"INI/GameOptions/Game/{j}/aimd.ini");
                if (mod)
                    Mix.PackToMix($"{ProgramConstants.GamePath}INI/GameOptions/Game/{j}", $"{ProgramConstants.GamePath}INI/GameOptions/Game/expandmd97.mix");
                else
                    Mix.PackToMix($"{ProgramConstants.GamePath}INI/GameOptions/Mission/{j}", $"{ProgramConstants.GamePath}INI/GameOptions/Mission/expandmd95.mix");

                DirectoryInfo directory = new System.IO.DirectoryInfo($"INI/GameOptions/Game/{j}");
                if (!mod)
                    directory = new System.IO.DirectoryInfo($"INI/GameOptions/Mission/{j}");

                foreach (System.IO.FileInfo file in directory.GetFiles()) file.Delete();
                foreach (System.IO.DirectoryInfo subDirectory in directory.GetDirectories()) subDirectory.Delete(true);
                if (mod)
                {

                    File.Move($"{ProgramConstants.GamePath}INI/GameOptions/Game/expandmd97.mix", $"{ProgramConstants.GamePath}INI/GameOptions/Game/{j}/expandmd97.mix");
                }
                else
                {

                    File.Move($"{ProgramConstants.GamePath}INI/GameOptions/Mission/expandmd95.mix", $"{ProgramConstants.GamePath}INI/GameOptions/Mission/{j}/expandmd95.mix");
                }


                innerPanel.reload();
            }
            catch (Exception ex)
            {
                XNAMessageBox.Show(WindowManager, "导入失败", "导入时出错，你可以把这个任务包发给作者看看是怎么回事。");
                return;
            }
        }

        /// <summary>
        /// Checks files which are required for the mod to function
        /// but not distributed with the mod (usually base game files
        /// for YR mods which can't be standalone).
        /// </summary>
        private void CheckRequiredFiles()
        {
            List<string> absentFiles = ClientConfiguration.Instance.RequiredFiles.ToList()
                .FindAll(f => !string.IsNullOrWhiteSpace(f) && !SafePath.GetFile(ProgramConstants.GamePath, f).Exists);

            if (absentFiles.Count > 0)
                XNAMessageBox.Show(WindowManager, "Missing Files".L10N("UI:Main:MissingFilesTitle"),
#if ARES
                    ("You are missing Yuri's Revenge files that are required" + Environment.NewLine +
                    "to play this mod! Yuri's Revenge mods are not standalone," + Environment.NewLine +
                    "so you need a copy of following Yuri's Revenge (v. 1.001)" + Environment.NewLine +
                    "files placed in the mod folder to play the mod:").L10N("UI:Main:MissingFilesText1Ares") +
#else
                    "The following required files are missing:".L10N("UI:Main:MissingFilesText1NonAres") +
#endif
                    Environment.NewLine + Environment.NewLine +
                    String.Join(Environment.NewLine, absentFiles) +
                    Environment.NewLine + Environment.NewLine +
                    "You won't be able to play without those files.".L10N("UI:Main:MissingFilesText2"));
        }

        private void CheckForbiddenFiles()
        {
            List<string> presentFiles = ClientConfiguration.Instance.ForbiddenFiles.ToList()
                .FindAll(f => !string.IsNullOrWhiteSpace(f) && SafePath.GetFile(ProgramConstants.GamePath, f).Exists);

            if (presentFiles.Count > 0)
                XNAMessageBox.Show(WindowManager, "Interfering Files Detected".L10N("UI:Main:InterferingFilesDetectedTitle"),
#if TS
                    ("You have installed the mod on top of a Tiberian Sun" + Environment.NewLine +
                    "copy! This mod is standalone, therefore you have to" + Environment.NewLine +
                    "install it in an empty folder. Otherwise the mod won't" + Environment.NewLine +
                    "function correctly." +
                    Environment.NewLine + Environment.NewLine +
                    "Please reinstall the mod into an empty folder to play.").L10N("UI:Main:InterferingFilesDetectedTextTS")
#else
                    "The following interfering files are present:".L10N("UI:Main:InterferingFilesDetectedTextNonTS1") +
                    Environment.NewLine + Environment.NewLine +
                    String.Join(Environment.NewLine, presentFiles) +
                    Environment.NewLine + Environment.NewLine +
                    "The mod won't work correctly without those files removed.".L10N("UI:Main:InterferingFilesDetectedTextNonTS2")
#endif
                    );
        }

        /// <summary>
        /// Checks whether the client is running for the first time.
        /// If it is, displays a dialog asking the user if they'd like
        /// to configure settings.
        /// </summary>
        private void CheckIfFirstRun()
        {
            if (UserINISettings.Instance.IsFirstRun)
            {
                UserINISettings.Instance.IsFirstRun.Value = false;
                UserINISettings.Instance.SaveSettings();
                //GuideWindow guideWindow = new GuideWindow(WindowManager);

                //AddChild(guideWindow);

                firstRunMessageBox = XNAMessageBox.ShowYesNoDialog(WindowManager, "Initial Installation".L10N("UI:Main:InitialInstallationTitle"),
                    string.Format(("You have just installed {0}." + Environment.NewLine +
                    "It's highly recommended that you configure your settings before playing." +
                    Environment.NewLine + "Do you want to configure them now?").L10N("UI:Main:InitialInstallationText"), ClientConfiguration.Instance.LocalGame).Replace("@", Environment.NewLine));
                firstRunMessageBox.YesClickedAction = FirstRunMessageBox_YesClicked;
                firstRunMessageBox.NoClickedAction = FirstRunMessageBox_NoClicked;

                if (!File.Exists("Client/custom_rules_all.ini"))
                {
                    File.Create("Client/custom_rules_all.ini");
                }
                if (!File.Exists("Client/custom_rules_ra2.ini"))
                {
                    File.Create("Client/custom_rules_ra2.ini");
                }
                if (!File.Exists("Client/custom_rules_yr.ini"))
                {
                    File.Create("Client/custom_rules_yr.ini");
                }
                if (!File.Exists("Client/custom_art_all.ini"))
                {
                    File.Create("Client/custom_art_all.ini");
                }
                if (!File.Exists("Client/custom_art_ra2.ini"))
                {
                    File.Create("Client/custom_art_ra2.ini");
                }
                if (!File.Exists("Client/custom_art_yr.ini"))
                {
                    File.Create("Client/custom_art_yr.ini");
                }

                string FA2Path = ProgramConstants.GamePath + ClientConfiguration.Instance.MapEditorExePath;
                if (!File.Exists(FA2Path))
                {
                    Logger.Log("没有找到地编");
                    btnMapEditor.Enabled = false;
                }
                else
                {
                    IniFile ini = new IniFile(ProgramConstants.GamePath + "Resources/FinalAlert2SP/FinalAlert.ini", Encoding.GetEncoding("GBK"));
                    ini.SetStringValue("TS", "Exe", (ProgramConstants.GamePath + "gamemd.exe").Replace('/', '\\')); //地编路径必须是\，这里写两个是因为有一个是转义符
                    ini.WriteIniFile();
                    Logger.Log("写入地编游戏路径");
                }
            }

            optionsWindow.PostInit();
        }

        private void FirstRunMessageBox_NoClicked(XNAMessageBox messageBox)
        {
            if (customComponentDialogQueued)
                Updater_OnCustomComponentsOutdated();
        }

        private void FirstRunMessageBox_YesClicked(XNAMessageBox messageBox) => optionsWindow.Open();

        private void SharedUILogic_GameProcessStarted() => MusicOff();

        private void WindowManager_GameClosing(object sender, EventArgs e) => Clean();

        private void SkirmishLobby_Exited(object sender, EventArgs e)
        {
            if (UserINISettings.Instance.StopMusicOnMenu)
                PlayMusic();
        }

        private void LanLobby_Exited(object sender, EventArgs e)
        {
            topBar.SetLanMode(false);

            if (UserINISettings.Instance.AutomaticCnCNetLogin)
                connectionManager.Connect();

            if (UserINISettings.Instance.StopMusicOnMenu)
                PlayMusic();
        }

        private void CnCNetInfoController_CnCNetGameCountUpdated(object sender, PlayerCountEventArgs e)
        {
            lock (locker)
            {
                if (e.PlayerCount == -1)
                    lblCnCNetPlayerCount.Text = "N/A".L10N("UI:Main:N/A");
                else
                    lblCnCNetPlayerCount.Text = e.PlayerCount.ToString();
            }
        }

        /// <summary>
        /// Attemps to "clean" the client session in a nice way if the user closes the game.
        /// </summary>
        private void Clean()
        {
            Updater.FileIdentifiersUpdated -= Updater_FileIdentifiersUpdated;

            if (cncnetPlayerCountCancellationSource != null) cncnetPlayerCountCancellationSource.Cancel();
            topBar.Clean();
            if (UpdateInProgress)
                Updater.StopUpdate();

            if (connectionManager.IsConnected)
                connectionManager.Disconnect();
        }

        /// <summary>
        /// Starts playing music, initiates an update check if automatic updates
        /// are enabled and checks whether the client is run for the first time.
        /// Called after all internal client UI logic has been initialized.
        /// </summary>
        public void PostInit()
        {
            
            DarkeningPanel.AddAndInitializeWithControl(WindowManager, skirmishLobby);
            DarkeningPanel.AddAndInitializeWithControl(WindowManager, cnCNetGameLoadingLobby);
            DarkeningPanel.AddAndInitializeWithControl(WindowManager, cnCNetGameLobby);
            DarkeningPanel.AddAndInitializeWithControl(WindowManager, cncnetLobby);
            DarkeningPanel.AddAndInitializeWithControl(WindowManager, lanLobby);
            optionsWindow.SetTopBar(topBar);
            DarkeningPanel.AddAndInitializeWithControl(WindowManager, optionsWindow);
            WindowManager.AddAndInitializeControl(privateMessagingPanel);
            privateMessagingPanel.AddChild(privateMessagingWindow);
            topBar.SetTertiarySwitch(privateMessagingWindow);
            topBar.SetOptionsWindow(optionsWindow);
            WindowManager.AddAndInitializeControl(gameInProgressWindow);
            skirmishLobby.Disable();
            cncnetLobby.Disable();
            cnCNetGameLobby.Disable();
            cnCNetGameLoadingLobby.Disable();
            lanLobby.Disable();
            privateMessagingWindow.Disable();
            optionsWindow.Disable();

            WindowManager.AddAndInitializeControl(topBar);
            topBar.AddPrimarySwitchable(this);

            SwitchMainMenuMusicFormat();

            themeSong = AssetLoader.LoadSong(ClientConfiguration.Instance.MainMenuMusicName);

            PlayMusic();
            if (!ClientConfiguration.Instance.ModMode)
            {
                if (NetWorkINISettings.Instance==null|| Updater.UpdateMirrors ==null || Updater.UpdateMirrors.Count < 1)
                {
                    lblUpdateStatus.Text = "No update download mirrors available.".L10N("UI:Main:NoUpdateMirrorsAvailable");
                    lblUpdateStatus.DrawUnderline = false;
                }
               
                else if (UserINISettings.Instance.CheckForUpdates)
                {
                    CheckForUpdates();
                }
                else
                {
                    lblUpdateStatus.Text = "Click to check for updates.".L10N("UI:Main:ClickToCheckUpdate");
                }
            }
            CheckCampaign();
            CheckRequiredFiles();
            CheckForbiddenFiles();
            CheckIfFirstRun();
        }

        private void SwitchMainMenuMusicFormat()
        {
#if GL || DX
            FileInfo wmaMainMenuMusicFile = SafePath.GetFile(ProgramConstants.GamePath, ProgramConstants.BASE_RESOURCE_PATH,
                FormattableString.Invariant($"{ClientConfiguration.Instance.MainMenuMusicName}.wma"));

            if (!wmaMainMenuMusicFile.Exists)
                return;

            FileInfo wmaBackupMainMenuMusicFile = SafePath.GetFile(ProgramConstants.GamePath, ProgramConstants.BASE_RESOURCE_PATH,
                FormattableString.Invariant($"{ClientConfiguration.Instance.MainMenuMusicName}.bak"));

            if (!wmaBackupMainMenuMusicFile.Exists)
                wmaMainMenuMusicFile.CopyTo(wmaBackupMainMenuMusicFile.FullName);

#endif
#if DX
            wmaBackupMainMenuMusicFile.CopyTo(wmaMainMenuMusicFile.FullName, true);
#elif GL
            FileInfo oggMainMenuMusicFile = SafePath.GetFile(ProgramConstants.GamePath, ProgramConstants.BASE_RESOURCE_PATH,
                FormattableString.Invariant($"{ClientConfiguration.Instance.MainMenuMusicName}.ogg"));

            if (oggMainMenuMusicFile.Exists)
                oggMainMenuMusicFile.CopyTo(wmaMainMenuMusicFile.FullName, true);
#endif
        }

        #region Updating / versioning system

        private void UpdateWindow_UpdateFailed(object sender, UpdateFailureEventArgs e)
        {
            innerPanel.Hide();
            lblUpdateStatus.Text = "Updating failed! Click to retry.".L10N("UI:Main:UpdateFailedClickToRetry");
            lblUpdateStatus.DrawUnderline = true;
            lblUpdateStatus.Enabled = true;
            UpdateInProgress = false;

            innerPanel.Show(null); // Darkening
            XNAMessageBox msgBox = new XNAMessageBox(WindowManager, "Update failed".L10N("UI:Main:UpdateFailedTitle"),
                string.Format(("An error occured while updating. Returned error was: {0}" +
                Environment.NewLine + Environment.NewLine +
                "If you are connected to the Internet and your firewall isn't blocking" + Environment.NewLine +
                "{1}, and the issue is reproducible, contact us at " + Environment.NewLine +
                "{2} for support.").L10N("UI:Main:UpdateFailedText"),
                e.Reason, Path.GetFileName(ProgramConstants.StartupExecutable), MainClientConstants.SUPPORT_URL_SHORT), XNAMessageBoxButtons.OK);
            msgBox.OKClickedAction = MsgBox_OKClicked;
            msgBox.Show();
        }

        private void MsgBox_OKClicked(XNAMessageBox messageBox)
        {
            innerPanel.Hide();
        }

        private void UpdateWindow_UpdateCancelled(object sender, EventArgs e)
        {
            innerPanel.Hide();
            lblUpdateStatus.Text = "The update was cancelled. Click to retry.".L10N("UI:Main:UpdateCancelledClickToRetry");
            lblUpdateStatus.DrawUnderline = true;
            lblUpdateStatus.Enabled = true;
            UpdateInProgress = false;
        }

        private void UpdateWindow_UpdateCompleted(object sender, EventArgs e)
        {
            innerPanel.Hide();
            lblUpdateStatus.Text = string.Format("{0} was succesfully updated to v.{1}".L10N("UI:Main:UpdateSuccess"),
                MainClientConstants.GAME_NAME_LONG, Updater.GameVersion);
            lblVersion.Text = Updater.GameVersion;
            UpdateInProgress = false;
            lblUpdateStatus.Enabled = true;
            lblUpdateStatus.DrawUnderline = false;
        }

        private void LblUpdateStatus_LeftClick(object sender, EventArgs e)
        {
            Logger.Log(Updater.VersionState.ToString());

            if (Updater.VersionState == VersionState.OUTDATED ||
                Updater.VersionState == VersionState.MISMATCHED ||
                Updater.VersionState == VersionState.UNKNOWN ||
                Updater.VersionState == VersionState.UPTODATE)
            {
                CheckForUpdates();
            }
        }

        private void LblVersion_LeftClick(object sender, EventArgs e)
        {
            ProcessLauncher.StartShellProcess(ClientConfiguration.Instance.ChangelogURL);
        }

        private void ForceUpdate()
        {
            UpdateInProgress = true;
            innerPanel.Hide();
            innerPanel.UpdateWindow.ForceUpdate();
            innerPanel.Show(innerPanel.UpdateWindow);
            lblUpdateStatus.Text = "Force updating...".L10N("UI:Main:ForceUpdating");
        }


        /// <summary>
        /// Starts a check for updates.
        /// </summary>
        private void CheckForUpdates()
        {
            if (Updater.UpdateMirrors==null||Updater.UpdateMirrors.Count < 1)
                return;
            innerPanel.UpdateQueryWindow.GetUpdateContentsAsync(Updater.VersionState.ToString(), VersionState.UPTODATE.ToString());
            Updater.CheckForUpdates();
            lblUpdateStatus.Enabled = false;
            lblUpdateStatus.Text = "Checking for " +
                "updates...".L10N("UI:Main:CheckingForUpdate");
            lastUpdateCheckTime = DateTime.Now;
        }

        private void Updater_FileIdentifiersUpdated()
            => WindowManager.AddCallback(new Action(HandleFileIdentifierUpdate), null);

        /// <summary>
        /// Used for displaying the result of an update check in the UI.
        /// </summary>
        private void HandleFileIdentifierUpdate()
        {
            if (UpdateInProgress)
            {
                return;
            }

            if (Updater.VersionState == VersionState.UPTODATE)
            {
                lblUpdateStatus.Text = string.Format("{0} is up to date.".L10N("UI:Main:GameUpToDate"), MainClientConstants.GAME_NAME_LONG);
                lblUpdateStatus.Enabled = true;
                lblUpdateStatus.DrawUnderline = false;
            }
            else if (Updater.VersionState == VersionState.OUTDATED && Updater.ManualUpdateRequired)
            {
                lblUpdateStatus.Text = "An update is available. Manual download & installation required.".L10N("UI:Main:UpdateAvailableManualDownloadRequired");
                lblUpdateStatus.Enabled = true;
                lblUpdateStatus.DrawUnderline = false;
                innerPanel.ManualUpdateQueryWindow.SetInfo(Updater.ServerGameVersion, Updater.ManualDownloadURL);

                if (!string.IsNullOrEmpty(Updater.ManualDownloadURL))
                    innerPanel.Show(innerPanel.ManualUpdateQueryWindow);
            }
            else if (Updater.VersionState == VersionState.OUTDATED)
            {
                lblUpdateStatus.Text = "An update is available.".L10N("UI:Main:UpdateAvailable");
                innerPanel.UpdateQueryWindow.SetInfo(Updater.ServerGameVersion, Updater.UpdateSizeInKb);
                innerPanel.Show(innerPanel.UpdateQueryWindow);
            }
            else if (Updater.VersionState == VersionState.UNKNOWN)
            {
                lblUpdateStatus.Text = "Checking for updates failed! Click to retry.".L10N("UI:Main:CheckUpdateFailedClickToRetry");
                lblUpdateStatus.Enabled = true;
                lblUpdateStatus.DrawUnderline = true;
            }
        }

        /// <summary>
        /// Asks the user if they'd like to update their custom components.
        /// Handles an event raised by the updater when it has detected
        /// that the custom components are out of date.
        /// </summary>
        private void Updater_OnCustomComponentsOutdated()
        {
            if (innerPanel.UpdateQueryWindow.Visible)
                return;

            if (UpdateInProgress)
                return;

            if ((firstRunMessageBox != null && firstRunMessageBox.Visible) || optionsWindow.Enabled)
            {
                // If the custom components are out of date on the first run
                // or the options window is already open, don't show the dialog
                customComponentDialogQueued = true;
                return;
            }

            customComponentDialogQueued = false;

            XNAMessageBox ccMsgBox = XNAMessageBox.ShowYesNoDialog(WindowManager,
                "Custom Component Updates Available".L10N("UI:Main:CustomUpdateAvailableTitle"),
                ("Updates for custom components are available. Do you want to open" + Environment.NewLine +
                "the Options menu where you can update the custom components?").L10N("UI:Main:CustomUpdateAvailableText"));
            ccMsgBox.YesClickedAction = CCMsgBox_YesClicked;
        }

        private void CCMsgBox_YesClicked(XNAMessageBox messageBox)
        {
            optionsWindow.Open();
            optionsWindow.SwitchToCustomComponentsPanel();
        }

        /// <summary>
        /// Called when the user has declined an update.
        /// </summary>
        private void UpdateQueryWindow_UpdateDeclined(object sender, EventArgs e)
        {
            UpdateQueryWindow uqw = (UpdateQueryWindow)sender;
            
            innerPanel.Hide();
            lblUpdateStatus.Text = "An update is available, click to install.".L10N("UI:Main:UpdateAvailableClickToInstall");
            lblUpdateStatus.Enabled = true;
            lblUpdateStatus.DrawUnderline = true;
        }

        /// <summary>
        /// Called when the user has accepted an update.
        /// </summary>
        private void UpdateQueryWindow_UpdateAccepted(object sender, EventArgs e)
        {
            innerPanel.Hide();
            innerPanel.UpdateWindow.SetData(Updater.ServerGameVersion);
            innerPanel.Show(innerPanel.UpdateWindow);
            lblUpdateStatus.Text = "Updating...".L10N("UI:Main:Updating");
            UpdateInProgress = true;
            Updater.StartUpdate();
        }

        private void ManualUpdateQueryWindow_Closed(object sender, EventArgs e)
            => innerPanel.Hide();

        #endregion

        private void BtnOptions_LeftClick(object sender, EventArgs e)
            => optionsWindow.Open();

        private void BtnNewCampaign_LeftClick(object sender, EventArgs e)
        {
            innerPanel.Show(innerPanel.CampaignSelector);
            innerPanel.reload();
            optionsWindow.tabControl.MakeSelectable(4);
        }

        private void BtnLoadGame_LeftClick(object sender, EventArgs e)
        => innerPanel.Show(innerPanel.GameLoadingWindow);

        private void BtnLan_LeftClick(object sender, EventArgs e)
        {
            foreach (string[] skin in UserINISettings.Instance.GetAIISkin())
            {
                if (skin[3] != "0")
                {
                    XNAMessageBox messageBox = new XNAMessageBox(WindowManager, "警告", "联机时禁止使用皮肤，请将皮肤还原成默认", XNAMessageBoxButtons.OK);
                    messageBox.Show();
                    return;
                }
            }

            optionsWindow.tabControl.MakeUnselectable(4);
            lanLobby.Open();

            if (UserINISettings.Instance.StopMusicOnMenu)
                MusicOff();

            if (connectionManager.IsConnected)
                connectionManager.Disconnect();

            topBar.SetLanMode(true);
        }

        private void BtnCnCNet_LeftClick(object sender, EventArgs e) => topBar.SwitchToSecondary();

        private void BtnSkirmish_LeftClick(object sender, EventArgs e)
        {
            skirmishLobby.Open();
            optionsWindow.tabControl.MakeSelectable(4);
            if (UserINISettings.Instance.StopMusicOnMenu)
                MusicOff();
        }

        private void BtnMapEditor_LeftClick(object sender, EventArgs e) => LaunchMapEditor();

        private void BtnStatistics_LeftClick(object sender, EventArgs e) =>
            innerPanel.Show(innerPanel.StatisticsWindow);

        private void BtnCredits_LeftClick(object sender, EventArgs e)
        {
            ProcessLauncher.StartShellProcess(MainClientConstants.CREDITS_URL);
        }

        private void BtnExtras_LeftClick(object sender, EventArgs e) =>
            innerPanel.Show(innerPanel.ExtrasWindow);

        private void BtnExit_LeftClick(object sender, EventArgs e)
        {
#if WINFORMS
            WindowManager.HideWindow();
#endif
            FadeMusicExit();
        }

        private void SharedUILogic_GameProcessExited() =>
            AddCallback(new Action(HandleGameProcessExited), null);

        private void HandleGameProcessExited()
        {
            innerPanel.GameLoadingWindow.ListSaves();
            innerPanel.Hide();

            // If music is disabled on menus, check if the main menu is the top-most
            // window of the top bar and only play music if it is
            // LAN has the top bar disabled, so to detect the LAN game lobby
            // we'll check whether the top bar is enabled
            if (!UserINISettings.Instance.StopMusicOnMenu ||
                (topBar.Enabled && topBar.LastSwitchType == SwitchType.PRIMARY &&
                topBar.GetTopMostPrimarySwitchable() == this))
                PlayMusic();
        }

        /// <summary>
        /// Switches to the main menu and performs a check for updates.
        /// </summary>
        private void CncnetLobby_UpdateCheck(object sender, EventArgs e)
        {
            CheckForUpdates();
            topBar.SwitchToPrimary();
        }

        public override void Update(GameTime gameTime)
        {
            if (isMusicFading)
                FadeMusic(gameTime);

            base.Update(gameTime);

        }

        public override void Draw(GameTime gameTime)
        {
            lock (locker)
            {
                base.Draw(gameTime);
            }
        }

        /// <summary>
        /// Attempts to start playing the menu music.
        /// </summary>
        private void PlayMusic()
        {
            if (!isMediaPlayerAvailable)
                return; // SharpDX fails at music playback on Vista

            if (themeSong != null && UserINISettings.Instance.PlayMainMenuMusic)
            {
                isMusicFading = false;
                MediaPlayer.IsRepeating = true;
                MediaPlayer.Volume = (float)UserINISettings.Instance.ClientVolume;

                try
                {
                    MediaPlayer.Play(themeSong);
                }
                catch (InvalidOperationException ex)
                {
                    Logger.Log("Playing main menu music failed! " + ex.Message);
                }
            }
        }

        /// <summary>
        /// Lowers the volume of the menu music, or stops playing it if the
        /// volume is unaudibly low.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        private void FadeMusic(GameTime gameTime)
        {
            if (!isMediaPlayerAvailable || !isMusicFading || themeSong == null)
                return;

            // Fade during 1 second
            float step = SoundPlayer.Volume * (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (MediaPlayer.Volume > step)
                MediaPlayer.Volume -= step;
            else
            {
                MediaPlayer.Stop();
                isMusicFading = false;
            }
        }

        /// <summary>
        /// Exits the client. Quickly fades the music if it's playing.
        /// </summary>
        private void FadeMusicExit()
        {
            if (!isMediaPlayerAvailable || themeSong == null)
            {
                ExitClient();
                return;
            }

            float step = MEDIA_PLAYER_VOLUME_EXIT_FADE_STEP * (float)UserINISettings.Instance.ClientVolume;

            if (MediaPlayer.Volume > step)
            {
                MediaPlayer.Volume -= step;
                AddCallback(new Action(FadeMusicExit), null);
            }
            else
            {
                MediaPlayer.Stop();
                ExitClient();
            }
        }

        private void ExitClient()
        {
            Logger.Log("Exiting.");
            WindowManager.CloseGame();
#if !XNA
            Thread.Sleep(1000);
            Environment.Exit(0);
#endif
        }

        public void SwitchOn()
        {
            if (UserINISettings.Instance.StopMusicOnMenu)
                PlayMusic();

            if (!ClientConfiguration.Instance.ModMode && UserINISettings.Instance.CheckForUpdates)
            {
                // Re-check for updates

                if ((DateTime.Now - lastUpdateCheckTime) > TimeSpan.FromSeconds(UPDATE_RE_CHECK_THRESHOLD))
                    CheckForUpdates();
            }
        }

        public void SwitchOff()
        {
            if (UserINISettings.Instance.StopMusicOnMenu)
                MusicOff();
        }

        private void MusicOff()
        {
            try
            {
                if (isMediaPlayerAvailable &&
                    MediaPlayer.State == MediaState.Playing)
                {
                    isMusicFading = true;
                }
            }
            catch (Exception ex)
            {
                Logger.Log("Turning music off failed! Message: " + ex.Message);
            }
        }

        /// <summary>
        /// Checks if media player is available currently.
        /// It is not available on Windows Vista or other systems without the appropriate media player components.
        /// </summary>
        /// <returns>True if media player is available, false otherwise.</returns>
        private bool IsMediaPlayerAvailable()
        {
            try
            {
                MediaState state = MediaPlayer.State;
                return true;
            }
            catch (Exception e)
            {
                Logger.Log("Error encountered when checking media player availability. Error message: " + e.Message);
                return false;
            }
        }

        private void LaunchMapEditor()
        {
            OSVersion osVersion = ClientConfiguration.Instance.GetOperatingSystemVersion();
            Process mapEditorProcess = new Process();

            string strCmdText = string.Format("/c cd /d {0} && FinalAlert2SP.exe", ProgramConstants.GamePath + "Resources/FinalAlert2SP");

            mapEditorProcess.StartInfo.FileName = "cmd.exe";
            mapEditorProcess.StartInfo.Arguments = strCmdText;
            mapEditorProcess.StartInfo.UseShellExecute = false;   //是否使用操作系统shell启动 
            mapEditorProcess.StartInfo.CreateNoWindow = true;   //是否在新窗口中启动该进程的值 (不显示程序窗口)
            mapEditorProcess.Start();
            mapEditorProcess.WaitForExit();  //等待程序执行完退出进程
            mapEditorProcess.Close();
        }

        public string GetSwitchName() => "Main Menu".L10N("UI:Main:MainMenu");

    }
}