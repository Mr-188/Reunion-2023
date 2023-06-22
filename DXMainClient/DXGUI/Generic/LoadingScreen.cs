using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using ClientCore;
using ClientCore.CnCNet5;
using ClientGUI;
using ClientUpdater;
using DTAClient.Domain.Multiplayer;
using DTAClient.DXGUI.Multiplayer.CnCNet;
using DTAClient.Online;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Framework.Media;
using MonoGame.Extended.VideoPlayback;
using Rampastring.Tools;
using Rampastring.XNAUI;


namespace DTAClient.DXGUI.Generic
{
    public class LoadingScreen : XNAWindow
    {
        public LoadingScreen(
            CnCNetManager cncnetManager,
            WindowManager windowManager,
            IServiceProvider serviceProvider,
            MapLoader mapLoader
        ) : base(windowManager)
        {
            this.cncnetManager = cncnetManager;
            this.serviceProvider = serviceProvider;
            this.mapLoader = mapLoader;
            content = new ContentManager(serviceProvider, "Content");
            videoRenderer = new VideoRenderer(windowManager, windowManager.Game.GraphicsDevice);
            // _graphics = new GraphicsDeviceManager();
            //Content.RootDirectory = "Content";
        }

        private static readonly object locker = new object();

        private MapLoader mapLoader;

        private PrivateMessagingPanel privateMessagingPanel;

        private bool visibleSpriteCursor;

        private Task updaterInitTask;
        private Task mapLoadTask;
        private readonly CnCNetManager cncnetManager;
        private readonly IServiceProvider serviceProvider;

        private VideoRenderer videoRenderer;
        // private Video video;
        // private VideoPlayer videoPlayer;
        private Texture2D videoTexture;

        ContentManager content;

        private const int WindowWidth = 1280;
        private const int WindowHeight = 768;
        private SpriteBatch _spriteBatch = null!;
        // private KeyboardStateHandler _keyboardStateHandler = null!;

        private Video _video = null!;
        private VideoPlayer _videoPlayer = null!;
        private Timer delayTimer;
        private Texture2D _helpTexture = null!;

        private readonly GraphicsDeviceManager _graphics;


        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool SetDllDirectory(string path);

        // 在你的代码中的适当位置调用这个方法，指定DLL文件的目录
        //SetDllDirectory("C:\\Path\\To\\Your\\DLLs");
        public override void Initialize()
        {
            ClientRectangle = new Rectangle(0, 0, 1280, 768);
            Name = "LoadingScreen";

          //  Console.WriteLine(SetDllDirectory(ProgramConstants.GamePath+ "Resources/"));

            //FFmpegBinariesHelper.RegisterFFmpegBinaries(ProgramConstants.GamePath);
            
            //Environment.SetEnvironmentVariable("PATH", ProgramConstants.GamePath);
            if (!UserINISettings.Instance.video_wallpaper || RuntimeInformation.ProcessArchitecture.ToString() == "X64")
            {

                string[] Wallpaper = Directory.GetFiles("Resources/" + UserINISettings.Instance.ClientTheme + "Wallpaper");

                if (UserINISettings.Instance.Random_wallpaper)
                {
                    Random ran = new Random();
                    int i = ran.Next(0, Wallpaper.Length);

                    BackgroundTexture = AssetLoader.LoadTexture(Wallpaper[i]);

                }
                else
                {
                    BackgroundTexture = AssetLoader.LoadTexture(Wallpaper[0]);
                }

            }
            else
            {

                _videoPlayer = new VideoPlayer(GraphicsDevice);
                _videoPlayer.IsLooped = true;

            }
            base.Initialize();

            CenterOnParent();

            bool initUpdater = !ClientConfiguration.Instance.ModMode;

            if (initUpdater)
            {
                updaterInitTask = new Task(InitUpdater);
                updaterInitTask.Start();
            }

            mapLoadTask = mapLoader.LoadMapsAsync();

            if (Cursor.Visible)
            {
                Cursor.Visible = false;
                visibleSpriteCursor = true;
            }
        }
        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            if (UserINISettings.Instance.video_wallpaper && RuntimeInformation.ProcessArchitecture.ToString() != "X64")
            {
                // Create a new SpriteBatch, which can be used to draw textures.


                // TODO: use this.Content to load your game content here

                // _helpTexture = TextureLoader.LoadTexture(GraphicsDevice, "Content/HelpTexture.png");
                // 在你的代码中的适当位置调用这个方法，指定DLL文件的目录
                //SetDllDirectory("D:\\File\\Documents\\My_File\\xna\\DXMainClient\\bin\\Debug\\Ares\\WindowsDX\\net7.0-windows\\runtimes\\win-x64");
                _video = VideoHelper.LoadFromFile("Resources/a.mp4");
                // _video += VideoEnded;

                _videoPlayer.Play(_video);
            }
        }
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
            // _helpTexture.Dispose();
            _videoPlayer.Stop();
            _video.Dispose();
            _videoPlayer.Dispose();
        }
        public override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            if (UserINISettings.Instance.video_wallpaper && RuntimeInformation.ProcessArchitecture.ToString() != "X64")
            {
                // TODO: Add your drawing code here
                var videoTexture = _videoPlayer.GetTexture();

                _spriteBatch.Begin();

                var destRect = new Rectangle(0, 0, WindowWidth, WindowHeight);
                _spriteBatch.Draw(videoTexture, destRect, Color.White);

                _spriteBatch.End();

                _spriteBatch.Begin(blendState: BlendState.NonPremultiplied);
                //  _spriteBatch.Draw(_helpTexture, Vector2.Zero, Color.White);
                _spriteBatch.End();
            }
            base.Draw(gameTime);
        }
        protected override void Dispose(bool disposing)
        {
            _video.Dispose();
            _videoPlayer.Dispose();

            // _keyboardStateHandler.KeyUp -= KeyboardStateHandler_KeyUp;

            base.Dispose(disposing);
        }


        private void InitUpdater()
        {
            Updater.OnLocalFileVersionsChecked += LogGameClientVersion;
            Updater.CheckLocalFileVersions();
        }

        private void LogGameClientVersion()
        {
            Logger.Log($"Game Client Version: {ClientConfiguration.Instance.LocalGame} {Updater.GameVersion}");
            Updater.OnLocalFileVersionsChecked -= LogGameClientVersion;
        }

        private void Finish()
        {

            ProgramConstants.GAME_VERSION = ClientConfiguration.Instance.ModMode ?
                "N/A" : Updater.GameVersion;

            MainMenu mainMenu = serviceProvider.GetService<MainMenu>();

            WindowManager.AddAndInitializeControl(mainMenu);
            mainMenu.PostInit();

            if (UserINISettings.Instance.AutomaticCnCNetLogin &&
                NameValidator.IsNameValid(ProgramConstants.PLAYERNAME) == null)
            {
                cncnetManager.Connect();
            }

            if (!UserINISettings.Instance.PrivacyPolicyAccepted)
            {
                WindowManager.AddAndInitializeControl(new PrivacyNotification(WindowManager));
            }
            if (UserINISettings.Instance.video_wallpaper && RuntimeInformation.ProcessArchitecture.ToString() == "x64")
                UnloadContent();



            WindowManager.RemoveControl(this);
            Cursor.Visible = visibleSpriteCursor;

        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (updaterInitTask == null || updaterInitTask.Status == TaskStatus.RanToCompletion)
            {
                if (mapLoadTask.Status == TaskStatus.RanToCompletion)
                {

                    Finish();
                }
            }
        }
    }


}
