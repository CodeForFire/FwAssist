using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Helfer.Helpers.DekonPlatz.Ui.Drawings;

namespace Helfer.Helpers.DekonPlatz.Ui;

/// <summary>
/// Turns a <see cref="SiteDrawing"/> into its control. A drawing has no text a screen reader can reach
/// on its own, so its description becomes the automation name here.
/// </summary>
public sealed class DrawingTemplate : IDataTemplate
{
    public bool Match(object? data) => data is SiteDrawing;

    public Control? Build(object? param)
    {
        if (param is not SiteDrawing drawing)
        {
            return null;
        }

        Control control = drawing.Kind switch
        {
            DrawingKind.DreiBereiche => new DreiBereicheDrawing(),
            DrawingKind.SchwarzWeiss => new SchwarzWeissDrawing(),
            DrawingKind.AblaufStufeDrei => new AblaufStufeDreiDrawing(),
            _ => throw new ArgumentOutOfRangeException(nameof(param), drawing.Kind, "Kein Zeichnungstyp."),
        };
        AutomationProperties.SetName(control, drawing.Description);
        return control;
    }
}
