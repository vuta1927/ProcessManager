using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ProcessManager.Models
{
    public class Process
    {
        public int Id { get; set; }

        private bool _isRunning;
        public bool IsRunning
        {
            get => _isRunning;
            set
            {
                _isRunning = value;
                if (!_isRunning)
                {
                    OrginProcess = null;
                    if (AutoRestart)
                    {
                        Task.Run(()=>Start());
                    }
                }
                OnStop?.Invoke(this, new EventArgs());
            }
        }

        public bool AutoRestart { get; set; }
        public string Application { get; set; }
        public string Arguments { get; set; }
        public System.Diagnostics.Process OrginProcess { get; set; }

        public delegate void ProcessStartHandler(object sender, EventArgs e);

        public event ProcessStartHandler OnStart;

        public delegate void ProcessStopHandler(object sender, EventArgs e);

        public event ProcessStopHandler OnStop;

        public void Stop()
        {
            OrginProcess.Kill();
            IsRunning = false;
            OrginProcess = null;
            OnStop?.Invoke(this, new EventArgs());
        }

        public void Start()
        {
            try
            {
                OrginProcess = new System.Diagnostics.Process()
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = Application,
                        Arguments = String.Format(Arguments),
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                    }
                };

                IsRunning = OrginProcess.Start();
                if (IsRunning)
                {
                    OnStart?.Invoke(this, new EventArgs());
                }

                OrginProcess.OutputDataReceived += ((sender, e) =>
                {
                    // Prepend line numbers to each line of the output.
                    if (!String.IsNullOrEmpty(e.Data))
                    {
                        LogHelper.WriteTo(Id + ".out", e.Data);
                    }
                });
                OrginProcess.ErrorDataReceived += ((sender, e) =>
                {
                    // Prepend line numbers to each line of the output.
                    if (!String.IsNullOrEmpty(e.Data))
                    {
                        LogHelper.WriteTo(Id + ".error", e.Data);
                    }
                });
                OrginProcess.BeginOutputReadLine();
                OrginProcess.BeginErrorReadLine();
                OrginProcess.WaitForExit();

                Task.Run(() =>
                {
                    while (OrginProcess != null && !OrginProcess.HasExited && OrginProcess.Responding)
                    {
                        Thread.Sleep(500);
                    }
                    IsRunning = false;
                });

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                LogHelper.WriteTo(Id + ".error", e.ToString());
            }
        }
    }
}
