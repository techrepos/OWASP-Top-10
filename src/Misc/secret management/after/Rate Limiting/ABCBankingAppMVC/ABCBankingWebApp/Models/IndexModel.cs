
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace ABCBankingWebApp.Models
{
    public class IndexModel
    {
        public IList<FundTransfer> FundTransfer { get; set; }


        public string SearchString { get; set; }


    }
}
