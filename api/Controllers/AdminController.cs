using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using BankingAPI.Services;
using BankingAPI.DTOs;
using BankingAPI.Data;
using BankingAPI.Models;

namespace BankingAPI.Controllers;

/// <summary>
/// Admin controller for banking operations and management
/// </summary>
[ApiController]
[Route("api/admin")]
[Authorize]
public class AdminController : ControllerBase
{
    private readonly BankingService _bankingService;
    private readonly BankingContext _context;
    private readonly ILogger<AdminController> _logger;

    public AdminController(BankingService bankingService, BankingContext context, ILogger<AdminController> logger)
    {
        _bankingService = bankingService;
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Create a new account with 3 cards
    /// </summary>
    [HttpPost("accounts")]
    public async Task<ActionResult<AccountDto>> CreateAccount([FromBody] CreateAccountRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.OwnerName))
            {
                return BadRequest("Owner name is required");
            }

            if (string.IsNullOrWhiteSpace(request.Username))
            {
                return BadRequest("Username is required");
            }

            if (string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest("Password is required");
            }

            if (string.IsNullOrWhiteSpace(request.Email))
            {
                return BadRequest("Email is required");
            }

            var account = await _bankingService.CreateAccountAsync(request);
            return CreatedAtAction(nameof(GetAccount), new { id = account.Id }, account);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating account for {OwnerName}", request.OwnerName);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get all accounts
    /// </summary>
    [HttpGet("accounts")]
    public async Task<ActionResult<List<AccountDto>>> GetAllAccounts()
    {
        try
        {
            var accounts = await _bankingService.GetAllAccountsAsync();
            return Ok(accounts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all accounts");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get account by ID
    /// </summary>
    [HttpGet("accounts/{id}")]
    public async Task<ActionResult<AccountDto>> GetAccount(int id)
    {
        try
        {
            var account = await _bankingService.GetAccountAsync(id);
            if (account == null)
            {
                return NotFound("Account not found");
            }
            return Ok(account);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting account {AccountId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Delete an account
    /// </summary>
    [HttpDelete("accounts/{id}")]
    public async Task<ActionResult> DeleteAccount(int id)
    {
        try
        {
            var deleted = await _bankingService.DeleteAccountAsync(id);
            if (!deleted)
            {
                return NotFound("Account not found");
            }
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting account {AccountId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Add money to an account
    /// </summary>
    [HttpPost("accounts/add-money")]
    public async Task<ActionResult<TransactionDto>> AddMoneyToAccount([FromBody] AddMoneyRequest request)
    {
        try
        {
            if (request.Amount <= 0)
            {
                return BadRequest("Amount must be positive");
            }

            var transaction = await _bankingService.AddMoneyToAccountAsync(request);
            return Ok(transaction);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding money to account {AccountId}", request.AccountId);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Create a card transaction
    /// </summary>
    [HttpPost("transactions/card")]
    public async Task<ActionResult<TransactionDto>> CreateCardTransaction([FromBody] CreateTransactionRequest request)
    {
        try
        {
            if (request.Amount <= 0)
            {
                return BadRequest("Amount must be positive");
            }

            var transaction = await _bankingService.CreateCardTransactionAsync(request);
            return Ok(transaction);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating card transaction");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Load money onto a card
    /// </summary>
    [HttpPost("cards/load")]
    public async Task<ActionResult<TransactionDto>> LoadCard([FromBody] CardLoadRequest request)
    {
        try
        {
            if (request.Amount <= 0)
            {
                return BadRequest("Amount must be positive");
            }

            var transaction = await _bankingService.LoadCardAsync(request);
            return Ok(transaction);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading card {CardId}", request.CardId);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Unload money from a card
    /// </summary>
    [HttpPost("cards/unload")]
    public async Task<ActionResult<TransactionDto>> UnloadCard([FromBody] CardLoadRequest request)
    {
        try
        {
            if (request.Amount <= 0)
            {
                return BadRequest("Amount must be positive");
            }

            var transaction = await _bankingService.UnloadCardAsync(request);
            return Ok(transaction);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unloading card {CardId}", request.CardId);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get all transactions with pagination
    /// </summary>
    [HttpGet("transactions")]
    public async Task<ActionResult<List<TransactionDto>>> GetAllTransactions(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        try
        {
            var transactions = await _context.Transactions
                .OrderByDescending(t => t.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
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
                })
                .ToListAsync();

            return Ok(transactions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting transactions");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get system statistics
    /// </summary>
    [HttpGet("stats")]
    public async Task<ActionResult<object>> GetSystemStats()
    {
        try
        {
            var stats = new
            {
                TotalAccounts = await _context.Accounts.CountAsync(),
                TotalCards = await _context.Cards.CountAsync(),
                TotalTransactions = await _context.Transactions.CountAsync(),
                TotalBalance = await _context.Accounts.SumAsync(a => a.BalanceInCents) / 100.0m,
                ActiveAccounts = await _context.Accounts.CountAsync(a => a.IsActive),
                ActiveCards = await _context.Cards.CountAsync(c => c.IsActive && !c.IsBlocked)
            };

            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting system stats");
            return StatusCode(500, "Internal server error");
        }
    }
}
