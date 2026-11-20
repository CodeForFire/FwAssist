using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.VisualTree;
using FwAssist.AppLogic.ViewModels;

namespace FwAssist.Ui.Views;

public partial class SettingsView : UserControl
{
    // The row being dragged in the helper order, and where it would land; -1 when none is.
    // Only what the finger is doing right now: the order itself is HelperOrderSection's.
    private int _from = -1;
    private int _to = -1;
    private double _startY;
    private double _rowHeight;

    // The finger (or mouse) that picked the row up. A second finger touching another row must
    // neither move nor drop it: every pointer event of another pointer is ignored.
    private int? _pointerId;

    public SettingsView()
    {
        InitializeComponent();

        // Tunnel, so the grip is claimed before the ScrollViewer's scroll gesture sees the press.
        HelperOrderRows.AddHandler(PointerPressedEvent, OnOrderPressed, RoutingStrategies.Tunnel);
        HelperOrderRows.PointerMoved += OnOrderMoved;
        HelperOrderRows.PointerReleased += OnOrderReleased;
        HelperOrderRows.PointerCaptureLost += (_, e) =>
        {
            if (IsDragPointer(e.Pointer))
            {
                EndDrag();
            }
        };
    }

    private void OnOrderPressed(object? sender, PointerPressedEventArgs e)
    {
        if (_from >= 0 || e.Source is not Visual source || !IsOnHandle(source))
        {
            return;
        }

        var container = ContainerOf(source);
        var index = container is null ? -1 : HelperOrderRows.IndexFromContainer(container);
        if (container is null || index < 0 || container.Bounds.Height <= 0)
        {
            return;
        }

        _from = _to = index;
        _pointerId = e.Pointer.Id;
        if (HelperOrderRows.DataContext is HelperOrderSection order)
        {
            order.IsDragging = true;
        }

        _startY = e.GetPosition(HelperOrderRows).Y;
        _rowHeight = container.Bounds.Height;
        container.ZIndex = 1;
        RowOf(container)?.Classes.Add("dragging");
        e.Pointer.Capture(HelperOrderRows);
        e.PreventGestureRecognition();
        e.Handled = true;
    }

    private void OnOrderMoved(object? sender, PointerEventArgs e)
    {
        if (_from < 0 || !IsDragPointer(e.Pointer))
        {
            return;
        }

        var count = HelperOrderRows.ItemCount;
        var offset = Math.Clamp(e.GetPosition(HelperOrderRows).Y - _startY, -_from * _rowHeight, (count - 1 - _from) * _rowHeight);
        _to = HelperOrderSection.DropPlace(_from, offset, _rowHeight, count);

        // The picked-up row follows the finger; the rows it passed make room for it.
        for (var i = 0; i < count; i++)
        {
            if (HelperOrderRows.ContainerFromIndex(i) is not { } container)
            {
                continue;
            }

            var shift = i == _from ? offset
                : _from < i && i <= _to ? -_rowHeight
                : _to <= i && i < _from ? _rowHeight
                : 0;
            container.RenderTransform = new TranslateTransform(0, shift);
        }

        e.Handled = true;
    }

    private void OnOrderReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (_from < 0 || !IsDragPointer(e.Pointer))
        {
            return;
        }

        var (from, to) = (_from, _to);
        EndDrag();
        e.Pointer.Capture(null);
        e.Handled = true;
        if (HelperOrderRows.DataContext is HelperOrderSection order)
        {
            order.Move(from, to);
        }
    }

    // Puts every row back where the layout has it; a lost capture cancels the drag this way.
    private void EndDrag()
    {
        if (_from < 0)
        {
            return;
        }

        _from = _to = -1;
        _pointerId = null;
        if (HelperOrderRows.DataContext is HelperOrderSection order)
        {
            order.IsDragging = false;
        }

        for (var i = 0; i < HelperOrderRows.ItemCount; i++)
        {
            if (HelperOrderRows.ContainerFromIndex(i) is { } container)
            {
                container.RenderTransform = null;
                container.ZIndex = 0;
                RowOf(container)?.Classes.Remove("dragging");
            }
        }
    }

    private bool IsDragPointer(IPointer pointer) => pointer.Id == _pointerId;

    private static bool IsOnHandle(Visual source) =>
        source.GetSelfAndVisualAncestors().OfType<Border>().Any(b => b.Classes.Contains("drag-handle"));

    private Control? ContainerOf(Visual source) =>
        source.GetSelfAndVisualAncestors().OfType<Control>()
            .FirstOrDefault(c => ReferenceEquals(c.GetVisualParent(), HelperOrderRows.ItemsPanelRoot));

    private static Border? RowOf(Control container) =>
        container.GetVisualDescendants().OfType<Border>().FirstOrDefault(b => b.Classes.Contains("order-row"));
}
