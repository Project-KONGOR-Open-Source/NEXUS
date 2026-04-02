const storageKey = "portal.theme";

export function getThemePreference() {
    const storedMode = window.localStorage.getItem(storageKey);

    if (storedMode === "dark" || storedMode === "light") {
        return {
            mode: storedMode,
            source: "stored"
        };
    }

    const mediaQueryList = window.matchMedia("(prefers-color-scheme: dark)");

    return {
        mode: mediaQueryList.matches ? "dark" : "light",
        source: "system"
    };
}

export function setThemePreference(mode) {
    if (mode === "dark" || mode === "light") {
        window.localStorage.setItem(storageKey, mode);
        return;
    }

    window.localStorage.removeItem(storageKey);
}
