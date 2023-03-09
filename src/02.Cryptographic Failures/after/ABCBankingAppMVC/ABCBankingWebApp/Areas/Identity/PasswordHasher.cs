using Microsoft.AspNetCore.Identity;
using System;
using System.Text;

using ABCBankingWebApp.Models;

using BC = BCrypt.Net.BCrypt;

namespace ABCBankingWebApp.Identity
{
    public class PasswordHasher : IPasswordHasher<Customer>
    {
        public string HashPassword(Customer customer, string password)
        {
            return BC.HashPassword(password);

        }

        public PasswordVerificationResult VerifyHashedPassword(Customer customer,
            string hashedPassword, string password)
        {
            if (BC.Verify(password, hashedPassword))
                return PasswordVerificationResult.Success;
            else
                return PasswordVerificationResult.Failed;
        }
    }

}
