using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClientCore;
using Rampastring.Tools;

namespace DTAClient.Domain;
public class Mod
{
    public string Name { get; set; } // mod名称
    public string ID { get; set; } // mod注册名
    public string Description { get; set; } // mod说明
    public string ModPath { get; set; } // mod在重聚中的路径
    public int Version { get; set; } // mod版本
    public bool Visible { get; set; } = true; //是否显示
    public bool Ares { get; set; } = false; // 是否使用Ares
    public bool Phobos { get; set; } = false; // 是否使用Phobos
    public int PhobosVersion { get; set; } // phobos版本
    public string md { get; set; } = "md"; // 是尤复mod吗 md为是，空为不是
    public bool rules { get; set; } = false;
    public bool art { get; set; } = false;
    public string INI { get; set; }
    public string Countries { get; set; } = "美国,韩国,法国,德国,英国,利比亚,伊拉克,古巴,苏联,尤里"; // 国家
    public  string RandomSides { get; set; } = "随机盟军,随机苏军";
    public  List<string> RandomSidesIndexs { get; set; } = new List<string> { "0,1,2,3,4", "5,6,7,8" };
    private string[] SpecialINI { get; set; }

    public string[] files = Directory.GetFiles(ClientConfiguration.Instance.Mod_AiIniPath, "Mod&AI*.ini");

    public static List<Mod> mods = new List<Mod>();

    public static List<Mod> ais = new List<Mod>();


    /// <summary>
    /// 读取外部Mod。目前仅支持非DTA客户端的Mod。
    /// </summary>
    /// <param name="path">Mod所在路径</param>
    /// <returns> </returns>
    public Mod ReadMod(string path)
    {
        var mod = new Mod();


        //判断目录是否合法
        if (File.Exists(Path.Combine(path, "gamemd.exe")))
        {
            mod.md = "md";
        }
        else if (File.Exists(Path.Combine(path, "gamemd.exe")))
        {
            mod.md = string.Empty;
            mod.Countries = "美国,韩国,法国,德国,英国,利比亚,伊拉克,古巴,苏联";
        }
        else
        {
            return null; // 不合法
        }

        // 判断是否有Ares
        if (File.Exists(Path.Combine(path, "Ares.dll")))
        {
            Ares = true;
        }

        // 判断是否有Phobos
        if (File.Exists(Path.Combine(path, "Phobos.dll")))
        {
            Phobos = true;
        }

       // SpecialINI = { $"rules{md}.ini", $"art{md}.ini","" };

        var mixs = Directory.GetFiles(path, $"expand{md}.mix");


        //foreach (var file in iniFile) {

        //    = Directory.GetFiles(path, $"*{md}.ini");
        //}
        var shps = Directory.GetFiles(path, "*.shp");
        var vxls = Directory.GetFiles(path, "*.vxl");
        

        return new Mod();
    }

    public static void WriteMod_AI(string name)
    {
        string[] Mod_files = Directory.GetDirectories($"{ProgramConstants.GamePath}/INI/GameOptions/{name}");

        string[] Mod_AI = Directory.GetFiles(ClientConfiguration.Instance.Mod_AiIniPath, "Mod&AI*.ini");

        var Mod_Ai_INI = new IniFile(Path.Combine(ClientConfiguration.Instance.Mod_AiIniPath, "Mod&AI.ini"));

        // 关联Mod文件夹和Mod&AI*.ini

        foreach (string file in Mod_files)
        {
            var cunzai = false;
            foreach (string Mod_AI_file in Mod_AI)
            {
                var Mod_Ai = new IniFile(Mod_AI_file);
                var section = Mod_Ai.GetSection(name);
                if (section != null) { 

                    foreach (KeyValuePair<string, string> keyname in section.Keys)
                    {
                        if (keyname.Value == Path.GetFileName(file))
                        {
                            cunzai = true;
                            break;
                        }
                    }
                    
                if (cunzai)
                    break;
                }
            }
            if (!cunzai)
            {
                Mod_Ai_INI.SetStringValue(name, Path.GetFileName(file), Path.GetFileName(file));
            }
            Mod_Ai_INI.WriteIniFile();
        }

    }

    public static void Load(string name)
    {
        if (name == "Game")
            mods.Clear();
        else
            ais.Clear();

        WriteMod_AI(name);

        string[] Mod_AI = Directory.GetFiles(ClientConfiguration.Instance.Mod_AiIniPath, "Mod&AI*.ini");

        foreach (string file in Mod_AI)
        {
            var ini = new IniFile(file);
            if (!ini.SectionExists(name))
                continue;
            foreach (string key in ini.GetSectionKeys(name))
            {
                string modid = ini.GetStringValue(name, key, string.Empty);
                var mod = new Mod();
                mod.ID = modid;
                mod.Name = ini.GetStringValue(modid, "Text", modid);
                mod.ModPath = ini.GetStringValue(modid, "File", $"INI/GameOptions/{name}/{modid}");
                mod.Visible = ini.GetBooleanValue(modid, "Visible", true);

                if (name == "Game")
                {
                    if (ini.KeyExists(modid, "Main"))
                        mod.md = ini.GetStringValue(modid, "Main", "RA2_Main") == "YR_Main" ? "md" : string.Empty;
                    if (ini.KeyExists(modid, "Sides"))
                    {
                        mod.Countries = ini.GetStringValue(modid, "Sides", string.Empty);
                    }
                    if (ini.KeyExists(modid, "RandomSides"))
                    {
                        mod.RandomSides = ini.GetStringValue(modid, "RandomSides", string.Empty);
                        mod.RandomSidesIndexs = new List<string>();
                        for (int i = 1; i < mod.RandomSides.Length; i++)
                        {
                            mod.RandomSidesIndexs.Add(ini.GetStringValue(modid, $"RandomSidesIndex{i}", string.Empty));
                        }
                    }

                    mod.INI = ini.GetStringValue(modid, "INI", string.Empty);
                    mods.Add(mod);
                }
                else
                    ais.Add(mod);
            }
        }

       

    }
}
