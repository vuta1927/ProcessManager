using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProcessManager.Models
{
    public static class ProcessModels
    {
        public class ProcessForAdd
        {
            public string Application { get; set; }
            public string Arguments { get; set; }
        }
    }
}
