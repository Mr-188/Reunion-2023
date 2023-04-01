using ClientCore;
using ClientGUI;
using DTAClient.Domain.Multiplayer;
using Localization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Rampastring.Tools;
using Rampastring.XNAUI;
using Rampastring.XNAUI.XNAControls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DTAClient.DXGUI.Multiplayer.GameLobby
{
    class GetRandomMap : XNAWindow
    {
        private const int OPTIONHEIGHT = 85;

        private XNALabel lblTitle;

        private XNALabel lblClimate; //气候
        private XNAClientDropDown ddClimate;

        private XNALabel lblPeople; //人数
        private XNAClientDropDown ddPeople;

        private XNAClientCheckBox cbDamage;//建筑物损伤

        private XNALabel lblSize;
        private XNAClientDropDown ddSize;
        private XNAClientButton btnGenerate;
        private XNAClientButton btnCancel;
        private XNAClientButton btnSave;
        private XNAButton btnpreview;

        private XNALabel lblStatus;

        private Thread thread1;

        private Thread thread;

        private  bool Stop = false;

        private bool isSave;

        private string[] People;

        private string Damage = string.Empty;

        public MapLoader MapLoader;

        public GetRandomMap(WindowManager windowManager, MapLoader mapLoader) : base(windowManager)
        {
            MapLoader = mapLoader;
        }
        public override void Initialize()
        {
            base.Initialize();
            Name = "GetRandomMap";
            CenterOnParent();
            #if WINFORMS
            System.Windows.Forms.Control.CheckForIllegalCrossThreadCalls = false;
            #endif
            ClientRectangle = new Rectangle(200, 100, 800, 500);

            lblTitle = new XNALabel(WindowManager);
            lblTitle.ClientRectangle = new Rectangle(350, 20, 0, 0);
            lblTitle.CenterOnParentHorizontally();
            lblTitle.Text = "Generate random map".L10N("UI:Main:GenRanMap");

            lblStatus = new XNALabel(WindowManager);
            lblStatus.ClientRectangle = new Rectangle(360, 420, 0, 0);

            btnGenerate = new XNAClientButton(WindowManager);
            btnGenerate.Name = "btnGenerate";
            btnGenerate.ClientRectangle = new Rectangle(350, 460, 100, 20);
            btnGenerate.Text = "Generate".L10N("UI:Main:Generate");
            btnGenerate.IdleTexture = AssetLoader.LoadTexture("92pxbtn.png");
            btnGenerate.HoverTexture = AssetLoader.LoadTexture("92pxbtn_c.png");
            btnGenerate.LeftClick += btnGenerat_LeftClick;


            btnCancel = new XNAClientButton(WindowManager);
            btnCancel.Name = "btnCancel";
            btnCancel.ClientRectangle = new Rectangle(40, 460, 100, 20);
            btnCancel.Text = "Cancel".L10N("UI:Main:ButtonCancel");
            btnCancel.IdleTexture = AssetLoader.LoadTexture("92pxbtn.png");
            btnCancel.HoverTexture = AssetLoader.LoadTexture("92pxbtn_c.png");
            btnCancel.LeftClick += btnCancel_LeftClick;

            btnSave = new XNAClientButton(WindowManager);
            btnSave.Name = "btnSave";
            btnSave.ClientRectangle = new Rectangle(660, 460, 100, 20);
            btnSave.Text = "Save".L10N("UI:Main:ButtonSave");
            btnSave.IdleTexture = AssetLoader.LoadTexture("92pxbtn.png");
            btnSave.HoverTexture = AssetLoader.LoadTexture("92pxbtn_c.png");
            btnSave.Enabled = false;
            btnSave.LeftClick += btnSave_LeftClick;

            lblClimate = new XNALabel(WindowManager);
            lblClimate.ClientRectangle = new Rectangle(40, OPTIONHEIGHT, 0, 0);
            lblClimate.Text = "Climatic".L10N("UI:Main:Climatic");

            ddClimate = new XNAClientDropDown(WindowManager);
            ddClimate.ClientRectangle = new Rectangle(lblClimate.X + 70, OPTIONHEIGHT, 80, 20);
            XNADropDownItem Desert = new XNADropDownItem();
            Desert.Text = "DESERT".L10N("UI:Main:DESERT");
            Desert.Tag = "DESERT";
            XNADropDownItem Newurban = new XNADropDownItem();
            Newurban.Text = "NEWURBAN".L10N("UI:Main:NEWURBAN"); ;
            Newurban.Tag = "NEWURBAN";
            XNADropDownItem Temperate = new XNADropDownItem();
            Temperate.Text = "TEMPERATE".L10N("UI:Main:TEMPERATE"); ;
            Temperate.Tag = "TEMPERATE";
            XNADropDownItem Temperate_Islands = new XNADropDownItem();
            Temperate_Islands.Text = "Islands".L10N("UI:Main:Islands"); ;
            Temperate_Islands.Tag = "TEMPERATE_Islands";

            btnpreview = new XNAButton(WindowManager);
            btnpreview.ClientRectangle = new Rectangle(100, 150, 600, 250);


            ddClimate.AddItem("Random".L10N("UI:Main:Random"));
            ddClimate.AddItem(Temperate);
            ddClimate.AddItem(Temperate_Islands);
            ddClimate.AddItem(Newurban);
            ddClimate.AddItem(Desert);
            ddClimate.SelectedIndex = 0;

            lblPeople = new XNALabel(WindowManager);
            lblPeople.ClientRectangle = new Rectangle(ddClimate.X + 100, OPTIONHEIGHT, 80, 0);
            lblPeople.Text = "Number".L10N("UI:Main:Number");

            ddPeople = new XNAClientDropDown(WindowManager);
            ddPeople.ClientRectangle = new Rectangle(lblPeople.X + 40, OPTIONHEIGHT, 80, 20);
            ddPeople.AddItem("Random".L10N("UI:Main:Random"));


            for (int i = 2; i <= 8; i++)
            {
                ddPeople.AddItem(i.ToString());
            }
            ddPeople.SelectedIndex = 0;

            lblSize = new XNALabel(WindowManager);
            lblSize.ClientRectangle = new Rectangle(ddPeople.X+100, OPTIONHEIGHT, 0, 0);
            lblSize.Text = "Size".L10N("UI:Main:Size");

            ddSize = new XNAClientDropDown(WindowManager);
            ddSize.ClientRectangle = new Rectangle(lblSize.X + 40, OPTIONHEIGHT, 80, 20);
            ddSize.AddItem("small".L10N("UI:Main:small"));
            ddSize.AddItem("medium".L10N("UI:Main:medium"));
            ddSize.AddItem("big".L10N("UI:Main:big"));
            ddSize.AddItem("Very big".L10N("UI:Main:Verybig"));
            ddSize.SelectedIndex = 1;

            
            cbDamage = new XNAClientCheckBox(WindowManager);
            cbDamage.ClientRectangle = new Rectangle(ddSize.X + 150, OPTIONHEIGHT, 0, 0);
            cbDamage.Text = "Random building damage".L10N("UI:Main:RanBuildDamage");


            //thread.Abort()
            AddChild(lblTitle);
            AddChild(lblStatus);
            AddChild(btnpreview);

            AddChild(lblClimate);
            AddChild(ddClimate);
            
            AddChild(lblPeople);
            AddChild(ddPeople);

            AddChild(lblSize);
            AddChild(ddSize);

            AddChild(cbDamage);
            AddChild(btnGenerate);
            AddChild(btnCancel);
            AddChild(btnSave);
        }

        public bool GetIsSave()
        {
            return isSave;
        }

        private void btnCancel_LeftClick(object sender, EventArgs e)
        {
            isSave = false;
            Disable();
        }

        private void btnSave_LeftClick(object sender, EventArgs e)
        {
            isSave = true;
            Disable();
        }

        private void btnGenerat_LeftClick(object sender, EventArgs e)
        {
            btnGenerate.Enabled = false;
            btnSave.Enabled = false;
            thread1 = new Thread(new ThreadStart(StartText));
            thread = new Thread(new ThreadStart(RunCmd));
            thread1.Start();
            thread.Start();
        }

        public void RunCmd()
        {
            string strCmdText;
            Random r = new Random();
            string Generate = (string)ddClimate.SelectedItem.Tag;
            if (ddClimate.SelectedIndex==0)
            {
                Generate = (string)ddClimate.Items[r.Next(1, 5)].Tag;
            }

            int sizex = 35*(ddSize.SelectedIndex+1) + r.Next(30,50);
            int sizey= 35 * (ddSize.SelectedIndex+1) +r.Next(30,50);

            People = GetPeople(ddPeople.SelectedItem.Text);

            if (cbDamage.Checked)
            {
                Damage = "-d";
            }

                strCmdText = "/c cd /d \"" + ProgramConstants.GamePath + "RandomMapGenerator_RA2\" &&" +
                    string.Format(" RandomMapGenerator.exe -w {10} -h {11} --nwp {0} --sep {1} --nep {2} --swp {3} --sp {4} --wp {5} --ep {6} --np {7} {8} --type {9} -g standard &&", People[0], People[1], People[2], People[3], People[4], People[5], People[6], People[7], Damage, Generate, sizex, sizey) +
                        string.Format(" cd Map Renderer &&" + " CNCMaps.Renderer.exe -i \"{0}Maps/Custom/随机地图.map\" -o 随机地图 -m \"{1}\" -Y -z +(1280,0) --thumb-png --bkp ", ProgramConstants.GamePath, ProgramConstants.GamePath.TrimEnd('\\'));

                Process process = new Process();
                process.StartInfo.FileName = "cmd.exe";
                process.StartInfo.Arguments = strCmdText;
                process.StartInfo.UseShellExecute = false;   //是否使用操作系统shell启动 
                process.StartInfo.CreateNoWindow = true;   //是否在新窗口中启动该进程的值 (不显示程序窗口)
                process.Start();
                process.WaitForExit();  //等待程序执行完退出进程
                process.Close();
            
            Stop = true;

        }
        public void StartText()
        {
            string[] TextList = { "Is dispersing civilians.".L10N("UI:Main:GenText1"), "Ore being mined".L10N("UI:Main:GenText2"), "The base construction vehicles are being loaded".L10N("UI:Main:GenText3"), "Ammunition being examined".L10N("UI:Main:GenText4"), "Bobosa is being distributed for mobilization".L10N("UI:Main:GenText5"), "Getting the Phantom tank familiar with the environment".L10N("UI:Main:GenText6"), "The police dogs are being calmed".L10N("UI:Main:GenText7"), "Catching dolphins".L10N("UI:Main:GenText8"), "Bargaining with the logistics".L10N("UI:Main:GenText9"), "The transport plane is being refuelled".L10N("UI:Main:GenText10"), "We're sinking the submarine".L10N("UI:Main:GenText11"), "The building is being painted".L10N("UI:Main:GenText12") };
            Random r = new Random();

            while (!Stop)
            {
                lblStatus.Text = TextList[r.Next(TextList.Length)];
                Thread.Sleep(500);
            }
            
          File.Delete("Maps/Custom/随机地图.png");
            FileInfo fi = new FileInfo("Maps/Custom/thumb_随机地图.png");
          
            try
            {
                fi.MoveTo("Maps/Custom/随机地图.png");
                btnpreview.IdleTexture = AssetLoader.LoadTextureUncached("Maps/Custom/随机地图.png");
            }
            catch
            {
                lblStatus.Text = "error".L10N("UI:Main:error");
                btnGenerate.Enabled = true;
                Stop = false;
                return;
            }
            lblStatus.Text = "completed".L10N("UI:Main:completed"); ;
         
            btnGenerate.Enabled = true;
            btnSave.Enabled = true;
            Stop = false;

            
        }


        private string[] GetPeople(string Peoples)
        {
            int[] p =  { 0, 0, 0, 0, 0, 0, 0, 0 };
            int Current;
            Random r = new Random();
            if (Peoples == "Random".L10N("UI:Main:Random"))
                Current = r.Next(2,8);
            else
                Current = int.Parse(Peoples);
            
            while(Current>0)
            {

                p[r.Next(8)]++;

                Current--;
            }
            return string.Join(",", p).Split(',');
        }


        public MapLoader GetMapLoader()
        {
            return MapLoader;
        }
    }
}
