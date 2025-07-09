namespace SilverbridgeWeb.Api.Extensions;

internal static class ConfigurationExtensions
{
    internal static void AddModuleConfiguration(this IConfigurationBuilder configurationBuidler, string[] modules)
    {
        foreach (string module in modules)
        {
            configurationBuidler.AddJsonFile($"modules.{module}.json", optional: false, reloadOnChange: true);
            configurationBuidler.AddJsonFile($"modules.{module}.Development.json", optional: true, reloadOnChange: true);
        }
    }
}
