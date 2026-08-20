(() => {
    const preference = window.localStorage.getItem("silverbridge-theme-preference") ?? "System";
    const systemIsDark = window.matchMedia("(prefers-color-scheme: dark)").matches;
    const isDarkMode = preference === "Dark" || preference === "System" && systemIsDark;

    document.documentElement.dataset.themePreference = preference.toLowerCase();
    document.documentElement.style.colorScheme = isDarkMode ? "dark" : "light";
})();
