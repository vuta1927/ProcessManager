using ProcessManagerCore;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Threading.Tasks;

namespace WebProcessManager.Models
{
    public class Process
    {
        public int Id { get; set; }
        public bool IsRunning { get; set; }
        public bool AutoRestart { get; set; }
        public string Application { get; set; }
        public string Arguments { get; set; }
        public int ContainerId { get; set; }
        public virtual Container Container { get; set; }
    }
}
