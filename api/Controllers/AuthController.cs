using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BankingAPI.Services;
using BankingAPI.DTOs;
using BankingAPI.Data;
using BankingAPI.Models;

namespace BankingAPI.Controllers;

/// <summary>
/// Authentication controller for login functionality
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly BankingContext _context;
    private readonly JwtService _jwtService;
    private readonly PasswordService _passwordService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        BankingContext context,
        JwtService jwtService,
        PasswordService passwordService,
        ILogger<AuthController> logger)
    {
        _context = context;
        _jwtService = jwtService;
        _passwordService = passwordService;
        _logger = logger;
    }

    /// <summary>
    /// Login with username and password
    /// </summary>
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest("Username and password are required");
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == request.Username && u.IsActive);

            if (user == null || !_passwordService.VerifyPassword(request.Password, user.Salt, user.PasswordHash))
            {
                _logger.LogWarning("Failed login attempt for username: {Username}", request.Username);
                return Unauthorized("Invalid username or password");
            }

            // Update last login
            user.LastLoginAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            var token = _jwtService.GenerateToken(user);
            var jwtSettings = HttpContext.RequestServices.GetRequiredService<IConfiguration>().GetSection("JwtSettings");
            var expireHours = int.Parse(jwtSettings["ExpireHours"] ?? "24");

            var response = new LoginResponse
            {
                Token = token,
                Username = user.Username,
                ExpiresAt = DateTime.UtcNow.AddHours(expireHours)
            };

            _logger.LogInformation("Successful login for user: {Username}", user.Username);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for username: {Username}", request.Username);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Create a new admin user (for initial setup only)
    /// </summary>
    [HttpPost("register")]
    public async Task<ActionResult<string>> Register([FromBody] LoginRequest request)
    {
        try
        {
            // Check if any users exist (only allow registration if no users exist)
            var userCount = await _context.Users.CountAsync();
            if (userCount > 0)
            {
                return Forbid("Registration is disabled when users already exist");
            }

            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest("Username and password are required");
            }

            if (request.Password.Length < 6)
            {
                return BadRequest("Password must be at least 6 characters long");
            }

            var salt = _passwordService.GenerateSalt();
            var passwordHash = _passwordService.HashPassword(request.Password, salt);

            var user = new User
            {
                Username = request.Username,
                Email = $"{request.Username}@admin.local",
                PasswordHash = passwordHash,
                Salt = salt,
                IsAdmin = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Created admin user: {Username}", user.Username);
            return Ok("Admin user created successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating admin user");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Check if the system needs initial setup
    /// </summary>
    [HttpGet("setup-required")]
    public async Task<ActionResult<bool>> IsSetupRequired()
    {
        try
        {
            var userCount = await _context.Users.CountAsync();
            return Ok(userCount == 0);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking setup status");
            return StatusCode(500, "Internal server error");
        }
    }
}
