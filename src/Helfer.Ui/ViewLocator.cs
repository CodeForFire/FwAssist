using Avalonia.Controls;
using Avalonia.Controls.Templates;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Helfer.Ui;

/// <summary>Builds the view for a page view model from <see cref="App.Views"/>.</summary>
public sealed class ViewLocator : IDataTemplate
{
    public Control Build(object? param)
    {
        if (param is null)
        {
            return new TextBlock { Text = "—" };
        }

        return App.Views.Create(param)
            ?? new TextBlock { Text = "Keine Ansicht für " + param.GetType().Name };
    }

    public bool Match(object? data) => data is ObservableObject;
}
