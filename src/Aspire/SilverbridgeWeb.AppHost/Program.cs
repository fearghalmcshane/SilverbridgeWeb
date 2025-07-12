using Aspire.Hosting.Azure;

IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

IResourceBuilder<AzurePostgresFlexibleServerResource> postgres = builder.AddAzurePostgresFlexibleServer("postgres")
    .RunAsContainer(options => options.WithDataVolume());

IResourceBuilder<AzurePostgresFlexibleServerDatabaseResource> silverbridgeDb = postgres.AddDatabase("silverbridgeDb");

IResourceBuilder<RedisResource> redis = builder.AddRedis("redis");

IResourceBuilder<ProjectResource> api = builder.AddProject<Projects.SilverbridgeWeb_Api>("silverbridgeweb-api")
    .WithReference(silverbridgeDb)
    .WithReference(redis)
    .WaitFor(silverbridgeDb)
    .WaitFor(redis)
    .WithHttpHealthCheck("/health");

builder.AddProject<Projects.SilverbridgeWeb_WebUI>("silverbridgeweb-webui")
    .WithExternalHttpEndpoints()
    .WithUrlForEndpoint("https", url => url.DisplayText = "Website (HTTPS)")
    .WithUrlForEndpoint("http", url => url.DisplayText = "Website (HTTP)")
    .WithHttpHealthCheck("/health")
    .WithReference(api)
    .WaitFor(api);

await builder.Build().RunAsync().ConfigureAwait(false);
