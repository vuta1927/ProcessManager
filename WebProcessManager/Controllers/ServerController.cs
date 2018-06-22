using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using WebProcessManager.Core;
using WebProcessManager.Data;
using WebProcessManager.Models;

namespace WebProcessManager.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ApiController]
    [Route("api/process/[action]")]
    public class ServerController : ControllerBase //This controller listen to request of all containers
    {
        private readonly ApplicationDbContext _context;
        private IContainerComunicate _containerComunicate;
        public ServerController(ApplicationDbContext dbContext, IContainerComunicate containerComunicate)
        {
            _context = dbContext;
            _containerComunicate = containerComunicate;
        }

        [HttpPost("{id}")]
        [ActionName("stopped")]
        public async Task<IActionResult> Stopped([FromRoute] int id)
        {
            var p = _context.Processes.SingleOrDefault(x => x.Id == id);
            if (p == null)
            {
                return Ok(new AppResponse(true, "Process not found!"));
            }
            else
            {
                p.IsRunning = false;
                await _context.SaveChangesAsync();
            }

            return Ok(new AppResponse(false));
        }

        [HttpPost]
        [ActionName("test")]
        public IActionResult test()
        {
            return Ok("ok");
        }

        [HttpPost("{id}")]
        [ActionName("started")]
        public async Task<IActionResult> Started([FromRoute] int id)
        {
            var p = _context.Processes.SingleOrDefault(x => x.Id == id);
            if (p == null)
            {
                return Ok(new AppResponse(true, "Process not found!"));
            }
            else
            {
                p.IsRunning = true;
                await _context.SaveChangesAsync();
            }

            return Ok(new AppResponse(false));
        }

        [HttpPost]
        [ActionName("syncall")]
        public async Task<IActionResult> SyncAll([FromBody] List<ProcessModels.ProcessForSync> processes)
        {
            var idToRemove = new Dictionary<int, int>();
            foreach (var process in processes)
            {
                var p = _context.Processes.SingleOrDefault(x => x.Id == process.Id);
                if (p == null)
                {
                    var cont = _context.Containers.SingleOrDefault(x => x.Address.Equals(process.ContainerUrl));
                    if (cont == null) return Ok(new AppResponse(true, "code: 401"));

                    var newP = new Process()
                    {
                        Application = process.Application,
                        Arguments = process.Arguments,
                        AutoRestart = process.AutoRestart,
                        IsRunning = process.IsRunning,
                        ContainerId = cont.Id,
                    };
                    
                    try
                    {
                        _context.Processes.Add(newP);
                        await _context.SaveChangesAsync();
                        idToRemove.Add(process.Id, newP.Id);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                }
            }

            return Ok(new AppResponse(false, JsonConvert.SerializeObject(idToRemove)));
        }
    }
}