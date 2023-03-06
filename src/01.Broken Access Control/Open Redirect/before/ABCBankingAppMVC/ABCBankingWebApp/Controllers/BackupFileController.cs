using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ABCBankingWebApp.Data;
using ABCBankingWebApp.Models;
using ABCBankingWebApp.Services;

namespace ABCBankingWebApp.Controllers
{
    public class BackupFileController : Controller
    {
        private readonly AppDbContext _context;

        public BackupFileController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Backup
        public async Task<IActionResult> Index()
        {
              return View(await _context.Backup.ToListAsync());
        }

        // GET: Backup/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Backup == null)
            {
                return NotFound();
            }

            var backup = await _context.Backup
                .FirstOrDefaultAsync(m => m.ID == id);
            if (backup == null)
            {
                return NotFound();
            }

            return View(backup);
        }

        // GET: Backup/Create
        public async Task<IActionResult> CreateAsync()
        {
            
            return View();
        }

        // POST: Backup/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromForm] Backup backup)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            var emptyBackup = new Backup();

            if (await TryUpdateModelAsync<Backup>(
                emptyBackup,  
                "",
                b => b.Name, b => b.BackupDate))
            {
                emptyBackup.BackupDate = DateTime.Now;
                _context.Backup.Add(emptyBackup);
                await _context.SaveChangesAsync();

                var service = new BackupService();
                await service.BackupDB(emptyBackup.Name);

                return RedirectToAction("Index");
            }

            return View();
        }

        // GET: Backup/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Backup == null)
            {
                return NotFound();
            }

            var backup = await _context.Backup.FindAsync(id);
            if (backup == null)
            {
                return NotFound();
            }
            return View(backup);
        }

        // POST: Backup/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,Name,BackupDate")] Backup backup)
        {
            if (id != backup.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(backup);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BackupExists(backup.ID))
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
            return View(backup);
        }

        // GET: Backup/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Backup == null)
            {
                return NotFound();
            }

            var backup = await _context.Backup
                .FirstOrDefaultAsync(m => m.ID == id);
            if (backup == null)
            {
                return NotFound();
            }

            return View(backup);
        }

        // POST: Backup/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Backup == null)
            {
                return Problem("Entity set 'AppDbContext.Backup'  is null.");
            }
            var backup = await _context.Backup.FindAsync(id);
            if (backup != null)
            {
                _context.Backup.Remove(backup);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BackupExists(int id)
        {
          return _context.Backup.Any(e => e.ID == id);
        }
    }
}
