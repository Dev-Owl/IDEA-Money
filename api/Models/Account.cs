using System.ComponentModel.DataAnnotations;

namespace BankingAPI.Models
{
    /// <summary>
    /// Represents a bank account in the system
    /// </summary>
    public class Account
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string AccountNumber { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string OwnerName { get; set; } = string.Empty;

        /// <summary>
        /// User ID that owns this account
        /// </summary>
        [Required]
        public int UserId { get; set; }

        /// <summary>
        /// Navigation property to the user that owns this account
        /// </summary>
        public virtual User User { get; set; } = null!;

        /// <summary>
        /// Account balance in EUR cents (to avoid floating point precision issues)
        /// </summary>
        public long BalanceInCents { get; set; }

        /// <summary>
        /// Account balance in EUR
        /// </summary>
        public decimal Balance
        {
            get => BalanceInCents / 100.0m;
            set => BalanceInCents = (long)(value * 100);
        }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Navigation property for cards associated with this account
        /// </summary>
        public virtual ICollection<Card> Cards { get; set; } = new List<Card>();

        /// <summary>
        /// Navigation property for transactions on this account
        /// </summary>
        public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    }
}
