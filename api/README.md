# Banking API - Mockup Banking System

A .NET Web API banking mockup system designed for testing integrations and trade show demonstrations. This API provides a complete banking simulation with account management, card operations, and transaction handling.

## Features

### Core Banking Operations
- **Account Management**: Create and manage EUR-based bank accounts
- **Card Management**: Each account comes with 3 cards automatically
- **Transaction Processing**: Support for various transaction types
- **Balance Inquiries**: Real-time account and card balances
- **Transaction History**: Complete audit trail with Unix timestamp support

### Admin Panel
- **Web-based Interface**: Simple HTML interface for administration
- **Account Creation**: Create new accounts with initial balance
- **Money Management**: Add money to accounts, load/unload cards
- **Transaction Creation**: Create card transactions for testing
- **System Overview**: View all accounts, cards, and statistics

### Security & Authentication
- **JWT Authentication**: Secure API access with JWT tokens
- **Admin Authentication**: Token-based admin panel access
- **Password Hashing**: Secure password storage with salt

### API Documentation
- **Swagger/OpenAPI**: Complete API documentation at `/swagger`
- **RESTful Design**: Standard HTTP methods and status codes
- **JSON Responses**: Consistent JSON API responses

## Technology Stack

- **.NET 9**: Latest .NET framework
- **Entity Framework Core**: ORM with SQLite database
- **JWT Authentication**: Secure token-based authentication
- **Swagger/OpenAPI**: API documentation
- **SQLite**: Lightweight database for easy deployment

## Quick Start

### Prerequisites
- .NET 9 SDK
- Visual Studio Code (recommended)

### Installation

1. **Clone or download the project**
2. **Restore dependencies**:
   ```bash
   dotnet restore
   ```

3. **Create database**:
   ```bash
   dotnet ef database update
   ```

4. **Run the application**:
   ```bash
   dotnet run
   ```

5. **Access the admin panel**:
   - Open browser to `https://localhost:5001` or `http://localhost:5000`
   - Create initial admin user (first-time setup)
   - Login and start managing accounts

### API Endpoints

#### Authentication
- `POST /api/auth/login` - Login with username/password
- `POST /api/auth/register` - Create initial admin user
- `GET /api/auth/setup-required` - Check if setup is needed

#### Banking API (Requires JWT token)
- `GET /api/banking/accounts/{id}` - Get account details
- `GET /api/banking/accounts/{id}/balance` - Get account balance
- `GET /api/banking/cards/{id}/balance` - Get card balance
- `GET /api/banking/transactions?since={unix}` - Get transactions since timestamp
- `POST /api/banking/cards/load` - Load money onto card
- `POST /api/banking/cards/unload` - Unload money from card

#### Admin API (Requires JWT token)
- `GET /api/admin/accounts` - List all accounts
- `POST /api/admin/accounts` - Create new account
- `DELETE /api/admin/accounts/{id}` - Delete account
- `POST /api/admin/accounts/add-money` - Add money to account
- `POST /api/admin/cards/load` - Load card
- `POST /api/admin/cards/unload` - Unload card
- `POST /api/admin/transactions/card` - Create card transaction
- `GET /api/admin/transactions` - List transactions
- `GET /api/admin/stats` - System statistics

#### Admin Panel (Web Interface)
- `/admin` - Main admin panel
- `/admin/login` - Login page

### Configuration

The application can be configured via `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=banking.db"
  },
  "JwtSettings": {
    "Key": "YourSecretKey",
    "Issuer": "BankingAPI",
    "Audience": "BankingAPI",
    "ExpireHours": "24"
  }
}
```

### Example Usage

#### 1. Create Admin User (First time only)
```bash
curl -X POST "https://localhost:5001/api/auth/register" \
  -H "Content-Type: application/json" \
  -d '{"username": "admin", "password": "password123"}'
```

#### 2. Login
```bash
curl -X POST "https://localhost:5001/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{"username": "admin", "password": "password123"}'
```

#### 3. Create Account
```bash
curl -X POST "https://localhost:5001/api/admin/accounts" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"ownerName": "John Doe", "initialBalance": 100.00}'
```

#### 4. Get Account Balance
```bash
curl -X GET "https://localhost:5001/api/banking/accounts/1/balance" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

## Database Schema

### Accounts
- Account number (unique)
- Owner name
- Balance (stored in cents for precision)
- Creation timestamp
- Active status

### Cards
- Card number (unique, 16 digits)
- Cardholder name
- Expiry date (month/year)
- CVV (3 digits)
- Balance (separate from account)
- Account association

### Transactions
- Unique transaction ID
- Transaction type (CardTransaction, LoadCard, UnloadCard, etc.)
- Amount (in cents)
- Description and merchant name
- Timestamps (both datetime and Unix)
- Account/Card associations
- Balance after transaction

### Users
- Username and email (unique)
- Hashed password with salt
- Admin privileges
- Login tracking

## Development

### Building
```bash
dotnet build
```

### Running Tests
```bash
dotnet test
```

### Database Migrations
```bash
# Add new migration
dotnet ef migrations add MigrationName

# Update database
dotnet ef database update

# Remove last migration
dotnet ef migrations remove
```

## Deployment

The application can be deployed as:
- **Self-contained**: Single executable with runtime
- **Framework-dependent**: Requires .NET runtime
- **Docker**: Containerized deployment
- **IIS**: Windows server deployment

## Security Considerations

- Change the JWT secret key in production
- Use HTTPS in production
- Consider rate limiting for public endpoints
- Regular backup of SQLite database
- Monitor and log authentication attempts

## API Integration

This API is designed to integrate with IDEA's financial systems. It provides:
- Compatible transaction formats
- Unix timestamp support for legacy systems
- EUR currency handling
- Real-time balance updates
- Comprehensive transaction history

## Support

For issues and questions:
- Check the Swagger documentation at `/swagger`
- Review the admin panel for visual management
- Monitor application logs for debugging

## License

Internal use for IDEA Money demonstrations and testing.
