using Helfer.AppLogic.Services;

namespace Helfer.AppLogic.Tests;

public class InMemoryHelperSettingsTests
{
    [Fact]
    public void A_key_never_set_reads_as_null() =>
        Assert.Null(new InMemoryHelperSettings().Get("stoppuhr.x"));

    [Fact]
    public void A_value_set_reads_back_and_the_last_one_wins()
    {
        var settings = new InMemoryHelperSettings();

        settings.Set("stoppuhr.x", "1");
        settings.Set("stoppuhr.x", "2");

        Assert.Equal("2", settings.Get("stoppuhr.x"));
    }

    [Fact]
    public void Keys_compare_exactly()
    {
        var settings = new InMemoryHelperSettings();

        settings.Set("stoppuhr.x", "1");

        Assert.Null(settings.Get("Stoppuhr.X"));
    }
}
