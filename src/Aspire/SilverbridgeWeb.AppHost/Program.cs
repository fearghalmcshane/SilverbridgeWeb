using Aspire.Hosting.Azure;

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

if (builder.ExecutionContext.IsPublishMode)
{
    IResourceBuilder<ParameterResource> postgresUser = builder.AddParameter("PostgresUser", value: "postgres");
    IResourceBuilder<ParameterResource> postgresPassword = builder.AddParameter("PostgresPassword", secret: true);
    postgres.WithPasswordAuthentication(postgresUser, postgresPassword);
}

IResourceBuilder<RedisResource> redis = builder.AddRedis("redis")
    .WithDataVolume();

IResourceBuilder<ParameterResource> clerkAuthority = builder.AddParameter("clerk-authority");
IResourceBuilder<ParameterResource> clerkSecretKey = builder.AddParameter("clerk-secret-key", secret: true);
IResourceBuilder<ParameterResource> clerkWebhookSigningSecret = builder.AddParameter("clerk-webhook-signing-secret", secret: true);
IResourceBuilder<ParameterResource> clerkClientId = builder.AddParameter("clerk-client-id");
IResourceBuilder<ParameterResource> clerkClientSecret = builder.AddParameter("clerk-client-secret", secret: true);

IResourceBuilder<ParameterResource> foireannPrimaryKey = builder.AddParameter("foireann-primary-api-key", secret: true);
IResourceBuilder<ParameterResource> foireannSecondaryKey = builder.AddParameter("foireann-secondary-api-key", secret: true);

IResourceBuilder<ProjectResource> api = builder.AddProject<Projects.SilverbridgeWeb_Api>("silverbridgeweb-api")
    .WithReference(silverbridgeDb)
    .WithReference(redis)
    .WaitFor(silverbridgeDb)
    .WaitFor(redis)
    .WithEnvironment("Clerk__Authority", clerkAuthority)
    .WithEnvironment("Clerk__SecretKey", clerkSecretKey)
    .WithEnvironment("Clerk__WebhookSigningSecret", clerkWebhookSigningSecret)
    .WithEnvironment("Foireann__PrimaryApiKey", foireannPrimaryKey)
    .WithEnvironment("Foireann__SecondaryApiKey", foireannSecondaryKey)
    .WithHttpHealthCheck("/health");

builder.AddProject<Projects.SilverbridgeWeb_WebUI>("silverbridgeweb-webui")
    .WithExternalHttpEndpoints()
    .WithUrlForEndpoint("https", url => url.DisplayText = "Website (HTTPS)")
    .WithUrlForEndpoint("http", url => url.DisplayText = "Website (HTTP)")
    .WithHttpHealthCheck("/health")
    .WithReference(api)
    .WaitFor(api)
    .WithEnvironment("Clerk__Authority", clerkAuthority)
    .WithEnvironment("Clerk__ClientId", clerkClientId)
    .WithEnvironment("Clerk__ClientSecret", clerkClientSecret);

string acaEnvironmentName = Environment.GetEnvironmentVariable("ACA_ENVIRONMENT_NAME") ?? "silverbridgeweb-env";

builder.AddAzureContainerAppEnvironment(acaEnvironmentName);

await builder.Build().RunAsync().ConfigureAwait(false);
