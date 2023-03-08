using ABCBankingWebAppAdmin.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using SendGrid.Helpers.Mail.Model;
using System.Text;

namespace ABCBankingWebAppAdmin.Controllers
{
    
    [ApiController]
    [Route("api/[controller]")]
    public class ExportController : ControllerBase
    {

        private readonly AppDbContext _context;
        

        public ExportController(AppDbContext context)
        {
            _context = context;
            
        }

        [HttpGet]
        [Route("DownloadAsExcel")]
        public async Task<IActionResult> DownloadAsExcel([FromQuery]string customerId)
        {
            var loans = from l in _context.Loan
                        select l;
            if (!string.IsNullOrEmpty(customerId))
            {
                loans = loans.Where(l => l.CustomerID.Contains(customerId));
            }

            var model = await loans.ToListAsync();
            var modelJson = JsonConvert.SerializeObject(model,Formatting.Indented);
            //byte[] byteContent = Encoding.UTF8.GetBytes(modelJson);

            //return new FileContentResult(byteContent, "application/json")
            //{
            //    FileDownloadName = $"Loan Data-{customerId}{DateTime.Now.ToString()}"
            //};
            return new ContentResult() {  ContentType ="text/plain", Content = modelJson};
        }
    }
}
