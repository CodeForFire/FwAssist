using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace FwAssist.Helpers.DekonPlatz.Ui;

public partial class DekonPlatzView : UserControl
{
    public DekonPlatzView() => InitializeComponent();

    // Each Stufe is its own page, so it starts at its first line, not at the last one's scroll offset.
    private void OnStufeChanged(object? sender, RoutedEventArgs e) => Scroller.Offset = Vector.Zero;
}
