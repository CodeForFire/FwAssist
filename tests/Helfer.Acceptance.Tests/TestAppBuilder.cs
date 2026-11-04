using Avalonia;
using Avalonia.Headless;

[assembly: AvaloniaTestApplication(typeof(Helfer.Acceptance.Tests.TestAppBuilder))]

namespace Helfer.Acceptance.Tests;

internal static class TestAppBuilder
{
    // UseHeadlessDrawing = false keeps the real Skia text/render backend so the embedded fonts
    // rasterize. The default headless drawing stub cannot realize embedded glyph typefaces.
    public static AppBuilder BuildAvaloniaApp()
    {
        Helfer.Composition.CompositionRoot.RegisterViews(Helfer.Ui.App.Views);
        return AppBuilder.Configure<Helfer.Ui.App>()
            .UseSkia()
            .UseHeadless(new AvaloniaHeadlessPlatformOptions { UseHeadlessDrawing = false });
    }
}
