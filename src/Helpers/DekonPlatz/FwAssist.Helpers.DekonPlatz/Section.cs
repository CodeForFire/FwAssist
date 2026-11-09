namespace FwAssist.Helpers.DekonPlatz;

/// <summary>A heading with its lines and drawings.</summary>
public sealed record Section(string Title, IReadOnlyList<Line> Lines, IReadOnlyList<SiteDrawing> Drawings);
