using Avalonia;
using Avalonia.Media;
using FwAssist.AppLogic.Services.Map;
using FwAssist.AppLogic.Services.Map.Tiles;
using FwAssist.Core.Map;

namespace FwAssist.Ui.Map;

/// <summary>
/// Paints the tiles around a centre at a zoom into a rectangle, with the water points over them.
/// </summary>
public static class MapDrawing
{
    /// <summary>A street named again closer than this, in pixels, is named once.</summary>
    public const double RepeatDistance = 256;

    // Free space kept around a label, so two names never touch.
    private const double LabelGap = 2;

    /// <summary>
    /// The map around <paramref name="center"/> at <paramref name="zoom"/>, turned clockwise by
    /// <paramref name="rotation"/> degrees about the middle of <paramref name="size"/>. Names are
    /// laid over it upright, a street's name along its street, and the
    /// <paramref name="waterPoints"/> over those: a name that would run under a water point's
    /// label is left out, since the label is what the crew is looking for. All of it in the colours
    /// of the cache's <see cref="MapTileCache.Style"/>.
    /// </summary>
    public static void Draw(DrawingContext context, MapTileCache tiles, GeoPoint center, int zoom, Size size, double rotation = 0, IReadOnlyList<WaterPoint>? waterPoints = null, WaterPoint? selected = null)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(tiles);
        ArgumentNullException.ThrowIfNull(center);
        if (size.Width <= 0 || size.Height <= 0)
        {
            return;
        }

        var style = tiles.Style;
        context.FillRectangle(style.Background, new Rect(size));
        var marks = waterPoints is null ? [] : WaterPointMarkers.LayOut(waterPoints, center, zoom, size, selected, rotation);
        var labels = new List<(MapLabel Label, Point At)>();
        if (tiles.Source is { } source)
        {
            // Several packs are drawn one over the other. Each pack's tiles are cut to its area when
            // it is built, so outside it a pack draws nothing that could cover its neighbour's streets.
            IReadOnlyList<IMapTileSource> layers = source is CombinedMapTileSource combined ? combined.Layers : [source];
            var turn = Turn(size, rotation);
            foreach (var layer in layers)
            {
                DrawLayer(context, tiles, layer, center, zoom, size, turn, labels);
            }
        }

        if (labels.Count > 0)
        {
            // The names of every vector tile, over all tiles and unclipped, so none is cut at a tile's
            // edge. Only a water point's flag keeps names away: a bare disc is drawn over the name.
            var reserved = marks.Where(m => m.HasText).Select(m => m.Bounds).ToList();
            foreach (var (label, placement) in PlaceLabels(labels, size, rotation, reserved))
            {
                using (context.PushTransform(placement))
                {
                    if (label.Halo is { } halo)
                    {
                        context.DrawGeometry(null, halo, label.Outline);
                    }

                    context.DrawGeometry(label.Ink, null, label.Outline);
                }
            }
        }

        foreach (var mark in marks)
        {
            WaterPointMarkers.Draw(context, mark, style.SelectedRing);
        }
    }

    /// <summary>
    /// Which of the tiles' labels to draw, in order, each with where it goes on screen: the most
    /// important first, and each only if it lies on the screen, overlaps none placed before it and
    /// does not repeat a name placed within <see cref="RepeatDistance"/>. <paramref name="labels"/>
    /// pairs each label with the point on screen it belongs to; on a map turned by
    /// <paramref name="rotation"/> degrees a street's name turns with its street, never upside
    /// down, and every other name stays level. No name overlaps a rectangle of
    /// <paramref name="reserved"/>, the screen space of the water points' labels.
    /// </summary>
    public static IReadOnlyList<(MapLabel Label, Matrix Placement)> PlaceLabels(IEnumerable<(MapLabel Label, Point At)> labels, Size size, double rotation = 0, IReadOnlyList<Rect>? reserved = null)
    {
        ArgumentNullException.ThrowIfNull(labels);
        var screen = new Rect(size);
        var placed = new List<(MapLabel Label, Point Center)>();
        var result = new List<(MapLabel Label, Matrix Placement)>();
        var taken = new List<Rect>(reserved ?? []);
        foreach (var (label, at) in labels.OrderBy(l => l.Label.Priority))
        {
            var placement = Placement(label, at, rotation);
            var bounds = new Rect(label.TextSize).TransformToAABB(placement);
            if (!screen.Intersects(bounds)
                || taken.Exists(t => t.Intersects(bounds.Inflate(LabelGap)))
                || placed.Exists(p => string.Equals(p.Label.Text, label.Text, StringComparison.Ordinal) && Distance(p.Center, bounds.Center) < RepeatDistance))
            {
                continue;
            }

            placed.Add((label, bounds.Center));
            result.Add((label, placement));
            taken.Add(bounds);
        }

        return result;
    }

    // The text centred on `at`, or its pin on it: level, or along its street turned with the map, the right way up.
    private static Matrix Placement(MapLabel label, Point at, double rotation)
    {
        var angle = 0.0;
        if (label.Angle is { } along)
        {
            angle = Math.IEEERemainder(along + (rotation * Math.PI / 180), 2 * Math.PI);
            if (angle > Math.PI / 2)
            {
                angle -= Math.PI;
            }
            else if (angle < -Math.PI / 2)
            {
                angle += Math.PI;
            }
        }

        var pin = label.Pin ?? new Point(label.TextSize.Width / 2, label.TextSize.Height / 2);
        return Matrix.CreateTranslation(-pin.X, -pin.Y) * Matrix.CreateRotation(angle) * Matrix.CreateTranslation(at.X, at.Y);
    }

    private static double Distance(Point a, Point b) => Math.Sqrt(((b.X - a.X) * (b.X - a.X)) + ((b.Y - a.Y) * (b.Y - a.Y)));

    // Turns the north-up map clockwise by `rotation` degrees about the middle of the screen.
    private static Matrix Turn(Size size, double rotation)
    {
        var middle = new Vector(size.Width / 2, size.Height / 2);
        return Matrix.CreateTranslation(-middle) * Matrix.CreateRotation(rotation * Math.PI / 180) * Matrix.CreateTranslation(middle);
    }

    private static void DrawLayer(DrawingContext context, MapTileCache tiles, IMapTileSource layer, GeoPoint center, int zoom, Size size, Matrix turn, List<(MapLabel Label, Point At)> labels)
    {
        var (centerX, centerY) = WebMercator.ToWorldPixel(center, zoom);
        var left = centerX - (size.Width / 2);
        var top = centerY - (size.Height / 2);

        // Turned, the screen shows a tilted square of the north-up map: draw every tile it touches.
        var shown = turn.TryInvert(out var back) ? new Rect(size).TransformToAABB(back) : new Rect(size);
        var (firstX, firstY) = WebMercator.ToTileIndex(left + shown.Left, top + shown.Top);
        var (lastX, lastY) = WebMercator.ToTileIndex(left + shown.Right, top + shown.Bottom);
        var tilesPerSide = 1 << zoom;

        using (context.PushTransform(turn))
        {
            for (var tx = Math.Max(firstX, 0); tx <= Math.Min(lastX, tilesPerSide - 1); tx++)
            {
                for (var ty = Math.Max(firstY, 0); ty <= Math.Min(lastY, tilesPerSide - 1); ty++)
                {
                    var target = new Rect(
                        (tx * (double)WebMercator.TileSizePixels) - left,
                        (ty * (double)WebMercator.TileSizePixels) - top,
                        WebMercator.TileSizePixels,
                        WebMercator.TileSizePixels);

                    // Past the pack's deepest zoom the cache draws the ancestor's part of the tile at
                    // this zoom, so a missing tile here is one the pack does not have.
                    if (tiles.Get(layer, zoom, tx, ty) is { } bitmap)
                    {
                        context.DrawImage(bitmap, target);
                        labels.AddRange(tiles.Labels(layer, zoom, tx, ty).Select(l => (l, turn.Transform(target.TopLeft + l.Anchor))));
                    }
                }
            }
        }
    }

    /// <summary>
    /// Where <paramref name="point"/> lands on a map of <paramref name="size"/> centred on
    /// <paramref name="center"/> and turned clockwise by <paramref name="rotation"/> degrees.
    /// </summary>
    public static Point ToScreen(GeoPoint center, int zoom, Size size, GeoPoint point, double rotation = 0)
    {
        var (centerX, centerY) = WebMercator.ToWorldPixel(center, zoom);
        var (x, y) = WebMercator.ToWorldPixel(point, zoom);
        return Turn(size, rotation).Transform(new Point(x - centerX + (size.Width / 2), y - centerY + (size.Height / 2)));
    }

    /// <summary>The inverse of <see cref="ToScreen"/>.</summary>
    public static GeoPoint ToGeo(GeoPoint center, int zoom, Size size, Point screen, double rotation = 0)
    {
        var (centerX, centerY) = WebMercator.ToWorldPixel(center, zoom);
        var northUp = Turn(size, -rotation).Transform(screen);
        return WebMercator.ToGeo(centerX + northUp.X - (size.Width / 2), centerY + northUp.Y - (size.Height / 2), zoom);
    }

    /// <summary>
    /// A movement of <paramref name="screen"/> pixels on a map turned by <paramref name="rotation"/>
    /// degrees, as the same movement on the north-up map: what a pan has to move the centre by.
    /// </summary>
    public static Vector ToNorthUp(Vector screen, double rotation) =>
        Matrix.CreateRotation(-rotation * Math.PI / 180).Transform(new Point(screen.X, screen.Y)) - default(Point);
}
