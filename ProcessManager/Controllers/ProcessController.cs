using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProcessManagerCore.Core;
using ProcessManagerCore.Models;

namespace ProcessManagerCore.Controllers
{
    [Authorize]
    [Produces("application/json")]
    public class ProcessController : Controller
    {
        private IServerCommunicate _serverCommunicate;
        public ProcessController(IServerCommunicate communicate)
        {
            _serverCommunicate = communicate;
        }
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
        [Route("api/process/sync")]
        public IActionResult Sync([FromBody] ProcessModels.ProcessForAdd p)
        {
            var result = Program.PManager.Sync(p);
            if (result.Message.Equals("exsit"))
            {
                return Ok(result);
            }

            var b = Program.PManager.GetAll().SingleOrDefault(x => x.Id == p.Id);
            b.OnStop += _serverCommunicate.ProcessOnStopHandler;
            b.OnStart += _serverCommunicate.ProcessOnStartHandler;

            return Ok(result);
        }

        [HttpPost]
        [Route("api/process/")]
        public IActionResult Add([FromBody] ProcessModels.ProcessForAdd p)
        {
            var result = Program.PManager.Add(p.Id, p.Application, p.Arguments, p.AutoRestart);
            var newP = Program.PManager.Get(p.Id);
            newP.OnStart += _serverCommunicate.ProcessOnStartHandler;
            newP.OnStop += _serverCommunicate.ProcessOnStopHandler;
            return Ok(result);
        }

        [HttpPut]
        [Route("api/process/")]
        public IActionResult Update([FromBody] ProcessModels.ProcessForAdd p)
        {
            var result = Program.PManager.Update(p.Id, p.Application, p.Arguments, p.AutoRestart, p.IsRunning);
            return Ok(result);
        }

        [HttpPost]
        [Route("api/process/run/{id}")]
        public IActionResult Run([FromRoute] int id)
        {
            return Ok(Program.PManager.Run(id));
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
        [Route("api/process/stopall")]
        public IActionResult StopAll()
        {
            return Ok(Program.PManager.EndAll());
        }
        [HttpPost]
        [Route("api/process/startall")]
        public IActionResult StartAll()
        {
            return Ok(Program.PManager.RunAll());
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