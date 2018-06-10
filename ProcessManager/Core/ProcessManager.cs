using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ProcessManager;
using ProcessManager;

namespace ProcessManager.Core
{
    public class ProcessManager
    {
        private static List<Models.Process> _processes = new List<Models.Process>();
        private static string filePath;
        public ProcessManager(string fileName)
        {
            var rootPath = Directory.GetCurrentDirectory();
            filePath = rootPath + "/" + fileName;

            if (!File.Exists(filePath))
            {
                File.CreateText(filePath).Close();
            }
        }

        public List<Models.Process> GetAll()
        {
            return _processes;
        }
        public void RunAll()
        {
            using (var f = new StreamReader(filePath))
            {
                var json = f.ReadToEnd();
                _processes = JsonConvert.DeserializeObject<List<Models.Process>>(json);
            }

            if (_processes == null || !_processes.Any()) return;

            foreach (var p in _processes)
            {
                p.OrginProcess = new Process()
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = p.Application,
                        Arguments = String.Format(p.Arguments),
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                    }
                };
                try
                {
                    p.OrginProcess.Start();
                    p.OrginProcess.OutputDataReceived += ((sender, e) =>
                    {
                        // Prepend line numbers to each line of the output.
                        if (!String.IsNullOrEmpty(e.Data))
                        {
                            LogHelper.WriteTo(p.Id +".output", "\n" + e.Data);
                        }
                    });
                    p.OrginProcess.ErrorDataReceived += ((sender, e) =>
                    {
                        // Prepend line numbers to each line of the output.
                        if (!String.IsNullOrEmpty(e.Data))
                        {
                            LogHelper.WriteTo(p.Id +".error", "\n" + e.Data);
                        }
                    });
                    p.OrginProcess.BeginOutputReadLine();
                    p.OrginProcess.BeginErrorReadLine();
                    p.OrginProcess.WaitForExit();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }
        public void Run(int id)
        {
            foreach (var p in _processes)
            {
                if (p.Id == id)
                {
                    try
                    {
                        p.OrginProcess.Start();
                        p.OrginProcess.OutputDataReceived +=  OutputHandler;
                        p.OrginProcess.ErrorDataReceived += ErrortHandler;
                        p.OrginProcess.BeginOutputReadLine();
                        p.OrginProcess.BeginErrorReadLine();
                        p.OrginProcess.WaitForExit();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        throw;
                    }
                }
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
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }
            }
        }
        public int Add(string fileName, string arguments)
        {
            var process = new Process()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = fileName,//"/bin/bash",
                    Arguments = String.Format(arguments),//$"-c \"{escapedArgs}\"",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                }
            };

            var newProcess = new Models.Process()
            {
                Id = _processes.Count + 1,
                Arguments = arguments,
                OrginProcess = process
            };

            _processes.Add(newProcess);

            using (StreamWriter file = File.CreateText(filePath))
            {
                JsonSerializer serializer = new JsonSerializer();
                //serialize object directly into file stream
                serializer.Serialize(file, _processes);
            }

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
                        p.OrginProcess.Kill();
                        return true;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
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
                        p.OrginProcess.Kill();
                        _processes.Remove(p);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        return false;
                    }

                    using (StreamWriter file = File.CreateText(filePath))
                    {
                        JsonSerializer serializer = new JsonSerializer();
                        //serialize object directly into file stream
                        serializer.Serialize(file, _processes);
                    }

                    return true;
                }
            }

            return false;
        }

        private static void OutputHandler(object sendingProcess,
            DataReceivedEventArgs outLine)
        {
            if (!String.IsNullOrEmpty(outLine.Data))
            {
                Console.WriteLine(outLine.Data);
            }
        }

        private static void ErrortHandler(object sendingProcess,
            DataReceivedEventArgs outLine)
        {
            if (!String.IsNullOrEmpty(outLine.Data))
            {
                Console.WriteLine(outLine.Data);
            }
        }
    }
}
