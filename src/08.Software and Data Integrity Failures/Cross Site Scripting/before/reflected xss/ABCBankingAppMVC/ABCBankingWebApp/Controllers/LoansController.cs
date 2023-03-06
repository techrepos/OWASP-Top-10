using ABCBankingWebApp.Data;
using ABCBankingWebApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ABCBankingWebApp.Controllers
{
    [Authorize]
    public class LoansController : Controller
    {
        private readonly AppDbContext _context;
        public LoansController(AppDbContext context)
        {
            _context = context;
        }
        // GET: LoanController
        public async Task<IActionResult>  Index(string? SearchString)
        {
            var loans = from l in _context.Loan
                        select l;
            if (!string.IsNullOrEmpty(SearchString))
            {
                loans = loans.Where(l => l.Note.Contains(SearchString));
            }

            var model = await loans.ToListAsync();
            ViewData["SearchString"] = SearchString;
            return View(model);
        }

        // GET: LoanController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: LoanController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: LoanController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromForm] Loan loan)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            var loggedInUser = HttpContext.User;
            var customerId = loggedInUser.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier).Value;

            var emptyLoan = new Loan
            {
                CustomerID = customerId,
                TransactionDate = DateTime.Now
            };

            if (await TryUpdateModelAsync<Loan>(
                 emptyLoan,
                 "",
                 l => l.ID, l => l.CustomerID, l => l.Amount, l => l.PeriodInMonths, l => l.TransactionDate, l => l.Note))
            {
                _context.Loan.Add(emptyLoan);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index");
            
        }

        // GET: LoanController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: LoanController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        [HttpGet]
        // GET: LoanController/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
             if (id == null)
            {
                return NotFound();
            }

            var model = await _context.Loan.FirstOrDefaultAsync(m => m.ID == id);

            if (model == null)
            {
                return NotFound();
            }
            return View(model);
        }

        // POST: LoanController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteRecord([FromForm]int? id)
        {
            try
            {
                if (id == null)
                {
                    return NotFound();
                }

                var model = await _context.Loan.FindAsync(id);

                if (model != null)
                {
                    _context.Loan.Remove(model);
                    await _context.SaveChangesAsync();
                }
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
