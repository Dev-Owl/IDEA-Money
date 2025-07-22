using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BankingAPI.Services;
using BankingAPI.DTOs;

namespace BankingAPI.Controllers;

/// <summary>
/// API controller for banking operations (IDEA integration)
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BankingController : ControllerBase
{
    private readonly BankingService _bankingService;
    private readonly ILogger<BankingController> _logger;

    public BankingController(BankingService bankingService, ILogger<BankingController> logger)
    {
        _bankingService = bankingService;
        _logger = logger;
    }

    /// <summary>
    /// Get account balance by account ID
    /// </summary>
    [HttpGet("accounts/{accountId}/balance")]
    public async Task<ActionResult<BalanceDto>> GetAccountBalance(int accountId)
    {
        try
        {
            var balance = await _bankingService.GetAccountBalanceAsync(accountId);
            if (balance == null)
            {
                return NotFound("Account not found");
            }
            return Ok(balance);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting account balance for account {AccountId}", accountId);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get card balance by card ID
    /// </summary>
    [HttpGet("cards/{cardId}/balance")]
    public async Task<ActionResult<BalanceDto>> GetCardBalance(int cardId)
    {
        try
        {
            var balance = await _bankingService.GetCardBalanceAsync(cardId);
            if (balance == null)
            {
                return NotFound("Card not found");
            }
            return Ok(balance);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting card balance for card {CardId}", cardId);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get all transactions since Unix timestamp
    /// </summary>
    [HttpGet("transactions")]
    public async Task<ActionResult<List<TransactionDto>>> GetTransactionsSince([FromQuery] long since)
    {
        try
        {
            var transactions = await _bankingService.GetTransactionsSinceAsync(since);
            return Ok(transactions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting transactions since {Since}", since);
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
    /// Get account details by ID
    /// </summary>
    [HttpGet("accounts/{accountId}")]
    public async Task<ActionResult<AccountDto>> GetAccount(int accountId)
    {
        try
        {
            var account = await _bankingService.GetAccountAsync(accountId);
            if (account == null)
            {
                return NotFound("Account not found");
            }
            return Ok(account);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting account {AccountId}", accountId);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get user's banking data (accounts, cards, transactions) by credentials
    /// </summary>
    [HttpPost("user/data")]
    [AllowAnonymous]
    public async Task<ActionResult<UserBankingDataDto>> GetUserBankingData([FromBody] BankingUserRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest("Username and password are required");
            }

            var userData = await _bankingService.GetUserBankingDataAsync(request.Username, request.Password);
            if (userData == null)
            {
                return Unauthorized("Invalid credentials");
            }

            return Ok(userData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user banking data for {Username}", request.Username);
            return StatusCode(500, "Internal server error");
        }
    }
}
