using System.Globalization;
using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using FwAssist.AppLogic.Services.Map;
using FwAssist.Core.Map;

namespace FwAssist.Ui.Map;

/// <summary>
/// The Löschwasser markers on the map. Each is anchored exactly on its water point: from
/// <see cref="WaterPointLayer.MinZoom"/> a disc centred on it, from
/// <see cref="WaterPointLayer.MediumMinZoom"/> a flag whose mast foot stands on it and whose body
/// grows to the right, so the body never covers the spot and close masts stay apart. The glyph in
/// the coloured field says what it is, the colour whether it hangs on the Trinkwassernetz (red) or
/// is independent of it (blue), the text on white what OSM knows, and the body is as wide as its
/// text.
/// </summary>
public static class WaterPointMarkers
{
    // A glyph is drawn into a square this many units wide.
    private const double GlyphUnits = 12;

    private const double Halo = 3;

    // The zoom-15 disc: the coloured dot's radius, and how far its white contour reaches.
    private const double DiscRadius = 4;
    private const double DiscReach = 6;

    // Where a flag has no text, the field alone, with this much white after it.
    private const double EmptyTextWidth = 2;

    // How thick the outline of the chosen water point is, and how far its arrow keeps above it.
    private const double SelectedOutline = 3.5;
    private const double PointerGap = 3;

    // The arrow over the chosen water point, its tip at the origin: a shaft and a head, so it reads as
    // an arrow pointing at the marker and not as a second pin like the Einsatzstelle's.
    private static readonly Geometry PointerShape = Geometry.Parse("M0,0 L-15,-16 L-6,-16 L-6,-32 L6,-32 L6,-16 L15,-16 Z");
    private static readonly IPen PointerHalo = new ImmutablePen(Brushes.White, 3, lineJoin: PenLineJoin.Round);

    private static readonly CultureInfo De = CultureInfo.GetCultureInfo("de-DE");
    private static readonly IImmutableBrush Ink = new ImmutableSolidColorBrush(Color.FromRgb(0x21, 0x21, 0x21));
    private static readonly IPen HaloPen = new ImmutablePen(Brushes.White, Halo, lineJoin: PenLineJoin.Round);
    private static readonly IPen GlyphPen = new ImmutablePen(Brushes.White, 1.4, lineCap: PenLineCap.Round, lineJoin: PenLineJoin.Round);
    private static readonly IPen BlockingBar = new ImmutablePen(new ImmutableSolidColorBrush(Color.FromRgb(0xD3, 0x2F, 0x2F)), 2, lineCap: PenLineCap.Round);
    private static readonly Typeface Regular = new(FontFamily.Default);

    /// <summary>A hydrant: fed from the Trinkwassernetz. 5.6:1 against the white body.</summary>
    public static IImmutableBrush MainsBrush { get; } = Solid(0xC6, 0x28, 0x28);

    /// <summary>A Saugstelle, Brunnen, Behälter or Teich: independent of the mains. 5.7:1 against white.</summary>
    public static IImmutableBrush IndependentBrush { get; } = Solid(0x15, 0x65, 0xC0);

    /// <summary>Außer Betrieb: the colour gone, whatever the source.</summary>
    public static IImmutableBrush OutOfServiceBrush { get; } = Solid(0x8A, 0x91, 0x99);

    /// <summary>The arrow over the chosen water point: the app's signal red, on a day and a night map alike.</summary>
    public static IImmutableBrush PointerBrush { get; } = Solid(0xFF, 0x3D, 0x2E);

    /// <summary>UH: a lid with its key hole.</summary>
    public static Geometry UnterflurhydrantGlyph { get; } = Geometry.Parse(
        "M2,2.5 H10 A1.2,1.2 0 0 1 11.2,3.7 V8.3 A1.2,1.2 0 0 1 10,9.5 H2 A1.2,1.2 0 0 1 0.8,8.3 V3.7 A1.2,1.2 0 0 1 2,2.5 Z "
        + "M5,6 A1,1 0 1 0 7,6 A1,1 0 1 0 5,6 Z");

    /// <summary>ÜH: the pillar with its outlets.</summary>
    public static Geometry UeberflurhydrantGlyph { get; } = Geometry.Parse("M6,1.5 V10.5 M4.5,1.5 H7.5 M2.5,4.5 H9.5 M3,10.5 H9");

    /// <summary>WH: a coupling on a wall.</summary>
    public static Geometry WandhydrantGlyph { get; } = Geometry.Parse(
        "M1.8,1.5 V10.5 M5,6 A3,3 0 1 0 11,6 A3,3 0 1 0 5,6 Z M7.4,6 A0.6,0.6 0 1 0 8.6,6 A0.6,0.6 0 1 0 7.4,6 Z");

    /// <summary>SS: water drawn down into the strainer.</summary>
    public static Geometry SaugstelleGlyph { get; } = Geometry.Parse(
        "M1.5,6 A4.5,4.5 0 1 0 10.5,6 A4.5,4.5 0 1 0 1.5,6 Z M6,3 V8.5 M4,6.5 L6,8.5 L8,6.5");

    /// <summary>BR: a well shaft.</summary>
    public static Geometry LoeschwasserbrunnenGlyph { get; } = Geometry.Parse("M1.5,3 H10.5 M3,3 V10.5 H9 V3 M4.5,1.2 H7.5 M5,5 V8.5 M7,5 V8.5");

    /// <summary>ZI: a tank with water in it.</summary>
    public static Geometry ZisterneGlyph { get; } = Geometry.Parse(
        "M2,2.5 H10 A1.2,1.2 0 0 1 11.2,3.7 V8.3 A1.2,1.2 0 0 1 10,9.5 H2 A1.2,1.2 0 0 1 0.8,8.3 V3.7 A1.2,1.2 0 0 1 2,2.5 Z "
        + "M3,6.5 Q4.5,4.5 6,6.5 Q7.5,8.5 9,6.5");

    /// <summary>OW: open water.</summary>
    public static Geometry OffenesGewaesserGlyph { get; } = Geometry.Parse(
        "M1,3 Q2.5,1.5 4,3 Q5.5,4.5 7,3 Q8.5,1.5 10,3 L11,3 "
        + "M1,6 Q2.5,4.5 4,6 Q5.5,7.5 7,6 Q8.5,4.5 10,6 L11,6 "
        + "M1,9 Q2.5,7.5 4,9 Q5.5,10.5 7,9 Q8.5,7.5 10,9 L11,9");

    /// <summary>The colour of <paramref name="point"/>'s marker: by its source, grey out of service.</summary>
    public static IImmutableBrush SourceBrush(WaterPoint point)
    {
        ArgumentNullException.ThrowIfNull(point);
        return point.IsOutOfService ? OutOfServiceBrush : point.IsIndependent ? IndependentBrush : MainsBrush;
    }

    /// <summary>The glyph of <paramref name="symbol"/>, drawn into a 12-unit square.</summary>
    public static Geometry Glyph(WaterPointSymbol symbol) => symbol switch
    {
        WaterPointSymbol.Ueberflurhydrant => UeberflurhydrantGlyph,
        WaterPointSymbol.Wandhydrant => WandhydrantGlyph,
        WaterPointSymbol.Saugstelle => SaugstelleGlyph,
        WaterPointSymbol.Loeschwasserbrunnen => LoeschwasserbrunnenGlyph,
        WaterPointSymbol.Zisterne => ZisterneGlyph,
        WaterPointSymbol.OffenesGewaesser => OffenesGewaesserGlyph,
        _ => UnterflurhydrantGlyph,
    };

    /// <summary>
    /// The markers of the <paramref name="points"/> inside the visible map, in the order they are
    /// drawn: by <see cref="WaterPointLayer.StackRank"/>, and the one whose card is open last.
    /// Below <see cref="WaterPointLayer.MinZoom"/> only the one whose card is open.
    /// </summary>
    internal static List<Marker> LayOut(IReadOnlyList<WaterPoint> points, GeoPoint center, int zoom, Size size, WaterPoint? selected, double rotation)
    {
        var markers = new List<Marker>();
        if (WaterPointLayer.LevelAt(zoom) is null)
        {
            if (selected is null)
            {
                return markers;
            }

            points = [selected];
        }

        var level = WaterPointLayer.SelectedLevelAt(zoom);

        // Compared in degrees first: thousands of points, a few dozen of them on screen. On a
        // turned map the screen's corners are no longer north-west and south-east, so take all four.
        // The margin covers a flag whose foot is just off the left or bottom edge.
        const double margin = 120;
        Point[] corners = [new(-margin, -margin), new(size.Width + margin, -margin), new(-margin, size.Height + margin), new(size.Width + margin, size.Height + margin)];
        var reach = corners.Select(c => MapDrawing.ToGeo(center, zoom, size, c, rotation)).ToList();
        var (north, south) = (reach.Max(g => g.Latitude), reach.Min(g => g.Latitude));
        var (west, east) = (reach.Min(g => g.Longitude), reach.Max(g => g.Longitude));
        foreach (var point in points)
        {
            var p = point.Position;
            if (p.Latitude > north || p.Latitude < south || p.Longitude < west || p.Longitude > east)
            {
                continue;
            }

            var at = MapDrawing.ToScreen(center, zoom, size, p, rotation);
            markers.Add(Marker.For(point, level, at, ReferenceEquals(point, selected)));
        }

        return [.. markers.OrderBy(m => WaterPointLayer.StackRank(m.Point, selected))];
    }

    /// <summary>
    /// Paints <paramref name="marker"/>; the one whose card is open is outlined thick in
    /// <paramref name="selection"/>, the map style's, so it shows on a day and a night map alike,
    /// and a red arrow above points down at it, so it is found at a glance by its shape as well.
    /// The marker itself keeps its white body on either: its numbers are read on white.
    /// </summary>
    internal static void Draw(DrawingContext context, Marker marker, IImmutableBrush selection)
    {
        var selected = new ImmutablePen(selection, SelectedOutline);
        if (marker.Level == WaterPointDetailLevel.Punkt)
        {
            DrawDisc(context, marker, selected);
        }
        else
        {
            DrawFlag(context, marker, selected);
        }

        if (marker.IsSelected)
        {
            using (context.PushTransform(Matrix.CreateTranslation(marker.PointerTip.X, marker.PointerTip.Y)))
            {
                context.DrawGeometry(null, PointerHalo, PointerShape);
                context.DrawGeometry(PointerBrush, null, PointerShape);
            }
        }
    }

    // A dot in the source's colour on a white contour. No glyph: at zoom 15 a town's hydrants stand
    // a few pixels apart, and a glyph this small reads as a smudge. Red and blue stay apart for an
    // eye that cannot tell red from green.
    private static void DrawDisc(DrawingContext context, Marker marker, IPen selected)
    {
        var at = marker.At;
        context.DrawEllipse(Brushes.White, null, at, DiscReach, DiscReach);
        context.DrawEllipse(marker.Colour, null, at, DiscRadius, DiscRadius);
        if (marker.IsSelected)
        {
            context.DrawEllipse(null, selected, at, DiscReach + 2, DiscReach + 2);
        }
    }

    private static void DrawFlag(DrawingContext context, Marker marker, IPen selected)
    {
        var body = marker.Body;
        var colour = marker.Colour;
        var mast = new StreamGeometry();
        using (var path = mast.Open())
        {
            path.BeginFigure(body.BottomLeft, isFilled: true);
            path.LineTo(new Point(body.Left + WaterPointLayer.MastWidth, body.Bottom));
            path.LineTo(marker.At);
            path.EndFigure(isClosed: true);
        }

        // The light outer contour first, so the marker reads on an aerial photo and a dark map alike.
        context.DrawRectangle(null, HaloPen, body.Inflate(1), 3, 3);
        context.DrawGeometry(null, HaloPen, mast);

        var field = new Rect(body.Left, body.Top, marker.FieldWidth, body.Height);
        context.DrawRectangle(Brushes.White, null, body, 2, 2);
        context.DrawRectangle(colour, null, field, 2, 2);
        context.DrawRectangle(colour, null, new Rect(field.Right - 2, field.Top, 2, field.Height));
        context.DrawRectangle(null, marker.IsSelected ? selected : new ImmutablePen(colour, marker.Level == WaterPointDetailLevel.Voll ? 2 : 1.5), body, 2, 2);
        context.DrawGeometry(colour, null, mast);

        var glyph = Math.Min(field.Width, field.Height) * 0.62;
        DrawGlyph(context, marker.Point.Symbol, new Rect(field.Center.X - (glyph / 2), field.Center.Y - (glyph / 2), glyph, glyph));

        var text = marker.TextLeft;
        if (marker.Secondary is { } secondary)
        {
            var top = body.Top + ((body.Height - marker.Primary.Height - secondary.Height) / 2);
            context.DrawText(marker.Primary, new Point(text, top));
            context.DrawText(secondary, new Point(text, top + marker.Primary.Height));
        }
        else
        {
            context.DrawText(marker.Primary, new Point(text, body.Top + ((body.Height - marker.Primary.Height) / 2)));
        }

        if (marker.Point.IsOutOfService)
        {
            context.DrawLine(BlockingBar, new Point(text - 1, body.Bottom - 3), new Point(body.Right - 3, body.Top + 3));
        }
    }

    private static void DrawGlyph(DrawingContext context, WaterPointSymbol symbol, Rect into)
    {
        var scale = into.Width / GlyphUnits;
        using (context.PushTransform(Matrix.CreateScale(scale, scale) * Matrix.CreateTranslation(into.X, into.Y)))
        {
            context.DrawGeometry(null, GlyphPen, Glyph(symbol));
        }
    }

    private static ImmutableSolidColorBrush Solid(byte r, byte g, byte b) => new(Color.FromRgb(r, g, b));

    /// <summary>
    /// A water point's marker as laid out on the screen: the anchor <see cref="At"/>, and for a
    /// flag its <see cref="Body"/>, as wide as its longest line of text.
    /// </summary>
    internal sealed record Marker(WaterPoint Point, WaterPointDetailLevel Level, Point At, bool IsSelected, Rect Body, double FieldWidth, FormattedText Primary, FormattedText? Secondary)
    {
        /// <summary>Whether the marker carries text a map name must keep clear of: every flag, no disc.</summary>
        public bool HasText => Level != WaterPointDetailLevel.Punkt;

        /// <summary>The marker's colour: its source's, or grey out of service.</summary>
        public IImmutableBrush Colour => SourceBrush(Point);

        /// <summary>Where the text starts, right of the coloured field.</summary>
        public double TextLeft => Body.Left + FieldWidth + 5;

        /// <summary>
        /// Where the tip of the chosen marker's arrow is: a little above the disc's outline, or above
        /// the flag's coloured field, which stands right over the mast.
        /// </summary>
        public Point PointerTip => HasText
            ? new Point(Body.Left + (FieldWidth / 2), Body.Top - 1 - (SelectedOutline / 2) - PointerGap)
            : new Point(At.X, At.Y - SelectedReach - PointerGap);

        /// <summary>Everything the marker covers, with its contour, and for the chosen one its outline and arrow.</summary>
        public Rect Bounds
        {
            get
            {
                var bounds = HasText
                    ? Body.Union(new Rect(At.X, Body.Bottom, WaterPointLayer.MastWidth, At.Y - Body.Bottom)).Inflate(Halo)
                    : new Rect(At.X - Reach, At.Y - Reach, 2 * Reach, 2 * Reach);
                return IsSelected ? bounds.Union(PointerShape.Bounds.Translate((Vector)PointerTip).Inflate(PointerHalo.Thickness / 2)) : bounds;
            }
        }

        // How far the disc reaches from its point, with the chosen one's outline.
        private static double SelectedReach => DiscReach + 2 + (SelectedOutline / 2);

        private double Reach => IsSelected ? SelectedReach : DiscReach;

        public static Marker For(WaterPoint point, WaterPointDetailLevel level, Point at, bool isSelected)
        {
            var primaryText = WaterPointMarking.PrimaryText(point);
            if (WaterPointLayer.FlagFor(level) is not { } flag)
            {
                return new Marker(point, level, at, isSelected, default, 0, Text(primaryText, 12, Ink), null);
            }

            var fieldWidth = Math.Round(flag.BodyHeight * 0.9);
            var full = level == WaterPointDetailLevel.Voll;
            var primary = Text(primaryText, full ? 14 : 13, Ink);
            var secondaryText = full ? WaterPointMarking.SecondaryText(point) : string.Empty;
            var secondary = secondaryText.Length > 0 ? Text(secondaryText, 10, Ink) : null;
            var textWidth = Math.Max(primary.Width, secondary?.Width ?? 0);
            var width = textWidth > 0 ? Math.Ceiling(fieldWidth + 5 + textWidth + 6) : fieldWidth + EmptyTextWidth;
            var body = new Rect(at.X, at.Y - flag.MastLength - flag.BodyHeight, width, flag.BodyHeight);
            return new Marker(point, level, at, isSelected, body, fieldWidth, primary, secondary);
        }

        private static FormattedText Text(string text, double size, IImmutableBrush ink) =>
            new(text, De, FlowDirection.LeftToRight, Regular, size, ink);
    }
}
