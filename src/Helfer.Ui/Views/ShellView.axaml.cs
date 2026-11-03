using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Helfer.AppLogic.ViewModels;

namespace Helfer.Ui.Views;

public partial class ShellView : UserControl
{
    private TopLevel? _topLevel;

    public ShellView() => InitializeComponent();

    // The system Back gesture reaches the TopLevel, not the view. Forward it to the shell; when
    // the shell does not take it, the platform's own Back runs and Android backgrounds the app.
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _topLevel = TopLevel.GetTopLevel(this);
        if (_topLevel is not null)
        {
            _topLevel.BackRequested += OnBackRequested;
        }
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        if (_topLevel is not null)
        {
            _topLevel.BackRequested -= OnBackRequested;
            _topLevel = null;
        }

        base.OnDetachedFromVisualTree(e);
    }

    private void OnBackRequested(object? sender, RoutedEventArgs e)
    {
        if (DataContext is ShellViewModel shell)
        {
            e.Handled = shell.HandleSystemBack();
        }
    }
}
