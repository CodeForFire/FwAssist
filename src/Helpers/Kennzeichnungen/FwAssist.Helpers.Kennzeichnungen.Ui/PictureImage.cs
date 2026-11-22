using Avalonia;
using Avalonia.Controls;
using Avalonia.LogicalTree;
using Avalonia.Media;

namespace FwAssist.Helpers.Kennzeichnungen.Ui;

/// <summary>
/// Shows the drawing that <c>Pictures.axaml</c> keeps under <see cref="Key"/>. The view model names a
/// picture by key and stays free of Avalonia; this control is where the key meets the resource.
/// </summary>
public sealed class PictureImage : Image
{
    public static readonly StyledProperty<string?> KeyProperty =
        AvaloniaProperty.Register<PictureImage, string?>(nameof(Key));

    public string? Key
    {
        get => GetValue(KeyProperty);
        set => SetValue(KeyProperty, value);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        ArgumentNullException.ThrowIfNull(change);
        base.OnPropertyChanged(change);
        if (change.Property == KeyProperty)
        {
            Resolve();
        }
    }

    // A template builds the control before it joins the tree, and only the tree can find the
    // view's resources, so the key is resolved again once it is attached.
    protected override void OnAttachedToLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        base.OnAttachedToLogicalTree(e);
        Resolve();
    }

    private void Resolve() =>
        Source = Key is { } key && this.TryFindResource(key, out var found) ? found as IImage : null;
}
