using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebProcessManager.Data;
using WebProcessManager.Models;

namespace WebProcessManager.Controllers
{
    public class ContainersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ContainersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Containers
        public async Task<IActionResult> Index()
        {
            return View(await _context.Containers.ToListAsync());
        }

        public IActionResult GetContainers([FromQuery]string orderby, int skip, int take = 12)
        {
            var all = new List<Container>();
            if (!string.IsNullOrEmpty(orderby))
            {
                if (orderby.Equals("name"))
                {
                    all = _context.Containers.OrderBy(x => x.Name).Skip(skip).Take(take).ToList();
                }
                if (orderby.Equals("name desc"))
                {
                    all = _context.Containers.OrderByDescending(x => x.Name).Skip(skip).Take(take).ToList();
                }
                if (orderby.Equals("address"))
                {
                    all = _context.Containers.OrderBy(x => x.Address).Skip(skip).Take(take).ToList();
                }

                if (orderby.Equals("address desc"))
                {
                    all = _context.Containers.OrderByDescending(x => x.Address).Skip(skip).Take(take).ToList();
                }
            }
            else
            {
                all = _context.Containers.Skip(skip).Take(take).ToList();
            }

            return Ok(new DxDataResponse()
            {
                Items = all,
                TotalCount = _context.Containers.Count()
            });
        }
        // GET: Containers/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var container = await _context.Containers
                .FirstOrDefaultAsync(m => m.Id == id);
            if (container == null)
            {
                return NotFound();
            }

            return View(container);
        }

        // GET: Containers/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Containers/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Address")] Container container)
        {
            if (ModelState.IsValid)
            {
                _context.Add(container);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(container);
        }

        // GET: Containers/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var container = await _context.Containers.FindAsync(id);
            if (container == null)
            {
                return NotFound();
            }
            return View(container);
        }

        // POST: Containers/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Address")] Container container)
        {
            if (id != container.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(container);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ContainerExists(container.Id))
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
            return View(container);
        }

        // GET: Containers/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var container = await _context.Containers
                .FirstOrDefaultAsync(m => m.Id == id);
            if (container == null)
            {
                return NotFound();
            }

            return View(container);
        }

        // POST: Containers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var container = await _context.Containers.FindAsync(id);
            _context.Containers.Remove(container);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ContainerExists(int id)
        {
            return _context.Containers.Any(e => e.Id == id);
        }
    }

    public class DxDataResponse
    {
        public List<Container> Items { get; set; }
        public int TotalCount { get; set; }
    }
}
