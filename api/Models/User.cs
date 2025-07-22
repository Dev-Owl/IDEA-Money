using System.ComponentModel.DataAnnotations;

namespace BankingAPI.Models
{
    /// <summary>
    /// Represents an admin user in the system
    /// </summary>
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Salt { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? LastLoginAt { get; set; }

        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Admin role for managing the banking system
        /// </summary>
        public bool IsAdmin { get; set; } = false;

        /// <summary>
        /// Navigation property for accounts owned by this user
        /// </summary>
        public virtual ICollection<Account> Accounts { get; set; } = new List<Account>();
    }
}
