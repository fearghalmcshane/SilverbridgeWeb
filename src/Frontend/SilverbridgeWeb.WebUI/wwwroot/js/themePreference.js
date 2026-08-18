const storageKey = "silverbridge-theme-preference";
const validPreferences = new Set(["System", "Light", "Dark"]);

let mediaQuery;
let mediaQueryHandler;

export function initialize(dotNetReference) {
    dispose();

    mediaQuery = window.matchMedia("(prefers-color-scheme: dark)");
    mediaQueryHandler = event => {
        applyEffectiveTheme(getStoredPreference(), event.matches);
        dotNetReference.invokeMethodAsync("OnSystemThemeChanged", event.matches);
    };
    mediaQuery.addEventListener("change", mediaQueryHandler);

    const preference = getStoredPreference();
    const isDarkMode = getEffectiveDarkMode(preference, mediaQuery.matches);
    applyEffectiveTheme(preference, mediaQuery.matches);

    return { preference, isDarkMode };
}

export function setPreference(preference) {
    const normalizedPreference = validPreferences.has(preference) ? preference : "System";

    if (normalizedPreference === "System") {
        window.localStorage.removeItem(storageKey);
    } else {
        window.localStorage.setItem(storageKey, normalizedPreference);
    }

    const systemIsDark = mediaQuery?.matches ?? window.matchMedia("(prefers-color-scheme: dark)").matches;
    applyEffectiveTheme(normalizedPreference, systemIsDark);
    return getEffectiveDarkMode(normalizedPreference, systemIsDark);
}

export function dispose() {
    if (mediaQuery && mediaQueryHandler) {
        mediaQuery.removeEventListener("change", mediaQueryHandler);
    }

    mediaQuery = undefined;
    mediaQueryHandler = undefined;
}

function getStoredPreference() {
    const preference = window.localStorage.getItem(storageKey);
    return validPreferences.has(preference) ? preference : "System";
}

function getEffectiveDarkMode(preference, systemIsDark) {
    return preference === "Dark" || preference === "System" && systemIsDark;
}

function applyEffectiveTheme(preference, systemIsDark) {
    const isDarkMode = getEffectiveDarkMode(preference, systemIsDark);
    document.documentElement.dataset.themePreference = preference.toLowerCase();
    document.documentElement.style.colorScheme = isDarkMode ? "dark" : "light";
}
