using FwAssist.AppLogic.Services;
using FwAssist.AppLogic.ViewModels;

namespace FwAssist.AppLogic.Tests;

public class NumberFieldTests
{
    private static readonly NumberFieldRule Rule = new("rechner.wert", "Wert in m", 1m, 10m, "Von 1 bis 10 m möglich.");

    [Fact]
    public void A_value_stored_under_its_key_loads_as_typed()
    {
        var settings = new InMemoryHelperSettings();
        settings.Set("rechner.wert", "2,5");

        var field = new NumberField(settings, Rule, () => { });

        Assert.Equal("2,5", field.Text);
        Assert.Equal(2.5m, field.Value);
        Assert.True(field.IsValid);
    }

    [Fact]
    public void A_field_never_typed_into_shows_its_default()
    {
        var field = new NumberField(new InMemoryHelperSettings(), Rule with { Default = "3" }, () => { });

        Assert.Equal("3", field.Text);
        Assert.Equal(3m, field.Value);
    }

    [Fact]
    public void An_empty_field_has_no_value_and_no_error()
    {
        var field = new NumberField(new InMemoryHelperSettings(), Rule, () => { });

        Assert.Equal(string.Empty, field.Text);
        Assert.Null(field.Value);
        Assert.Null(field.Error);
        Assert.True(field.IsValid);
    }

    [Fact]
    public void Typing_stores_the_text_and_reports_the_change()
    {
        var settings = new InMemoryHelperSettings();
        var changes = 0;
        var field = new NumberField(settings, Rule, () => changes++);

        field.Text = "4";

        Assert.Equal("4", settings.Get("rechner.wert"));
        Assert.Equal(4m, field.Value);
        Assert.Equal(1, changes);
    }

    [Fact]
    public void Typing_the_same_text_again_changes_nothing()
    {
        var changes = 0;
        var field = new NumberField(new InMemoryHelperSettings(), Rule, () => changes++) { Text = "4" };

        field.Text = "4";

        Assert.Equal(1, changes);
    }

    [Fact]
    public void Text_that_is_no_number_asks_for_one()
    {
        var field = new NumberField(new InMemoryHelperSettings(), Rule, () => { }) { Text = "abc" };

        Assert.Equal("Zahl eingeben.", field.Error);
        Assert.False(field.IsValid);
        Assert.Null(field.Value);
    }

    [Theory]
    [InlineData("0,9")]
    [InlineData("10,1")]
    public void A_number_outside_the_range_shows_the_range_error(string text)
    {
        var field = new NumberField(new InMemoryHelperSettings(), Rule, () => { }) { Text = text };

        Assert.Equal("Von 1 bis 10 m möglich.", field.Error);
        Assert.Null(field.Value);
    }

    [Theory]
    [InlineData("1", 1)]
    [InlineData("10", 10)]
    public void Both_ends_of_the_range_are_allowed(string text, int expected)
    {
        var field = new NumberField(new InMemoryHelperSettings(), Rule, () => { }) { Text = text };

        Assert.Equal(expected, field.Value);
        Assert.True(field.IsValid);
    }

    [Fact]
    public void A_binding_writing_null_reads_as_empty()
    {
        var field = new NumberField(new InMemoryHelperSettings(), Rule, () => { }) { Text = "4" };

        field.Text = null;

        Assert.Equal(string.Empty, field.Text);
        Assert.Null(field.Value);
        Assert.True(field.IsValid);
    }

    [Fact]
    public void Error_and_validity_are_announced_with_the_text()
    {
        var field = new NumberField(new InMemoryHelperSettings(), Rule, () => { });
        var announced = new List<string?>();
        field.PropertyChanged += (_, e) => announced.Add(e.PropertyName);

        field.Text = "abc";

        Assert.Contains(nameof(NumberField.Text), announced);
        Assert.Contains(nameof(NumberField.Error), announced);
        Assert.Contains(nameof(NumberField.IsValid), announced);
    }

    [Fact]
    public void The_label_comes_from_the_rule() =>
        Assert.Equal("Wert in m", new NumberField(new InMemoryHelperSettings(), Rule, () => { }).Label);
}
