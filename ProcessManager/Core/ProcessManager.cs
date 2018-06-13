﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ProcessManager;
using ProcessManager.Models;
using Process = System.Diagnostics.Process;

namespace ProcessManager.Core
{
    public class ProcessManager
    {
        private static List<Models.Process> _processes;
        private static string filePath;
        public ProcessManager(string fileName)
        {

            var rootPath = Directory.GetCurrentDirectory();
            filePath = rootPath + "/" + fileName;

            if (!File.Exists(filePath))
            {
                File.CreateText(filePath).Close();
            }

            if (_processes == null)
            {
                _processes = new List<Models.Process>();
            };

            using (var f = new StreamReader(filePath))
            {
                var json = f.ReadToEnd();
                _processes = JsonConvert.DeserializeObject<List<Models.Process>>(json);
                foreach (var p in _processes)
                {
                    p.IsRunning = false;
                }
            }
        }

        public List<Models.Process> GetAll()
        {
            return _processes;
        }
        public void RunAll()
        {
            if (_processes == null || !_processes.Any()) return;
            
            foreach (var p in _processes)
            {
                if (p.IsRunning || p.AutoRestart) continue;
                Task.Run(()=>
                {
                    p.Start();
                });
            }
        }
        public void Run(int id)
        {
            foreach (var p in _processes)
            {
                if (p.Id == id && !p.IsRunning)
                {
                    p.Start();
                    return;
                }
            }
        }

        public void SetAutoRestart(int id)
        {
            var p = _processes.SingleOrDefault(x => x.Id == id);
            if (p == null) return;
            p.AutoRestart = true;
        }

        public void EndAll()
        {
            if (_processes.Any())
            {
                foreach (var p in _processes)
                {
                    try
                    {
                        p.Stop();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        LogHelper.WriteTo(p.Id + ".error", e.ToString());
                    }
                }
            }
        }
        public int Add(string application, string arguments, bool autoRestart)
        {
            if (_processes == null)
            {
                _processes = new List<Models.Process>();
            }
            var process = new Process()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = application,
                    Arguments = String.Format(arguments),
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                }
            };

            var newProcess = new Models.Process()
            {
                Id = _processes.Count,
                Arguments = arguments,
                OrginProcess = process,
                Application = application,
                IsRunning = false,
                AutoRestart = autoRestart
            };

            _processes.Add(newProcess);

            StoreProcess(newProcess);

            return newProcess.Id;
        }

        public bool Stop(int id)
        {
            foreach (var p in _processes)
            {
                if (p.Id == id)
                {
                    try
                    {
                        p.Stop();
                        return true;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        LogHelper.WriteTo(p.Id + ".error", e.ToString());
                        return false;
                    }
                }
            }
            return false;
        }

        public bool Remove(int id)
        {
            foreach (var p in _processes)
            {
                if (p.Id == id)
                {
                    try
                    {
                        p.AutoRestart = false;
                        p.Stop();
                        _processes.Remove(p);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        LogHelper.WriteTo(p.Id + ".error", e.ToString());
                        return false;
                    }
                    //rewrite database
                    StoreProcess(p);
                    return true;
                }
            }
            return false;
        }

        private void StoreProcess(Models.Process p)
        {
            var l = new List<ProcessModels.ProcessForAddToFile>
            {
                new ProcessModels.ProcessForAddToFile()
                {
                    Id = p.Id,
                    Application = p.Application,
                    Arguments = p.Arguments,
                    AutoRestart = p.AutoRestart
                }
            };

            using (var file = File.CreateText(filePath))
            {
                var serializer = new JsonSerializer();
                //serialize object directly into file stream
                serializer.Serialize(file, l);
            }
        }
    }
}
