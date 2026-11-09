using Avalonia;
using Avalonia.Headless;

[assembly: AvaloniaTestApplication(typeof(FwAssist.Acceptance.Tests.TestAppBuilder))]

namespace FwAssist.Acceptance.Tests;

internal static class TestAppBuilder
{
    // UseHeadlessDrawing = false keeps the real Skia text/render backend so the embedded fonts
    // rasterize. The default headless drawing stub cannot realize embedded glyph typefaces.
    public static AppBuilder BuildAvaloniaApp()
    {
        FwAssist.Composition.CompositionRoot.RegisterViews(FwAssist.Ui.App.Views);
        return AppBuilder.Configure<FwAssist.Ui.App>()
            .UseSkia()
            .UseHeadless(new AvaloniaHeadlessPlatformOptions { UseHeadlessDrawing = false });
    }
}
