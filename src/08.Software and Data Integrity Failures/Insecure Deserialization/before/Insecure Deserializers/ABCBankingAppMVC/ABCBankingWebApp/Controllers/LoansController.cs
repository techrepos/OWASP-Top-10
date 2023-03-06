using ABCBankingWebApp.Data;
using ABCBankingWebApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace ABCBankingWebApp.Controllers
{
    [Authorize]
    public class LoansController : Controller
    {
        private readonly AppDbContext _context;
        private readonly HtmlEncoder _htmlEncoder;
        private readonly IWebHostEnvironment _environment;

        public LoansController(AppDbContext context, HtmlEncoder htmlEncoder,
            IWebHostEnvironment environment)
        {
            _context = context;
            _htmlEncoder = htmlEncoder;
            _environment = environment;
        }
        // GET: LoanController
        public async Task<IActionResult>  Index([FromQuery]string? SearchString)
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
                 l => l.ID, l => l.CustomerID, l => l.Amount, l => l.PeriodInMonths, 
                 l => l.TransactionDate, l => l.Note))
            {
                emptyLoan.Note = _htmlEncoder.Encode(emptyLoan.Note);
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

        [HttpGet]
        public async Task<IActionResult> Upload()
        {
            return View();
        }

        [HttpPost]
        public async Task UploadFile(IFormFile Upload)
        {
            Loan emptyLoan = null;
            var folderPath = Path.Combine(_environment.ContentRootPath, "uploads");
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
            var file = Path.Combine(folderPath, Upload.FileName);

            using (var fileStream = new FileStream(file, FileMode.Create))
            {
                await Upload.CopyToAsync(fileStream);
                BinaryFormatter formatter = new BinaryFormatter();
                fileStream.Position = 0;
                emptyLoan = (Loan)formatter.Deserialize(fileStream);
            }

            var loggedInUser = HttpContext.User;
            var customerId = loggedInUser.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier).Value;
            emptyLoan.CustomerID = customerId;
            emptyLoan.TransactionDate = DateTime.Now;

            if (await TryUpdateModelAsync<Loan>(
                emptyLoan,
                "",
                l => l.ID, l => l.CustomerID, l => l.Amount, l => l.PeriodInMonths, l => l.TransactionDate, l => l.Note))
            {
                _context.Loan.Add(emptyLoan);
                await _context.SaveChangesAsync();
            }

        }
    }
    }

