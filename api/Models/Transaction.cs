using System.ComponentModel.DataAnnotations;

namespace BankingAPI.Models
{
    /// <summary>
    /// Types of transactions supported by the system
    /// </summary>
    public enum TransactionType
    {
        CardTransaction,
        LoadCard,
        UnloadCard,
        AccountDeposit,
        AccountWithdrawal,
        AccountTransfer
    }

    /// <summary>
    /// Represents a transaction in the banking system
    /// </summary>
    public class Transaction
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string TransactionId { get; set; } = string.Empty;

        public TransactionType Type { get; set; }

        /// <summary>
        /// Transaction amount in EUR cents
        /// </summary>
        public long AmountInCents { get; set; }

        /// <summary>
        /// Transaction amount in EUR
        /// </summary>
        public decimal Amount
        {
            get => AmountInCents / 100.0m;
            set => AmountInCents = (long)(value * 100);
        }

        [StringLength(200)]
        public string Description { get; set; } = string.Empty;

        [StringLength(100)]
        public string MerchantName { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Unix timestamp for external system compatibility
        /// </summary>
        public long CreatedAtUnix => ((DateTimeOffset)CreatedAt).ToUnixTimeSeconds();

        /// <summary>
        /// Foreign key to the associated account (nullable for card-only transactions)
        /// </summary>
        public int? AccountId { get; set; }

        /// <summary>
        /// Navigation property to the associated account
        /// </summary>
        public virtual Account? Account { get; set; }

        /// <summary>
        /// Foreign key to the associated card (nullable for account-only transactions)
        /// </summary>
        public int? CardId { get; set; }

        /// <summary>
        /// Navigation property to the associated card
        /// </summary>
        public virtual Card? Card { get; set; }

        /// <summary>
        /// Balance after this transaction (for the account or card)
        /// </summary>
        public long BalanceAfterInCents { get; set; }

        /// <summary>
        /// Balance after this transaction in EUR
        /// </summary>
        public decimal BalanceAfter
        {
            get => BalanceAfterInCents / 100.0m;
            set => BalanceAfterInCents = (long)(value * 100);
        }
    }
}
