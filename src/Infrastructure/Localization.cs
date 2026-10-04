using System.Globalization;
using LrCatalogSync.Resources.Strings;

namespace LrCatalogSync.Infrastructure;

public sealed record LanguageOption(string Code, string DisplayName);

public static class Localization
{
    public const string SystemCode = "System";

    private static readonly CultureInfo systemUiCulture = CultureInfo.CurrentUICulture;
    private static CultureInfo activeUiCulture = systemUiCulture;

    public static IReadOnlyList<LanguageOption> Languages => new[]
    {
        new LanguageOption(SystemCode, Strings.Language_SystemWindows),
        new LanguageOption("en", "English"),
        new LanguageOption("de", "Deutsch")
    };

    public static void Apply(string code)
    {
        CultureInfo culture = code switch
        {
            "en" => CultureInfo.GetCultureInfo("en"),
            "de" => CultureInfo.GetCultureInfo("de"),
            _ => systemUiCulture
        };

        activeUiCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;
        ApplyToCurrentThread();
    }

    public static void ApplyToCurrentThread()
    {
        CultureInfo.CurrentUICulture = activeUiCulture;
    }
}