using System;
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

            int count = 0;
            foreach (var p in _processes)
            {
                if (p.IsRunning) continue;

                p.OrginProcess = new Process()
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = p.Application,
                        Arguments = String.Format(p.Arguments),
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                    }
                };
                p.Id = count;
                var t = Task.Run(() => { ProcessStart(p.Id); });
                count++;
            }
        }
        public void Run(int id)
        {
            foreach (var p in _processes)
            {
                if (p.Id == id && !p.IsRunning)
                {
                    var t = Task.Run(() => { ProcessStart(p.Id); });
                    return;
                }
            }
        }

        private void ProcessStart(int id)
        {
            try
            {
                var p = _processes.Find(x => x.Id == id);
                if (p == null) return;

                p.OrginProcess = new Process()
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = p.Application,
                        Arguments = String.Format(p.Arguments),
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                    }
                };

                p.IsRunning = p.OrginProcess.Start();
                p.OrginProcess.OutputDataReceived += ((sender, e) =>
                {
                    // Prepend line numbers to each line of the output.
                    if (!String.IsNullOrEmpty(e.Data))
                    {
                        LogHelper.WriteTo(p.Id + ".out", e.Data);
                    }
                });
                p.OrginProcess.ErrorDataReceived += ((sender, e) =>
                {
                    // Prepend line numbers to each line of the output.
                    if (!String.IsNullOrEmpty(e.Data))
                    {
                        LogHelper.WriteTo(p.Id + ".error", e.Data);
                    }
                });
                p.OrginProcess.BeginOutputReadLine();
                p.OrginProcess.BeginErrorReadLine();
                p.OrginProcess.WaitForExit();

                Task.Run(() =>
                {
                    while (p.OrginProcess != null && !p.OrginProcess.HasExited)
                    {
                        Thread.Sleep(500);
                    }

                    p.IsRunning = false;
                });

                if(p.AutoRestart) Task.Run(() => { SetAutoRestart(p.Id); });
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                LogHelper.WriteTo(id + ".error", e.ToString());
            }
        }

        public void SetAutoRestart(int id)
        {
            var p = _processes.SingleOrDefault(x => x.Id == id);
            if (p == null) return;

            while (p.AutoRestart)
            {
                if (p.IsRunning)
                {
                    Thread.Sleep(1000);
                    continue;
                }
                Task.Run(() => { ProcessStart(p.Id); });
                break;
            }
        }

        public void EndAll()
        {
            if (_processes.Any())
            {
                foreach (var p in _processes)
                {
                    try
                    {
                        p.OrginProcess.Kill();
                        p.IsRunning = false;
                        p.OrginProcess = null;
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

        private List<ProcessModels.ProcessForAdd> Clone()
        {
            var result = new List<Models.ProcessModels.ProcessForAdd>();
            foreach (var p in _processes)
            {
                var newP = new Models.ProcessModels.ProcessForAdd()
                {
                    Application = p.Application,
                    Arguments = p.Arguments,
                    IsRunning = p.IsRunning
                };
                result.Add(newP);
            }

            return result;
        }

        public bool Stop(int id)
        {
            foreach (var p in _processes)
            {
                if (p.Id == id)
                {
                    try
                    {
                        p.OrginProcess.Kill();
                        p.OrginProcess = null;
                        p.IsRunning = false;
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
                        p.OrginProcess.Kill();
                        _processes.Remove(p);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        LogHelper.WriteTo(p.Id + ".error", e.ToString());
                        return false;
                    }

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
