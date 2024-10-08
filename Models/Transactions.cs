using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Assignment2.Models
{
    public class Transactions
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int TransactionId { get; set; }
        [ForeignKey("BankAccount")]
        public int AccountNumber { get; set; }
        [Required]
        [StringLength(50)]
        public string TransactionType { get; set; }
        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Amount { get; set; }
        [Required]
        public DateTime TransactionDate { get; set; }
        [ForeignKey("BankAccount")]
        public int RelatedAccount {  get; set; }

        public string? Description { get; set; }
    }
}
