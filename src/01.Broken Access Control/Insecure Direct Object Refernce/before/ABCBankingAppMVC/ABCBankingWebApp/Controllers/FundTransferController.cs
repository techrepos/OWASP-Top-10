using ABCBankingWebApp.Data;
using ABCBankingWebApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ABCBankingWebApp.Controllers
{
    public class FundTransferController : Controller
    {

        private readonly AppDbContext _context;

        public FundTransferController(AppDbContext context)
        {
            _context = context;
        }
        
        public async Task<IActionResult> Index(string? SearchString)
        {
            IList<FundTransfer> FundTransfer = new List<FundTransfer>();

            var fundtransfer = from f in _context.FundTransfer
                               select f;

            if (!string.IsNullOrEmpty(SearchString))
            {
                fundtransfer = _context.FundTransfer.FromSqlRaw("Select * from FundTransfer Where Note Like'%" + SearchString + "%'");
            }

            FundTransfer = await fundtransfer.ToListAsync();
            IndexModel model = new();
            model.SearchString = SearchString;
            model.FundTransfer = FundTransfer;
            return View(model);
        }

        public async Task<IActionResult> Details(int? id)
        {
            FundTransfer fundTransfer = new();
            if (id == null)
            {
                return NotFound();
            }

           fundTransfer = await _context.FundTransfer
                                .Where(f => f.ID == id)
                                .Include(f => f.Customer)
                                .OrderBy(f => f.TransactionDate)
                                .FirstOrDefaultAsync<FundTransfer>();

            if (fundTransfer == null)
            {
                return NotFound();
            }

            return View(fundTransfer);
        }
    }
}
