using ABCBankingWebApp.Data;
using ABCBankingWebApp.Models;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ABCBankingWebApp.Controllers
{
    public class CustomerController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IDataProtector _dataProtector;
        public CustomerController(AppDbContext context, IDataProtectionProvider dataProtector)
        {
            _context = context;
            _dataProtector = dataProtector.CreateProtector("ABCBankingWebApp.Customers");
        }
        public async Task<IActionResult> Index()
        {
            foreach (var cust in _context.Customer)
            {
                cust.EncCustomerID = _dataProtector.Protect(cust.Id.ToString());
            }

            var model = await _context.Customer.ToListAsync();
            return View(model);
        }


        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var decID = _dataProtector.Unprotect(id);

            var model = await _context.Customer.FirstOrDefaultAsync(m => m.Id == decID);

            if (model == null)
            {
                return NotFound();
            }
            return View(model);
        }
    }
}
