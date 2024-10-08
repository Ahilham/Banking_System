using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Assignment2.Models
{
    public class BankAccount
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int AccountNumber { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Balance { get; set; }
        [ForeignKey("User")]
        public int UserId {  get; set; } 

    }
}
