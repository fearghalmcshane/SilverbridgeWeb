var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres");

var silverbridgeDb = postgres.AddDatabase("silverbridgeDb");

var api = builder.AddProject<Projects.SilverbridgeWeb_Api>("silverbridgeweb-api")
    .WithReference(silverbridgeDb)
    .WaitFor(silverbridgeDb)
    .WithHttpHealthCheck("/health"); ;

builder.AddProject<Projects.SilverbridgeWeb>("frontend")
    .WithExternalHttpEndpoints()
    .WithUrlForEndpoint("https", url => url.DisplayText = "Website (HTTPS)")
    .WithUrlForEndpoint("http", url => url.DisplayText = "Website (HTTP)")
    .WithHttpHealthCheck("/health")
    .WithReference(api)
    .WaitFor(api);

builder.Build().Run();
