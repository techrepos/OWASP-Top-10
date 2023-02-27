using ABCBankingWebApp.Data;
using ABCBankingWebApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Diagnostics;

namespace ABCBankingWebApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IConfiguration Configuration;
        private readonly FundTransferDAL fundtransferDAL;
        public HomeController(ILogger<HomeController> logger, IConfiguration configuration)
        {
            _logger = logger;
            fundtransferDAL = new FundTransferDAL(configuration);
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
            if (!string.IsNullOrEmpty(SearchString))
            {   
                FundTransfer = await Task.FromResult(fundtransferDAL.GetFundTransfers(SearchString).ToList());
            }
            else
            {
                FundTransfer = await Task.FromResult(fundtransferDAL.GetFundTransfers().ToList());
            }
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