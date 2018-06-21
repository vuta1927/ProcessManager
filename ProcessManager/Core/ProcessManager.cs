using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ProcessManagerCore.Models;
using Process = System.Diagnostics.Process;

namespace ProcessManagerCore.Core
{
    public class ProcessManager
    {
        private static List<Models.Process> _processes;
        private static string filePath;

        public ProcessManager(string fileName)
        {
            var rootPath = Directory.GetCurrentDirectory();
            filePath = rootPath + "\\" + fileName;

            if (!File.Exists(filePath))
            {
                File.CreateText(filePath).Close();
            }

            using (var f = new StreamReader(filePath))
            {
                var json = f.ReadToEnd();
                _processes = JsonConvert.DeserializeObject<List<Models.Process>>(json);
                if (_processes == null)
                {
                    _processes = new List<Models.Process>();
                }

                ;
                foreach (var p in _processes)
                {
                    p.IsRunning = false;
                }
            }

            Task.Run(() => LogHelper.Start());
        }

        public Models.Process Get(int id)
        {
            foreach (var p in _processes)
            {
                if (p.Id == id)
                {
                    return p;
                }
            }

            return null;
        }

        public List<Models.Process> GetAll()
        {
            return _processes;
        }

        public AppResponse RunAll()
        {
            if (_processes == null || !_processes.Any()) return new AppResponse(true, "there are no process!");

            foreach (var p in _processes)
            {
                if (p.IsRunning || p.AutoRestart) continue;
                Task.Run(() => { p.Start(); });
            }

            return new AppResponse(false, null);
        }

        public AppResponse Run(int id)
        {
            foreach (var p in _processes)
            {
                if (p.Id == id && !p.IsRunning)
                {
                    return p.Start();
                }
            }

            return new AppResponse(true, "process not found");
        }

        public void SetAutoRestart(int id)
        {
            var p = _processes.SingleOrDefault(x => x.Id == id);
            if (p == null) return;
            p.AutoRestart = true;
        }

        public AppResponse EndAll()
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
                        LogHelper.Add(new LogMessage() {FileName = p.Id + ".error", Message = e.ToString()});
                        return new AppResponse(true, e.ToString());
                    }
                }
            }

            return new AppResponse(false, null);
        }

        public AppResponse Add(int id, string application, string arguments, bool autoRestart)
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
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                }
            };

            var newProcess = new Models.Process()
            {
                Id = id,
                Arguments = arguments,
                OrginProcess = process,
                Application = application,
                IsRunning = false,
                AutoRestart = autoRestart
            };

            _processes.Add(newProcess);

            ReloadProcessFile();

            return new AppResponse(false, "code: 200");
        }

        public AppResponse Sync(ProcessModels.ProcessForAdd processForSync)
        {
            var p = _processes.SingleOrDefault(x => x.Id == processForSync.Id);
            if (p != null)
                return new AppResponse(false, "exsit");

            return Add(processForSync.Id, processForSync.Application, processForSync.Arguments,
                processForSync.AutoRestart);
        }

        public AppResponse Update(int id, string application, string arguments, bool autoRestart, bool isRunning)
        {
            var process = _processes.SingleOrDefault(x => x.Id == id);
            if (process == null) return new AppResponse(true, "process not found!");

            if(process.IsRunning)
                process.Stop();
            
            process.Application = application;
            process.Arguments = arguments;
            process.AutoRestart = autoRestart;
            process.IsRunning = isRunning;

            ReloadProcessFile();

            return new AppResponse(false, "code: 200");
        }

        public AppResponse Stop(int id)
        {
            foreach (var p in _processes)
            {
                if (p.Id == id)
                {
                    try
                    {
                        p.Stop();
                        return new AppResponse(false, null);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        LogHelper.Add(new LogMessage() {FileName = p.Id + ".error", Message = e.ToString()});
                        return new AppResponse(true, e.ToString());
                    }
                }
            }

            return new AppResponse(false, "process not found!");
        }

        public AppResponse Remove(int id)
        {
            foreach (var p in _processes)
            {
                if (p.Id == id)
                {
                    try
                    {
                        LogHelper.Stop();
                        p.AutoRestart = false;
                        p.Stop();
                        _processes.Remove(p);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        LogHelper.Add(new LogMessage() {FileName = p.Id + ".out", Message = e.ToString()});
                        return new AppResponse(true, e.ToString());
                    }

                    //rewrite database
                    ReloadProcessFile();
                    return new AppResponse(false, null);
                }
            }

            return new AppResponse(true, "process not found!");
        }

        private void ReloadProcessFile()
        {
            var l = new List<ProcessModels.ProcessForAddToFile>();
            foreach (var p in _processes)
            {
                l.Add(new ProcessModels.ProcessForAddToFile()
                {
                    Id = p.Id,
                    Application = p.Application,
                    Arguments = p.Arguments,
                    AutoRestart = p.AutoRestart
                });
            }

            using (var file = File.CreateText(filePath))
            {
                var serializer = new JsonSerializer();
                //serialize object directly into file stream
                serializer.Serialize(file, l);
            }
        }
    }
}