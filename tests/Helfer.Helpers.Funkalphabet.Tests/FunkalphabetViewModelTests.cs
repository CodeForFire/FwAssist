namespace Helfer.Helpers.Funkalphabet.Tests;

public class FunkalphabetViewModelTests
{
    [Fact]
    public void Starts_on_the_old_table_with_nothing_spelled()
    {
        var vm = new FunkalphabetViewModel();

        Assert.Equal("ALT (DIN 5009:1983)", vm.TableName);
        Assert.True(vm.IsAlt);
        Assert.False(vm.IsDin2022);
        Assert.False(vm.HasSpelling);
        Assert.Empty(vm.Spelled);
        Assert.Equal(Buchstabiertafel.Alt.Entries, vm.Entries);
    }

    [Fact]
    public void Typing_spells_the_text()
    {
        var vm = new FunkalphabetViewModel { Text = "ab" };

        Assert.True(vm.HasSpelling);
        Assert.Equal(["Anton", "Berta"], vm.Spelled.Select(c => c.Word));
    }

    [Fact]
    public void Clearing_the_text_clears_the_spelling()
    {
        var vm = new FunkalphabetViewModel { Text = "ab" };

        vm.Text = string.Empty;

        Assert.False(vm.HasSpelling);
        Assert.Empty(vm.Spelled);
    }

    [Fact]
    public void Switching_the_table_respells_the_text_and_changes_the_list()
    {
        var vm = new FunkalphabetViewModel { Text = "ab" };

        vm.UseDin2022Command.Execute(null);

        Assert.Equal("DIN 5009:2022", vm.TableName);
        Assert.True(vm.IsDin2022);
        Assert.False(vm.IsAlt);
        Assert.Equal(["Aachen", "Berlin"], vm.Spelled.Select(c => c.Word));
        Assert.Equal(Buchstabiertafel.Din2022.Entries, vm.Entries);
    }

    [Fact]
    public void Switching_back_returns_to_the_old_table()
    {
        var vm = new FunkalphabetViewModel { Text = "a" };

        vm.UseDin2022Command.Execute(null);
        vm.UseAltCommand.Execute(null);

        Assert.Equal(["Anton"], vm.Spelled.Select(c => c.Word));
    }

    [Fact]
    public void Switching_the_table_tells_the_view_about_everything_that_changed()
    {
        var vm = new FunkalphabetViewModel();
        var changed = new List<string?>();
        vm.PropertyChanged += (_, e) => changed.Add(e.PropertyName);

        vm.UseDin2022Command.Execute(null);

        Assert.Contains(nameof(vm.TableName), changed);
        Assert.Contains(nameof(vm.IsAlt), changed);
        Assert.Contains(nameof(vm.IsDin2022), changed);
        Assert.Contains(nameof(vm.Spelled), changed);
        Assert.Contains(nameof(vm.Entries), changed);
    }

    [Fact]
    public void Typing_tells_the_view_about_the_spelling()
    {
        var vm = new FunkalphabetViewModel();
        var changed = new List<string?>();
        vm.PropertyChanged += (_, e) => changed.Add(e.PropertyName);

        vm.Text = "a";

        Assert.Contains(nameof(vm.Spelled), changed);
        Assert.Contains(nameof(vm.HasSpelling), changed);
    }
}
