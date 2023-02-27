﻿using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace ABCBankingWebApp.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(
            DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<ABCBankingWebApp.Models.Customer> Customer { get; set; }
        public DbSet<ABCBankingWebApp.Models.Account> Account { get; set; }
        public DbSet<ABCBankingWebApp.Models.FundTransfer> FundTransfer { get; set; }

    }
}
