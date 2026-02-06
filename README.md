# Trading Platform Template

A production-ready .NET 10 web application template featuring Blazor, ASP.NET Core Identity, and .NET Aspire orchestration.

## 🚀 Features

- **Modern Architecture**: Clean separation across 4 layers (Presentation, Infrastructure, Orchestration, Tests)
- **Blazor Web App**: Interactive Auto render mode with server and WASM components
- **Full Authentication**: ASP.NET Core Identity with role-based authorization, 2FA, and passkey support
- **Admin Dashboard**: User and role management interface
- **Aspire Orchestration**: Container orchestration with PostgreSQL and Redis
- **Observability**: OpenTelemetry integration for traces, metrics, and logs
- **Resilience**: Polly patterns on all HTTP clients
- **CSS Isolation**: Scoped component styles following best practices
- **Test Coverage**: xUnit unit and integration tests

## 📁 Project Structure

```
TradingPlatform/
├── src/
│   ├── Presentation/
│   │   ├── Trading.Web/              # Blazor Web App (server + WASM)
│   │   ├── Trading.Web.Client/       # WASM client components
│   │   └── Trading.Shared.UI/        # Razor Class Library (shared components)
│   ├── Infrastructure/
│   │   ├── Trading.Contracts/        # DTOs and interfaces
│   │   └── Trading.Persistence/      # EF Core contexts (future)
│   └── Orchestration/
│       ├── Trading.AppHost/          # Aspire orchestrator
│       └── Trading.ServiceDefaults/  # Shared telemetry & resilience
└── tests/
    ├── Trading.Unit.Tests/           # Unit tests
    └── Trading.AppHost.Tests/        # Aspire integration tests
```

## 🛠️ Tech Stack

- **.NET 10** - Latest framework
- **Blazor** - Interactive Auto render mode
- **ASP.NET Core Identity** - Authentication & authorization
- **.NET Aspire** - Cloud-native orchestration
- **PostgreSQL** - Primary database (via Aspire)
- **Redis** - Output caching (via Aspire)
- **Bootstrap 5** - UI framework
- **xUnit** - Testing framework

## 🏃 Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop) (for Aspire containers)

### Running the Application

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd TradingPlatform
   ```

2. **Start Docker Desktop**

3. **Run the application**
   ```bash
   dotnet run --project src/Orchestration/Trading.AppHost
   ```

4. **Access the application**
   - Aspire Dashboard: https://localhost:17252
   - Trading Web App: Check the Aspire dashboard for the URL

### Default Users

| Email | Password | Role |
|-------|----------|------|
| admin@trading.local | Admin123! | Admin |
| demo@trading.local | Demo1234 | User |
| trader@trading.local | Trader1234 | User |

## 🧪 Running Tests

```bash
# Run all tests
dotnet test

# Run unit tests only
dotnet test tests/Trading.Unit.Tests

# Run integration tests (requires Docker)
dotnet test tests/Trading.AppHost.Tests
```

## 📦 Building

```bash
dotnet build TradingPlatform.slnx
```

## 🎨 Component Structure

Components follow the three-file pattern:
- `Component.razor` - Markup only
- `Component.razor.cs` - Code-behind (partial class)
- `Component.razor.css` - Scoped styles (CSS isolation)

Example:
```
Data.razor       # UI markup
Data.razor.cs    # C# logic
Data.razor.css   # Scoped styles
```

## 🔐 Admin Features

Login as admin to access:
- **User Management**: Create, delete, and manage users
- **Role Management**: Create roles and assign to users
- **System Stats**: View user counts, confirmed accounts, etc.

Navigate to `/admin/data` or click "Data" in the nav menu (admin only).

## 🐳 Docker Containers

Aspire automatically manages:
- **PostgreSQL** - Identity database with persistent volume
- **Redis** - Output caching with persistent volume
- **pgAdmin** - Database management UI
- **Redis Commander** - Redis management UI

## 📊 Observability

- **OpenTelemetry**: Traces, metrics, and logs
- **Health Checks**: `/health` and `/alive` endpoints
- **Aspire Dashboard**: Real-time monitoring and logs

## 🔧 Configuration

- `appsettings.json` - Application settings
- `launchSettings.json` - Development profiles
- `Directory.Build.props` - Global MSBuild properties
- `.editorconfig` - Code style rules

## 📝 License

This is a template project. Use it as a starting point for your applications.

## 👤 Author

Gal Tamir (galtamir@live.com)

## 🤝 Contributing

This is a template project. Fork it and make it your own!
