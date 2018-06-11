using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ProcessManager.Models;

namespace ProcessManager
{
    public static class LogHelper
    {
        private static readonly object Locker = new object();
        private static ReaderWriterLockSlim _readWriteLock = new ReaderWriterLockSlim();
        public static void WriteTo(string filename, string data)
        {
            lock (Locker)
            {
                var rootPath = Directory.GetCurrentDirectory();
                var filePath = rootPath + "/logs/" + filename + ".log";
                if (!Directory.Exists(rootPath + "/logs"))
                {
                    Directory.CreateDirectory(rootPath + "/logs");
                }

                if (!File.Exists(filePath))
                {
                    File.CreateText(filePath).Close();
                }

                var jsonData = File.ReadAllText(filePath);
                var ps = JsonConvert.DeserializeObject<List<Report>>(jsonData) ?? new List<Report>();
                var newReport = new Report()
                {
                    Time = DateTime.Now,
                    Message = data
                };
                ps.Add(newReport);
                jsonData = JsonConvert.SerializeObject(ps);
                File.WriteAllText(filePath, jsonData);
            }

        }

        public static List<Report> GetLog(string filename)
        {
            lock (Locker)
            {
                var result = new List<Report>();
                var rootPath = Directory.GetCurrentDirectory();
                var filePath = rootPath + "/logs/" + filename + ".log";
                if (!Directory.Exists(rootPath + "/logs"))
                {
                    return null;
                }

                if (!File.Exists(filePath))
                {
                    return null;
                }

                var jsonData = File.ReadAllText(filePath);
                result = JsonConvert.DeserializeObject<List<Report>>(jsonData) ?? new List<Report>();
                return result;
            }

        }
    }
}
