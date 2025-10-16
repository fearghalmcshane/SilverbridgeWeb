using Aspire.Hosting.Azure;

IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

IResourceBuilder<KeycloakResource> keycloak = builder.AddKeycloak("silverbridgeweb-auth", 8080)
    .WithDataVolume()
    .WithRealmImport("./Realms")
    .WithExternalHttpEndpoints();

IResourceBuilder<AzurePostgresFlexibleServerResource> postgres = builder.AddAzurePostgresFlexibleServer("postgres")
    .RunAsContainer(options => options.WithDataVolume());

IResourceBuilder<AzurePostgresFlexibleServerDatabaseResource> silverbridgeDb = postgres.AddDatabase("silverbridgeDb");

IResourceBuilder<RedisResource> redis = builder.AddRedis("redis")
    .WithDataVolume();

IResourceBuilder<ParameterResource> keycloakConfidentialClientSecret = builder.AddParameter("keycloak-confidential-client-secret", secret: true);

IResourceBuilder<ProjectResource> api = builder.AddProject<Projects.SilverbridgeWeb_Api>("silverbridgeweb-api")
    .WithReference(keycloak)
    .WithReference(silverbridgeDb)
    .WithReference(redis)
    .WithEnvironment("KeyCloak__Realm", "silverbridge")
    .WithEnvironment("KeyCloak__ConfidentialClientId", "silverbridge-confidential-client")
    .WithEnvironment("KeyCloak__ConfidentialClientSecret", keycloakConfidentialClientSecret)
    .WithEnvironment("KeyCloak__PublicClientId", "silverbridge-public-client")
    .WaitFor(keycloak)
    .WaitFor(silverbridgeDb)
    .WaitFor(redis)
    .WithHttpHealthCheck("/health");

builder.AddProject<Projects.SilverbridgeWeb_WebUI>("silverbridgeweb-webui")
    .WithExternalHttpEndpoints()
    .WithUrlForEndpoint("https", url => url.DisplayText = "Website (HTTPS)")
    .WithUrlForEndpoint("http", url => url.DisplayText = "Website (HTTP)")
    .WithHttpHealthCheck("/health")
    .WithReference(keycloak)
    .WithReference(api)
    .WaitFor(api);

builder.AddAzureContainerAppEnvironment("silverbridgeweb-env");

await builder.Build().RunAsync().ConfigureAwait(false);
