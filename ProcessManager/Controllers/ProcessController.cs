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
    [Route("api/Process")]
    public class ProcessController : Controller
    {
        [HttpGet]
        [Route("api/process/")]
        public IActionResult GetAll()
        {
            var result = Program.PManager.GetAll();
            return Ok(result);
        }

        [HttpGet]
        [Route("api/process/log/{id}")]
        public IActionResult GetLog([FromRoute] int id)
        {
            var result = LogHelper.GetLog(id.ToString());
            return Ok(result);
        }

        [HttpPost]
        [Route("api/process/")]
        public IActionResult Add([FromBody] ProcessModels.ProcessForAdd p)
        {
            var id = Program.PManager.Add(p.Application, p.Arguments);
            return Ok(id);
        }

        [HttpPost]
        [Route("api/process/addAndRun/")]
        public IActionResult AddAndRun([FromBody] ProcessModels.ProcessForAdd p)
        {
            var id = Program.PManager.Add(p.Application, p.Arguments);
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
        [Route("api/process/{id}")]
        public IActionResult Stop([FromRoute] int id)
        {
            return Ok(Program.PManager.Stop(id));
        }
    }
}