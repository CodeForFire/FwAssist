using Helfer.AppLogic.Helpers;
using Helfer.AppLogic.ViewModels;
using Helfer.Testing;

namespace Helfer.AppLogic.Tests;

public class ShellViewModelTests
{
    private readonly TestModule _stoppuhr = new("stoppuhr", "Stoppuhr");
    private readonly TestModule _other = new("other", "Anderes");

    private ShellViewModel Shell() => new(
        [_stoppuhr, _other],
        new HelperServices(new FakeTimeProvider(), new ManualTicker()),
        new FakeLauncherIcons(),
        "1.2.3");

    [Fact]
    public void Starts_on_the_home_screen_with_one_tile_per_helper_in_order()
    {
        var shell = Shell();

        Assert.True(shell.IsHome);
        Assert.Equal(ShellViewModel.HomeTitle, shell.Title);
        Assert.Equal(["Stoppuhr", "Anderes"], shell.Home.Tiles.Select(t => t.Title));
        Assert.Equal("Version 1.2.3", shell.Home.VersionText);
    }

    [Fact]
    public void Tapping_a_tile_opens_that_helper_under_its_title()
    {
        var shell = Shell();

        shell.Home.Tiles[0].OpenCommand.Execute(null);

        Assert.IsType<TestModule.Page>(shell.Current);
        Assert.Equal("Stoppuhr", shell.Title);
        Assert.False(shell.IsHome);
    }

    [Fact]
    public void A_helper_keeps_its_page_when_the_user_leaves_and_comes_back()
    {
        var shell = Shell();
        shell.Open("stoppuhr");
        var first = shell.Current;

        shell.GoHome();
        shell.Open("stoppuhr");

        Assert.Same(first, shell.Current);
        Assert.Equal(1, _stoppuhr.Created);
    }

    [Fact]
    public void System_back_from_a_helper_opened_on_the_home_screen_returns_home()
    {
        var shell = Shell();
        shell.Open("stoppuhr");

        Assert.True(shell.HandleSystemBack());
        Assert.True(shell.IsHome);
    }

    [Fact]
    public void System_back_on_the_home_screen_is_left_to_the_platform()
    {
        var shell = Shell();

        Assert.False(shell.HandleSystemBack());
        Assert.True(shell.IsHome);
    }

    [Fact]
    public void System_back_from_a_helper_opened_by_its_own_icon_leaves_the_app()
    {
        var shell = Shell();
        shell.OpenFromLauncher("stoppuhr");

        Assert.False(shell.HandleSystemBack());
        Assert.Equal("Stoppuhr", shell.Title);
    }

    [Fact]
    public void The_arrow_goes_home_even_from_a_helper_opened_by_its_own_icon()
    {
        var shell = Shell();
        shell.OpenFromLauncher("stoppuhr");

        shell.GoHomeCommand.Execute(null);

        Assert.True(shell.IsHome);
        Assert.False(shell.HandleSystemBack());
    }

    [Fact]
    public void Opening_from_a_tile_after_a_launcher_start_makes_back_return_home_again()
    {
        var shell = Shell();
        shell.OpenFromLauncher("stoppuhr");
        shell.GoHome();

        shell.Open("other");

        Assert.True(shell.HandleSystemBack());
        Assert.True(shell.IsHome);
    }

    [Fact]
    public void An_icon_of_a_helper_that_no_longer_exists_lands_on_the_home_screen()
    {
        var shell = Shell();
        shell.Open("stoppuhr");

        shell.OpenFromLauncher("removed-helper");

        Assert.True(shell.IsHome);
    }

    [Fact]
    public void Opening_an_unknown_helper_from_a_tile_changes_nothing()
    {
        var shell = Shell();
        shell.Open("stoppuhr");
        var page = shell.Current;

        shell.Open("removed-helper");

        Assert.Same(page, shell.Current);
        Assert.Equal("Stoppuhr", shell.Title);
    }

    [Fact]
    public void Settings_open_from_the_home_screen_and_back_returns_home()
    {
        var shell = Shell();

        shell.Home.OpenSettingsCommand.Execute(null);

        Assert.IsType<SettingsViewModel>(shell.Current);
        Assert.Equal(SettingsViewModel.PageTitle, shell.Title);
        Assert.True(shell.HandleSystemBack());
        Assert.True(shell.IsHome);
    }

    [Fact]
    public void Opening_another_helper_after_a_launcher_start_makes_back_return_home()
    {
        var shell = Shell();
        shell.OpenFromLauncher("stoppuhr");

        shell.Open("other");

        Assert.True(shell.HandleSystemBack());
    }

    [Fact]
    public void Settings_after_a_launcher_start_return_home_on_back()
    {
        var shell = Shell();
        shell.OpenFromLauncher("stoppuhr");
        shell.GoHome();

        shell.Home.OpenSettingsCommand.Execute(null);

        Assert.True(shell.HandleSystemBack());
        Assert.True(shell.IsHome);
    }
}
