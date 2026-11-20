using FwAssist.AppLogic.Helpers;
using FwAssist.AppLogic.Services;
using FwAssist.AppLogic.ViewModels;

namespace FwAssist.AppLogic.Tests;

public class HelperOrderSectionTests
{
    private readonly IReadOnlyList<IHelperModule> _modules =
    [
        new TestModule("stoppuhr", "Stoppuhr"),
        new TestModule("funk", "Funkalphabet"),
        new TestModule("dekon", "Dekon-Platz"),
    ];

    private readonly InMemoryHelperSettings _settings = new();
    private readonly List<IReadOnlyList<string>> _heard = [];

    private HelperOrderSection Section() => new(_modules, _settings, _heard.Add);

    private static IEnumerable<string> Ids(IEnumerable<IHelperModule> modules) => modules.Select(m => m.Id);

    [Fact]
    public void Without_a_stored_order_the_helpers_keep_the_order_the_app_ships_with() =>
        Assert.Equal(["Stoppuhr", "Funkalphabet", "Dekon-Platz"], Section().Rows.Select(r => r.Title));

    [Fact]
    public void Moving_a_helper_up_stores_the_new_order_and_tells_the_overview()
    {
        var section = Section();

        section.Rows[2].MoveUpCommand.Execute(null);

        Assert.Equal(["stoppuhr", "dekon", "funk"], section.Ids);
        Assert.Equal("stoppuhr,dekon,funk", _settings.Get(HelperOrderSection.SettingsKey));
        Assert.Equal(["stoppuhr", "dekon", "funk"], Assert.Single(_heard));
    }

    [Fact]
    public void Moving_a_helper_down_swaps_it_with_the_next_one()
    {
        var section = Section();

        section.Rows[0].MoveDownCommand.Execute(null);

        Assert.Equal(["funk", "stoppuhr", "dekon"], section.Ids);
    }

    [Fact]
    public void The_first_helper_cannot_move_up_and_the_last_cannot_move_down()
    {
        var section = Section();

        Assert.False(section.Rows[0].MoveUpCommand.CanExecute(null));
        Assert.True(section.Rows[0].MoveDownCommand.CanExecute(null));
        Assert.True(section.Rows[2].MoveUpCommand.CanExecute(null));
        Assert.False(section.Rows[2].MoveDownCommand.CanExecute(null));
    }

    [Fact]
    public void A_helper_moved_to_the_top_can_no_longer_move_up_and_the_one_it_passed_can()
    {
        var section = Section();
        var funk = section.Rows[1];
        var stoppuhr = section.Rows[0];

        funk.MoveUpCommand.Execute(null);

        Assert.False(funk.MoveUpCommand.CanExecute(null));
        Assert.True(stoppuhr.MoveUpCommand.CanExecute(null));
    }

    [Fact]
    public void A_stored_order_comes_back_after_the_process_died()
    {
        Section().Rows[2].MoveUpCommand.Execute(null);

        Assert.Equal(["stoppuhr", "dekon", "funk"], Section().Ids);
    }

    [Fact]
    public void Reset_restores_the_shipped_order_and_stores_none()
    {
        var section = Section();
        section.Rows[2].MoveUpCommand.Execute(null);
        section.Rows[1].MoveUpCommand.Execute(null);

        section.ResetCommand.Execute(null);

        Assert.Equal(["stoppuhr", "funk", "dekon"], section.Ids);
        Assert.Equal(string.Empty, _settings.Get(HelperOrderSection.SettingsKey));
        Assert.Equal(["stoppuhr", "funk", "dekon"], _heard[^1]);
        Assert.False(section.Rows[0].MoveUpCommand.CanExecute(null));
    }

    [Fact]
    public void A_helper_new_in_an_update_goes_at_the_end() =>
        Assert.Equal(["dekon", "stoppuhr", "funk"], Ids(HelperOrderSection.Arrange(_modules, "dekon,stoppuhr")));

    [Fact]
    public void A_stored_id_no_helper_claims_any_more_is_skipped() =>
        Assert.Equal(["funk", "stoppuhr", "dekon"], Ids(HelperOrderSection.Arrange(_modules, "gone,funk,stoppuhr,dekon")));

    [Fact]
    public void A_stored_id_given_twice_counts_once() =>
        Assert.Equal(["dekon", "stoppuhr", "funk"], Ids(HelperOrderSection.Arrange(_modules, "dekon,dekon,stoppuhr")));

    [Fact]
    public void The_buttons_are_named_for_a_screen_reader()
    {
        var row = Section().Rows[0];

        Assert.Equal("Stoppuhr nach oben", row.MoveUpName);
        Assert.Equal("Stoppuhr nach unten", row.MoveDownName);
    }
}
