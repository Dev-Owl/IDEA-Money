# Copilot Instructions for Banking API

<!-- Use this file to provide workspace-specific custom instructions to Copilot. For more details, visit https://code.visualstudio.com/docs/copilot/copilot-customization#_use-a-githubcopilotinstructionsmd-file -->

This is a .NET Web API banking mockup system project with the following characteristics:

## Project Overview
- **Purpose**: Create a banking API for testing integrations and trade show demos
- **Technology Stack**: .NET 9 Web API, Entity Framework Core, SQLite, JWT Authentication, Swagger
- **Target Framework**: .NET 9

## Key Features
- Account management with EUR currency
- Card transactions and balance management
- JWT-based authentication
- Admin panel for account creation and management
- Transaction history and example data generation
- Swagger documentation

## Architecture Guidelines
- Use Entity Framework Code First approach
- Implement Repository pattern for data access
- Use DTOs for API responses
- Separate controllers for public API and admin functionality
- Include comprehensive error handling and logging

## Database Schema
- Accounts (with EUR balance)
- Cards (3 per account)
- Transactions (various types: card, load/unload, account transfers)
- Users/Logins for admin access

## Security
- JWT tokens for API authentication
- Admin panel requires token authentication
- Secure endpoints appropriately

## Code Style
- Follow C# naming conventions
- Use async/await patterns
- Include XML documentation comments
- Use proper HTTP status codes
- Implement proper validation
