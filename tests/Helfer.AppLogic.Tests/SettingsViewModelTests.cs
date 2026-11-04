using Helfer.AppLogic.ViewModels;
using Helfer.Testing;

namespace Helfer.AppLogic.Tests;

public class SettingsViewModelTests
{
    private readonly FakeLauncherIcons _icons = new();

    private SettingsViewModel Settings() => new([new TestModule("stoppuhr", "Stoppuhr")], _icons);

    [Fact]
    public void Each_helper_gets_a_switch_showing_whether_its_icon_is_on_the_launcher()
    {
        _icons.ChangeOutside("stoppuhr", false);

        var toggle = Assert.Single(Settings().LauncherIcons);

        Assert.Equal("Stoppuhr", toggle.Title);
        Assert.False(toggle.IsEnabled);
    }

    [Fact]
    public void Switching_off_hides_the_icon_and_switching_on_shows_it_again()
    {
        var toggle = Settings().LauncherIcons[0];

        toggle.IsEnabled = false;
        Assert.False(_icons.IsEnabled("stoppuhr"));

        toggle.IsEnabled = true;
        Assert.True(_icons.IsEnabled("stoppuhr"));
    }

    [Fact]
    public void A_refused_switch_springs_back_and_says_why()
    {
        var settings = Settings();
        var toggle = settings.LauncherIcons[0];
        _icons.Refuse = true;

        toggle.IsEnabled = false;

        Assert.True(toggle.IsEnabled);
        Assert.Equal("Das Symbol für Stoppuhr konnte nicht geändert werden.", settings.Error);
    }

    [Fact]
    public void A_switch_that_takes_clears_the_last_error()
    {
        var settings = Settings();
        var toggle = settings.LauncherIcons[0];
        _icons.Refuse = true;
        toggle.IsEnabled = false;
        _icons.Refuse = false;

        toggle.IsEnabled = false;

        Assert.Null(settings.Error);
        Assert.False(_icons.IsEnabled("stoppuhr"));
    }

    [Fact]
    public void Refresh_picks_up_a_change_made_outside_the_app_and_clears_the_error()
    {
        var settings = Settings();
        settings.Error = "alt";
        _icons.ChangeOutside("stoppuhr", false);

        settings.Refresh();

        Assert.False(settings.LauncherIcons[0].IsEnabled);
        Assert.Null(settings.Error);
    }

    [Fact]
    public void Refresh_only_reads_and_never_writes_an_icon_back()
    {
        var settings = Settings();
        _icons.ChangeOutside("stoppuhr", false);

        settings.Refresh();

        Assert.Equal(0, _icons.Writes);
    }
}
