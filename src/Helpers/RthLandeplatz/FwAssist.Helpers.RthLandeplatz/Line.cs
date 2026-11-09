namespace FwAssist.Helpers.RthLandeplatz;

/// <summary>One line on the screen, and where its facts come from.</summary>
/// <param name="Text">The wording. Our own: no sentence is taken from a source.</param>
/// <param name="Source">The source's short name, as listed on <see cref="Landeplatzvorgaben"/>.</param>
/// <param name="IsSubItem">Belongs to the line above it.</param>
public sealed record Line(string Text, string Source, bool IsSubItem = false);
