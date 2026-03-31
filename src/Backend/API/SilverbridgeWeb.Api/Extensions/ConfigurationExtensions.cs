namespace SilverbridgeWeb.Api.Extensions;

internal static class ConfigurationExtensions
{
    internal static void AddModuleConfiguration(this IConfigurationBuilder configurationBuilder, string[] modules)
    {
        foreach (string module in modules)
        {
            configurationBuilder.AddJsonFile($"modules.{module}.json", optional: false, reloadOnChange: true);
            configurationBuilder.AddJsonFile($"modules.{module}.Development.json", optional: true, reloadOnChange: true);
        }

        // Re-add environment variables so they take precedence over module JSON files.
        // This ensures values injected by Aspire (e.g. Foireann__PrimaryApiKey) are not
        // overridden by placeholder values in the module config files.
        configurationBuilder.AddEnvironmentVariables();
    }
}
