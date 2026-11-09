using FwAssist.AppLogic.Services;

namespace FwAssist.Helpers.DekonPlatz.Tests;

public class DekonPlatzViewModelTests
{
    private readonly InMemoryHelperSettings _settings = new();

    [Fact]
    public void Starts_on_stufe_eins()
    {
        var vm = new DekonPlatzViewModel(_settings);

        Assert.True(vm.IsStufeEins);
        Assert.False(vm.IsStufeZwei);
        Assert.False(vm.IsStufeDrei);
        Assert.Same(Dekonvorgaben.StufeEins, vm.Sections);
    }

    [Fact]
    public void Choosing_stufe_zwei_shows_only_its_sections()
    {
        var vm = new DekonPlatzViewModel(_settings) { IsStufeZwei = true };

        Assert.False(vm.IsStufeEins);
        Assert.True(vm.IsStufeZwei);
        Assert.False(vm.IsStufeDrei);
        Assert.Same(Dekonvorgaben.StufeZwei, vm.Sections);
    }

    [Fact]
    public void Choosing_stufe_drei_shows_only_its_sections()
    {
        var vm = new DekonPlatzViewModel(_settings) { IsStufeDrei = true };

        Assert.False(vm.IsStufeEins);
        Assert.False(vm.IsStufeZwei);
        Assert.True(vm.IsStufeDrei);
        Assert.Same(Dekonvorgaben.StufeDrei, vm.Sections);
    }

    [Fact]
    public void Choosing_stufe_eins_again_returns_to_it()
    {
        var vm = new DekonPlatzViewModel(_settings) { IsStufeDrei = true };

        vm.IsStufeEins = true;

        Assert.Equal(DekonStufe.Eins, vm.Stufe);
    }

    [Fact]
    public void A_radio_button_unchecking_itself_leaves_the_stufe_alone()
    {
        var vm = new DekonPlatzViewModel(_settings) { IsStufeZwei = true };

        vm.IsStufeZwei = false;
        vm.IsStufeEins = false;
        vm.IsStufeDrei = false;

        Assert.Equal(DekonStufe.Zwei, vm.Stufe);
    }

    [Theory]
    [InlineData(DekonStufe.Zwei)]
    [InlineData(DekonStufe.Drei)]
    public void The_stufe_survives_a_process_death(DekonStufe stufe)
    {
        _ = new DekonPlatzViewModel(_settings) { Stufe = stufe };

        Assert.Equal(stufe, new DekonPlatzViewModel(_settings).Stufe);
    }

    [Fact]
    public void Going_back_to_stufe_eins_survives_a_process_death()
    {
        var vm = new DekonPlatzViewModel(_settings) { IsStufeDrei = true };

        vm.IsStufeEins = true;

        Assert.True(new DekonPlatzViewModel(_settings).IsStufeEins);
    }

    [Theory]
    [InlineData("Vier")]
    [InlineData("7")]
    public void An_unreadable_stored_stufe_starts_on_stufe_eins(string stored)
    {
        _settings.Set(DekonPlatzModule.HelperId + ".stufe", stored);

        Assert.True(new DekonPlatzViewModel(_settings).IsStufeEins);
    }

    [Fact]
    public void Switching_the_stufe_tells_the_view_about_everything_that_changed()
    {
        var vm = new DekonPlatzViewModel(_settings);
        var changed = new List<string?>();
        vm.PropertyChanged += (_, e) => changed.Add(e.PropertyName);

        vm.IsStufeDrei = true;

        Assert.Contains(nameof(vm.Stufe), changed);
        Assert.Contains(nameof(vm.IsStufeEins), changed);
        Assert.Contains(nameof(vm.IsStufeZwei), changed);
        Assert.Contains(nameof(vm.IsStufeDrei), changed);
        Assert.Contains(nameof(vm.Sections), changed);
    }

    [Fact]
    public void The_module_creates_a_working_view_model_without_stored_settings()
    {
        var page = new DekonPlatzModule().CreateViewModel(
            new AppLogic.Helpers.HelperServices(TimeProvider.System, new ManualTicker()));

        Assert.IsType<DekonPlatzViewModel>(page);
    }

    private sealed class ManualTicker : ITicker
    {
        public IDisposable Subscribe(Action onTick) => new Nothing();

        private sealed class Nothing : IDisposable
        {
            public void Dispose()
            {
                // The view model under test never subscribes.
            }
        }
    }
}
