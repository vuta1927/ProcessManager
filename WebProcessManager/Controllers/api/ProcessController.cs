using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebProcessManager.Core;
using WebProcessManager.Data;
using WebProcessManager.Models;

namespace WebProcessManager.Controllers.api
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProcessController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IContainerComunicate _containerComunicate;
        public ProcessController(ApplicationDbContext context, IContainerComunicate containerComunicate)
        {
            _context = context;
            _containerComunicate = containerComunicate;
        }

        public IActionResult GetProcess([FromQuery]string orderField, int skip, int take)
        {
            var all = new List<Process>();
            if (!string.IsNullOrEmpty(orderField))
            {
                if (orderField.Equals("Application"))
                {
                    all = _context.Processes.Include(x=>x.Container).OrderBy(x=>x.Application).Skip(skip).Take(take).ToList();
                }
                if (orderField.Equals("Arguments"))
                {
                    all = _context.Processes.Include(x => x.Container).OrderBy(x => x.Arguments).Skip(skip).Take(take).ToList();
                }
            }
            else
            {
                all = _context.Processes.Include(x => x.Container).Skip(skip).Take(take).ToList();
            }

            
            var result = new DxDataResponse()
            {
                Items = new List<ProcessModels.ProcessForView2>(), TotalCount = _context.Processes.Count()
            };
            foreach (var p in all)
            {
                result.Items.Add(new ProcessModels.ProcessForView2()
                {
                    Id = p.Id,
                    IsRunning = p.IsRunning,
                    Application = p.Application,
                    Arguments = p.Arguments,
                    AutoRestart = p.AutoRestart,
                    ContainerName = p.Container.Name + " (" + p.Container.Address+ ")"
                });
            }

            return Ok(result);
        }
    }

    public class DxDataResponse
    {
        public List<ProcessModels.ProcessForView2> Items { get; set; }
        public int TotalCount { get; set; }
    }
}