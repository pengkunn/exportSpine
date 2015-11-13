using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using exportSpine.Core;
using System.IO;

namespace exportSpine.Command
{
    class Program
    {
        static void Main(string[] args)
        {
            var sourceFolder = @"D:\work\flight\trunk\cocostudio\spine动画工程";
            var outputFolder = @"D:\work\flight\trunk\cocostudio\spine动画工程\export";
            var settingFiles = Directory.GetFiles(sourceFolder, "spinesetting.json", SearchOption.AllDirectories);
            
            foreach(var settingFile in settingFiles)
            {
                spineExporter.export(settingFile, outputFolder);
            }

            
            Console.ReadKey();
        }
    }
}
