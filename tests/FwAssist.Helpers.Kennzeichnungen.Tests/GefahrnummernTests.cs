namespace FwAssist.Helpers.Kennzeichnungen.Tests;

public class GefahrnummernTests
{
    [Theory]
    [InlineData("33", "leicht entzündbarer flüssiger Stoff (Flammpunkt unter 23 °C)")]
    [InlineData("80", "ätzender oder schwach ätzender Stoff")]
    [InlineData("X423", "fester Stoff, der mit Wasser gefährlich reagiert und entzündbare Gase bildet, oder entzündbarer fester Stoff, der mit Wasser gefährlich reagiert und entzündbare Gase bildet, oder selbsterhitzungsfähiger fester Stoff, der mit Wasser gefährlich reagiert und entzündbare Gase bildet")]
    [InlineData("99", "verschiedene gefährliche erwärmte Stoffe")]
    public void A_listed_number_reads_as_the_adr_entry(string text, string meaning)
    {
        Assert.True(Gefahrnummern.TryRead(text, out var number));

        Assert.Equal(text, number.Number);
        Assert.Equal(meaning, number.Meaning);
    }

    [Theory]
    [InlineData(" 3 3 ", "33")]
    [InlineData("x80", "X80")]
    [InlineData("X 88", "X88")]
    public void Spaces_and_a_lower_case_x_are_read_as_typed_on_a_phone(string text, string expected)
    {
        Assert.True(Gefahrnummern.TryRead(text, out var number));

        Assert.Equal(expected, number.Number);
        Assert.NotNull(number.Meaning);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("3")]
    [InlineData("3333")]
    [InlineData("03")]
    [InlineData("13")]
    [InlineData("31")]
    [InlineData("1203")]
    [InlineData("XX80")]
    [InlineData("Y80")]
    [InlineData("8O")]
    [InlineData("X")]
    public void Anything_but_two_or_three_hazard_digits_is_no_gefahrnummer(string? text)
    {
        Assert.False(Gefahrnummern.TryRead(text, out var number));
        Assert.Null(number);
    }

    [Fact]
    public void The_digits_are_read_one_by_one_and_a_doubled_digit_is_stronger()
    {
        Assert.True(Gefahrnummern.TryRead("336", out var number));

        Assert.Equal(
            [
                new Ziffer('3', "Entzündbarkeit von flüssigen Stoffen (Dämpfen) und Gasen oder selbsterhitzungsfähiger flüssiger Stoff"),
                new Ziffer('3', "verstärkt: Entzündbarkeit von flüssigen Stoffen (Dämpfen) und Gasen oder selbsterhitzungsfähiger flüssiger Stoff"),
                new Ziffer('6', "Giftigkeit oder Ansteckungsgefahr"),
            ],
            number.Digits);
        Assert.False(number.IsSpecialCombination);
        Assert.False(number.ReactsWithWater);
    }

    [Fact]
    public void A_zero_says_one_digit_is_enough()
    {
        Assert.True(Gefahrnummern.TryRead("60", out var number));

        Assert.Equal(new Ziffer('0', "keine weitere Gefahr"), number.Digits[1]);
    }

    [Theory]
    [InlineData("22")]
    [InlineData("323")]
    [InlineData("333")]
    [InlineData("362")]
    [InlineData("382")]
    [InlineData("423")]
    [InlineData("44")]
    [InlineData("446")]
    [InlineData("462")]
    [InlineData("482")]
    [InlineData("539")]
    [InlineData("606")]
    [InlineData("623")]
    [InlineData("642")]
    [InlineData("823")]
    [InlineData("842")]
    [InlineData("90")]
    [InlineData("99")]
    public void A_special_combination_is_not_read_digit_by_digit(string text)
    {
        Assert.True(Gefahrnummern.TryRead(text, out var number));

        Assert.True(number.IsSpecialCombination);
        Assert.Empty(number.Digits);
        Assert.NotNull(number.Meaning);
    }

    [Fact]
    public void A_special_combination_stays_special_behind_an_x()
    {
        Assert.True(Gefahrnummern.TryRead("X323", out var number));

        Assert.True(number.IsSpecialCombination);
        Assert.True(number.ReactsWithWater);
    }

    [Fact]
    public void An_x_means_the_stoff_reacts_dangerously_with_water()
    {
        Assert.True(Gefahrnummern.TryRead("X88", out var number));

        Assert.True(number.ReactsWithWater);
        Assert.Equal("stark ätzender Stoff, der mit Wasser gefährlich reagiert", number.Meaning);
    }

    [Fact]
    public void A_number_the_adr_does_not_list_is_read_but_has_no_meaning()
    {
        Assert.True(Gefahrnummern.TryRead("57", out var number));

        Assert.Null(number.Meaning);
        Assert.Equal(2, number.Digits.Count);
    }

    [Fact]
    public void Seventy_eight_is_no_longer_listed_since_adr_2025()
    {
        Assert.True(Gefahrnummern.TryRead("78", out var number));

        Assert.Null(number.Meaning);
        Assert.Equal(['7', '8'], number.Digits.Select(d => d.Digit));
    }

    [Fact]
    public void Every_listed_number_reads_back_as_itself_with_its_meaning()
    {
        Assert.All(Gefahrnummern.ListedNumbers, listed =>
        {
            Assert.True(Gefahrnummern.TryRead(listed, out var number), listed);
            Assert.Equal(listed, number.Number);
            Assert.False(string.IsNullOrWhiteSpace(number.Meaning));
        });
    }

    [Fact]
    public void The_list_holds_every_number_of_adr_5_3_2_3_2_once()
    {
        Assert.Equal(97, Gefahrnummern.ListedNumbers.Count);
        Assert.Equal(Gefahrnummern.ListedNumbers.Count, Gefahrnummern.ListedNumbers.Distinct(StringComparer.Ordinal).Count());
    }

    [Fact]
    public void Every_x_entry_is_a_water_reactive_reading()
    {
        Assert.All(
            Gefahrnummern.ListedNumbers.Where(n => n.StartsWith('X')),
            listed =>
            {
                Assert.True(Gefahrnummern.TryRead(listed, out var number));
                Assert.True(number.ReactsWithWater, listed);
            });
    }
}
