using Aspire.Hosting.Azure;

const string keycloakRealm = "silverbridge";

IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

IResourceBuilder<KeycloakResource> keycloak = builder.AddKeycloak("silverbridgewebAuth", 8085)
    .WithDataVolume()
    .WithRealmImport("./Realms");

var keycloakEndpoint = ReferenceExpression.Create($"{keycloak.GetEndpoint("http").Property(EndpointProperty.Url)}");
var keycloakAuthority = ReferenceExpression.Create($"{keycloak.GetEndpoint("http").Property(EndpointProperty.Url)}/realms/{keycloakRealm}");

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
    .WaitFor(keycloak)
    .WaitFor(silverbridgeDb)
    .WaitFor(redis)
    .WithEnvironment("KeyCloak__Url", keycloakEndpoint)
    .WithEnvironment("KeyCloak__Authority", keycloakAuthority)
    .WithEnvironment("KeyCloak__Realm", keycloakRealm)
    .WithEnvironment("KeyCloak__ConfidentialClientId", "silverbridge-confidential-client")
    .WithEnvironment("KeyCloak__ConfidentialClientSecret", keycloakConfidentialClientSecret)
    .WithEnvironment("KeyCloak__PublicClientId", "silverbridge-public-client")
    .WithHttpHealthCheck("/health");

IResourceBuilder<ParameterResource> keycloakClientSecret = builder.AddParameter("keycloakClientSecret", secret: true);

builder.AddProject<Projects.SilverbridgeWeb_WebUI>("silverbridgeweb-webui")
    .WithExternalHttpEndpoints()
    .WithUrlForEndpoint("https", url => url.DisplayText = "Website (HTTPS)")
    .WithUrlForEndpoint("http", url => url.DisplayText = "Website (HTTP)")
    .WithHttpHealthCheck("/health")
    .WithReference(keycloak)
    .WithReference(api)
    .WaitFor(api)
    .WithEnvironment("Authentication__Schemes__OpenIdConnect__ClientSecret", keycloakClientSecret)
    .WithEnvironment("Authentication__Schemes__OpenIdconnect__Authority", keycloakAuthority);

string acaEnvironmentName = Environment.GetEnvironmentVariable("ACA_ENVIRONMENT_NAME") ?? "silverbridgeweb-env";

builder.AddAzureContainerAppEnvironment(acaEnvironmentName);

await builder.Build().RunAsync().ConfigureAwait(false);
