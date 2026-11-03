using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Threading;
using Helfer.AppLogic.Helpers;
using Helfer.AppLogic.ViewModels;
using Helfer.Composition;
using Helfer.Testing;
using Helfer.Ui.Views;

namespace Helfer.Acceptance.Tests;

/// <summary>
/// The whole app in a phone-sized window, the way MainActivity builds it. RENDER_OUT, RENDER_WIDTH
/// and RENDER_HEIGHT turn every <see cref="Capture"/> into a PNG for a pull request or the Play
/// listing (`make phone-screenshots`, `make play-screenshots`).
/// </summary>
internal sealed class PhoneRender
{
    // Medium_Phone_API_35: 1080 px at 420 dpi is 411 dp wide.
    private const int DefaultWidth = 412;
    private const int DefaultHeight = 915;

    public PhoneRender()
    {
        Shell = CompositionRoot.CreateShell(new HelperServices(Time, Ticker), Icons, "0.1.0");
        Window = new Window
        {
            Content = new ShellView { DataContext = Shell },
            Width = Size("RENDER_WIDTH", DefaultWidth),
            Height = Size("RENDER_HEIGHT", DefaultHeight),
        };
        Window.Show();
        Settle();
    }

    public FakeTimeProvider Time { get; } = new();

    public ManualTicker Ticker { get; } = new();

    public FakeLauncherIcons Icons { get; } = new();

    public ShellViewModel Shell { get; }

    public Window Window { get; }

    public static void Settle() => Dispatcher.UIThread.RunJobs();

    public void Capture(string name)
    {
        var dir = Environment.GetEnvironmentVariable("RENDER_OUT");
        if (string.IsNullOrWhiteSpace(dir))
        {
            return;
        }

        // Button and TextBox brushes fade over 150 ms of wall-clock time; a frame captured straight
        // after a change shows them half-way (lagebuch #623). Only screenshots wait for that: no
        // assertion depends on it.
        Settle();
        Thread.Sleep(200);
        Settle();
        Directory.CreateDirectory(dir);
        using var frame = Window.CaptureRenderedFrame();
        frame?.SavePng(Path.Join(dir, name));
    }

    private static int Size(string variable, int fallback) =>
        int.TryParse(Environment.GetEnvironmentVariable(variable), System.Globalization.CultureInfo.InvariantCulture, out var v) && v > 0
            ? v
            : fallback;
}
