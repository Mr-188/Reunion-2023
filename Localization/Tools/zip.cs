using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Localization.Tools;
public class ZIP
{
    private static readonly string  zippro = "Resources/7z.exe";

    public static async Task Unpack(string name)
    {
        
        string arguments = $"x -y -o./ \"{name}\"";

      
        ProcessStartInfo startInfo = new ProcessStartInfo(zippro, arguments);
        startInfo.WindowStyle = ProcessWindowStyle.Hidden;
        startInfo.CreateNoWindow = true;

        using (Process process = new Process())
        {
            process.StartInfo = startInfo;
            process.Start();
            process.WaitForExit();
        }
        
        string fileName = Path.GetFileNameWithoutExtension(name);
        Console.WriteLine(fileName);
        string[] splitFiles = Directory.GetFiles("./",fileName + ".*");
        foreach (string file in splitFiles)
        {
            File.Delete(file);
        }
    }
}

