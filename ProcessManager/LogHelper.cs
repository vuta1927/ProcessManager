using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using ProcessManagerCore.Models;

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

                var data = JsonConvert.SerializeObject(new Report() {Time = DateTime.Now, Message = msg.Message});
                var line = new string[] { data };
                try
                {
                    File.AppendAllLines(filePath, line);
                }
                catch (Exception e)
                {
                    Add(msg);
                }
                
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

        public static List<Report> GetLog(string filename, DateTime from, DateTime to)
        {
            lock (Locker)
            {
                try
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
                    var result = new List<Report>();
                    var lines = File.ReadAllLines(filePath);
                    foreach (var line in lines)
                    {
                        var rp = JsonConvert.DeserializeObject<Report>(line);
                        if (rp.Time >= from && rp.Time <= to)
                            result.Add(rp);
                    }
                    
                    //var result = File.ReadAllLines(filePath);
                    //var r = File.ReadLines(filePath);
                    return result;
                }
                catch (Exception)
                {
                    return GetLog(filename, from, to);
                }
                
            }

        }
    }
}
