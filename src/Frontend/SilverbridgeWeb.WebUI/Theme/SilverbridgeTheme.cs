using MudBlazor;

namespace SilverbridgeWeb.WebUI.Theme;

internal static class SilverbridgeTheme
{
    internal const string BrandBlue = "#263C9F";
    internal const string BrandYellow = "#FCCF02";
    internal const string AccessibleGold = "#735A00";

    internal static MudTheme Create() => new()
    {
        PaletteLight = new PaletteLight
        {
            Primary = BrandBlue,
            PrimaryContrastText = "#FFFFFF",
            Secondary = AccessibleGold,
            SecondaryContrastText = "#FFFFFF",
            Tertiary = "#0C6B58",
            TertiaryContrastText = "#FFFFFF",
            Background = "#F5F7FC",
            BackgroundGray = "#EDF1F8",
            Surface = "#FFFFFF",
            AppbarBackground = BrandBlue,
            AppbarText = "#FFFFFF",
            DrawerBackground = "#FFFFFF",
            DrawerText = "#1A2340",
            DrawerIcon = BrandBlue,
            TextPrimary = "#18213B",
            TextSecondary = "#566078",
            Divider = "#DCE2EE",
            LinesDefault = "#CBD3E2",
            LinesInputs = "#8A94A8",
            Info = "#1769AA",
            Success = "#18794E",
            Warning = "#9A6700",
            Error = "#B42318"
        },
        PaletteDark = new PaletteDark
        {
            Primary = "#AAB9FF",
            PrimaryContrastText = "#101946",
            Secondary = BrandYellow,
            SecondaryContrastText = "#17204A",
            Tertiary = "#69D6BC",
            TertiaryContrastText = "#082B24",
            Background = "#0C1224",
            BackgroundGray = "#111A31",
            Surface = "#151F38",
            AppbarBackground = "#111A38",
            AppbarText = "#F7F8FF",
            DrawerBackground = "#111A31",
            DrawerText = "#F1F4FF",
            DrawerIcon = "#C4CEFF",
            TextPrimary = "#F4F6FF",
            TextSecondary = "#B8C1D8",
            Divider = "#2C3855",
            LinesDefault = "#3A4662",
            LinesInputs = "#72809D",
            Info = "#75BDF2",
            Success = "#6DDBA7",
            Warning = "#FFD166",
            Error = "#FF8A80"
        },
        LayoutProperties = new LayoutProperties
        {
            DefaultBorderRadius = "12px",
            AppbarHeight = "68px",
            DrawerWidthLeft = "284px"
        }
    };
}
