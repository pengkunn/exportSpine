using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace exportSpine.Core
{
    public class exportSetting
    {
        string _strContent;

        public void parse(string content) 
        {
            _strContent = content;

            //if (!File.Exists(settingFileFullName))
            //    return null;

            //string strSetting = File.ReadAllText(settingFileFullName);

            var lines = content.Split('\n');

            var query = from line in lines
                        where line.Contains("scale")
                        select line;

            foreach (var line in query.ToList<string>())
            {
                var arr = line.Split(':');
                string name = arr[0];
                var valueArray = arr[1].Replace("[", "").Replace("]", "").Split(',');
                if (name.Trim() == "scale")
                {
                    IList<float> scales = new List<float>();
                    foreach(var value in valueArray)
                    {
                        if (value != name)
                            scales.Add(float.Parse(value.Trim()));
                    }
                    this.scale = scales.ToArray<float>();
                }
                else 
                {
                    IList<string> scaleSuffixs = new List<string>();
                    foreach (var value in valueArray)
                    {
                        scaleSuffixs.Add(value.Trim());
                    }
                    this.scaleSuffix = scaleSuffixs.ToArray<string>();
                }
                
            }
        }

        public string convertToString() 
        {
            var lines = _strContent.Split('\n');

            for (var i = 0; i < lines.Count(); i++)
            {
                if (lines[i].Contains("scale") && !lines[i].Contains("scaleSuffix"))
                {
                    StringBuilder newScale = new StringBuilder();
                    newScale.Append("scale:[");
                    foreach (var s in scale) 
                    {
                        newScale.Append(s.ToString());
                        newScale.Append(",");
                    }
                    if (scale.Count()>0)
                        newScale.Remove(newScale.Length-1, 1);
                    newScale.Append("]");
                    lines[i] = newScale.ToString();
                }

                if (lines[i].Contains("scaleSuffix"))
                {
                    StringBuilder newScaleSuffix = new StringBuilder();
                    newScaleSuffix.Append("scaleSuffix:[");
                    foreach (var s in scaleSuffix)
                    {
                        newScaleSuffix.Append(s);
                        newScaleSuffix.Append(",");
                    }
                    if (scale.Count() > 0)
                        newScaleSuffix.Remove(newScaleSuffix.Length - 1, 1);
                    newScaleSuffix.Append("]");
                    lines[i] = newScaleSuffix.ToString();
                }
            }

            StringBuilder newContent = new StringBuilder();
            foreach(var line in lines)
            {
                newContent.Append(line);
                newContent.Append("\n");
            }
            newContent.Remove(newContent.Length - 1, 1);

            return newContent.ToString();
            
        }

        public float[] scale
        {
            get;
            set;
        }

        public string[] scaleSuffix
        {
            get;
            set;
        }

    }

    public class spineExporter
    {
        static public string export(string spineFileFullName, string settingFileName, string outputFolder = "")
        {
            if (!File.Exists(spineFileFullName))
                return "";

            string spineFileName = Path.GetFileName(spineFileFullName);
            string folderFullName = Path.GetDirectoryName(spineFileFullName);            
            string settingFileFullName = Path.Combine(folderFullName, settingFileName);
            string exportFolderName = "SpineExport";

            

            try
            {
                if (Directory.Exists(Path.Combine(folderFullName, exportFolderName)))
                {
                    Directory.Delete(Path.Combine(folderFullName, exportFolderName), true);
                }
                Directory.CreateDirectory(Path.Combine(folderFullName, exportFolderName));


                Process p = new Process();
                p.StartInfo.WorkingDirectory = folderFullName;
                p.StartInfo.FileName = "spine.com"; //确定程序名
                p.StartInfo.Arguments = @"-i " + spineFileName + " -o " + exportFolderName + " -e " + settingFileName; //确定程式命令行
                Console.WriteLine(@"-i " + spineFileName + " -o " + exportFolderName + " -e " + settingFileName);
                p.StartInfo.UseShellExecute = false; //Shell的使用
                p.StartInfo.RedirectStandardInput = true; //重定向输入
                p.StartInfo.RedirectStandardOutput = true; //重定向输出
                p.StartInfo.RedirectStandardError = true; //重定向输出错误
                p.StartInfo.CreateNoWindow = true; //设置置不显示示窗口
                p.Start(); //00

                StreamReader reader = p.StandardOutput;//截取输出流
                string line = reader.ReadLine();//每次读取一行
                while (!reader.EndOfStream)
                {
                    //tbResult.AppendText(line + " ");
                    line = reader.ReadLine();
                    Console.WriteLine(line);
                    if (line.Contains("ERROR"))
                    {
                        return line;
                    }
                }
                p.WaitForExit();//等待程序执行完退出进程
                p.Close();//关闭进程
                reader.Close();//关闭流

                var text = File.ReadAllText(settingFileFullName);
                var lines = text.Split('\n');
                var query = from s in lines
                            where s.Contains("scaleSuffix")
                            select s;

                var scaleSuffixString = query.First<string>();
                var scaleSuffixs = scaleSuffixString.Remove(0, scaleSuffixString.IndexOf(':') + 1).Replace("[", "").Replace("]", "").Split(',');

                var directoryInfo = new DirectoryInfo(Path.Combine(folderFullName, exportFolderName));
                var fileInfos = directoryInfo.GetFiles("*.json");

                foreach (var fileInfo in fileInfos)
                {
                    foreach (var suffix in scaleSuffixs)
                    {
                        fileInfo.CopyTo(Path.Combine(directoryInfo.FullName, fileInfo.Name.Replace(fileInfo.Extension, "") + suffix.Trim() + fileInfo.Extension));
                    }
                    fileInfo.Delete();
                }

                if (!string.IsNullOrEmpty(outputFolder))
                {
                    if (!Directory.Exists(outputFolder))
                        Directory.CreateDirectory(outputFolder);

                    foreach (var exportFile in directoryInfo.GetFiles())
                    {
                        if (File.Exists(Path.Combine(outputFolder, exportFile.Name)))
                            File.Delete(Path.Combine(outputFolder, exportFile.Name));

                        exportFile.MoveTo(Path.Combine(outputFolder, exportFile.Name));
                    }
                }
                
            }
            catch(Exception e)
            {
                return e.ToString();
            }

            return "";

        }
    }
}
