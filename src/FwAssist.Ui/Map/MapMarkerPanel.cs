using Avalonia;
using Avalonia.Controls;
using FwAssist.Core.Map;

namespace FwAssist.Ui.Map;

/// <summary>
/// Lays its children over a <see cref="MapCanvasControl"/> showing the same <see cref="Center"/>,
/// <see cref="Zoom"/> and <see cref="Rotation"/>: each child sits centred on its
/// <see cref="PositionProperty"/>. Marks are real controls rather than drawn pixels, so they carry
/// automation names and can be tapped. A child without a position is not shown.
/// </summary>
public sealed class MapMarkerPanel : Panel
{
    public static readonly StyledProperty<GeoPoint?> CenterProperty =
        AvaloniaProperty.Register<MapMarkerPanel, GeoPoint?>(nameof(Center));

    public static readonly StyledProperty<int> ZoomProperty =
        AvaloniaProperty.Register<MapMarkerPanel, int>(nameof(Zoom), defaultValue: 13);

    /// <summary>The map's turn in degrees clockwise. The marks move with the map and stay upright.</summary>
    public static readonly StyledProperty<double> RotationProperty =
        AvaloniaProperty.Register<MapMarkerPanel, double>(nameof(Rotation));

    public static readonly AttachedProperty<GeoPoint?> PositionProperty =
        AvaloniaProperty.RegisterAttached<MapMarkerPanel, Control, GeoPoint?>("Position");

    static MapMarkerPanel()
    {
        AffectsArrange<MapMarkerPanel>(CenterProperty, ZoomProperty, RotationProperty);
        AffectsParentArrange<MapMarkerPanel>(PositionProperty);
        ClipToBoundsProperty.OverrideDefaultValue<MapMarkerPanel>(true);
    }

    public GeoPoint? Center
    {
        get => GetValue(CenterProperty);
        set => SetValue(CenterProperty, value);
    }

    public int Zoom
    {
        get => GetValue(ZoomProperty);
        set => SetValue(ZoomProperty, value);
    }

    public double Rotation
    {
        get => GetValue(RotationProperty);
        set => SetValue(RotationProperty, value);
    }

    public static GeoPoint? GetPosition(Control control)
    {
        ArgumentNullException.ThrowIfNull(control);
        return control.GetValue(PositionProperty);
    }

    public static void SetPosition(Control control, GeoPoint? value)
    {
        ArgumentNullException.ThrowIfNull(control);
        control.SetValue(PositionProperty, value);
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        foreach (var child in Children)
        {
            child.Measure(Size.Infinity);
        }

        // The panel takes whatever the map is given; the marks never decide its size.
        return new Size(
            double.IsInfinity(availableSize.Width) ? 0 : availableSize.Width,
            double.IsInfinity(availableSize.Height) ? 0 : availableSize.Height);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        // An ItemsControl sets the position on its item containers, the panel's own children.
        foreach (var child in Children)
        {
            if (Center is { } center && GetPosition(child) is { } position)
            {
                var at = MapDrawing.ToScreen(center, Zoom, finalSize, position, Rotation);
                var size = child.DesiredSize;
                child.Arrange(new Rect(new Point(at.X - (size.Width / 2), at.Y - (size.Height / 2)), size));
            }
            else
            {
                child.Arrange(new Rect(-10000, -10000, 0, 0));
            }
        }

        return finalSize;
    }
}
