using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace ProcessManagerCore.Models
{
    public class Process
    {
        public int Id { get; set; }
        public bool IsRunning { get; set; }
        public bool AutoRestart { get; set; }
        public string Application { get; set; }
        public string Arguments { get; set; }
        public System.Diagnostics.Process OrginProcess { get; set; }

        public delegate void ProcessStartHandler(Process sender, EventArgs e);

        public event ProcessStartHandler OnStart;

        public delegate void ProcessStopHandler(Process sender, EventArgs e);

        public event ProcessStopHandler OnStop;

        public void Stop()
        {
            try
            {
                OrginProcess.Kill();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            
        }

        public AppResponse Start()
        {
            try
            {
                if (OrginProcess == null)
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
                }

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
                        LogHelper.Add(new LogMessage(){ FileName = Id+".out", Message = e.Data});
                    }
                });
                OrginProcess.ErrorDataReceived += ((sender, e) =>
                {
                    // Prepend line numbers to each line of the output.
                    if (!String.IsNullOrEmpty(e.Data))
                    {
                        LogHelper.Add(new LogMessage(){ FileName = Id+".out", Message = e.Data});
                    }
                });
                OrginProcess.BeginOutputReadLine();
                OrginProcess.BeginErrorReadLine();
                OrginProcess.EnableRaisingEvents = true;
                //OrginProcess.WaitForExit();
                OrginProcess.Exited += OrginProcessOnExited;

                return new AppResponse(false,Id.ToString());
            }
            catch (Exception e)
            {
                LogHelper.Add(new LogMessage() { FileName = Id + ".error", Message = e.ToString() });
                OnStop?.Invoke(this, new EventArgs());
                return new AppResponse(true, e.ToString());
            }
        }

        private void OrginProcessOnExited(object sender, EventArgs e)
        {
            IsRunning = false;
            OrginProcess = null;
            OnStop?.Invoke(this, new EventArgs());
            if (AutoRestart)
            {
                Task.Run(() => Start());
            }
        }
    }
}
