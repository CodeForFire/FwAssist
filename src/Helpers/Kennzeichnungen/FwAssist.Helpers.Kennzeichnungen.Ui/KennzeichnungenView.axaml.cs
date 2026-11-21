using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace FwAssist.Helpers.Kennzeichnungen.Ui;

public partial class KennzeichnungenView : UserControl
{
    public KennzeichnungenView() => InitializeComponent();

    // Each group is its own page, so it starts at its first entry, not at the last one's scroll offset.
    private void OnGruppeChanged(object? sender, RoutedEventArgs e) => Scroller.Offset = Vector.Zero;
}
