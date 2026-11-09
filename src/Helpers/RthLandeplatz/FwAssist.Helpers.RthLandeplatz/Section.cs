namespace FwAssist.Helpers.RthLandeplatz;

/// <summary>A heading with its lines. A phase's heading stays with its lines at all times.</summary>
public sealed record Section(string Title, IReadOnlyList<Line> Lines, IReadOnlyList<SiteDrawing> Drawings);
