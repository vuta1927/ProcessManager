using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebProcessManager.Models
{
    public class Container
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public List<Process> Processes { get; set; }
    }
}
