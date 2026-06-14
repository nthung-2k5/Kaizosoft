using System.Text;
using Kaizosoft.Localization.Constants;

namespace Kaizosoft.Localization.Helpers;

/// <summary>
/// Service for parsing Kairosoft language files.
/// </summary>
public static class LanguageFileParser
{
    public static string ParseApplicationId(string[] languageFileLines)
    {
        string? line = languageFileLines.FirstOrDefault(l => l.Contains(GameConstants.LanguageFileKeys.APPLICATION_ID, StringComparison.Ordinal));
        
        if (line == null)
        {
            throw new InvalidOperationException(
                $"Application ID not found in language file. Expected key: {GameConstants.LanguageFileKeys.APPLICATION_ID}");
        }

        return ExtractValue(line);
    }

    public static string ParseApplicationVersion(string[] languageFileLines)
    {
        string? line = languageFileLines.FirstOrDefault(l => l.Contains(GameConstants.LanguageFileKeys.VERSION, StringComparison.Ordinal));
        
        if (line == null)
        {
            throw new InvalidOperationException(
                $"Version not found in language file. Expected key: {GameConstants.LanguageFileKeys.VERSION}");
        }

        return ExtractValue(line);
    }

    public static KairosoftLanguageEntry[] ParseTemplateEntries(byte[] templateData)
    {
        if (templateData == null || templateData.Length == 0)
        {
            throw new ArgumentException("Template data cannot be null or empty.", nameof(templateData));
        }
        
        var templateSpan = templateData.AsSpan();
        if (templateSpan.StartsWith(Encoding.UTF8.Preamble))
        {
            // Skip BOM if present
            templateSpan = templateSpan[Encoding.UTF8.Preamble.Length..];
        }

        string content = Encoding.UTF8.GetString(templateSpan);
        var rawEntries = content.ReplaceLineEndings()
                                .Split(Environment.NewLine)
                                .Select(line => 
                                {
                                    string[] split = line.Split(',', 4);
                                    return (Key: split[0], split[^1]); 
                                })
                                .ToArray();

        return ParseTemplateEntries(rawEntries);
    }
    
    public static KairosoftLanguageEntry[] ParseTemplateEntries(params IReadOnlyList<(string Key, string Text)> rawEntries)
    {
        var entries = new List<KairosoftLanguageEntry>();

        for (int i = 0; i < rawEntries.Count;)
        {
            (string key, string value) = rawEntries[i];
            
            var lines = new List<string> { value };
            ++i;
            
            while (i < rawEntries.Count && string.IsNullOrWhiteSpace(rawEntries[i].Key))
            {
                lines.Add(rawEntries[i].Text);
                ++i;
            }

            if (!int.TryParse(key, out int index))
            {
                throw new FormatException($"Invalid entry key format: {key}");
            }

            entries.Add(new KairosoftLanguageEntry(index, lines.ToArray()));
        }

        return entries.ToArray();
    }

    private static string ExtractValue(string line)
    {
        string[] parts = line.Split(',', 2);
        return parts.Length < 2 ? throw new FormatException($"Invalid line format: {line}") : parts[1].Trim('"');
    }
}
