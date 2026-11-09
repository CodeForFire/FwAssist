namespace FwAssist.Helpers.Funkalphabet.Tests;

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

    [Fact]
    public void Nato_has_the_full_table_in_order()
    {
        string[] expected =
        [
            "Alfa", "Alfa Echo", "Bravo", "Charlie", "Delta", "Echo", "Foxtrot", "Golf", "Hotel", "India",
            "Juliett", "Kilo", "Lima", "Mike", "November", "Oscar", "Oscar Echo", "Papa", "Quebec", "Romeo",
            "Sierra", "Sierra Sierra", "Tango", "Uniform", "Uniform Echo", "Victor", "Whiskey", "X-ray",
            "Yankee", "Zulu",
        ];

        Assert.Equal(expected, Buchstabiertafel.Nato.Entries.Select(e => e.Word));
    }

    [Fact]
    public void Nato_spells_sch_letter_by_letter() =>
        Assert.Equal(["Sierra", "Charlie", "Hotel"], Words(Buchstabiertafel.Nato, "Sch"));

    [Fact]
    public void Nato_spells_a_german_letter_with_its_two_letters() =>
        Assert.Equal(
            ["Alfa Echo", "Oscar Echo", "Uniform Echo", "Sierra Sierra"],
            Words(Buchstabiertafel.Nato, "äöüß"));

    [Fact]
    public void Nato_keeps_the_german_radio_digits() =>
        Assert.Equal(["zwo", "drei"], Words(Buchstabiertafel.Nato, "23"));

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
    public void A_digit_is_spoken_as_its_word_and_a_sign_stands_for_itself()
    {
        var spelled = Buchstabiertafel.Alt.Spell("B-7");

        Assert.Equal(["Berta", "-", "sieben"], spelled.Select(c => c.Word));
        Assert.Equal(["B", "-", "7"], spelled.Select(c => c.Symbol));
    }

    [Fact]
    public void Every_digit_has_its_word_and_two_is_zwo()
    {
        string[] words = ["null", "eins", "zwo", "drei", "vier", "fünf", "sechs", "sieben", "acht", "neun"];

        Assert.Equal(words, Words(Buchstabiertafel.Alt, "0123456789"));
        Assert.Equal(words, Words(Buchstabiertafel.Din2022, "0123456789"));
    }

    [Fact]
    public void A_number_is_spoken_digit_by_digit_in_either_table()
    {
        string[] words = ["eins", "/", "vier", "vier", "-", "zwo"];

        Assert.Equal(words, Words(Buchstabiertafel.Alt, "1/44-2"));
        Assert.Equal(words, Words(Buchstabiertafel.Din2022, "1/44-2"));
    }

    [Fact]
    public void The_digits_stay_out_of_the_reference_tables()
    {
        Assert.DoesNotContain(Buchstabiertafel.Alt.Entries, e => char.IsDigit(e.Symbol[0]));
        Assert.DoesNotContain(Buchstabiertafel.Din2022.Entries, e => char.IsDigit(e.Symbol[0]));
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

    [Theory]
    [InlineData("A\u200BB")] // zero width space
    [InlineData("A\u200DB")] // zero width joiner
    [InlineData("A\u00ADB")] // soft hyphen
    [InlineData("A\uFE0FB")] // variation selector
    public void An_invisible_character_leaves_no_row(string text) =>
        Assert.Equal(["Anton", "Berta"], Words(Buchstabiertafel.Alt, text));

    [Fact]
    public void An_invisible_character_outside_the_bmp_leaves_no_row() =>
        Assert.Equal(["Anton", "Berta"], Words(Buchstabiertafel.Alt, "A\U000E0001B"));

    [Fact]
    public void A_soft_hyphen_does_not_split_sch() =>
        Assert.Equal(["Schule"], Words(Buchstabiertafel.Alt, "S\u00ADCH"));

    [Fact]
    public void A_joined_emoji_spells_as_its_visible_parts() =>
        Assert.Equal(["\U0001F469", "\U0001F692"], Words(Buchstabiertafel.Alt, "\U0001F469\u200D\U0001F692"));

    [Fact]
    public void Nothing_but_invisible_characters_spells_as_nothing() =>
        Assert.Empty(Buchstabiertafel.Alt.Spell("\u200B\u00AD\uFE0F"));

    [Fact]
    public void A_broken_surrogate_does_not_throw()
    {
        // A MaxLength can cut an emoji in half.
        var spelled = Buchstabiertafel.Alt.Spell("A\uD83D");

        Assert.Equal(2, spelled.Count);
        Assert.Equal("Anton", spelled[0].Word);
    }
}
