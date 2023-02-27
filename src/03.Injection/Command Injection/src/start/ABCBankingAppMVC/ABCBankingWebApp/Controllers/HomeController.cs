using ABCBankingWebApp.Data;
using ABCBankingWebApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Diagnostics;

namespace ABCBankingWebApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IConfiguration Configuration;
        private readonly AppDbContext _context;
        public HomeController(ILogger<HomeController> logger, 
            IConfiguration configuration,
            AppDbContext context)
        {
            _logger = logger;
            _context = context;
            Configuration = configuration;
        }

        public async Task<IActionResult> IndexAsync()
        {
           
            return View();
        }
        [Route("Home/SearchString:string")]
        public async Task<IActionResult> FundTransfersAsync(string SearchString)
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
            model.FundTransfer= FundTransfer;
            return View(model);
        }
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}