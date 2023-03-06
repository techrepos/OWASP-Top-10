using ABCBankingWebApp.Data;
using ABCBankingWebApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ABCBankingWebApp.Controllers
{
    public class FundTransferController : Controller
    {

        private readonly AppDbContext _context;
        protected IAuthorizationService _authorizationService { get; }
        protected UserManager<Customer> _userManager { get; }

        public FundTransferController(AppDbContext context,
              IAuthorizationService authorizationService,
                            UserManager<Customer> userManager)
        {
            _context = context;
            _userManager = userManager;
            _authorizationService = authorizationService;
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

        public async Task<IActionResult> Details(Guid? id)
        {
            FundTransfer fundTransfer = new();
            if (id == null)
            {
                return NotFound();
            }
            if (!User.Identity.IsAuthenticated)
            {
                return Challenge();
            }
            fundTransfer = await _context.FundTransfer
                                .Where(f => f.ID == id)
                                .Include(f => f.Customer)
                                .OrderBy(f => f.TransactionDate)
                                .FirstOrDefaultAsync<FundTransfer>();

            var isAuthorized = await _authorizationService.AuthorizeAsync(
                                                   User, fundTransfer,
                                                   "Owner");
            if (!isAuthorized.Succeeded)
            {
                return Forbid();
            }

            if (fundTransfer == null)
            {
                return NotFound();
            }

            return View(fundTransfer);
        }
    }
}
