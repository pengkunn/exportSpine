using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using exportSpine.Core;
using System.IO;
using System.Configuration;

namespace exportSpine.Command
{
    class Program
    {
        static void Main(string[] args)
        {
            var sourceFolder = ConfigurationSettings.AppSettings["sourceFolder"]; //@"D:\work\flight\trunk\cocostudio\spine动画工程";
            var outputFolder = ConfigurationSettings.AppSettings["outputFolder"]; //@"D:\work\flight\trunk\cocostudio\spine动画工程\export";

            var spineFiles = Directory.GetFiles(sourceFolder, "*.spine", SearchOption.AllDirectories);
            //var settingFiles = Directory.GetFiles(sourceFolder, "spinesetting.json", SearchOption.AllDirectories);
            
            foreach(var spineFileName in spineFiles)
            {
                spineExporter.export(spineFileName, "spinesetting.json", outputFolder);
            }

            
            Console.ReadKey();
        }
    }
}
