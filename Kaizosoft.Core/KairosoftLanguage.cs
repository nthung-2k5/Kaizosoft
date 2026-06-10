using System.Text;
using Kaizosoft.Core.Constants;
using nietras.SeparatedValues;

namespace Kaizosoft.Core;

/// <summary>
/// Represents a Kairosoft language file with all its metadata and translation entries.
/// </summary>
/// <param name="Title">The display title of the language file.</param>
/// <param name="Language">The language code (e.g., "en", "jp").</param>
/// <param name="Scale">The scale setting for the UI.</param>
/// <param name="ApplicationId">The application identifier.</param>
/// <param name="Version">The application version.</param>
/// <param name="Author">The author of the translation.</param>
/// <param name="Entries">The collection of translation entries.</param>
public record KairosoftLanguage(
    string Title, 
    string Language, 
    string Scale, 
    string ApplicationId, 
    string Version, 
    string Author, 
    IReadOnlyList<KairosoftLanguageEntry> Entries)
{
    /// <summary>
    /// Converts the language file to a CSV string format suitable for external editing tools.
    /// </summary>
    public string ToCsvString()
    {
        var lines = Entries.SelectMany(entry =>
        {
            var text = entry.Text;
            string[][] result = new string[entry.Text.Count][];
            result[0] = [entry.Key.ToString(), text[0]];

            for (int i = 1; i < text.Count; i++)
            {
                result[i] = ["", text[i]];
            }

            return result;
        });
        
        using var writer = Sep.Writer(opt => opt with
        {
            Escape = true
        }).ToText();
        
        foreach (string[] line in lines)
        {
            using var writeRow = writer.NewRow();
            writeRow["Key", "Text"].Set(line);
        }

        return writer.ToString();
    }
    
    /// <summary>
    /// Converts the language file to the Kairosoft game format string.
    /// </summary>
    public override string ToString()
    {
        var stream = new StringBuilder();
        
        stream.AppendLine($"{GameConstants.LanguageFileKeys.TITLE},{Title}")
              .AppendLine($"{GameConstants.LanguageFileKeys.LANGUAGE},{Language}")
              .AppendLine($"{GameConstants.LanguageFileKeys.SCALE},{Scale}")
              .AppendLine($"{GameConstants.LanguageFileKeys.APPLICATION_ID},{ApplicationId}")
              .AppendLine($"{GameConstants.LanguageFileKeys.VERSION},{Version}")
              .Append($"{GameConstants.LanguageFileKeys.AUTHOR},{Author}");

        foreach ((int key, var text) in Entries)
        {
            stream.AppendLine().Append($"#{key:00000},{text[0]}");
            for (int i = 1; i < text.Count; i++)
            {
                stream.AppendLine().Append($",{text[i]}");
            }
        }
        
        return stream.ToString();
    }
}


