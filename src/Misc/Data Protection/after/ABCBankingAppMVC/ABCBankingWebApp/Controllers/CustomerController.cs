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
        public CustomerController(AppDbContext context)
        {
            _context = context;

        }
        public async Task<IActionResult> Index()
        {
            

            var model = await _context.Customer.ToListAsync();
            return View(model);
        }
    }
}
