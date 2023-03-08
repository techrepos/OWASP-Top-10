using System.ComponentModel.DataAnnotations;

namespace ABCBankingWebAppAdmin.Models
{
    public class Backup
    {
        public int ID { get; set; }


        [Display(Name = "Backup Name")]
        [StringLength(100)]
        public string Name { get; set; }

        [Display(Name = "Backup Date")]
        [DataType(DataType.Date)]
        public DateTime BackupDate { get; set; }

    }
}
