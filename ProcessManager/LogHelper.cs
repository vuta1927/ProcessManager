using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ProcessManager
{
    public static class LogHelper
    {
        public static object locker;
        public static void WriteTo(string filename, string data)
        {
            lock (locker)
            {
                var rootPath = Directory.GetCurrentDirectory();
                var filePath = rootPath + "/logs" + filename + ".txt";
                if (!Directory.Exists(rootPath + "/logs"))
                {
                    Directory.CreateDirectory(rootPath + "/logs");
                }

                if (!File.Exists(filePath))
                {
                    File.CreateText(filePath).Close();
                }

                using (var f = new StreamWriter(filePath))
                {
                    f.WriteLine(data);
                }
            }
        }

        public static string[] GetLog(string filename)
        {
            lock (locker)
            {
                var rootPath = Directory.GetCurrentDirectory();
                var filePath = rootPath + "/logs" + filename + ".txt";
                if (!Directory.Exists(rootPath + "/logs"))
                {
                    return null;
                }

                string[] lines = File.ReadAllLines(filePath);
                return lines;
            }
            
        }
    }
}
