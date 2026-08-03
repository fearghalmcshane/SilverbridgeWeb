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

IResourceBuilder<AzureStorageResource> storage = builder.AddAzureStorage("storage");

if (!builder.ExecutionContext.IsPublishMode)
{
    storage.RunAsEmulator(emulator =>
    {
        emulator.WithDataVolume();
    });
}

IResourceBuilder<AzureBlobStorageResource> blobs = storage.AddBlobs("blobs");
IResourceBuilder<AzureBlobStorageContainerResource> newsMedia = storage.AddBlobContainer("newsMedia", "news-media");

IResourceBuilder<RedisResource> redis = builder.AddRedis("redis")
    .WithDataVolume();

IResourceBuilder<ParameterResource> clerkAuthority = builder.AddParameter("clerk-authority");
IResourceBuilder<ParameterResource> clerkApiKey = builder.AddParameter("clerk-api-key", secret: true);
IResourceBuilder<ParameterResource> clerkWebhookSigningSecret = builder.AddParameter("clerk-webhook-signing-secret", secret: true);
IResourceBuilder<ParameterResource> clerkClientId = builder.AddParameter("clerk-client-id");
IResourceBuilder<ParameterResource> clerkClientSecret = builder.AddParameter("clerk-client-secret", secret: true);

IResourceBuilder<ParameterResource> foireannPrimaryKey = builder.AddParameter("foireann-primary-api-key", secret: true);
IResourceBuilder<ParameterResource> foireannSecondaryKey = builder.AddParameter("foireann-secondary-api-key", secret: true);

IResourceBuilder<ProjectResource> migrator = builder.AddProject<Projects.SilverbridgeWeb_Migrator>("silverbridgeweb-migrator")
    .WithReference(silverbridgeDb)
    .WaitFor(silverbridgeDb)
    .WithEnvironment("Clerk__BackfillOnStartup", (!builder.ExecutionContext.IsPublishMode).ToString())
    .WithEnvironment("Clerk__Authority", clerkAuthority)
    .WithEnvironment("Clerk__ApiKey", clerkApiKey);

IResourceBuilder<ProjectResource> api = builder.AddProject<Projects.SilverbridgeWeb_Api>("silverbridgeweb-api")
    .WithReference(silverbridgeDb)
    .WithReference(blobs)
    .WithReference(newsMedia)
    .WithReference(redis)
    .WaitFor(redis)
    .WaitForCompletion(migrator)
    .WithEnvironment("Clerk__Authority", clerkAuthority)
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
