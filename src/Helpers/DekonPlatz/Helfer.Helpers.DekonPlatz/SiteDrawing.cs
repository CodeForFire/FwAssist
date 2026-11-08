namespace Helfer.Helpers.DekonPlatz;

/// <summary>A drawing, with the description a screen reader reads out in its place.</summary>
public sealed record SiteDrawing(DrawingKind Kind, string Description, string Source);
