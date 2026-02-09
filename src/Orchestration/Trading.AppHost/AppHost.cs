var builder = DistributedApplication.CreateBuilder(args);

// ========================================
// INFRASTRUCTURE
// ========================================

var redis = builder.AddRedis("cache")
    .WithRedisCommander()
    .WithDataVolume("trading-redis-data")
    .WithLifetime(ContainerLifetime.Persistent);

var postgres = builder.AddPostgres("postgres")
    .WithDataVolume("trading-pg-data")
    .WithLifetime(ContainerLifetime.Persistent)
    .WithPgAdmin();

var identityDb = postgres.AddDatabase("IdentityDb");
var tradingDb = postgres.AddDatabase("TradingDb");

// ========================================
// WEB FRONTEND (Blazor + Identity)
// ========================================

builder.AddProject<Projects.Trading_Web>("trading-web")
    .WithReference(identityDb)
    .WaitFor(identityDb)
    .WithReference(tradingDb)
    .WaitFor(tradingDb)
    .WithReference(redis)
    .WaitFor(redis)
    .WithExternalHttpEndpoints();

// ========================================
// ADMIN PORTAL (separate UI, same databases)
// ========================================

builder.AddProject<Projects.Trading_Admin>("trading-admin")
    .WithReference(identityDb)
    .WaitFor(identityDb)
    .WithReference(tradingDb)
    .WaitFor(tradingDb)
    .WithExternalHttpEndpoints();

builder.Build().Run();
