using Ecorama.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ecorama.Controllers
{
    public class AboutUsController : Controller
    {
        private readonly MyDbContext _context;

        public AboutUsController(MyDbContext context)
        {
            _context = context;
        }

        // GET: AboutU
        public async Task<IActionResult> Index()
        {
            var teamMembers = await _context.TeamMembers.ToListAsync();
            var aboutUs = await _context.AboutUs.ToListAsync();

            var viewModel = new TeamViewModel
            {
                TeamMembers = teamMembers,
                AboutUs = aboutUs
            };

            return View(viewModel);

        }

        // GET: AboutU/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var aboutU = await _context.AboutUs.FirstOrDefaultAsync(m => m.Id == id);
            if (aboutU == null) return NotFound();

            return View(aboutU);
        }

        // GET: AboutU/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: AboutU/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Description,CreatedAt")] AboutU aboutU)
        {
            if (ModelState.IsValid)
            {
                _context.Add(aboutU);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(aboutU);
        }

        // GET: AboutU/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var aboutU = await _context.AboutUs.FindAsync(id);
            if (aboutU == null) return NotFound();

            return View(aboutU);
        }

        // POST: AboutU/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,CreatedAt")] AboutU aboutU)
        {
            if (id != aboutU.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(aboutU);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AboutUExists(aboutU.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(aboutU);
        }

        // GET: AboutU/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var aboutU = await _context.AboutUs.FirstOrDefaultAsync(m => m.Id == id);
            if (aboutU == null) return NotFound();

            return View(aboutU);
        }

        // POST: AboutU/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var aboutU = await _context.AboutUs.FindAsync(id);
            if (aboutU != null)
            {
                _context.AboutUs.Remove(aboutU);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool AboutUExists(int id)
        {
            return _context.AboutUs.Any(e => e.Id == id);
        }
    }
}
