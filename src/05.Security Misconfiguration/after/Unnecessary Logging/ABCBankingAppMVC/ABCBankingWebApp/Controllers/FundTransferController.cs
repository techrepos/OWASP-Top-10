using ABCBankingWebApp.Data;
using ABCBankingWebApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using System.Security.Claims;

namespace ABCBankingWebApp.Controllers
{
    [Authorize]
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


        [HttpGet]
        public IActionResult Create()
        {
            ViewData["drpList"] = PopulateAccountsDropDownList();

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromForm] FundTransfer fundtransfer)
        {

            ViewData["drpList"] = PopulateAccountsDropDownList();
            if (!ModelState.IsValid)
            {
                return View();
            }

            var loggedInUser = HttpContext.User;
            var customerId = loggedInUser.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier).Value;

            var emptyFundTransfer = new FundTransfer();
            emptyFundTransfer.CustomerID = customerId;

            if (await TryUpdateModelAsync<FundTransfer>(
                 emptyFundTransfer,
                 "",
                 f => f.ID, f => f.AccountFrom, f => f.AccountTo, f => f.Amount, f => f.TransactionDate, f => f.Note, f => f.CustomerID))
            {
                _context.FundTransfer.Add(emptyFundTransfer);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            
            return View();

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

        private  SelectList PopulateAccountsDropDownList()
        {
            //ViewData["CustomerID"] = 1;
            var accountsQuery = from a in _context.Account
            //where a.Customer.Id == ViewData["CustomerID"].ToString()
                                orderby a.ID
                                select a;

           return new SelectList(accountsQuery.AsNoTracking(),
                        "ID", "Name");
        }
    }
}
