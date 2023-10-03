using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Rampastring.Tools;


namespace ClientCore.Settings;
public class NetWorkINISettings
{
    private static NetWorkINISettings _instance;
    
    private const string remoteFileUrl = "https://raa2022.top/Public/settings.ini"; // 远程设置
    private const string localFilePath = "Resources/settings.ini"; // 本地保存路径
    private const string Updater = "Updater"; //更新
    private const string Components = "Components"; //组件
    private const string Main = "Main"; //主要

    public static event EventHandler DownloadCompleted;

    public IniFile SettingsIni { get; private set ; }

    public string UpdaterMirrors { get; private set; } = string.Empty; //更新服务器地址

    public string UpdaterName { get; private set; } = string.Empty; //更新服务器名称

    public string UpdaterLocation { get; private set; } = string.Empty; //更新服务器位置
    public string Updaterlog { get; private set; } = string.Empty;
    public string ComponentsMirrors { get; private set; } = string.Empty; //组件地址
    public string Announcement { get; private set; } = string.Empty; //公告信息


    public static NetWorkINISettings Instance
    {
        get
        {
            return _instance;
        }
    }

    public static async Task Initialize()
    {
        if (_instance != null)
            throw new InvalidOperationException("NetWorkINISettings has already been initialized!");

        if (await DownloadSettingFileAsync())
        {
            var iniFile = new IniFile(SafePath.CombineFilePath(ProgramConstants.GamePath, localFilePath));
            _instance = new NetWorkINISettings(iniFile);

            ClientUpdater.Updater.Initialize(ProgramConstants.GamePath,
               ProgramConstants.GetBaseResourcePath(),
               ClientConfiguration.Instance.SettingsIniName,
               ClientConfiguration.Instance.LocalGame,
               SafePath.GetFile(ProgramConstants.StartupExecutable).Name,
               NetWorkINISettings.Instance.UpdaterMirrors,
               NetWorkINISettings.Instance.UpdaterName,
               NetWorkINISettings.Instance.UpdaterLocation);
            DownloadCompleted?.Invoke(null, EventArgs.Empty);
        }

    }

    protected static async Task<bool> DownloadSettingFileAsync()
    {
        try
        {
            using (WebClient client = new WebClient())
            {
                await client.DownloadFileTaskAsync(new Uri(remoteFileUrl), localFilePath);

                //Console.WriteLine($"文件已下载到：{localFilePath}");
                // 如果下载和处理成功，返回true
                
                return true;
            }
        }
        catch (Exception ex)
        {
            // 如果发生异常，返回false
            Logger.Log("连接服务器出错。" + ex);
            return false;
        }
    }


    protected NetWorkINISettings(IniFile iniFile)
    {
        SettingsIni = iniFile;
        if (SettingsIni.SectionExists(Updater))
        {
            UpdaterMirrors = SettingsIni.GetStringValue(Updater, "UpdaterMirrors", string.Empty);
            UpdaterName = SettingsIni.GetStringValue(Updater, "UpdaterName", string.Empty);
            UpdaterLocation = SettingsIni.GetStringValue(Updater, "UpdaterLocation", string.Empty);
            Updaterlog = SettingsIni.GetStringValue(Updater, "Updaterlog", string.Empty);
        }

        if (SettingsIni.SectionExists(Components))
        {

            ComponentsMirrors = SettingsIni.GetStringValue(Components, "ComponentsMirrors", string.Empty);

        }

        if (SettingsIni.SectionExists(Main))
        {
            Announcement = SettingsIni.GetStringValue(Main, "Announcement", string.Empty);
        }

    }


}
