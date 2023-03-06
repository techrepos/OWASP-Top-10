using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using ABCBankingWebApp.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace ABCBankingWebApp.Data
{
    public class AppDbContext : IdentityDbContext<Customer>
    {
        public AppDbContext(
            DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<ABCBankingWebApp.Models.Customer> Customer { get; set; }
        public DbSet<ABCBankingWebApp.Models.Account> Account { get; set; }
        public DbSet<ABCBankingWebApp.Models.FundTransfer> FundTransfer { get; set; }
        public DbSet<ABCBankingWebApp.Models.Backup> Backup { get; set; }
        public DbSet<ABCBankingWebApp.Models.Loan> Loan { get; set; }

    }
}
