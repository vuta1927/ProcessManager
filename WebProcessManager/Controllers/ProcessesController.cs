﻿using System;
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

        // GET: Processes
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Processes;
            return View(await applicationDbContext.ToListAsync());
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

            var dataOutput = await _containerComunicate.GetLogsAsync(process.Id);
            var ouput = "";
            if (dataOutput != null)
            {
                for (var i = dataOutput.Length - 1; i >= 0; i--)
                {
                    ouput += dataOutput[i] + "\r\n";
                }
            }

            var dataError = await _containerComunicate.GetErrorLogsAsync(process.Id);
            var errors = "";
            if (dataError != null)
            {
                for (var i = dataError.Length - 1; i >= 0; i--)
                {
                    errors += dataError[i] + "\r\n";
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
                ViewData["error"] = result.Message;
            }
            else
            {
                process.IsRunning = true;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Stop(int id)
        {
            var process = _context.Processes.Include(x => x.Container).SingleOrDefault(x => x.Id == id);
            if (process == null) return NotFound();

            var result = await _containerComunicate.StopAsync(process);
            if (result.IsError)
            {
                ViewData["error"] = result.Message;
            }
            else
            {
                process.IsRunning = false;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Sync(int id)
        {
            var process = _context.Processes.Include(x => x.Container).SingleOrDefault(x => x.Id == id);
            if (process == null) return NotFound();

            var result = await _containerComunicate.Sync(process);
            if (result.IsError)
            {
                ViewData["error"] = result.Message;
            }
            else
            {
                process.IsRunning = false;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> GetLog(int id)
        {
            var process = _context.Processes.Include(x => x.Container).SingleOrDefault(x => x.Id == id);
            if (process == null) return NotFound();

            var result = await _containerComunicate.GetLogsAsync(id);
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
                var result = await _containerComunicate.CreateProcess(process);
                if (result.IsError)
                {
                    ViewData["error"] = result.Message;
                }
                else
                {
                    process.IsRunning = true;
                    _context.Add(process);
                    await _context.SaveChangesAsync();
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
}
