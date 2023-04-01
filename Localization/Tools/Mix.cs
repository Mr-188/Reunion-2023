using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rampastring.Tools;
using static System.Net.Mime.MediaTypeNames;

namespace Localization.Tools;
public static class Mix
{
    //将目录打包成mix
    public static void PackToMix(string path,string MixName)
    {
        string strCmdText = string.Format("\"Resources\\ccmixar\" pack -game ra2 -mix {0} -dir {1}", MixName, path);
        Logger.Log(strCmdText);
        Process process = new Process();
        process.StartInfo.FileName = "cmd.exe";
        process.StartInfo.Arguments = strCmdText;
        process.StartInfo.UseShellExecute = false;   //是否使用操作系统shell启动 
        process.StartInfo.CreateNoWindow = true;   //是否在新窗口中启动该进程的值 (不显示程序窗口)
        process.Start();
        process.WaitForExit();  //等待程序执行完退出进程
        process.Close();
    }

    public static void UnpackToMix(string path,string MixName)
    {
        string strCmdText = string.Format("ccmixar unpack -game ra2 -mix {0} -dir {1}", MixName, path);
        Process process = new Process();
        process.StartInfo.FileName = "cmd.exe";
        process.StartInfo.Arguments = strCmdText;
        process.StartInfo.UseShellExecute = false;   //是否使用操作系统shell启动 
        process.StartInfo.CreateNoWindow = true;   //是否在新窗口中启动该进程的值 (不显示程序窗口)
        process.Start();
        process.WaitForExit();  //等待程序执行完退出进程
        process.Close();
    }

}
