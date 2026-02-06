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

// ========================================
// WEB FRONTEND (Blazor + Identity)
// ========================================

builder.AddProject<Projects.Trading_Web>("trading-web")
    .WithReference(identityDb)
    .WaitFor(identityDb)
    .WithReference(redis)
    .WaitFor(redis)
    .WithExternalHttpEndpoints();

builder.Build().Run();
