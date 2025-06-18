using Aspire.Hosting.Azure;

IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

IResourceBuilder<AzurePostgresFlexibleServerResource> postgres = builder.AddAzurePostgresFlexibleServer("postgres")
    .RunAsContainer();

IResourceBuilder<AzurePostgresFlexibleServerDatabaseResource> silverbridgeDb = postgres.AddDatabase("silverbridgeDb");

IResourceBuilder<ProjectResource> api = builder.AddProject<Projects.SilverbridgeWeb_Api>("silverbridgeweb-api")
    .WithReference(silverbridgeDb)
    .WaitFor(silverbridgeDb)
    .WithHttpHealthCheck("/health");

builder.AddProject<Projects.SilverbridgeWeb_WebUI>("silverbridgeweb-webui")
    .WithExternalHttpEndpoints()
    .WithUrlForEndpoint("https", url => url.DisplayText = "Website (HTTPS)")
    .WithUrlForEndpoint("http", url => url.DisplayText = "Website (HTTP)")
    .WithHttpHealthCheck("/health")
    .WithReference(api)
    .WaitFor(api);

await builder.Build().RunAsync().ConfigureAwait(false);
