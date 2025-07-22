using System.ComponentModel.DataAnnotations;

namespace BankingAPI.Models
{
    /// <summary>
    /// Represents a card associated with an account
    /// </summary>
    public class Card
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(16)]
        public string CardNumber { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string CardholderName { get; set; } = string.Empty;

        [Required]
        [StringLength(4)]
        public string ExpiryMonth { get; set; } = string.Empty;

        [Required]
        [StringLength(4)]
        public string ExpiryYear { get; set; } = string.Empty;

        [Required]
        [StringLength(3)]
        public string CVV { get; set; } = string.Empty;

        /// <summary>
        /// Card balance in EUR cents (separate from account balance)
        /// </summary>
        public long BalanceInCents { get; set; }

        /// <summary>
        /// Card balance in EUR
        /// </summary>
        public decimal Balance
        {
            get => BalanceInCents / 100.0m;
            set => BalanceInCents = (long)(value * 100);
        }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;

        public bool IsBlocked { get; set; } = false;

        /// <summary>
        /// Foreign key to the associated account
        /// </summary>
        public int AccountId { get; set; }

        /// <summary>
        /// Navigation property to the associated account
        /// </summary>
        public virtual Account Account { get; set; } = null!;

        /// <summary>
        /// Navigation property for transactions on this card
        /// </summary>
        public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    }
}
