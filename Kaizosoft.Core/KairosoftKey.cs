namespace Kaizosoft.Core;

/// <summary>
/// Represents an encryption key for a Kairosoft game.
/// </summary>
/// <param name="EnglishName">The English name of the game.</param>
/// <param name="JapaneseName">The Japanese name of the game.</param>
/// <param name="Key">The encryption key bytes.</param>
public record KairosoftKey(string EnglishName, string JapaneseName, byte[] Key);
