using Aspire.Hosting.Azure;

const string keycloakRealm = "silverbridge";

IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

IResourceBuilder<AzurePostgresFlexibleServerResource> postgres = builder.AddAzurePostgresFlexibleServer("postgres")
    .RunAsContainer(options =>
    {
        options.WithDataVolume();
        options.WithPgAdmin(pgadmin =>
        {
            pgadmin.WithHostPort(5050);
        });
    });

IResourceBuilder<AzurePostgresFlexibleServerDatabaseResource> silverbridgeDb = postgres.AddDatabase("silverbridgeDb");

IResourceBuilder<ParameterResource> keycloakPassword = builder.AddParameter("keycloakPassword", secret: true, value: "admin");
int? keycloakPort = builder.ExecutionContext.IsRunMode ? 8085 : null;

IResourceBuilder<KeycloakResource> keycloak = builder.AddKeycloak("silverbridgewebAuth", adminPassword: keycloakPassword, port: keycloakPort)
    .WithLifetime(ContainerLifetime.Persistent);

if (builder.ExecutionContext.IsRunMode)
{
    keycloak.WithDataVolume()
        .WithRealmImport("./Realms");
}

if (builder.ExecutionContext.IsPublishMode)
{
    IResourceBuilder<ParameterResource> postgresUser = builder.AddParameter("PostgresUser", value: "postgres");
    IResourceBuilder<ParameterResource> postgresPassword = builder.AddParameter("PostgresPassword", secret: true);
    postgres.WithPasswordAuthentication(postgresUser, postgresPassword);

    IResourceBuilder<AzurePostgresFlexibleServerDatabaseResource> keycloakDb = postgres.AddDatabase("keycloakDb");

    var keycloakDbUrl = ReferenceExpression.Create($"jdbc:postgresql://{postgres.Resource.HostName}/{keycloakDb.Resource.DatabaseName}");

    keycloak.WithEndpoint("http", e =>
        {
            e.IsExternal = true;
            e.UriScheme = "https";
        });

    var keycloakExternalUrl = ReferenceExpression.Create($"{keycloak.GetEndpoint("http").Property(EndpointProperty.Url)}");

    keycloak.WithEnvironment("KC_HTTP_ENABLED", "true")
        .WithEnvironment("KC_PROXY_HEADERS", "xforwarded")
        .WithEnvironment("KC_HOSTNAME", keycloakExternalUrl)
        .WithEnvironment("KC_HOSTNAME_STRICT", "true")
        .WithEnvironment("KC_DB", "postgres")
        .WithEnvironment("KC_DB_URL", keycloakDbUrl)
        .WithEnvironment("KC_DB_USERNAME", postgresUser)
        .WithEnvironment("KC_DB_PASSWORD", postgresPassword);
}

var keycloakEndpoint = ReferenceExpression.Create($"{keycloak.GetEndpoint("http").Property(EndpointProperty.Url)}");
var keycloakAuthority = ReferenceExpression.Create($"{keycloak.GetEndpoint("http").Property(EndpointProperty.Url)}/realms/{keycloakRealm}");

IResourceBuilder<RedisResource> redis = builder.AddRedis("redis")
    .WithDataVolume();

IResourceBuilder<ParameterResource> keycloakConfidentialClientSecret = builder.AddParameter("keycloak-confidential-client-secret", secret: true);

IResourceBuilder<ParameterResource> foireannPrimaryKey = builder.AddParameter("foireann-primary-api-key", secret: true);
IResourceBuilder<ParameterResource> foireannSecondaryKey = builder.AddParameter("foireann-secondary-api-key", secret: true);

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
    .WithEnvironment("Foireann__PrimaryApiKey", foireannPrimaryKey)
    .WithEnvironment("Foireann__SecondaryApiKey", foireannSecondaryKey)
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
