using Helfer.AppLogic.Services;

namespace Helfer.Helpers.RthLandeplatz.Tests;

public class RthLandeplatzViewModelTests
{
    private readonly InMemoryHelperSettings _settings = new();

    [Fact]
    public void Starts_by_day()
    {
        var vm = new RthLandeplatzViewModel(_settings);

        Assert.True(vm.IsTag);
        Assert.False(vm.IsNacht);
        Assert.Same(Landeplatzvorgaben.Tag, vm.Sections);
    }

    [Fact]
    public void Choosing_night_shows_the_night_sections()
    {
        var vm = new RthLandeplatzViewModel(_settings) { IsNacht = true };

        Assert.False(vm.IsTag);
        Assert.Same(Landeplatzvorgaben.Nacht, vm.Sections);
    }

    [Fact]
    public void Unchecking_one_mode_selects_the_other()
    {
        var vm = new RthLandeplatzViewModel(_settings);

        vm.IsTag = false;
        Assert.True(vm.IsNacht);

        vm.IsTag = true;
        Assert.False(vm.IsNacht);
    }

    [Fact]
    public void The_night_mode_survives_a_process_death()
    {
        _ = new RthLandeplatzViewModel(_settings) { IsNacht = true };

        var relaunched = new RthLandeplatzViewModel(_settings);

        Assert.True(relaunched.IsNacht);
        Assert.Same(Landeplatzvorgaben.Nacht, relaunched.Sections);
    }

    [Fact]
    public void Going_back_to_day_survives_a_process_death()
    {
        var vm = new RthLandeplatzViewModel(_settings) { IsNacht = true };

        vm.IsNacht = false;

        Assert.False(new RthLandeplatzViewModel(_settings).IsNacht);
    }

    [Fact]
    public void An_unreadable_stored_mode_starts_by_day()
    {
        _settings.Set(RthLandeplatzModule.HelperId + ".nacht", "Dämmerung");

        Assert.True(new RthLandeplatzViewModel(_settings).IsTag);
    }

    [Fact]
    public void Switching_the_mode_tells_the_view_about_everything_that_changed()
    {
        var vm = new RthLandeplatzViewModel(_settings);
        var changed = new List<string?>();
        vm.PropertyChanged += (_, e) => changed.Add(e.PropertyName);

        vm.IsNacht = true;

        Assert.Contains(nameof(vm.IsNacht), changed);
        Assert.Contains(nameof(vm.IsTag), changed);
        Assert.Contains(nameof(vm.Sections), changed);
    }
}
