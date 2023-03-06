using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ABCBankingWebApp.Data;
using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace ABCBankingWebApp.Models
{
    public static class SeedData
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            var userManager = serviceProvider.GetService<UserManager<Customer>>();

            var user1 = await userManager.FindByNameAsync("testuser01@kb.ca");
            if (user1 == null)
            {
                user1 = new Customer
                {
                    FirstName = "Test",
                    MiddleName = "",
                    LastName = "User1",
                    DateOfBirth = DateTime.Parse("10/11/1933"),
                    UserName = "testuser01@kb.ca",
                    Email = "testuser01@kb.ca",
                    Accounts = new List<Account>{
                        new Account {
                                ID = new Guid("79f5c2ef-cd92-4542-8478-deeac12a75cb"),
                                Name = "Savings",
                                AccountType = AccountType.Savings,
                                Balance = 1250.55m
                            } ,
                        new Account {
                                ID = new Guid("c70cdd5b-0a5d-4891-99f3-065cedbce0f2"),
                                Name = "Checking",
                                AccountType = AccountType.Checking,
                                Balance = 2340.10m
                            }
                        },
                    FundTransfers = new List<FundTransfer>{
                        new FundTransfer {
                                AccountFrom = new Guid("79f5c2ef-cd92-4542-8478-deeac12a75cb"),
                                AccountTo = new Guid("c70cdd5b-0a5d-4891-99f3-065cedbce0f2"),
                                Amount = 510.00m,
                                TransactionDate = DateTime.Parse("06/12/2021"),
                                Note = "Transfer between accounts"
                            }
                    }

                };
                user1.PasswordHash = userManager.PasswordHasher.HashPassword(user1, "Test123456!!");

                await userManager.CreateAsync(user1);
            }

            if (user1 == null)
            {
                throw new Exception("The password is probably not strong enough!");
            }

            var user2 = await userManager.FindByNameAsync("UserEmp01@kk.com");
            if (user2 == null)
            {
                user2 = new Customer
                {
                    FirstName = "User",
                    MiddleName = "",
                    LastName = "Emp01",
                    DateOfBirth = DateTime.Parse("03/11/1945"),
                    Email = "UserEmp01@kk.com",
                    UserName = "UserEmp01@kk.com",
                    Accounts = new List<Account>{
                            new Account {
                                    Name = "Savings",
                                    AccountType = AccountType.Savings,
                                    Balance = 15030.00m
                                } ,
                            new Account {
                                    Name = "Checking",
                                    AccountType = AccountType.Checking,
                                    Balance = 2010.35m
                                }
                        }
                };
                user2.PasswordHash = userManager.PasswordHasher.HashPassword(user2, "Test45678!!");
                await userManager.CreateAsync(user2);
            }

            if (user2 == null)
            {
                throw new Exception("The password is probably not strong enough!");
            }

        }
    }

}