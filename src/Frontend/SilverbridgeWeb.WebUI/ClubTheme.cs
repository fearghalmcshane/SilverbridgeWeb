using MudBlazor;

namespace SilverbridgeWeb.WebUI;

internal static class ClubTheme
{
    private static readonly string[] BodyFonts = ["Inter", "Helvetica", "Arial", "sans-serif"];
    private static readonly string[] HeadingFonts = ["Montserrat", "sans-serif"];

    public static readonly MudTheme Theme = new()
    {
        PaletteLight = new PaletteLight
        {
            Primary = "#263C9F",
            Secondary = "#FCCF02",
            AppbarBackground = "#263C9F",
            AppbarText = "#FFFFFF",
            DrawerBackground = "#263C9F",
            DrawerText = "#FFFFFF",
            DrawerIcon = "#FCCF02",
        },
        PaletteDark = new PaletteDark
        {
            Primary = "#4A64C8",
            Secondary = "#FCCF02",
            AppbarBackground = "#1A2B70",
            DrawerBackground = "#1A2B70",
            DrawerText = "#FFFFFF",
            DrawerIcon = "#FCCF02",
        },
        Typography = new Typography
        {
            Default = new DefaultTypography { FontFamily = BodyFonts },
            H1 = new H1Typography { FontFamily = HeadingFonts, FontWeight = "700" },
            H2 = new H2Typography { FontFamily = HeadingFonts, FontWeight = "700" },
            H3 = new H3Typography { FontFamily = HeadingFonts, FontWeight = "700" },
            H4 = new H4Typography { FontFamily = HeadingFonts, FontWeight = "700" },
            H5 = new H5Typography { FontFamily = HeadingFonts, FontWeight = "600" },
            H6 = new H6Typography { FontFamily = HeadingFonts, FontWeight = "600" },
        },
        LayoutProperties = new LayoutProperties
        {
            DefaultBorderRadius = "8px",
        },
    };
}
