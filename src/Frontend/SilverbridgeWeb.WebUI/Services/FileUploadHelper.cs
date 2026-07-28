using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Components.Forms;

namespace SilverbridgeWeb.WebUI.Services;

internal static class FileUploadHelper
{
    [SuppressMessage(
        "Sonar",
        "S5693:Limit the content length of HTTP requests",
        Justification = "The browser file stream is explicitly capped at 25MB to match the backend News media upload limit.")]
    public static Stream OpenLimitedReadStream(IBrowserFile file, long maxAllowedSize)
    {
        return file.OpenReadStream(maxAllowedSize);
    }
}
