using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace ABCBankingWebAppAdmin.Models
{
    public class Customer : IdentityUser
    {
        //public int ID { get; set; }

        [PersonalData]
        [RegularExpression(@"^[A-Z]+[a-zA-Z]*$")]
        [Display(Name = "First Name")]
        [StringLength(60, MinimumLength = 3)]
        [Required]
        public string FirstName { get; set; }

        [PersonalData]
        [RegularExpression(@"^[A-Z]+[a-zA-Z]*$")]
        [Display(Name = "Middle Name")]
        [StringLength(60, MinimumLength = 3)]
        [Required]
        public string MiddleName { get; set; }

        [PersonalData]
        [RegularExpression(@"^[A-Z]+[a-zA-Z]*$")]
        [Display(Name = "Last Name")]
        [StringLength(60, MinimumLength = 3)]
        [Required]
        public string LastName { get; set; }

        [PersonalData]
        [DisplayFormat(DataFormatString = "{mm/dd/yyyy}")]
        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; }

        // [ReputableEmail]
        // [Display(Name = "Email Address")]
        // [Required]
        // [EmailAddress]
        // public string Email { get; set; }

        // [PersonalData]
        // [Display(Name = "Phone Number")]
        // [StringLength(9)]
        // [Required]
        // public string Phone { get; set; }

        public List<Account> Accounts { get; set; }

        public virtual List<FundTransfer> FundTransfers { get; set; }

    }
}