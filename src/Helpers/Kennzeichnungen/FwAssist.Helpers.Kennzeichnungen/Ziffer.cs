namespace FwAssist.Helpers.Kennzeichnungen;

/// <summary>One digit of a <see cref="Gefahrnummer"/> and what it says there.</summary>
public sealed record Ziffer(char Digit, string Meaning);
