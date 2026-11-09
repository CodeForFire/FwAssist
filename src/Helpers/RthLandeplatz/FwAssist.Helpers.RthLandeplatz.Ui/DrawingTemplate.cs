using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using FwAssist.Helpers.RthLandeplatz.Ui.Drawings;

namespace FwAssist.Helpers.RthLandeplatz.Ui;

/// <summary>
/// Turns a <see cref="SiteDrawing"/> into its control. A drawing has no text a screen reader can reach
/// on its own, so its description becomes the automation name here, for all seven at once.
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
            DrawingKind.LandeplatzTag => new LandeplatzTagDrawing(),
            DrawingKind.LandeplatzNacht => new LandeplatzNachtDrawing(),
            DrawingKind.Lichtkreuz => new LichtkreuzDrawing(),
            DrawingKind.Gefahrenbereich => new GefahrenbereichDrawing(),
            DrawingKind.AbstandUnbeteiligte => new AbstandUnbeteiligteDrawing(),
            DrawingKind.AnnaeherungHang => new AnnaeherungHangDrawing(),
            DrawingKind.RotorUeberStrasse => new RotorUeberStrasseDrawing(),
            _ => throw new ArgumentOutOfRangeException(nameof(param), drawing.Kind, "Kein Zeichnungstyp."),
        };
        AutomationProperties.SetName(control, drawing.Description);
        return control;
    }
}
