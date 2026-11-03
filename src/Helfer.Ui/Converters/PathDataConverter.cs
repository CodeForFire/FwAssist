using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace Helfer.Ui.Converters;

/// <summary>
/// Turns a helper's <c>IconPathData</c> string into a geometry for a <see cref="Avalonia.Controls.PathIcon"/>.
/// The path data is compiled into the app, never input, so a parse failure is a bug and is left to throw.
/// </summary>
public sealed class PathDataConverter : IValueConverter
{
    public static PathDataConverter Instance { get; } = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is string data ? StreamGeometry.Parse(data) : null;

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
