using Microsoft.EntityFrameworkCore;
using BankingAPI.Models;

namespace BankingAPI.Data
{
    /// <summary>
    /// Entity Framework DbContext for the Banking API
    /// </summary>
    public class BankingContext : DbContext
    {
        public BankingContext(DbContextOptions<BankingContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Account> Accounts { get; set; }
        public DbSet<Card> Cards { get; set; }
        public DbSet<Transaction> Transactions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Account entity
            modelBuilder.Entity<Account>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.AccountNumber).IsRequired().HasMaxLength(50);
                entity.Property(e => e.OwnerName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.BalanceInCents).IsRequired();
                entity.HasIndex(e => e.AccountNumber).IsUnique();

                // Configure relationships
                entity.HasMany(e => e.Cards)
                    .WithOne(e => e.Account)
                    .HasForeignKey(e => e.AccountId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(e => e.Transactions)
                    .WithOne(e => e.Account)
                    .HasForeignKey(e => e.AccountId)
                    .OnDelete(DeleteBehavior.SetNull);

                // Configure User-Account relationship
                entity.HasOne(e => e.User)
                    .WithMany(e => e.Accounts)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure Card entity
            modelBuilder.Entity<Card>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.CardNumber).IsRequired().HasMaxLength(16);
                entity.Property(e => e.CardholderName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.ExpiryMonth).IsRequired().HasMaxLength(4);
                entity.Property(e => e.ExpiryYear).IsRequired().HasMaxLength(4);
                entity.Property(e => e.CVV).IsRequired().HasMaxLength(3);
                entity.Property(e => e.BalanceInCents).IsRequired();
                entity.HasIndex(e => e.CardNumber).IsUnique();

                // Configure relationships
                entity.HasMany(e => e.Transactions)
                    .WithOne(e => e.Card)
                    .HasForeignKey(e => e.CardId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Configure Transaction entity
            modelBuilder.Entity<Transaction>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.TransactionId).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Type).IsRequired();
                entity.Property(e => e.AmountInCents).IsRequired();
                entity.Property(e => e.Description).HasMaxLength(200);
                entity.Property(e => e.MerchantName).HasMaxLength(100);
                entity.Property(e => e.BalanceAfterInCents).IsRequired();
                entity.HasIndex(e => e.TransactionId).IsUnique();
                entity.HasIndex(e => e.CreatedAt);
            });

            // Configure User entity
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Username).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
                entity.Property(e => e.PasswordHash).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Salt).IsRequired().HasMaxLength(50);
                entity.HasIndex(e => e.Username).IsUnique();
                entity.HasIndex(e => e.Email).IsUnique();
            });

            // Configure enum conversion for TransactionType
            modelBuilder.Entity<Transaction>()
                .Property(e => e.Type)
                .HasConversion<string>();
        }
    }
}
