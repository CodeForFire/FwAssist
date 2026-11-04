using Android.Runtime;
using Avalonia;
using Avalonia.Android;

namespace Helfer.App.Android;

/// <summary>
/// Android application entry point. Avalonia 12 initializes the app here, once per process and
/// before any Activity exists; Activity-scoped wiring stays on <see cref="MainActivity"/>.
/// </summary>
[Application]
public class MainApplication : AvaloniaAndroidApplication<Helfer.Ui.App>
{
    public MainApplication(IntPtr handle, JniHandleOwnership ownership)
        : base(handle, ownership)
    {
    }

    protected override AppBuilder CustomizeAppBuilder(AppBuilder builder) =>
        base.CustomizeAppBuilder(builder).WithInterFont();
}
