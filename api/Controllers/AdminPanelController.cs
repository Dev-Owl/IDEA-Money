using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BankingAPI.Services;

namespace BankingAPI.Controllers;

/// <summary>
/// Admin panel controller that serves HTML views
/// </summary>
[Route("admin")]
public class AdminPanelController : Controller
{
    private readonly BankingService _bankingService;

    public AdminPanelController(BankingService bankingService)
    {
        _bankingService = bankingService;
    }

    /// <summary>
    /// Main admin panel page
    /// </summary>
    [HttpGet("")]
    public IActionResult Index()
    {
        return Content(GetAdminPanelHtml(), "text/html");
    }

    /// <summary>
    /// Login page
    /// </summary>
    [HttpGet("login")]
    public IActionResult Login()
    {
        return Content(GetLoginPageHtml(), "text/html");
    }

    private string GetLoginPageHtml()
    {
        return @"
<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Banking API - Admin Login</title>
    <style>
        body { 
            font-family: Arial, sans-serif; 
            margin: 0; 
            padding: 20px; 
            background-color: #f5f5f5; 
        }
        .container { 
            max-width: 400px; 
            margin: 100px auto; 
            background: white; 
            padding: 30px; 
            border-radius: 8px; 
            box-shadow: 0 2px 10px rgba(0,0,0,0.1); 
        }
        h1 { 
            color: #333; 
            text-align: center; 
            margin-bottom: 30px; 
        }
        .form-group { 
            margin-bottom: 20px; 
        }
        label { 
            display: block; 
            margin-bottom: 5px; 
            font-weight: bold; 
        }
        input[type=""text""], input[type=""password""] { 
            width: 100%; 
            padding: 10px; 
            border: 1px solid #ddd; 
            border-radius: 4px; 
            box-sizing: border-box; 
        }
        button { 
            width: 100%; 
            padding: 12px; 
            background-color: #007bff; 
            color: white; 
            border: none; 
            border-radius: 4px; 
            cursor: pointer; 
            font-size: 16px; 
        }
        button:hover { 
            background-color: #0056b3; 
        }
        .error { 
            color: red; 
            margin-top: 10px; 
            text-align: center; 
        }
        .success { 
            color: green; 
            margin-top: 10px; 
            text-align: center; 
        }
    </style>
</head>
<body>
    <div class=""container"">
        <h1>Banking API Admin</h1>
        <form id=""loginForm"">
            <div class=""form-group"">
                <label for=""username"">Username:</label>
                <input type=""text"" id=""username"" name=""username"" required>
            </div>
            <div class=""form-group"">
                <label for=""password"">Password:</label>
                <input type=""password"" id=""password"" name=""password"" required>
            </div>
            <button type=""submit"">Login</button>
        </form>
        <div id=""setupSection"" style=""display: none; margin-top: 30px; border-top: 1px solid #ddd; padding-top: 20px;"">
            <h3>Initial Setup</h3>
            <p>No admin user found. Create the first admin user:</p>
            <form id=""setupForm"">
                <div class=""form-group"">
                    <label for=""setupUsername"">Admin Username:</label>
                    <input type=""text"" id=""setupUsername"" name=""username"" required>
                </div>
                <div class=""form-group"">
                    <label for=""setupPassword"">Admin Password:</label>
                    <input type=""password"" id=""setupPassword"" name=""password"" required minlength=""6"">
                </div>
                <button type=""submit"">Create Admin User</button>
            </form>
        </div>
        <div id=""message""></div>
    </div>

    <script>
        let token = localStorage.getItem('adminToken');
        
        // Check if setup is required
        async function checkSetup() {
            try {
                const response = await fetch('/api/auth/setup-required');
                const setupRequired = await response.json();
                if (setupRequired) {
                    document.getElementById('setupSection').style.display = 'block';
                }
            } catch (error) {
                console.error('Error checking setup:', error);
            }
        }

        // Handle login
        document.getElementById('loginForm').addEventListener('submit', async (e) => {
            e.preventDefault();
            const formData = new FormData(e.target);
            const data = Object.fromEntries(formData);

            try {
                const response = await fetch('/api/auth/login', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify(data)
                });

                if (response.ok) {
                    const result = await response.json();
                    localStorage.setItem('adminToken', result.token);
                    window.location.href = '/admin';
                } else {
                    const error = await response.text();
                    document.getElementById('message').innerHTML = '<div class=""error"">' + error + '</div>';
                }
            } catch (error) {
                document.getElementById('message').innerHTML = '<div class=""error"">Login failed: ' + error.message + '</div>';
            }
        });

        // Handle setup
        document.getElementById('setupForm').addEventListener('submit', async (e) => {
            e.preventDefault();
            const formData = new FormData(e.target);
            const data = Object.fromEntries(formData);

            try {
                const response = await fetch('/api/auth/register', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify(data)
                });

                if (response.ok) {
                    document.getElementById('message').innerHTML = '<div class=""success"">Admin user created! Please login.</div>';
                    document.getElementById('setupSection').style.display = 'none';
                    document.getElementById('setupForm').reset();
                } else {
                    const error = await response.text();
                    document.getElementById('message').innerHTML = '<div class=""error"">' + error + '</div>';
                }
            } catch (error) {
                document.getElementById('message').innerHTML = '<div class=""error"">Setup failed: ' + error.message + '</div>';
            }
        });

        // Check if already logged in
        if (token) {
            window.location.href = '/admin';
        }

        // Check setup status on load
        checkSetup();
    </script>
</body>
</html>";
    }

    private string GetAdminPanelHtml()
    {
        return @"
<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Banking API - Admin Panel</title>
    <style>
        body { 
            font-family: Arial, sans-serif; 
            margin: 0; 
            padding: 20px; 
            background-color: #f5f5f5; 
        }
        .header { 
            background: white; 
            padding: 20px; 
            border-radius: 8px; 
            margin-bottom: 20px; 
            box-shadow: 0 2px 4px rgba(0,0,0,0.1); 
            display: flex; 
            justify-content: space-between; 
            align-items: center; 
        }
        .container { 
            display: grid; 
            grid-template-columns: 1fr 1fr; 
            gap: 20px; 
        }
        .panel { 
            background: white; 
            padding: 20px; 
            border-radius: 8px; 
            box-shadow: 0 2px 4px rgba(0,0,0,0.1); 
        }
        h1, h2 { 
            color: #333; 
            margin-top: 0; 
        }
        .form-group { 
            margin-bottom: 15px; 
        }
        label { 
            display: block; 
            margin-bottom: 5px; 
            font-weight: bold; 
        }
        input, select, textarea { 
            width: 100%; 
            padding: 8px; 
            border: 1px solid #ddd; 
            border-radius: 4px; 
            box-sizing: border-box; 
            font-size: 14px;
        }
        select {
            background-color: white;
            cursor: pointer;
        }
        select option[style*=""color: #999""] {
            background-color: #f8f9fa;
        }
        button { 
            padding: 10px 20px; 
            background-color: #007bff; 
            color: white; 
            border: none; 
            border-radius: 4px; 
            cursor: pointer; 
            margin-right: 10px; 
        }
        button:hover { 
            background-color: #0056b3; 
        }
        button.danger { 
            background-color: #dc3545; 
        }
        button.danger:hover { 
            background-color: #c82333; 
        }
        .accounts-list { 
            max-height: 400px; 
            overflow-y: auto; 
        }
        .account-item { 
            border: 1px solid #ddd; 
            padding: 15px; 
            margin-bottom: 10px; 
            border-radius: 4px; 
            background: #f9f9f9; 
        }
        .error { 
            color: red; 
            margin-top: 10px; 
        }
        .success { 
            color: green; 
            margin-top: 10px; 
        }
        .stats { 
            display: grid; 
            grid-template-columns: repeat(auto-fit, minmax(150px, 1fr)); 
            gap: 15px; 
            margin-bottom: 20px; 
        }
        .stat-card { 
            background: #007bff; 
            color: white; 
            padding: 20px; 
            border-radius: 8px; 
            text-align: center; 
        }
        .stat-value { 
            font-size: 24px; 
            font-weight: bold; 
        }
        .stat-label { 
            font-size: 14px; 
            opacity: 0.9; 
        }
        .logout-btn { 
            background-color: #6c757d; 
        }
        .logout-btn:hover { 
            background-color: #545b62; 
        }
    </style>
</head>
<body>
    <div class=""header"">
        <h1>Banking API Admin Panel</h1>
        <button class=""logout-btn"" onclick=""logout()"">Logout</button>
    </div>
    
    <div id=""stats"" class=""stats""></div>
    
    <div class=""container"">
        <div class=""panel"">
            <h2>Create Account</h2>
            <form id=""createAccountForm"">
                <div class=""form-group"">
                    <label for=""ownerName"">Owner Name:</label>
                    <input type=""text"" id=""ownerName"" name=""ownerName"" required>
                </div>
                <div class=""form-group"">
                    <label for=""username"">Username:</label>
                    <input type=""text"" id=""username"" name=""username"" required>
                </div>
                <div class=""form-group"">
                    <label for=""email"">Email:</label>
                    <input type=""email"" id=""email"" name=""email"" required>
                </div>
                <div class=""form-group"">
                    <label for=""password"">Password:</label>
                    <input type=""password"" id=""password"" name=""password"" required minlength=""6"">
                </div>
                <div class=""form-group"">
                    <label for=""initialBalance"">Initial Balance (EUR):</label>
                    <input type=""number"" id=""initialBalance"" name=""initialBalance"" min=""0"" step=""0.01"" value=""100"">
                </div>
                <button type=""submit"">Create Account</button>
            </form>
            <div id=""createMessage""></div>
            
            <hr style=""margin: 30px 0;"">
            
            <h2>Add Money to Account</h2>
            <form id=""addMoneyForm"">
                <div class=""form-group"">
                    <label for=""accountId"">Select Account:</label>
                    <select id=""accountId"" name=""accountId"" required>
                        <option value="""">Select an account...</option>
                    </select>
                </div>
                <div class=""form-group"">
                    <label for=""amount"">Amount (EUR):</label>
                    <input type=""number"" id=""amount"" name=""amount"" min=""0.01"" step=""0.01"" required>
                </div>
                <div class=""form-group"">
                    <label for=""description"">Description:</label>
                    <input type=""text"" id=""description"" name=""description"" value=""Admin deposit"">
                </div>
                <button type=""submit"">Add Money</button>
            </form>
            <div id=""addMoneyMessage""></div>
        </div>
        
        <div class=""panel"">
            <h2>Card Operations</h2>
            <div style=""background: #f8f9fa; padding: 15px; border-radius: 4px; margin-bottom: 20px; border-left: 4px solid #17a2b8;"">
                <strong>Note:</strong> Loading a card transfers money FROM the account TO the card. Unloading transfers money FROM the card TO the account.
            </div>
            <form id=""cardOperationForm"">
                <div class=""form-group"">
                    <label for=""operation"">Operation:</label>
                    <select id=""operation"" name=""operation"" required>
                        <option value=""load"">Load Card (Account → Card)</option>
                        <option value=""unload"">Unload Card (Card → Account)</option>
                        <option value=""transaction"">Create Transaction (Spend from Card)</option>
                    </select>
                </div>
                <div class=""form-group"">
                    <label for=""cardId"">Select Card:</label>
                    <select id=""cardId"" name=""cardId"" required>
                        <option value="""">Select a card...</option>
                    </select>
                </div>
                <div class=""form-group"">
                    <label for=""cardAmount"">Amount (EUR):</label>
                    <input type=""number"" id=""cardAmount"" name=""amount"" min=""0.01"" step=""0.01"" required>
                </div>
                <div class=""form-group"">
                    <label for=""cardDescription"">Description:</label>
                    <input type=""text"" id=""cardDescription"" name=""description"" placeholder=""Optional description"">
                </div>
                <div class=""form-group"" id=""merchantGroup"" style=""display: none;"">
                    <label for=""merchantName"">Merchant Name:</label>
                    <input type=""text"" id=""merchantName"" name=""merchantName"" placeholder=""e.g., Amazon, Starbucks"">
                </div>
                <button type=""submit"">Execute Operation</button>
            </form>
            <div id=""cardMessage""></div>
            
            <hr style=""margin: 30px 0;"">
            
            <h2>Quick Actions</h2>
            <button onclick=""loadAccounts()"">Refresh Accounts</button>
            <button onclick=""viewTransactions()"">View Recent Transactions</button>
        </div>
    </div>
    
    <div class=""panel"" style=""margin-top: 20px;"">
        <h2>Accounts</h2>
        <div id=""accountsList"" class=""accounts-list""></div>
    </div>
    
    <div class=""panel"" style=""margin-top: 20px; display: none;"" id=""transactionsPanel"">
        <h2>Recent Transactions</h2>
        <button onclick=""hideTransactions()"" style=""float: right;"">Hide</button>
        <div id=""transactionsList"" style=""max-height: 400px; overflow-y: auto;""></div>
    </div>

    <script>
        const token = localStorage.getItem('adminToken');
        if (!token) {
            window.location.href = '/admin/login';
        }

        const apiHeaders = {
            'Authorization': 'Bearer ' + token,
            'Content-Type': 'application/json'
        };

        // Load statistics
        async function loadStats() {
            try {
                const response = await fetch('/api/admin/stats', { headers: apiHeaders });
                if (response.ok) {
                    const stats = await response.json();
                    document.getElementById('stats').innerHTML = 
                        '<div class=""stat-card"">' +
                            '<div class=""stat-value"">' + stats.totalAccounts + '</div>' +
                            '<div class=""stat-label"">Total Accounts</div>' +
                        '</div>' +
                        '<div class=""stat-card"">' +
                            '<div class=""stat-value"">' + stats.totalCards + '</div>' +
                            '<div class=""stat-label"">Total Cards</div>' +
                        '</div>' +
                        '<div class=""stat-card"">' +
                            '<div class=""stat-value"">' + stats.totalTransactions + '</div>' +
                            '<div class=""stat-label"">Total Transactions</div>' +
                        '</div>' +
                        '<div class=""stat-card"">' +
                            '<div class=""stat-value"">€' + stats.totalBalance.toFixed(2) + '</div>' +
                            '<div class=""stat-label"">Total Balance</div>' +
                        '</div>';
                }
            } catch (error) {
                console.error('Error loading stats:', error);
            }
        }

        // Load accounts
        async function loadAccounts() {
            try {
                const response = await fetch('/api/admin/accounts', { headers: apiHeaders });
                if (response.ok) {
                    const accounts = await response.json();
                    
                    // Update account dropdown
                    updateAccountDropdown(accounts);
                    
                    // Update card dropdown with all cards
                    updateCardDropdown(accounts);
                    
                    const accountsList = document.getElementById('accountsList');
                    accountsList.innerHTML = accounts.map(account => {
                        const cardsHtml = account.cards.map(card => 
                            '<div style=""margin-left: 20px; margin-top: 8px; padding: 8px; background: #f8f9fa; border-radius: 4px;"">' +
                                '<strong>Card ' + card.id + ':</strong> **** **** **** ' + card.cardNumber.slice(-4) + '<br>' +
                                '<strong>Balance:</strong> €' + card.balance.toFixed(2) + ' | ' +
                                '<strong>Status:</strong> ' + (card.isActive ? (card.isBlocked ? 'Blocked' : 'Active') : 'Inactive') +
                            '</div>'
                        ).join('');
                        
                        return '<div class=""account-item"">' +
                            '<strong>ID: ' + account.id + '</strong> - ' + account.ownerName + '<br>' +
                            '<strong>Account:</strong> ' + account.accountNumber + '<br>' +
                            '<strong>Account Balance:</strong> €' + account.balance.toFixed(2) + '<br>' +
                            '<strong>Cards:</strong> ' + account.cards.length + '<br>' +
                            cardsHtml +
                            '<strong>Created:</strong> ' + new Date(account.createdAt).toLocaleDateString() + '<br>' +
                            '<div style=""margin-top: 10px;"">' +
                                '<button onclick=""deleteAccount(' + account.id + ')"" class=""danger"">Delete Account</button>' +
                            '</div>' +
                        '</div>';
                    }).join('');
                }
            } catch (error) {
                console.error('Error loading accounts:', error);
            }
        }

        // Update account dropdown
        function updateAccountDropdown(accounts) {
            const accountSelect = document.getElementById('accountId');
            accountSelect.innerHTML = '<option value="""">Select an account...</option>';
            accounts.forEach(account => {
                const option = document.createElement('option');
                option.value = account.id;
                option.textContent = `ID: ${account.id} - ${account.ownerName} (€${account.balance.toFixed(2)})`;
                accountSelect.appendChild(option);
            });
        }

        // Update card dropdown
        function updateCardDropdown(accounts) {
            const cardSelect = document.getElementById('cardId');
            cardSelect.innerHTML = '<option value="""">Select a card...</option>';
            accounts.forEach(account => {
                account.cards.forEach(card => {
                    const option = document.createElement('option');
                    option.value = card.id;
                    const status = card.isActive ? (card.isBlocked ? 'Blocked' : 'Active') : 'Inactive';
                    option.textContent = `Card ${card.id} - ****${card.cardNumber.slice(-4)} (${account.ownerName}) - €${card.balance.toFixed(2)} [${status}]`;
                    if (!card.isActive || card.isBlocked) {
                        option.style.color = '#999';
                        option.textContent += ' - UNAVAILABLE';
                    }
                    cardSelect.appendChild(option);
                });
            });
        }

        // Delete account
        async function deleteAccount(accountId) {
            if (confirm('Are you sure you want to delete this account? This cannot be undone.')) {
                try {
                    const response = await fetch('/api/admin/accounts/' + accountId, {
                        method: 'DELETE',
                        headers: apiHeaders
                    });
                    if (response.ok) {
                        loadAccounts();
                        loadStats();
                    }
                } catch (error) {
                    console.error('Error deleting account:', error);
                }
            }
        }

        // Load transactions with proper authentication
        async function loadTransactions() {
            try {
                const response = await fetch('/api/admin/transactions?page=1&pageSize=50', { 
                    headers: apiHeaders 
                });
                if (response.ok) {
                    const transactions = await response.json();
                    const transactionsList = document.getElementById('transactionsList');
                    const transactionsPanel = document.getElementById('transactionsPanel');
                    
                    transactionsList.innerHTML = transactions.map(t => 
                        '<div style=""border: 1px solid #ddd; padding: 10px; margin-bottom: 10px; border-radius: 4px; background: #f9f9f9;"">' +
                            '<strong>ID: ' + t.id + '</strong> - ' + t.type + '<br>' +
                            '<strong>Amount:</strong> €' + t.amount.toFixed(2) + '<br>' +
                            '<strong>Description:</strong> ' + t.description + '<br>' +
                            (t.merchantName ? '<strong>Merchant:</strong> ' + t.merchantName + '<br>' : '') +
                            '<strong>Date:</strong> ' + new Date(t.createdAt).toLocaleString() + '<br>' +
                            '<strong>Balance After:</strong> €' + t.balanceAfter.toFixed(2) + '<br>' +
                            (t.accountId ? '<strong>Account ID:</strong> ' + t.accountId + '<br>' : '') +
                            (t.cardId ? '<strong>Card ID:</strong> ' + t.cardId + '<br>' : '') +
                        '</div>'
                    ).join('');
                    
                    transactionsPanel.style.display = 'block';
                } else {
                    alert('Failed to load transactions: ' + response.statusText);
                }
            } catch (error) {
                alert('Error loading transactions: ' + error.message);
            }
        }

        function viewTransactions() {
            loadTransactions();
        }

        function hideTransactions() {
            document.getElementById('transactionsPanel').style.display = 'none';
        }

        // Create account form
        document.getElementById('createAccountForm').addEventListener('submit', async (e) => {
            e.preventDefault();
            const formData = new FormData(e.target);
            const data = Object.fromEntries(formData);
            data.initialBalance = parseFloat(data.initialBalance);

            try {
                const response = await fetch('/api/admin/accounts', {
                    method: 'POST',
                    headers: apiHeaders,
                    body: JSON.stringify(data)
                });

                if (response.ok) {
                    document.getElementById('createMessage').innerHTML = '<div class=""success"">Account created successfully!</div>';
                    e.target.reset();
                    loadAccounts();
                    loadStats();
                } else {
                    const error = await response.text();
                    document.getElementById('createMessage').innerHTML = '<div class=""error"">' + error + '</div>';
                }
            } catch (error) {
                document.getElementById('createMessage').innerHTML = '<div class=""error"">Error: ' + error.message + '</div>';
            }
        });

        // Add money form
        document.getElementById('addMoneyForm').addEventListener('submit', async (e) => {
            e.preventDefault();
            const formData = new FormData(e.target);
            const data = Object.fromEntries(formData);
            data.accountId = parseInt(data.accountId);
            data.amount = parseFloat(data.amount);

            if (!data.accountId) {
                document.getElementById('addMoneyMessage').innerHTML = '<div class=""error"">Please select an account.</div>';
                return;
            }

            try {
                const response = await fetch('/api/admin/accounts/add-money', {
                    method: 'POST',
                    headers: apiHeaders,
                    body: JSON.stringify(data)
                });

                if (response.ok) {
                    document.getElementById('addMoneyMessage').innerHTML = '<div class=""success"">Money added successfully!</div>';
                    e.target.reset();
                    document.getElementById('accountId').value = '';
                    loadAccounts();
                    loadStats();
                } else {
                    const error = await response.text();
                    document.getElementById('addMoneyMessage').innerHTML = '<div class=""error"">' + error + '</div>';
                }
            } catch (error) {
                document.getElementById('addMoneyMessage').innerHTML = '<div class=""error"">Error: ' + error.message + '</div>';
            }
        });

        // Card operation form
        document.getElementById('cardOperationForm').addEventListener('submit', async (e) => {
            e.preventDefault();
            const formData = new FormData(e.target);
            const data = Object.fromEntries(formData);
            data.cardId = parseInt(data.cardId);
            data.amount = parseFloat(data.amount);

            if (!data.cardId) {
                document.getElementById('cardMessage').innerHTML = '<div class=""error"">Please select a card.</div>';
                return;
            }

            let endpoint;
            const operation = data.operation;
            delete data.operation;

            switch (operation) {
                case 'load':
                    endpoint = '/api/admin/cards/load';
                    break;
                case 'unload':
                    endpoint = '/api/admin/cards/unload';
                    break;
                case 'transaction':
                    endpoint = '/api/admin/transactions/card';
                    data.type = 'CardTransaction';
                    break;
            }

            try {
                const response = await fetch(endpoint, {
                    method: 'POST',
                    headers: apiHeaders,
                    body: JSON.stringify(data)
                });

                if (response.ok) {
                    document.getElementById('cardMessage').innerHTML = '<div class=""success"">Operation completed successfully!</div>';
                    e.target.reset();
                    document.getElementById('cardId').value = '';
                    document.getElementById('operation').value = 'load';
                    document.getElementById('merchantGroup').style.display = 'none';
                    loadAccounts();
                    loadStats();
                } else {
                    const error = await response.text();
                    document.getElementById('cardMessage').innerHTML = '<div class=""error"">' + error + '</div>';
                }
            } catch (error) {
                document.getElementById('cardMessage').innerHTML = '<div class=""error"">Error: ' + error.message + '</div>';
            }
        });

        // Show/hide merchant field based on operation
        document.getElementById('operation').addEventListener('change', (e) => {
            const merchantGroup = document.getElementById('merchantGroup');
            if (e.target.value === 'transaction') {
                merchantGroup.style.display = 'block';
            } else {
                merchantGroup.style.display = 'none';
            }
        });

        function logout() {
            localStorage.removeItem('adminToken');
            window.location.href = '/admin/login';
        }

        // Initial load
        loadStats();
        loadAccounts();
    </script>
</body>
</html>";
    }
}