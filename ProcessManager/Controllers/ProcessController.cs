using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProcessManager.Models;

namespace ProcessManager.Controllers
{
    [Produces("application/json")]
    public class ProcessController : Controller
    {
        [HttpGet]
        [Route("api/process/")]
        public IActionResult GetAll()
        {
            var result = new List<ProcessModels.ProcessForView>();
            var data = Program.PManager.GetAll();
            foreach (var d in data)
            {
                result.Add(new ProcessModels.ProcessForView()
                {
                    Application = d.Application,
                    Arguments = d.Arguments,
                    Id = d.Id,
                    IsRunning = d.IsRunning
                });
            }
            return Ok(result);
        }

        [HttpGet]
        [Route("api/process/log/{id}")]
        public IActionResult GetLog([FromRoute] int id)
        {
            var result = LogHelper.GetLog(id + ".out");
            return Ok(result);
        }
        [HttpGet]
        [Route("api/process/error/{id}")]
        public IActionResult GetErrorLog([FromRoute] int id)
        {
            var result = LogHelper.GetLog(id + ".error");
            return Ok(result);
        }

        [HttpPost]
        [Route("api/process/")]
        public IActionResult Add([FromBody] ProcessModels.ProcessForAdd p)
        {
            var id = Program.PManager.Add(p.Application, p.Arguments, p.AutoRestart);
            return Ok(id);
        }

        [HttpPost]
        [Route("api/process/run/{id}")]
        public IActionResult Run([FromRoute] int id)
        {
            Program.PManager.Run(id);
            return Ok();
        }
        [HttpPost]
        [Route("api/process/offautorestart/{id}")]
        public IActionResult DisableAutoRestart([FromRoute] int id)
        {
            var ps = Program.PManager.GetAll();
            if (ps.Any())
            {
                var p = ps.SingleOrDefault(x => x.Id == id);
                if (p != null)
                {
                    p.AutoRestart = false;
                }
            }

            return Ok();
        }

        [HttpPost]
        [Route("api/process/autorestart/{id}")]
        public IActionResult SetAutoRestart([FromRoute] int id)
        {
            var ps = Program.PManager.GetAll();
            if (!ps.Any()) return Content("There are no process!");

            var p = ps.SingleOrDefault(x => x.Id == id);
            if (p == null) return Content("Process not found!");

            p.AutoRestart = true;
            Program.PManager.SetAutoRestart(p.Id);
            return Ok();
        }

        [HttpPost]
        [Route("api/process/addAndRun/")]
        public IActionResult AddAndRun([FromBody] ProcessModels.ProcessForAdd p)
        {
            var id = Program.PManager.Add(p.Application, p.Arguments, p.AutoRestart);
            if (id > 0)
                Program.PManager.Run(id);

            return Ok(id);
        }

        [HttpPost]
        [Route("api/process/stopall")]
        public IActionResult StopAll()
        {
            Program.PManager.EndAll();
            return Ok();
        }

        [HttpDelete]
        [Route("api/process/{id}")]
        public IActionResult Remove([FromRoute] int id)
        {
            return Ok(Program.PManager.Remove(id));
        }

        [HttpPost]
        [Route("api/process/stop/{id}")]
        public IActionResult Stop([FromRoute] int id)
        {
            return Ok(Program.PManager.Stop(id));
        }
    }
}