using Kaizosoft.Localization.Constants;

namespace Kaizosoft.Localization;

/// <summary>
/// Builder class for creating KairosoftLanguage instances following the Builder pattern.
/// Uses immutable approach for better thread-safety and clarity.
/// </summary>
public sealed class KairosoftLanguageBuilder
{
    private readonly string title;
    private readonly string language;
    private readonly string applicationId;
    private readonly string version;
    private readonly string scale;
    private readonly string author;
    private readonly IReadOnlyList<KairosoftLanguageEntry> entries;

    public KairosoftLanguageBuilder(string title, string language, string applicationId, string version)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be null or empty.", nameof(title));
        if (string.IsNullOrWhiteSpace(language))
            throw new ArgumentException("Language cannot be null or empty.", nameof(language));
        if (string.IsNullOrWhiteSpace(applicationId))
            throw new ArgumentException("Application ID cannot be null or empty.", nameof(applicationId));
        if (string.IsNullOrWhiteSpace(version))
            throw new ArgumentException("Version cannot be null or empty.", nameof(version));

        this.title = title;
        this.language = language;
        this.applicationId = applicationId;
        this.version = version;
        scale = GameConstants.DefaultValues.SCALE;
        author = GameConstants.DefaultValues.AUTHOR;
        entries = [];
    }

    private KairosoftLanguageBuilder(
        string title,
        string language,
        string scale,
        string applicationId,
        string version,
        string author,
        IReadOnlyList<KairosoftLanguageEntry> entries)
    {
        this.title = title;
        this.language = language;
        this.scale = scale;
        this.applicationId = applicationId;
        this.version = version;
        this.author = author;
        this.entries = entries;
    }

    public KairosoftLanguageBuilder WithTitle(string title)
    {
        return string.IsNullOrWhiteSpace(title) ? throw new ArgumentException("Title cannot be null or empty.", nameof(title)) : new KairosoftLanguageBuilder(this.title, language, scale, applicationId, version, author, entries);
    }

    public KairosoftLanguageBuilder WithLanguage(string language)
    {
        return string.IsNullOrWhiteSpace(language) ? throw new ArgumentException("Language cannot be null or empty.", nameof(language)) : new KairosoftLanguageBuilder(title, language, scale, applicationId, version, author, entries);
    }

    public KairosoftLanguageBuilder WithScale(string scale)
    {
        return string.IsNullOrWhiteSpace(scale) ? throw new ArgumentException("Scale cannot be null or empty.", nameof(scale)) : new KairosoftLanguageBuilder(title, language, scale, applicationId, version, author, entries);
    }

    public KairosoftLanguageBuilder WithApplicationId(string applicationId)
    {
        return string.IsNullOrWhiteSpace(applicationId) ? throw new ArgumentException("Application ID cannot be null or empty.", nameof(applicationId)) : new KairosoftLanguageBuilder(title, language, scale, applicationId, version, author, entries);
    }

    public KairosoftLanguageBuilder WithAuthor(string author)
    {
        return string.IsNullOrWhiteSpace(author) ? throw new ArgumentException("Author cannot be null or empty.", nameof(author)) : new KairosoftLanguageBuilder(title, language, scale, applicationId, version, author, entries);
    }

    public KairosoftLanguageBuilder WithVersion(string version)
    {
        return string.IsNullOrWhiteSpace(version) ? throw new ArgumentException("Version cannot be null or empty.", nameof(version)) : new KairosoftLanguageBuilder(title, language, scale, applicationId, version, author, entries);
    }

    public KairosoftLanguageBuilder WithEntries(params IReadOnlyList<KairosoftLanguageEntry> entries)
    {
        return entries == null ? throw new ArgumentNullException(nameof(entries)) : new KairosoftLanguageBuilder(title, language, scale, applicationId, version, author, entries);
    }

    /// <summary>
    /// Builds the KairosoftLanguage instance with the configured values.
    /// </summary>
    public KairosoftLanguage Build()
    {
        return new KairosoftLanguage(title, language, scale, applicationId, version, author, entries);
    }
}
