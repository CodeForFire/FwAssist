namespace Helfer.Helpers.Funkalphabet.Tests;

public class BuchstabiertafelTests
{
    private static List<string> Words(Buchstabiertafel table, string text) =>
        table.Spell(text).Select(c => c.Word).ToList();

    [Fact]
    public void Alt_has_the_full_1983_table_in_order()
    {
        string[] expected =
        [
            "Anton", "Ärger", "Berta", "Cäsar", "Charlotte", "Dora", "Emil", "Friedrich", "Gustav",
            "Heinrich", "Ida", "Julius", "Kaufmann", "Ludwig", "Martha", "Nordpol", "Otto", "Ökonom",
            "Paula", "Quelle", "Richard", "Samuel", "Schule", "Eszett", "Theodor", "Ulrich", "Übermut",
            "Viktor", "Wilhelm", "Xanthippe", "Ypsilon", "Zacharias",
        ];

        Assert.Equal(expected, Buchstabiertafel.Alt.Entries.Select(e => e.Word));
    }

    [Fact]
    public void Din2022_has_the_full_city_table_in_order()
    {
        string[] expected =
        [
            "Aachen", "Umlaut Aachen", "Berlin", "Chemnitz", "Düsseldorf", "Essen", "Frankfurt",
            "Goslar", "Hamburg", "Ingelheim", "Jena", "Köln", "Leipzig", "München", "Nürnberg",
            "Offenbach", "Umlaut Offenbach", "Potsdam", "Quickborn", "Rostock", "Salzwedel", "Eszett",
            "Tübingen", "Unna", "Umlaut Unna", "Völklingen", "Wuppertal", "Xanten", "Ypsilon", "Zwickau",
        ];

        Assert.Equal(expected, Buchstabiertafel.Din2022.Entries.Select(e => e.Word));
    }

    [Theory]
    [InlineData("A", "Anton", "Aachen")]
    [InlineData("B", "Berta", "Berlin")]
    [InlineData("S", "Samuel", "Salzwedel")]
    [InlineData("Z", "Zacharias", "Zwickau")]
    [InlineData("Ä", "Ärger", "Umlaut Aachen")]
    [InlineData("Ö", "Ökonom", "Umlaut Offenbach")]
    [InlineData("Ü", "Übermut", "Umlaut Unna")]
    [InlineData("ß", "Eszett", "Eszett")]
    public void A_single_letter_spells_as_its_word_in_either_table(string letter, string alt, string din)
    {
        Assert.Equal([alt], Words(Buchstabiertafel.Alt, letter));
        Assert.Equal([din], Words(Buchstabiertafel.Din2022, letter));
    }

    [Fact]
    public void Lower_case_spells_like_upper_case() =>
        Assert.Equal(["Otto", "Theodor", "Theodor", "Otto"], Words(Buchstabiertafel.Alt, "oTtO"));

    [Fact]
    public void Alt_spells_sch_and_ch_as_one_word_each() =>
        Assert.Equal(["Schule", "Ulrich", "Charlotte"], Words(Buchstabiertafel.Alt, "Schuch"));

    [Fact]
    public void Din2022_spells_sch_letter_by_letter() =>
        Assert.Equal(["Salzwedel", "Chemnitz", "Hamburg"], Words(Buchstabiertafel.Din2022, "Sch"));

    [Fact]
    public void Capital_eszett_spells_like_small_eszett() =>
        Assert.Equal(["Eszett"], Words(Buchstabiertafel.Alt, "ẞ"));

    [Fact]
    public void A_decomposed_umlaut_spells_as_the_umlaut() =>
        Assert.Equal(["Ärger"], Words(Buchstabiertafel.Alt, "Ä"));

    [Fact]
    public void Digits_and_signs_pass_through_unchanged()
    {
        var spelled = Buchstabiertafel.Alt.Spell("B-7");

        Assert.Equal(["Berta", "-", "7"], spelled.Select(c => c.Word));
        Assert.Equal(["B", "-", "7"], spelled.Select(c => c.Symbol));
    }

    [Fact]
    public void A_space_becomes_one_gap_however_many_there_are()
    {
        var spelled = Buchstabiertafel.Alt.Spell("A   B");

        Assert.Equal(3, spelled.Count);
        Assert.Equal(CodeWord.Gap, spelled[1]);
    }

    [Theory]
    [InlineData(" A")]
    [InlineData("A ")]
    public void Spaces_at_the_ends_leave_no_gap(string text) =>
        Assert.Equal(["Anton"], Words(Buchstabiertafel.Alt, text));

    [Fact]
    public void An_empty_text_spells_as_nothing() =>
        Assert.Empty(Buchstabiertafel.Alt.Spell(string.Empty));

    [Fact]
    public void An_emoji_passes_through_whole() =>
        Assert.Equal(["\U0001F692"], Words(Buchstabiertafel.Alt, "\U0001F692"));

    [Fact]
    public void A_broken_surrogate_does_not_throw()
    {
        // A MaxLength can cut an emoji in half.
        var spelled = Buchstabiertafel.Alt.Spell("A\uD83D");

        Assert.Equal(2, spelled.Count);
        Assert.Equal("Anton", spelled[0].Word);
    }
}
