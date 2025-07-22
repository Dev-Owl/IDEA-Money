using Microsoft.EntityFrameworkCore;
using BankingAPI.Data;
using BankingAPI.Models;
using BankingAPI.DTOs;

namespace BankingAPI.Services
{
    /// <summary>
    /// Service for banking operations
    /// </summary>
    public class BankingService
    {
        private readonly BankingContext _context;
        private readonly ILogger<BankingService> _logger;
        private readonly PasswordService _passwordService;

        public BankingService(BankingContext context, ILogger<BankingService> logger, PasswordService passwordService)
        {
            _context = context;
            _logger = logger;
            _passwordService = passwordService;
        }

        /// <summary>
        /// Create a new account with 3 cards and a banking user
        /// </summary>
        public async Task<AccountDto> CreateAccountAsync(CreateAccountRequest request)
        {
            // Validate required fields
            if (string.IsNullOrWhiteSpace(request.Username))
                throw new ArgumentException("Username is required");
            if (string.IsNullOrWhiteSpace(request.Password))
                throw new ArgumentException("Password is required");
            if (string.IsNullOrWhiteSpace(request.Email))
                throw new ArgumentException("Email is required");
            if (string.IsNullOrWhiteSpace(request.OwnerName))
                throw new ArgumentException("Owner name is required");

            // Check if username already exists
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == request.Username);
            if (existingUser != null)
                throw new ArgumentException("Username already exists");

            // Check if email already exists
            var existingEmail = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (existingEmail != null)
                throw new ArgumentException("Email already exists");

            // Create the banking user
            var salt = _passwordService.GenerateSalt();
            var passwordHash = _passwordService.HashPassword(request.Password, salt);

            var user = new User
            {
                Username = request.Username,
                Email = request.Email,
                PasswordHash = passwordHash,
                Salt = salt,
                IsAdmin = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Create the account
            var account = new Account
            {
                AccountNumber = GenerateAccountNumber(),
                OwnerName = request.OwnerName,
                Balance = request.InitialBalance,
                UserId = user.Id,
                CreatedAt = DateTime.UtcNow
            };

            _context.Accounts.Add(account);
            await _context.SaveChangesAsync();

            // Create 3 cards for the account
            var cards = new List<Card>();
            for (int i = 0; i < 3; i++)
            {
                var card = new Card
                {
                    CardNumber = GenerateCardNumber(),
                    CardholderName = request.OwnerName,
                    ExpiryMonth = DateTime.Now.AddYears(3).Month.ToString("00"),
                    ExpiryYear = DateTime.Now.AddYears(3).Year.ToString(),
                    CVV = GenerateCVV(),
                    Balance = 0,
                    AccountId = account.Id,
                    CreatedAt = DateTime.UtcNow
                };
                cards.Add(card);
                _context.Cards.Add(card);
            }

            await _context.SaveChangesAsync();

            // Generate example transactions
            await GenerateExampleTransactionsAsync(account, cards);

            return await GetAccountAsync(account.Id) ?? throw new InvalidOperationException("Failed to retrieve created account");
        }

        /// <summary>
        /// Get account by ID
        /// </summary>
        public async Task<AccountDto?> GetAccountAsync(int accountId)
        {
            var account = await _context.Accounts
                .Include(a => a.Cards)
                .FirstOrDefaultAsync(a => a.Id == accountId);

            if (account == null) return null;

            return new AccountDto
            {
                Id = account.Id,
                AccountNumber = account.AccountNumber,
                OwnerName = account.OwnerName,
                Balance = account.Balance,
                CreatedAt = account.CreatedAt,
                IsActive = account.IsActive,
                Cards = account.Cards.Select(c => new CardDto
                {
                    Id = c.Id,
                    CardNumber = c.CardNumber,
                    CardholderName = c.CardholderName,
                    ExpiryMonth = c.ExpiryMonth,
                    ExpiryYear = c.ExpiryYear,
                    Balance = c.Balance,
                    IsActive = c.IsActive,
                    IsBlocked = c.IsBlocked,
                    CreatedAt = c.CreatedAt,
                    AccountId = c.AccountId
                }).ToList()
            };
        }

        /// <summary>
        /// Delete an account and all associated data (user, cards, transactions)
        /// </summary>
        public async Task<bool> DeleteAccountAsync(int accountId)
        {
            var account = await _context.Accounts
                .Include(a => a.User)
                .Include(a => a.Cards)
                .ThenInclude(c => c.Transactions)
                .Include(a => a.Transactions)
                .FirstOrDefaultAsync(a => a.Id == accountId);

            if (account == null) return false;

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Delete all card transactions
                var cardTransactions = account.Cards.SelectMany(c => c.Transactions).ToList();
                if (cardTransactions.Any())
                {
                    _context.Transactions.RemoveRange(cardTransactions);
                }

                // Delete all account transactions
                if (account.Transactions.Any())
                {
                    _context.Transactions.RemoveRange(account.Transactions);
                }

                // Delete all cards
                if (account.Cards.Any())
                {
                    _context.Cards.RemoveRange(account.Cards);
                }

                // Delete the account
                _context.Accounts.Remove(account);

                // Delete the associated user if this is their only account
                var userAccountCount = await _context.Accounts.CountAsync(a => a.UserId == account.UserId && a.Id != accountId);
                if (userAccountCount == 0 && account.User != null)
                {
                    _context.Users.Remove(account.User);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Successfully deleted account {AccountId} and all associated data", accountId);
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Failed to delete account {AccountId}", accountId);
                throw;
            }
        }

        /// <summary>
        /// Add money to an account
        /// </summary>
        public async Task<TransactionDto> AddMoneyToAccountAsync(AddMoneyRequest request)
        {
            var account = await _context.Accounts.FindAsync(request.AccountId);
            if (account == null) throw new ArgumentException("Account not found");

            account.Balance += request.Amount;

            var transaction = new Transaction
            {
                TransactionId = GenerateTransactionId(),
                Type = TransactionType.AccountDeposit,
                Amount = request.Amount,
                Description = request.Description,
                CreatedAt = DateTime.UtcNow,
                AccountId = account.Id,
                BalanceAfter = account.Balance
            };

            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();

            return new TransactionDto
            {
                Id = transaction.Id,
                TransactionId = transaction.TransactionId,
                Type = transaction.Type.ToString(),
                Amount = transaction.Amount,
                Description = transaction.Description,
                CreatedAt = transaction.CreatedAt,
                CreatedAtUnix = transaction.CreatedAtUnix,
                BalanceAfter = transaction.BalanceAfter,
                AccountId = transaction.AccountId
            };
        }

        /// <summary>
        /// Load money onto a card (transfers from account to card)
        /// </summary>
        public async Task<TransactionDto> LoadCardAsync(CardLoadRequest request)
        {
            var card = await _context.Cards
                .Include(c => c.Account)
                .FirstOrDefaultAsync(c => c.Id == request.CardId);
            if (card == null) throw new ArgumentException("Card not found");

            var account = card.Account;
            if (account.Balance < request.Amount)
                throw new ArgumentException("Insufficient account balance");

            // Transfer money from account to card
            account.Balance -= request.Amount;
            card.Balance += request.Amount;

            // Create account withdrawal transaction
            var accountTransaction = new Transaction
            {
                TransactionId = GenerateTransactionId(),
                Type = TransactionType.AccountWithdrawal,
                Amount = request.Amount,
                Description = $"Transfer to card {card.CardNumber[^4..]} - {request.Description}",
                CreatedAt = DateTime.UtcNow,
                AccountId = account.Id,
                BalanceAfter = account.Balance
            };

            // Create card load transaction
            var cardTransaction = new Transaction
            {
                TransactionId = GenerateTransactionId(),
                Type = TransactionType.LoadCard,
                Amount = request.Amount,
                Description = request.Description,
                CreatedAt = DateTime.UtcNow,
                CardId = card.Id,
                BalanceAfter = card.Balance
            };

            _context.Transactions.AddRange(accountTransaction, cardTransaction);
            await _context.SaveChangesAsync();

            return new TransactionDto
            {
                Id = cardTransaction.Id,
                TransactionId = cardTransaction.TransactionId,
                Type = cardTransaction.Type.ToString(),
                Amount = cardTransaction.Amount,
                Description = cardTransaction.Description,
                CreatedAt = cardTransaction.CreatedAt,
                CreatedAtUnix = cardTransaction.CreatedAtUnix,
                BalanceAfter = cardTransaction.BalanceAfter,
                CardId = cardTransaction.CardId
            };
        }

        /// <summary>
        /// Unload money from a card (transfers back to account)
        /// </summary>
        public async Task<TransactionDto> UnloadCardAsync(CardLoadRequest request)
        {
            var card = await _context.Cards
                .Include(c => c.Account)
                .FirstOrDefaultAsync(c => c.Id == request.CardId);
            if (card == null) throw new ArgumentException("Card not found");
            if (card.Balance < request.Amount) throw new ArgumentException("Insufficient card balance");

            var account = card.Account;

            // Transfer money from card back to account
            card.Balance -= request.Amount;
            account.Balance += request.Amount;

            // Create card unload transaction
            var cardTransaction = new Transaction
            {
                TransactionId = GenerateTransactionId(),
                Type = TransactionType.UnloadCard,
                Amount = request.Amount,
                Description = request.Description,
                CreatedAt = DateTime.UtcNow,
                CardId = card.Id,
                BalanceAfter = card.Balance
            };

            // Create account deposit transaction
            var accountTransaction = new Transaction
            {
                TransactionId = GenerateTransactionId(),
                Type = TransactionType.AccountDeposit,
                Amount = request.Amount,
                Description = $"Transfer from card {card.CardNumber[^4..]} - {request.Description}",
                CreatedAt = DateTime.UtcNow,
                AccountId = account.Id,
                BalanceAfter = account.Balance
            };

            _context.Transactions.AddRange(cardTransaction, accountTransaction);
            await _context.SaveChangesAsync();

            return new TransactionDto
            {
                Id = cardTransaction.Id,
                TransactionId = cardTransaction.TransactionId,
                Type = cardTransaction.Type.ToString(),
                Amount = cardTransaction.Amount,
                Description = cardTransaction.Description,
                CreatedAt = cardTransaction.CreatedAt,
                CreatedAtUnix = cardTransaction.CreatedAtUnix,
                BalanceAfter = cardTransaction.BalanceAfter,
                CardId = cardTransaction.CardId
            };
        }

        /// <summary>
        /// Create a card transaction (with proper balance validation)
        /// </summary>
        public async Task<TransactionDto> CreateCardTransactionAsync(CreateTransactionRequest request)
        {
            if (request.CardId == null) throw new ArgumentException("Card ID is required");

            var card = await _context.Cards.FindAsync(request.CardId);
            if (card == null) throw new ArgumentException("Card not found");
            if (card.IsBlocked) throw new ArgumentException("Card is blocked");
            if (!card.IsActive) throw new ArgumentException("Card is not active");
            if (card.Balance < request.Amount) throw new ArgumentException("Insufficient card balance");

            card.Balance -= request.Amount;

            var transaction = new Transaction
            {
                TransactionId = GenerateTransactionId(),
                Type = TransactionType.CardTransaction,
                Amount = request.Amount,
                Description = request.Description,
                MerchantName = request.MerchantName,
                CreatedAt = DateTime.UtcNow,
                CardId = card.Id,
                BalanceAfter = card.Balance
            };

            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();

            return new TransactionDto
            {
                Id = transaction.Id,
                TransactionId = transaction.TransactionId,
                Type = transaction.Type.ToString(),
                Amount = transaction.Amount,
                Description = transaction.Description,
                MerchantName = transaction.MerchantName,
                CreatedAt = transaction.CreatedAt,
                CreatedAtUnix = transaction.CreatedAtUnix,
                BalanceAfter = transaction.BalanceAfter,
                CardId = transaction.CardId
            };
        }

        /// <summary>
        /// Get account balance
        /// </summary>
        public async Task<BalanceDto?> GetAccountBalanceAsync(int accountId)
        {
            var account = await _context.Accounts.FindAsync(accountId);
            if (account == null) return null;

            return new BalanceDto
            {
                Balance = account.Balance,
                AsOf = DateTime.UtcNow,
                AccountId = accountId
            };
        }

        /// <summary>
        /// Get card balance
        /// </summary>
        public async Task<BalanceDto?> GetCardBalanceAsync(int cardId)
        {
            var card = await _context.Cards.FindAsync(cardId);
            if (card == null) return null;

            return new BalanceDto
            {
                Balance = card.Balance,
                AsOf = DateTime.UtcNow,
                CardId = cardId
            };
        }

        /// <summary>
        /// Get all transactions since a Unix timestamp
        /// </summary>
        public async Task<List<TransactionDto>> GetTransactionsSinceAsync(long sinceUnix)
        {
            var sinceDateTime = DateTimeOffset.FromUnixTimeSeconds(sinceUnix).DateTime;

            var transactions = await _context.Transactions
                .Where(t => t.CreatedAt >= sinceDateTime)
                .OrderBy(t => t.CreatedAt)
                .ToListAsync();

            return transactions.Select(t => new TransactionDto
            {
                Id = t.Id,
                TransactionId = t.TransactionId,
                Type = t.Type.ToString(),
                Amount = t.Amount,
                Description = t.Description,
                MerchantName = t.MerchantName,
                CreatedAt = t.CreatedAt,
                CreatedAtUnix = t.CreatedAtUnix,
                BalanceAfter = t.BalanceAfter,
                AccountId = t.AccountId,
                CardId = t.CardId
            }).ToList();
        }

        /// <summary>
        /// Get all accounts
        /// </summary>
        public async Task<List<AccountDto>> GetAllAccountsAsync()
        {
            var accounts = await _context.Accounts
                .Include(a => a.Cards)
                .OrderBy(a => a.CreatedAt)
                .ToListAsync();

            return accounts.Select(a => new AccountDto
            {
                Id = a.Id,
                AccountNumber = a.AccountNumber,
                OwnerName = a.OwnerName,
                Balance = a.Balance,
                CreatedAt = a.CreatedAt,
                IsActive = a.IsActive,
                Cards = a.Cards.Select(c => new CardDto
                {
                    Id = c.Id,
                    CardNumber = c.CardNumber,
                    CardholderName = c.CardholderName,
                    ExpiryMonth = c.ExpiryMonth,
                    ExpiryYear = c.ExpiryYear,
                    Balance = c.Balance,
                    IsActive = c.IsActive,
                    IsBlocked = c.IsBlocked,
                    CreatedAt = c.CreatedAt,
                    AccountId = c.AccountId
                }).ToList()
            }).ToList();
        }

        private async Task GenerateExampleTransactionsAsync(Account account, List<Card> cards)
        {
            var random = new Random();
            var merchants = new[] { "Amazon", "Starbucks", "Shell", "McDonald's", "IKEA", "H&M", "Metro", "Rewe", "DM", "Saturn" };
            var descriptions = new[] { "Purchase", "Payment", "Online shopping", "Fuel", "Food", "Groceries", "Pharmacy", "Electronics" };

            var transactions = new List<Transaction>();
            var startDate = DateTime.UtcNow.AddMonths(-3);

            // First, add some money to the account if needed
            if (account.Balance < 1000)
            {
                account.Balance += 1000; // Add 1000 EUR to account for initial funding
                var initialDeposit = new Transaction
                {
                    TransactionId = GenerateTransactionId(),
                    Type = TransactionType.AccountDeposit,
                    Amount = 1000,
                    Description = "Initial account funding",
                    CreatedAt = startDate,
                    AccountId = account.Id,
                    BalanceAfter = account.Balance
                };
                transactions.Add(initialDeposit);
            }

            // Load cards with money from account (realistic transfers)
            foreach (var card in cards)
            {
                var loadAmount = Math.Round((decimal)(random.NextDouble() * 200 + 100), 2);

                // Transfer from account to card
                if (account.Balance >= loadAmount)
                {
                    account.Balance -= loadAmount;
                    card.Balance += loadAmount;

                    // Account withdrawal transaction
                    var accountWithdrawal = new Transaction
                    {
                        TransactionId = GenerateTransactionId(),
                        Type = TransactionType.AccountWithdrawal,
                        Amount = loadAmount,
                        Description = $"Transfer to card {card.CardNumber[^4..]}",
                        CreatedAt = startDate.AddDays(random.Next(0, 10)),
                        AccountId = account.Id,
                        BalanceAfter = account.Balance
                    };

                    // Card load transaction
                    var loadTransaction = new Transaction
                    {
                        TransactionId = GenerateTransactionId(),
                        Type = TransactionType.LoadCard,
                        Amount = loadAmount,
                        Description = "Initial card load",
                        CreatedAt = startDate.AddDays(random.Next(0, 10)),
                        CardId = card.Id,
                        BalanceAfter = card.Balance
                    };

                    transactions.AddRange(new[] { accountWithdrawal, loadTransaction });
                }
            }

            // Generate 15-25 random card transactions distributed over 3 months
            var transactionCount = random.Next(15, 26);

            for (int i = 0; i < transactionCount; i++)
            {
                var transactionDate = startDate.AddDays(random.Next(10, 90));
                var card = cards[random.Next(cards.Count)];
                var amount = Math.Round((decimal)(random.NextDouble() * 80 + 5), 2);

                // Only create transaction if card has enough balance
                if (card.Balance >= amount)
                {
                    card.Balance -= amount;

                    var transaction = new Transaction
                    {
                        TransactionId = GenerateTransactionId(),
                        Type = TransactionType.CardTransaction,
                        Amount = amount,
                        Description = descriptions[random.Next(descriptions.Length)],
                        MerchantName = merchants[random.Next(merchants.Length)],
                        CreatedAt = transactionDate,
                        CardId = card.Id,
                        BalanceAfter = card.Balance
                    };

                    transactions.Add(transaction);
                }
            }

            _context.Transactions.AddRange(transactions);
            await _context.SaveChangesAsync();
        }

        private string GenerateAccountNumber()
        {
            var random = new Random();
            return $"DE{random.Next(10, 99)}{random.Next(1000, 9999)}{random.Next(1000, 9999)}{random.Next(10, 99)}";
        }

        private string GenerateCardNumber()
        {
            var random = new Random();
            return $"4{random.Next(100, 999)}{random.Next(1000, 9999)}{random.Next(1000, 9999)}";
        }

        private string GenerateCVV()
        {
            var random = new Random();
            return random.Next(100, 999).ToString();
        }

        private string GenerateTransactionId()
        {
            return $"TXN{DateTime.UtcNow:yyyyMMddHHmmss}{Guid.NewGuid().ToString("N")[..8].ToUpper()}";
        }

        /// <summary>
        /// Authenticate a banking user
        /// </summary>
        public async Task<User?> AuthenticateBankingUserAsync(string username, string password)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == username && !u.IsAdmin && u.IsActive);

            if (user == null)
                return null;

            var isValid = _passwordService.VerifyPassword(password, user.Salt, user.PasswordHash);
            if (!isValid)
                return null;

            // Update last login
            user.LastLoginAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return user;
        }

        /// <summary>
        /// Get user's banking data (accounts, cards, transactions)
        /// </summary>
        public async Task<UserBankingDataDto?> GetUserBankingDataAsync(int userId)
        {
            var user = await _context.Users
                .Include(u => u.Accounts)
                    .ThenInclude(a => a.Cards)
                .Include(u => u.Accounts)
                    .ThenInclude(a => a.Transactions.OrderByDescending(t => t.CreatedAt).Take(10))
                .FirstOrDefaultAsync(u => u.Id == userId && !u.IsAdmin && u.IsActive);

            if (user == null)
                return null;

            var accounts = user.Accounts.Select(a => new AccountDto
            {
                Id = a.Id,
                AccountNumber = a.AccountNumber,
                OwnerName = a.OwnerName,
                Balance = a.Balance,
                CreatedAt = a.CreatedAt,
                IsActive = a.IsActive,
                Cards = a.Cards.Select(c => new CardDto
                {
                    Id = c.Id,
                    CardNumber = c.CardNumber,
                    CardholderName = c.CardholderName,
                    ExpiryMonth = c.ExpiryMonth,
                    ExpiryYear = c.ExpiryYear,
                    Balance = c.Balance,
                    IsActive = c.IsActive,
                    IsBlocked = c.IsBlocked,
                    CreatedAt = c.CreatedAt
                }).ToList()
            }).ToList();

            var allCards = accounts.SelectMany(a => a.Cards).ToList();

            var recentTransactions = user.Accounts
                .SelectMany(a => a.Transactions)
                .OrderByDescending(t => t.CreatedAt)
                .Take(20)
                .Select(t => new TransactionDto
                {
                    Id = t.Id,
                    TransactionId = t.TransactionId,
                    Type = t.Type.ToString(),
                    Amount = t.Amount,
                    Description = t.Description,
                    MerchantName = t.MerchantName,
                    CreatedAt = t.CreatedAt,
                    CreatedAtUnix = t.CreatedAtUnix,
                    BalanceAfter = t.BalanceAfter,
                    AccountId = t.AccountId,
                    CardId = t.CardId
                }).ToList();

            return new UserBankingDataDto
            {
                UserId = user.Id,
                Username = user.Username,
                OwnerName = user.Accounts.FirstOrDefault()?.OwnerName ?? "",
                Accounts = accounts,
                Cards = allCards,
                RecentTransactions = recentTransactions
            };
        }

        /// <summary>
        /// Get user's banking data by username/password
        /// </summary>
        public async Task<UserBankingDataDto?> GetUserBankingDataAsync(string username, string password)
        {
            var user = await AuthenticateBankingUserAsync(username, password);
            if (user == null)
                return null;

            return await GetUserBankingDataAsync(user.Id);
        }
    }
}
