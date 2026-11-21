namespace FwAssist.Helpers.Kennzeichnungen.Tests;

public class KennzeichenkatalogTests
{
    [Fact]
    public void Every_entry_sits_in_the_group_that_lists_it()
    {
        foreach (var gruppe in Enum.GetValues<KennzeichenGruppe>())
        {
            Assert.All(Kennzeichenkatalog.Of(gruppe), e => Assert.Equal(gruppe, e.Gruppe));
        }
    }

    [Fact]
    public void Every_entry_has_a_picture_of_its_own()
    {
        var keys = Kennzeichenkatalog.All.Select(e => e.PictureKey).ToList();

        Assert.Equal(keys.Count, keys.Distinct(StringComparer.Ordinal).Count());
    }

    [Fact]
    public void Every_entry_names_its_source_and_says_what_it_looks_like()
    {
        Assert.All(Kennzeichenkatalog.All, e =>
        {
            Assert.False(string.IsNullOrWhiteSpace(e.Source), e.PictureKey);
            Assert.False(string.IsNullOrWhiteSpace(e.Look), e.PictureKey);
            Assert.False(string.IsNullOrWhiteSpace(e.Meaning), e.PictureKey);
        });
    }

    [Fact]
    public void The_gefahrzettel_are_every_label_of_adr_5_2_2_2_2()
    {
        Assert.Equal(
            ["1", "1.4", "1.5", "1.6", "2.1", "2.2", "2.3", "3", "4.1", "4.2", "4.3", "5.1", "5.2", "6.1", "6.2", "7A", "7B", "7C", "7E", "8", "9", "9A"],
            Kennzeichenkatalog.Gefahrzettel.Select(e => e.Code));
    }

    [Fact]
    public void The_ghs_piktogramme_are_ghs01_to_ghs09()
    {
        Assert.Equal(
            Enumerable.Range(1, 9).Select(i => "GHS0" + i),
            Kennzeichenkatalog.Ghs.Select(e => e.Code));
    }

    [Fact]
    public void All_lists_every_group_in_screen_order()
    {
        Assert.Equal(
            [.. Kennzeichenkatalog.Warntafeln, .. Kennzeichenkatalog.Gefahrzettel, .. Kennzeichenkatalog.Ghs],
            Kennzeichenkatalog.All);
    }

    [Fact]
    public void An_unknown_group_falls_back_to_the_warntafeln()
    {
        Assert.Same(Kennzeichenkatalog.Warntafeln, Kennzeichenkatalog.Of((KennzeichenGruppe)99));
    }
}
