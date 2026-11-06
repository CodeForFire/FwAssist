namespace Helfer.Helpers.Funkalphabet;

/// <summary>What is said for one symbol: "A" is "Anton", "2" is "zwo". A sign stands for itself.</summary>
public sealed record CodeWord(string Symbol, string Word)
{
    /// <summary>The pause between two words of the spelled text.</summary>
    public static CodeWord Gap { get; } = new(string.Empty, string.Empty);
}
