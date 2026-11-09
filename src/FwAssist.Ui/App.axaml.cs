using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using FwAssist.AppLogic.ViewModels;
using FwAssist.Ui.Views;

namespace FwAssist.Ui;

public partial class App : Application
{
    /// <summary>
    /// Set by the Android head in <c>MainActivity.OnCreate</c>, before the first view is built:
    /// only the head knows the platform services. Read lazily through
    /// <see cref="IActivityApplicationLifetime.MainViewFactory"/>, because on Android framework
    /// init runs in <c>MainApplication</c>, before any Activity exists.
    /// </summary>
    public static Func<ShellViewModel>? CreateShell { get; set; }

    /// <summary>Which view shows which view model; the composition root registers every helper's.</summary>
    public static ViewRegistry Views { get; } = new();

    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        // The headless acceptance-test harness sets no lifetime and never assigns CreateShell, so
        // it must fall straight through to base.
        if (ApplicationLifetime is IActivityApplicationLifetime activity)
        {
            activity.MainViewFactory = () =>
            {
                var create = CreateShell
                    ?? throw new InvalidOperationException("MainActivity must set App.CreateShell before the first view is built.");
                return new ShellView { DataContext = create() };
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}
