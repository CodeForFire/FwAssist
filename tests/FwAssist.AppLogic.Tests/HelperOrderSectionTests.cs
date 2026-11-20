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
    public void Dropping_the_last_helper_at_the_top_stores_the_new_order_and_tells_the_overview()
    {
        var section = Section();

        section.Move(2, 0);

        Assert.Equal(["dekon", "stoppuhr", "funk"], section.Ids);
        Assert.Equal("dekon,stoppuhr,funk", _settings.Get(HelperOrderSection.SettingsKey));
        Assert.Equal(["dekon", "stoppuhr", "funk"], Assert.Single(_heard));
    }

    [Fact]
    public void Dropping_the_first_helper_at_the_bottom_moves_the_others_up()
    {
        var section = Section();

        section.Move(0, 2);

        Assert.Equal(["funk", "dekon", "stoppuhr"], section.Ids);
    }

    [Theory]
    [InlineData(1, 1)]
    [InlineData(-1, 0)]
    [InlineData(0, -1)]
    [InlineData(3, 0)]
    [InlineData(0, 3)]
    public void A_drop_on_the_same_place_or_outside_the_list_changes_and_stores_nothing(int from, int to)
    {
        var section = Section();

        section.Move(from, to);

        Assert.Equal(["stoppuhr", "funk", "dekon"], section.Ids);
        Assert.Null(_settings.Get(HelperOrderSection.SettingsKey));
        Assert.Empty(_heard);
    }

    [Fact]
    public void A_stored_order_comes_back_after_the_process_died()
    {
        Section().Move(2, 1);

        Assert.Equal(["stoppuhr", "dekon", "funk"], Section().Ids);
    }

    [Fact]
    public void Reset_restores_the_shipped_order_and_stores_none()
    {
        var section = Section();
        section.Move(2, 0);
        section.Move(2, 1);

        section.ResetCommand.Execute(null);

        Assert.Equal(["stoppuhr", "funk", "dekon"], section.Ids);
        Assert.Equal(string.Empty, _settings.Get(HelperOrderSection.SettingsKey));
        Assert.Equal(["stoppuhr", "funk", "dekon"], _heard[^1]);
    }

    [Fact]
    public void While_a_row_is_dragged_reset_is_neither_offered_nor_carried_out()
    {
        var section = Section();
        section.Move(2, 0);
        var raised = 0;
        section.ResetCommand.CanExecuteChanged += (_, _) => raised++;

        section.IsDragging = true;
        Assert.False(section.ResetCommand.CanExecute(null));
        section.ResetCommand.Execute(null);

        Assert.Equal(["dekon", "stoppuhr", "funk"], section.Ids);
        Assert.Equal("dekon,stoppuhr,funk", _settings.Get(HelperOrderSection.SettingsKey));
        Assert.True(raised > 0);

        section.IsDragging = false;
        Assert.True(section.ResetCommand.CanExecute(null));
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

    [Theory]
    [InlineData(2, 0.0, 2)]
    [InlineData(2, 29.0, 2)]
    [InlineData(2, 30.0, 3)]
    [InlineData(2, -30.0, 1)]
    [InlineData(2, -31.0, 1)]
    [InlineData(2, -90.0, 0)]
    [InlineData(2, 119.0, 4)]
    [InlineData(2, -600.0, 0)]
    [InlineData(2, 600.0, 4)]
    [InlineData(4, 1e12, 4)]
    public void A_dragged_row_lands_one_place_per_row_height_passed_and_stays_in_the_list(int from, double offset, int expected) =>
        Assert.Equal(expected, HelperOrderSection.DropPlace(from, offset, rowHeight: 60, count: 5));

    [Theory]
    [InlineData(0.0, 5)]
    [InlineData(-1.0, 5)]
    [InlineData(60.0, 0)]
    public void A_row_of_no_height_or_an_empty_list_stays_put(double rowHeight, int count) =>
        Assert.Equal(2, HelperOrderSection.DropPlace(2, 200, rowHeight, count));

    [Fact]
    public void A_lost_finger_position_stays_put() =>
        Assert.Equal(2, HelperOrderSection.DropPlace(2, double.NaN, 60, 5));

    [Fact]
    public void The_handle_is_named_for_a_screen_reader() =>
        Assert.Equal("Stoppuhr verschieben", Section().Rows[0].DragName);
}
