namespace BankingAPI.DTOs
{
    /// <summary>
    /// DTO for authentication requests
    /// </summary>
    public class LoginRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO for authentication responses
    /// </summary>
    public class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
    }

    /// <summary>
    /// DTO for creating new accounts
    /// </summary>
    public class CreateAccountRequest
    {
        public string OwnerName { get; set; } = string.Empty;
        public decimal InitialBalance { get; set; } = 0;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO for account information
    /// </summary>
    public class AccountDto
    {
        public int Id { get; set; }
        public string AccountNumber { get; set; } = string.Empty;
        public string OwnerName { get; set; } = string.Empty;
        public decimal Balance { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
        public List<CardDto> Cards { get; set; } = new List<CardDto>();
    }

    /// <summary>
    /// DTO for card information
    /// </summary>
    public class CardDto
    {
        public int Id { get; set; }
        public string CardNumber { get; set; } = string.Empty;
        public string CardholderName { get; set; } = string.Empty;
        public string ExpiryMonth { get; set; } = string.Empty;
        public string ExpiryYear { get; set; } = string.Empty;
        public decimal Balance { get; set; }
        public bool IsActive { get; set; }
        public bool IsBlocked { get; set; }
        public DateTime CreatedAt { get; set; }
        public int AccountId { get; set; }
    }

    /// <summary>
    /// DTO for transaction information
    /// </summary>
    public class TransactionDto
    {
        public int Id { get; set; }
        public string TransactionId { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Description { get; set; } = string.Empty;
        public string MerchantName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public long CreatedAtUnix { get; set; }
        public decimal BalanceAfter { get; set; }
        public int? AccountId { get; set; }
        public int? CardId { get; set; }
    }

    /// <summary>
    /// DTO for creating transactions
    /// </summary>
    public class CreateTransactionRequest
    {
        public string Type { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Description { get; set; } = string.Empty;
        public string MerchantName { get; set; } = string.Empty;
        public int? AccountId { get; set; }
        public int? CardId { get; set; }
    }

    /// <summary>
    /// DTO for card load/unload operations
    /// </summary>
    public class CardLoadRequest
    {
        public int CardId { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO for adding money to account
    /// </summary>
    public class AddMoneyRequest
    {
        public int AccountId { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO for balance inquiry response
    /// </summary>
    public class BalanceDto
    {
        public decimal Balance { get; set; }
        public DateTime AsOf { get; set; }
        public int? AccountId { get; set; }
        public int? CardId { get; set; }
    }

    /// <summary>
    /// DTO for user banking credentials
    /// </summary>
    public class BankingUserRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO for user's banking data response
    /// </summary>
    public class UserBankingDataDto
    {
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string OwnerName { get; set; } = string.Empty;
        public List<AccountDto> Accounts { get; set; } = new List<AccountDto>();
        public List<CardDto> Cards { get; set; } = new List<CardDto>();
        public List<TransactionDto> RecentTransactions { get; set; } = new List<TransactionDto>();
    }
}
