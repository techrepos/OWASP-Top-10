using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using ABCBankingWebAppAdmin.Models;

namespace ABCBankingWebAppAdmin.Data
{
    public class AppDbContext : IdentityDbContext<Customer>
    {
        public AppDbContext(
            DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Customer> Customer { get; set; }
        public DbSet<Account> Account { get; set; }
        public DbSet<FundTransfer> FundTransfer { get; set; }
        public DbSet<Backup> Backup { get; set; }
        public DbSet<Loan> Loan { get; set; }

    }
}
