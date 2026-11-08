using Avalonia.Controls;
using Helfer.AppLogic.ViewModels;
using Helfer.Ui.Views;

namespace Helfer.Ui;

/// <summary>
/// Which view shows which view model. Lagebuch finds views by name through reflection; here the
/// views live in one assembly per helper, so each helper registers its own instead. An explicit
/// table is also what lets a test assert that every helper page has a view.
/// </summary>
public sealed class ViewRegistry
{
    private readonly Dictionary<Type, Func<Control>> _views = [];

    public ViewRegistry()
    {
        Register<HomeViewModel>(() => new HomeView());
        Register<SettingsViewModel>(() => new SettingsView());
        Register<AboutViewModel>(() => new AboutView());
    }

    public void Register<TViewModel>(Func<Control> createView)
    {
        ArgumentNullException.ThrowIfNull(createView);
        _views[typeof(TViewModel)] = createView;
    }

    public bool Has(Type viewModelType) => _views.ContainsKey(viewModelType);

    /// <summary>The view for <paramref name="viewModel"/>, or <see langword="null"/> when none is registered.</summary>
    public Control? Create(object viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);
        return _views.TryGetValue(viewModel.GetType(), out var create) ? create() : null;
    }
}
