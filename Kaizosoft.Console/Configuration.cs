using System.Globalization;

namespace Kaizosoft.Console;

/// <summary>
/// Configuration for the application.
/// </summary>
public static class Configuration
{
    public static AndroidConfiguration? Android { get; set; }

    public static TranslationConfiguration? Translation { get; set; }
}

/// <summary>
/// Configuration settings for Android.
/// </summary>
public class AndroidConfiguration
{
    /// <summary>
    /// The path to the keystore file.
    /// </summary>
    public string KeystorePath { get; set; } = string.Empty;

    /// <summary>
    /// The keystore password.
    /// </summary>
    public string KeystorePassword { get; set; } = string.Empty;

    /// <summary>
    /// The key alias.
    /// </summary>
    public string KeyAlias { get; set; } = string.Empty;

    /// <summary>
    /// The key password.
    /// </summary>
    public string KeyPassword { get; set; } = string.Empty;
}

/// <summary>
/// Configuration settings for translation handling.
/// </summary>
public class TranslationConfiguration
{
    /// <summary>
    /// The suffix to append to translation files (e.g., ".translated").
    /// </summary>
    public string? TranslationSuffix { get; set; }

    /// <summary>
    /// The default language for translations.
    /// </summary>
    public CultureInfo? DefaultLanguage { get; set; }
}