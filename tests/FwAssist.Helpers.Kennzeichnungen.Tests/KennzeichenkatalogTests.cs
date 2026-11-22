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
            [.. Kennzeichenkatalog.Warntafeln, .. Kennzeichenkatalog.Gefahrzettel, .. Kennzeichenkatalog.Ghs, .. Kennzeichenkatalog.Rohrleitungen, .. Kennzeichenkatalog.Gasflaschen],
            Kennzeichenkatalog.All);
    }

    [Fact]
    public void The_rohrleitungen_are_the_ten_groups_of_trgs_201_anhang_3()
    {
        Assert.Equal(
            ["Gruppe 1", "Gruppe 2", "Gruppe 3", "Gruppe 4", "Gruppe 5", "Gruppe 6", "Gruppe 7", "Gruppe 8", "Gruppe 9", "Gruppe 0"],
            Kennzeichenkatalog.Rohrleitungen.Select(e => e.Code));
    }

    [Theory]
    [InlineData("rohr-4", "Brennbare Gase", "Gruppenfarbe gelb, Zusatzfarbe rot.")]
    [InlineData("rohr-5", "Nichtbrennbare Gase", "Gruppenfarbe gelb, Zusatzfarbe schwarz.")]
    [InlineData("rohr-1", "Wasser", "Gruppenfarbe grün.")]
    public void A_rohrleitung_says_its_colours_in_words(string key, string stoff, string colours)
    {
        var entry = Kennzeichenkatalog.Rohrleitungen.Single(e => e.PictureKey == key);

        Assert.Equal(stoff, entry.Name);
        Assert.StartsWith(colours, entry.Meaning, StringComparison.Ordinal);
    }

    [Fact]
    public void A_rohrleitung_without_an_extra_colour_says_no_extra_colour()
    {
        var wasser = Kennzeichenkatalog.Rohrleitungen.Single(e => e.PictureKey == "rohr-1");

        Assert.DoesNotContain("Zusatzfarbe", wasser.Meaning, StringComparison.Ordinal);
        Assert.Equal("Band grün, Pfeil und Schrift weiß", wasser.Look);
    }

    [Fact]
    public void The_gasflaschen_cover_every_shoulder_colour_of_en_1089_3()
    {
        Assert.Equal(
            ["gelb", "rot", "hellblau", "leuchtend grün", "kastanienbraun", "weiß", "blau", "dunkelgrün", "schwarz", "grau", "braun", "türkisblau"],
            Kennzeichenkatalog.Gasflaschen.Select(e => e.Code).Where(c => !c.Contains('+', StringComparison.Ordinal) && c != "weißer Mantel"));
        Assert.All(Kennzeichenkatalog.Gasflaschen, e => Assert.StartsWith("EN 1089-3:2011, ", e.Source, StringComparison.Ordinal));
    }

    [Fact]
    public void An_unknown_group_falls_back_to_the_warntafeln()
    {
        Assert.Same(Kennzeichenkatalog.Warntafeln, Kennzeichenkatalog.Of((KennzeichenGruppe)99));
    }
}
