using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Threading;
using FwAssist.AppLogic.Helpers;
using FwAssist.AppLogic.Services.Map;
using FwAssist.AppLogic.ViewModels;
using FwAssist.Composition;
using FwAssist.Testing;
using FwAssist.Ui.Views;

namespace FwAssist.Acceptance.Tests;

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

    // The made-up Fürstenfeldbruck pack, installed once per test run. Every phone shares it and
    // the catalogue below: nothing here ever reaches the network.
    private static readonly Lazy<string> MapRoot = new(() =>
    {
        var root = Path.Join(Directory.CreateTempSubdirectory("fwassist-phone-").FullName, "maps");
        FictionalRegionPack.Install(root);
        return root;
    });

    private static readonly HttpClient Http = new(new FictionalCatalogue());

    public PhoneRender()
    {
        Maps = new RegionPackLibrary(Http, MapRoot.Value);
        Shell = CompositionRoot.CreateShell(new HelperServices(Time, Ticker, Maps: Maps), Icons, new FakeExternalLinks(), "0.1.0");
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

    public RegionPackLibrary Maps { get; }

    public ShellViewModel Shell { get; }

    public Window Window { get; }

    public static void Settle() => Dispatcher.UIThread.RunJobs();

    /// <summary>
    /// Runs the UI thread's jobs until <paramref name="done"/> holds, for work that finishes on a
    /// thread-pool thread, such as a pack's water points being read. Fails after five seconds.
    /// </summary>
    public static void SettleUntil(Func<bool> done)
    {
        ArgumentNullException.ThrowIfNull(done);
        var deadline = DateTime.UtcNow.AddSeconds(5);
        Settle();
        while (!done())
        {
            Assert.True(DateTime.UtcNow < deadline, "The UI did not settle within five seconds.");
            Thread.Sleep(10);
            Settle();
        }
    }

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
