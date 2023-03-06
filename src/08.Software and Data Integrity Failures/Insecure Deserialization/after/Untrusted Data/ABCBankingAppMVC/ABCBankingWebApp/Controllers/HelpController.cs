using ABCBankingWebApp.Models;
using ABCBankingWebApp.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace ABCBankingWebApp.Controllers
{
    public class HelpController : Controller
    {
        IKnowledgebaseService _knowledgebaseService;
        public HelpController(IKnowledgebaseService knowledgebaseService)
        {
            _knowledgebaseService = knowledgebaseService;
        }

        [HttpGet]
        public IActionResult Index() => View(new List<Knowledge>());


        [HttpPost]
        public IActionResult Index([FromForm] string SearchString)
        {
            var knowledgeModel = new List<Knowledge>();

            if (!string.IsNullOrEmpty(SearchString))
            {
                knowledgeModel = _knowledgebaseService.Search(SearchString);
            }
            ViewData["SearchString"] = SearchString;
            return View(knowledgeModel);
        }
    }
}
