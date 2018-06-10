using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProcessManager.Models
{
    public class Process
    {
        public int Id { get; set; }
        public string Application { get; set; }
        public string Arguments { get; set; }
        public System.Diagnostics.Process OrginProcess { get; set; }
    }
}
