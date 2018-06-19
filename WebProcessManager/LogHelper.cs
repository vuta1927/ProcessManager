using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using WebProcessManager.Models;

namespace ProcessManagerCore
{
    public static class LogHelper
    {
        private static readonly object Locker = new object();
        private static BlockingCollection<LogMessage> _logMessages = new BlockingCollection<LogMessage>();

        public static void Start()
        {
            foreach (var msg in _logMessages.GetConsumingEnumerable())
            {
                // of course you'll want your exception handling in here
                var rootPath = Directory.GetCurrentDirectory();
                var filePath = rootPath + "/logs/" + msg.FileName + ".log";
                if (!Directory.Exists(rootPath + "/logs"))
                {
                    Directory.CreateDirectory(rootPath + "/logs");
                }

                if (!File.Exists(filePath))
                {
                    File.CreateText(filePath).Close();
                }

                var n = new string[] {"[" + DateTime.Now + "]:" + msg.Message};
                File.AppendAllLines(filePath, n);
            }
        }

        public static void Stop()
        {
            _logMessages.CompleteAdding();
        }
        public static void Add(LogMessage m)
        {
            _logMessages.Add(m);
        }
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

        public static string[] GetLog(string filename)
        {
            lock (Locker)
            {
                //var result = new List<Report>();
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

                //var jsonData = File.ReadAllText(filePath);
                //result = JsonConvert.DeserializeObject<List<Report>>(jsonData) ?? new List<Report>();
                var result = File.ReadAllLines(filePath);
                return result;
            }

        }
    }
}
