using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebProcessManager.Core;
using WebProcessManager.Data;
using WebProcessManager.Models;

namespace WebProcessManager.Controllers
{
    public class ProcessesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IContainerComunicate _containerComunicate;
        public ProcessesController(ApplicationDbContext context, IContainerComunicate containerComunicate)
        {
            _context = context;
            _containerComunicate = containerComunicate;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult GetProcesses([FromQuery]string orderby, int skip, int take)
        {
            var all = new List<Process>();
            if (!string.IsNullOrEmpty(orderby))
            {
                if (orderby.Equals("application desc"))
                {
                    all = _context.Processes.Include(x => x.Container).OrderByDescending(x => x.Application).Skip(skip).Take(take).ToList();
                }
                if (orderby.Equals("application"))
                {
                    all = _context.Processes.Include(x => x.Container).OrderBy(x => x.Application).Skip(skip).Take(take).ToList();
                }
                if (orderby.Equals("arguments desc"))
                {
                    all = _context.Processes.Include(x => x.Container).OrderByDescending(x => x.Arguments).Skip(skip).Take(take).ToList();
                }
                if (orderby.Equals("arguments"))
                {
                    all = _context.Processes.Include(x => x.Container).OrderBy(x => x.Arguments).Skip(skip).Take(take).ToList();
                }
            }
            else
            {
                all = _context.Processes.Include(x => x.Container).Skip(skip).Take(take).ToList();
            }


            var result = new DxProcessDataResponse()
            {
                Items = new List<ProcessModels.ProcessForView2>(),
                TotalCount = _context.Processes.Count()
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
                    ContainerName = p.Container.Name
                });
            }

            return Ok(result);
        }

        // GET: Processes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var process = await _context.Processes
                .Include(p => p.Container)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (process == null)
            {
                return NotFound();
            }

            var dataOutput = await _containerComunicate.GetLogsAsync(process.Id, new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day - 1), DateTime.Now);
            var ouput = "";
            if (dataOutput != null)
            {
                for (var i = dataOutput.Count - 1; i >= 0; i--)
                {
                    ouput += "[" + dataOutput[i].Time +"]:" + dataOutput[i].Message + "\r\n";
                }
            }

            var dataError = await _containerComunicate.GetErrorLogsAsync(process.Id, new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day - 1), DateTime.Now);
            var errors = "";
            if (dataError != null)
            {
                for (var i = dataError.Count - 1; i >= 0; i--)
                {
                    errors += "[" + dataError[i].Time + "]:" + dataError[i].Message + "\r\n";
                }
            }


            var pForView = new ProcessModels.ProcessForView()
            {
                Id = process.Id,
                IsRunning = process.IsRunning,
                Application = process.Application,
                AutoRestart = process.AutoRestart,
                Arguments = process.Arguments,
                Container = process.Container,
                ContainerId = process.ContainerId,
                Output = ouput,
                Errors = errors
            };

            return View(pForView);
        }

        [HttpPost]
        [ActionName("GetLog")]
        public async Task<IActionResult> GetLog([FromRoute] int id, [FromBody] FormGetLog data)
        {
            var process = await _context.Processes
                .Include(p => p.Container)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (process == null)
            {
                return NotFound();
            }

            var dataOutput = await _containerComunicate.GetLogsAsync(process.Id, data.From, data.To);
            var output = "";
            if (dataOutput != null)
            {
                for (var i = dataOutput.Count - 1; i >= 0; i--)
                {
                    output += "[" + dataOutput[i].Time + "]:" + dataOutput[i].Message + "\r\n";
                }
            }

            return Ok(output);
        }

        [HttpPost]
        public async Task<IActionResult> GetError([FromRoute] int id, [FromBody] FormGetLog data)
        {
            var process = await _context.Processes
                .Include(p => p.Container)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (process == null)
            {
                return NotFound();
            }

            var dataOutput = await _containerComunicate.GetErrorLogsAsync(process.Id, data.From, data.To);
            var output = "";
            if (dataOutput != null)
            {
                for (var i = dataOutput.Count - 1; i >= 0; i--)
                {
                    output += "[" + dataOutput[i].Time + "]:" + dataOutput[i].Message + "\r\n";
                }
            }

            return Ok(output);
        }
        // GET: Processes/Create
        public IActionResult Create()
        {
            ViewData["ContainerId"] = new SelectList(_context.Containers, "Id", "Id");
            return View();
        }

        public async Task<IActionResult> Start(int id)
        {
            var process = _context.Processes.Include(x => x.Container).SingleOrDefault(x => x.Id == id);
            if (process == null) ViewData["error"] = "Process not found!";

            var result = await _containerComunicate.StartAsync(process);
            if (result.IsError)
            {
                process.IsRunning = false;
                return Ok(result.Message);
            }
            process.IsRunning = true;
            await _context.SaveChangesAsync();

            return Ok();
        }
        public async Task<IActionResult> Stop(int id)
        {
            var process = _context.Processes.Include(x => x.Container).SingleOrDefault(x => x.Id == id);
            if (process == null) return NotFound();

            var result = await _containerComunicate.StopAsync(process);
            if (result.IsError)
            {
                return Ok(result.Message);
            }

            process.IsRunning = false;
            await _context.SaveChangesAsync();
            return Ok();
        }
        public async Task<IActionResult> Sync(int id)
        {
            var process = _context.Processes.Include(x => x.Container).SingleOrDefault(x => x.Id == id);
            if (process == null) return NotFound();

            var result = await _containerComunicate.Sync(process);
            if (result.IsError)
            {
                return Ok(result.Message);
            }
            else
            {
                process.IsRunning = false;
                await _context.SaveChangesAsync();
            }

            return Ok();
        }

        public async Task<IActionResult> GetLog(int id, DateTime from, DateTime to)
        {
            var process = _context.Processes.Include(x => x.Container).SingleOrDefault(x => x.Id == id);
            if (process == null) return NotFound();

            var result = await _containerComunicate.GetLogsAsync(id, from, to);
            if (result == null)
            {
                ViewData["error"] = "No log";
            }
            else
            {
                process.IsRunning = false;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // POST: Processes/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,AutoRestart,Application,Arguments,ContainerId")] Process process)
        {
            if (ModelState.IsValid)
            {
                process.IsRunning = false;
                _context.Add(process);
                await _context.SaveChangesAsync();

                var result = await _containerComunicate.CreateProcess(process);

                if (result.IsError)
                {
                    
                }

                return RedirectToAction(nameof(Index));
            }
            ViewData["ContainerId"] = new SelectList(_context.Containers, "Id", "Id", process.ContainerId);
            return View(process);
        }

        // GET: Processes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var process = await _context.Processes.FindAsync(id);

            if (process == null)
            {
                return NotFound();
            }
            ViewData["ContainerId"] = new SelectList(_context.Containers, "Id", "Id", process.ContainerId);
            return View(process);
        }

        // POST: Processes/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,AutoRestart,Application,Arguments,ContainerId")] Process process)
        {
            if (id != process.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var result = await _containerComunicate.EditAsync(process);
                    if (result.IsError)
                    {
                        ViewData["error"] = result.Message;
                        return RedirectToAction(nameof(Index));
                    }

                    _context.Update(process);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProcessExists(process.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["ContainerId"] = new SelectList(_context.Containers, "Id", "Id", process.ContainerId);
            return View(process);
        }

        // GET: Processes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var process = await _context.Processes
                .Include(p => p.Container)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (process == null)
            {
                return NotFound();
            }

            return View(process);
        }

        // POST: Processes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var result = await _containerComunicate.RemoveASync(id);
            if (result.IsError)
            {
                ViewData["error"] = result.Message;
                return RedirectToAction(nameof(Index));
            }

            var process = await _context.Processes.FindAsync(id);
            _context.Processes.Remove(process);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProcessExists(int id)
        {
            return _context.Processes.Any(e => e.Id == id);
        }
    }
    public class DxProcessDataResponse
    {
        public List<ProcessModels.ProcessForView2> Items { get; set; }
        public int TotalCount { get; set; }
    }
}
