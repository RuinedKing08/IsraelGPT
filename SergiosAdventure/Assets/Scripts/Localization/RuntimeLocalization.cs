using System;
using UnityEngine;
using UnityEngine.Localization.Settings;

public static class RuntimeLocalization
{
    public const string DefaultTable = "StringTable";

    public static string GetText(string entryKey, string fallback, params object[] arguments)
    {
        if (string.IsNullOrWhiteSpace(entryKey))
        {
            return fallback ?? string.Empty;
        }

        try
        {
            string localized = arguments == null || arguments.Length == 0
                ? LocalizationSettings.StringDatabase.GetLocalizedString(DefaultTable, entryKey)
                : LocalizationSettings.StringDatabase.GetLocalizedString(DefaultTable, entryKey, arguments);

            return string.IsNullOrWhiteSpace(localized) ? fallback ?? string.Empty : localized;
        }
        catch (Exception exception)
        {
            Debug.LogWarning($"Localization lookup failed for key '{entryKey}': {exception.Message}");
            return fallback ?? string.Empty;
        }
    }

    public static bool TrySelectLocale(string localeCode)
    {
        if (string.IsNullOrWhiteSpace(localeCode) || LocalizationSettings.AvailableLocales == null)
        {
            return false;
        }

        var locale = LocalizationSettings.AvailableLocales.GetLocale(localeCode.Trim());
        if (locale == null)
        {
            return false;
        }

        LocalizationSettings.SelectedLocale = locale;
        return true;
    }

    public static string GetSelectedLocaleCode()
    {
        return LocalizationSettings.SelectedLocale?.Identifier.Code ?? string.Empty;
    }
}
