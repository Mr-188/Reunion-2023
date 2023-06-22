using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static System.Net.Mime.MediaTypeNames;

namespace Localization.Tools;
public class CSFConverter
{
    public static JObject Csf(string csfName,string jsonName)
    {
        Dictionary<string, string> keyValues = new Dictionary<string, string>();

        string command = $" -C \"{csfName}\" -J \"{jsonName}\"";

        Process process = new Process();
        process.StartInfo.FileName = "Resources\\csf.exe";
        process.StartInfo.Arguments = command;
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.CreateNoWindow = true;

        process.Start();
        string output = process.StandardOutput.ReadToEnd();

        process.WaitForExit();

        // 处理输出结果或进行其他操作
       /// Console.WriteLine(output);

        string jsonContent = File.ReadAllText(jsonName);

        // 解析 JSON
        JObject json = JObject.Parse(jsonContent);


       // keyValues.Add("LoadMsg:All01md", json.SelectToken("LoadMsg:All01md").ToString());

       

        return json;
    }

}
