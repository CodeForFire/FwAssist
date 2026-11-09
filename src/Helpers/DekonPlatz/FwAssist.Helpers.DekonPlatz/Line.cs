namespace FwAssist.Helpers.DekonPlatz;

/// <summary>One line on the screen, and where its facts come from.</summary>
/// <param name="Text">The wording. Our own: no sentence is taken from a source.</param>
/// <param name="Source">The source's short name, as listed on <see cref="Dekonvorgaben"/>.</param>
public sealed record Line(string Text, string Source);
