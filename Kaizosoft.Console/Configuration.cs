using System.Globalization;

namespace Kaizosoft.Console;

/// <summary>
/// Global configuration holder for the application.
/// </summary>
public static class Configuration
{
    /// <summary>
    /// The APK signer configuration.
    /// </summary>
    public static ApkSignerConfiguration ApkSigner { get; set; } = null!;

    public static string? TranslationSuffix { get; set; }

    /// <summary>
    /// The default culture for translations.
    /// </summary>
    public static CultureInfo? DefaultCulture { get; set; }
}

/// <summary>
/// Configuration settings for APK signing.
/// </summary>
public class ApkSignerConfiguration
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
