using Kaizosoft.Core.Interfaces;
using nietras.SeparatedValues;

namespace Kaizosoft.Core.Repositories;

/// <summary>
/// Repository for managing Kairosoft game encryption keys.
/// </summary>
public sealed class KeyRepository : IKeyRepository
{
    private readonly List<KairosoftKey> keys = [];

    public KairosoftKey? GetKeyByGameName(string gameName)
    {
        return keys.Find(k => 
            k.EnglishName.Equals(gameName, StringComparison.OrdinalIgnoreCase) || 
            k.JapaneseName.Equals(gameName, StringComparison.OrdinalIgnoreCase));
    }

    public void LoadKeysFromFile(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Keys file not found: {filePath}", filePath);
        }

        keys.Clear();
        
        using var csv = Sep.Reader().FromFile(filePath);
        foreach (var readRow in csv)
        {
            if (readRow.ColCount < 3)
            {
                continue; // Skip invalid rows
            }

            string englishName = readRow[0].ToString();
            string japaneseName = readRow[1].ToString();
            byte[] key = Convert.FromHexString(readRow[2].ToString());
            
            keys.Add(new KairosoftKey(englishName, japaneseName, key));
        }
    }

    public HashSet<byte[]> Keys => keys.Select(k => k.Key).Distinct().ToHashSet();
}
