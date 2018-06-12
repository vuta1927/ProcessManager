﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProcessManager.Models
{
    public static class ProcessModels
    {
        public class ProcessForAddToFile
        {
            public int Id { get; set; }
            public string Application { get; set; }
            public string Arguments { get; set; }
            public bool AutoRestart { get; set; }
        }
        public class ProcessForAdd
        {
            public string Application { get; set; }
            public string Arguments { get; set; }
            public bool IsRunning { get; set; }
            public bool AutoRestart { get; set; }
        }
        public class ProcessForView
        {
            public int Id { get; set; }
            public string Application { get; set; }
            public string Arguments { get; set; }
            public bool IsRunning { get; set; }
        }
    }
}
