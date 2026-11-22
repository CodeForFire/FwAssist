using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.VisualTree;
using FwAssist.AppLogic.ViewModels;
using FwAssist.Composition;
using FwAssist.Helpers.DekonPlatz;
using FwAssist.Helpers.Funkalphabet;
using FwAssist.Helpers.Hydranten;
using FwAssist.Helpers.Kennzeichnungen;
using FwAssist.Helpers.Kennzeichnungen.Ui;
using FwAssist.Helpers.RthLandeplatz;
using FwAssist.Helpers.Schaummittel;
using FwAssist.Helpers.Strahlenschutz;

namespace FwAssist.Acceptance.Tests;

// The screens a new user sees, in order. Doubles as the screenshot source: `make screenshots`,
// `make phone-screenshots` and `make play-screenshots` run this class with RENDER_OUT set.
public class DemoFlowRenderTests
{
    private static List<string?> Texts(Control root) =>
        root.GetVisualDescendants().OfType<TextBlock>().Select(t => t.Text).ToList();

    // The spelled line only: the reference table below it shows every code word anyway.
    private static string? SpelledLine(Control root)
    {
        var spelled = root.GetVisualDescendants().OfType<TextBlock>().Single(c => c.Name == "Spelled");
        Assert.True(spelled.IsVisible);
        return spelled.Text;
    }

    // The result lines only, label then value: the form above them holds similar words.
    private static List<string?> ResultTexts(Control root)
    {
        var results = root.GetVisualDescendants().OfType<ItemsControl>().Single(c => c.Name == "Results");
        Assert.True(results.IsEffectivelyVisible);
        return Texts(results);
    }

    private static SchaummittelViewModel OpenSchaummittel(PhoneRender phone)
    {
        phone.Shell.Open(SchaummittelModule.HelperId);
        PhoneRender.Settle();
        return phone.Window.GetVisualDescendants().OfType<Control>()
            .Select(c => c.DataContext).OfType<SchaummittelViewModel>().First();
    }

    [AvaloniaFact]
    public void Home_lists_every_helper_with_its_summary()
    {
        var phone = new PhoneRender();

        var texts = Texts(phone.Window);
        foreach (var module in CompositionRoot.Modules)
        {
            Assert.Contains(module.Title, texts);
            Assert.Contains(module.Summary, texts);
        }

        phone.Capture("01-uebersicht.png");
    }

    [AvaloniaFact]
    public void Info_on_the_home_screen_opens_the_about_page()
    {
        var phone = new PhoneRender();
        var info = phone.Window.GetVisualDescendants().OfType<Button>().Single(b => b.Name == "AboutButton");

        Assert.True(info.Bounds.Height >= 48, $"INFO is {info.Bounds.Height} high");
        Assert.NotNull(info.Command);
        info.Command.Execute(null);
        PhoneRender.Settle();

        var texts = Texts(phone.Window);
        Assert.Contains("0.1.0", texts);
        Assert.Contains("Ein Projekt von CodeForFire", texts);
        Assert.Contains("https://www.github.com/codeforfire/FwAssist", texts);
        Assert.Contains("Veröffentlicht unter der MIT-Lizenz.", texts);
        Assert.Contains("Copyright (C) 2026 Christian Arlt", texts);
        Assert.True(phone.Window.GetVisualDescendants().OfType<Button>().Single(b => b.Name == "RepositoryLink").Bounds.Height >= 48);

        phone.Capture("11-info.png");
    }

    [AvaloniaFact]
    public void Tapping_the_stoppuhr_tile_opens_it_and_the_time_runs()
    {
        var phone = new PhoneRender();
        var tile = phone.Window.GetVisualDescendants().OfType<Button>().First(b => b.Classes.Contains("tile"));

        Assert.NotNull(tile.Command);
        tile.Command.Execute(null);
        PhoneRender.Settle();
        var start = phone.Window.GetVisualDescendants().OfType<Button>().First(b => Equals(b.Content, "START"));
        Assert.NotNull(start.Command);
        start.Command.Execute(null);
        phone.Time.Advance(TimeSpan.FromSeconds(247));
        phone.Ticker.Tick();
        PhoneRender.Settle();

        var texts = Texts(phone.Window);
        Assert.Contains("04:07", texts);
        Assert.Contains("Stoppuhr", texts);
        Assert.True(phone.Window.GetVisualDescendants().OfType<Button>().Single(b => b.Name == "HomeButton").IsVisible);

        phone.Capture("02-stoppuhr.png");
    }

    [AvaloniaFact]
    public void Settings_list_a_launcher_icon_switch_per_helper()
    {
        var phone = new PhoneRender();
        phone.Shell.Home.OpenSettingsCommand.Execute(null);
        PhoneRender.Settle();

        var toggles = phone.Window.GetVisualDescendants().OfType<ToggleSwitch>().ToList();
        Assert.Equal(CompositionRoot.Modules.Count, toggles.Count);
        Assert.All(toggles, t => Assert.True(t.IsChecked));

        phone.Capture("03-einstellungen.png");
    }

    [AvaloniaFact]
    public void Dragging_a_helper_by_its_grip_in_einstellungen_moves_it_on_the_overview()
    {
        var phone = new PhoneRender();
        var first = CompositionRoot.Modules[0].Title;
        var third = CompositionRoot.Modules[2].Title;
        phone.Shell.Home.OpenSettingsCommand.Execute(null);
        PhoneRender.Settle();
        Named(phone, "ResetOrder").BringIntoView();
        PhoneRender.Settle();
        var grip = phone.Window.GetVisualDescendants().OfType<Border>()
            .Single(b => AutomationProperties.GetName(b) == third + " verschieben");
        var rowHeight = grip.GetVisualAncestors().OfType<Border>().First(b => b.Classes.Contains("order-row")).Bounds.Height;
        var start = grip.TranslatePoint(new Point(grip.Bounds.Width / 2, grip.Bounds.Height / 2), phone.Window);
        Assert.NotNull(start);
        Assert.True(rowHeight > 0);

        // Up by two rows, in small steps as a finger moves, and let go above the first helper.
        phone.Window.MouseDown(start.Value, MouseButton.Left);
        for (var step = 1; step <= 10; step++)
        {
            phone.Window.MouseMove(start.Value - new Point(0, rowHeight * 2.2 * step / 10));
        }

        phone.Window.MouseUp(start.Value - new Point(0, rowHeight * 2.2), MouseButton.Left);
        phone.Shell.GoHome();
        PhoneRender.Settle();

        var tiles = Named(phone, "Tiles").GetVisualDescendants().OfType<Button>().Select(AutomationProperties.GetName).ToList();
        Assert.Equal([third, first], tiles.Take(2));
    }

    // A drag in Einstellungen, picked up at the grip of the helper at index, and moved up by rows.
    private static (HelperOrderSection Order, Point At) StartDrag(PhoneRender phone, int index, double rows)
    {
        phone.Shell.Home.OpenSettingsCommand.Execute(null);
        PhoneRender.Settle();
        Named(phone, "ResetOrder").BringIntoView();
        PhoneRender.Settle();
        var order = Assert.IsType<SettingsViewModel>(phone.Shell.Current).Order;
        Assert.NotNull(order);
        var grip = phone.Window.GetVisualDescendants().OfType<Border>()
            .Single(b => AutomationProperties.GetName(b) == CompositionRoot.Modules[index].Title + " verschieben");
        var rowHeight = grip.GetVisualAncestors().OfType<Border>().First(b => b.Classes.Contains("order-row")).Bounds.Height;
        var start = grip.TranslatePoint(new Point(grip.Bounds.Width / 2, grip.Bounds.Height / 2), phone.Window);
        Assert.NotNull(start);
        phone.Window.MouseDown(start.Value, MouseButton.Left);
        for (var step = 1; step <= 10; step++)
        {
            phone.Window.MouseMove(start.Value - new Point(0, rowHeight * rows * step / 10));
        }

        return (order, start.Value - new Point(0, rowHeight * rows));
    }

    [AvaloniaFact]
    public void While_a_helper_is_dragged_the_order_cannot_be_reset()
    {
        var phone = new PhoneRender();
        var (order, at) = StartDrag(phone, 2, 2.2);

        Assert.True(order.IsDragging);
        Assert.False(Named(phone, "ResetOrder").IsEffectivelyEnabled);

        phone.Window.MouseUp(at, MouseButton.Left);
        PhoneRender.Settle();
        Assert.False(order.IsDragging);
        Assert.True(Named(phone, "ResetOrder").IsEffectivelyEnabled);
        Assert.Equal(CompositionRoot.Modules[2].Id, order.Ids[0]);
    }

    [AvaloniaFact]
    public void A_second_finger_let_go_elsewhere_does_not_end_the_first_fingers_drag()
    {
        var phone = new PhoneRender();
        var before = CompositionRoot.Modules.Select(m => m.Id).ToList();
        var (order, at) = StartDrag(phone, 2, 2.2);
        var rows = Named(phone, "HelperOrderRows");

        // Another finger, touching and letting go of another row while the first still drags.
        var other = new Pointer(Pointer.GetNextFreeId(), PointerType.Touch, isPrimary: false);
        rows.RaiseEvent(new PointerReleasedEventArgs(
            rows, other, phone.Window, new Point(10, 10), 0, new PointerPointProperties(), KeyModifiers.None, MouseButton.Left));
        PhoneRender.Settle();

        Assert.Equal(before, order.Ids);
        Assert.True(order.IsDragging);

        phone.Window.MouseUp(at, MouseButton.Left);
        PhoneRender.Settle();
        Assert.Equal(CompositionRoot.Modules[2].Id, order.Ids[0]);
    }

    [AvaloniaFact]
    public void The_funkalphabet_spells_what_is_typed_and_switches_to_the_din_table()
    {
        var phone = new PhoneRender();
        phone.Shell.Open(FunkalphabetModule.HelperId);
        PhoneRender.Settle();
        var input = phone.Window.GetVisualDescendants().OfType<TextBox>().Single(t => t.Name == "Input");

        input.Text = "Schlauch";
        PhoneRender.Settle();

        Assert.Contains("BOS (DIN 5009:1983)", Texts(phone.Window));
        Assert.Equal("Schule - Ludwig - Anton - Ulrich - Charlotte", SpelledLine(phone.Window));

        phone.Capture("04-funkalphabet.png");

        var alt = phone.Window.GetVisualDescendants().OfType<RadioButton>().Single(b => b.Name == "AltTable");
        var din = phone.Window.GetVisualDescendants().OfType<RadioButton>().Single(b => b.Name == "Din2022Table");
        Assert.True(alt.IsChecked);
        din.IsChecked = true;
        PhoneRender.Settle();

        Assert.False(alt.IsChecked);
        Assert.Contains("DIN 5009:2022", Texts(phone.Window));
        Assert.Equal(
            "Salzwedel - Chemnitz - Hamburg - Leipzig - Aachen - Unna - Chemnitz - Hamburg",
            SpelledLine(phone.Window));

        var nato = phone.Window.GetVisualDescendants().OfType<RadioButton>().Single(b => b.Name == "NatoTable");
        nato.IsChecked = true;
        PhoneRender.Settle();

        Assert.False(din.IsChecked);
        Assert.True(nato.IsChecked);
        Assert.Contains("NATO", Texts(phone.Window));
        Assert.Equal(
            "Sierra - Charlie - Hotel - Lima - Alfa - Uniform - Charlie - Hotel",
            SpelledLine(phone.Window));

        var clear = phone.Window.GetVisualDescendants().OfType<Button>().Single(b => b.Name == "ClearButton");
        Assert.NotNull(clear.Command);
        clear.Command.Execute(null);
        PhoneRender.Settle();

        Assert.True(string.IsNullOrEmpty(input.Text));
    }

    [AvaloniaFact]
    public void The_schaummittel_rechner_works_out_a_room_and_shows_only_that_mode()
    {
        var phone = new PhoneRender();
        var page = OpenSchaummittel(phone);
        var panels = phone.Window.GetVisualDescendants().OfType<StackPanel>().ToList();
        Assert.False(panels.Single(p => p.Name == "RaumFields").IsVisible);

        phone.Window.GetVisualDescendants().OfType<RadioButton>().Single(b => b.Name == "RaumMode").IsChecked = true;
        page.Verschaeumungszahl.Text = "10";
        page.Laenge.Text = "10";
        page.Breite.Text = "5";
        page.Hoehe.Text = "3";
        PhoneRender.Settle();

        Assert.True(page.IsRaum);
        Assert.True(panels.Single(p => p.Name == "RaumFields").IsVisible);
        Assert.False(panels.Single(p => p.Name == "VorratFields").IsVisible);
        Assert.Equal(
            ["Raum", "150,0 m³", "Schaum", "150,0 m³", "Gemisch", "15.000 l", "Schaummittel", "450 l", "Wasser", "14.550 l"],
            ResultTexts(phone.Window));

        phone.Capture("05-schaummittel.png");
    }

    [AvaloniaFact]
    public void A_bad_schaummittel_value_shows_its_error_under_the_field()
    {
        var phone = new PhoneRender();
        var page = OpenSchaummittel(phone);

        page.Zumischrate.Text = "300";
        PhoneRender.Settle();

        var errors = phone.Window.GetVisualDescendants().OfType<TextBlock>()
            .Where(t => t.Classes.Contains("field-error") && t.IsEffectivelyVisible)
            .Select(t => t.Text);
        Assert.Equal(["Von 0,1 bis 50 % möglich."], errors);
    }

    [AvaloniaFact]
    public void The_smallest_schaummittel_preset_sets_a_valid_zumischrate()
    {
        var phone = new PhoneRender();
        var page = OpenSchaummittel(phone);
        var button = phone.Window.GetVisualDescendants().OfType<Button>().Single(b => Equals(b.Content, "0,1 %"));

        Assert.NotNull(button.Command);
        button.Command.Execute(button.CommandParameter);
        PhoneRender.Settle();

        Assert.Equal("0,1", page.Zumischrate.Text);
        Assert.Equal(0.1m, page.Zumischrate.Value);
    }

    [AvaloniaTheory]
    [InlineData("Zumischrate in %", "0,1 %")]
    [InlineData("Zumischrate in %", "1 %")]
    [InlineData("Verschäumungszahl", "LEICHT 500")]
    public void A_schaummittel_preset_sits_beside_its_field_at_glove_size(string field, string preset)
    {
        var phone = new PhoneRender();
        OpenSchaummittel(phone);

        var input = phone.Window.GetVisualDescendants().OfType<TextBox>()
            .Single(t => AutomationProperties.GetName(t) == field);
        var button = phone.Window.GetVisualDescendants().OfType<Button>().Single(b => Equals(b.Content, preset));
        var inputTop = input.TranslatePoint(default, phone.Window);
        var buttonTop = button.TranslatePoint(default, phone.Window);

        Assert.NotNull(inputTop);
        Assert.NotNull(buttonTop);
        Assert.Equal(inputTop.Value.Y, buttonTop.Value.Y, precision: 0);
        Assert.True(button.Bounds.Height >= 48, $"{preset} is {button.Bounds.Height} high");
    }

    [AvaloniaFact]
    public void The_schaummittel_zuschlag_is_as_narrow_as_the_fields_with_presets()
    {
        var phone = new PhoneRender();
        var page = OpenSchaummittel(phone);
        page.IsRaum = true;
        PhoneRender.Settle();

        double WidthOf(string field) => phone.Window.GetVisualDescendants().OfType<TextBox>()
            .Single(t => AutomationProperties.GetName(t) == field).Bounds.Width;

        Assert.Equal(WidthOf("Zumischrate in %"), WidthOf("Zuschlag für Verluste in %"), precision: 0);
    }

    [AvaloniaFact]
    public void Every_schaummittel_preset_stays_glove_wide_on_a_320_dp_phone()
    {
        var phone = new PhoneRender();
        phone.Window.Width = 320;
        OpenSchaummittel(phone);

        var presets = phone.Window.GetVisualDescendants().OfType<Button>()
            .Where(b => b.Classes.Contains("preset")).ToList();

        Assert.Equal(7, presets.Count);
        Assert.All(presets, b => Assert.True(b.Bounds.Width >= 48, $"{b.Content} is {b.Bounds.Width} wide"));
    }

    // 780 dp is about what a 915 dp phone leaves between its status and navigation bars.
    [AvaloniaFact]
    public void A_whole_schaummittel_room_calculation_fits_without_scrolling()
    {
        var phone = new PhoneRender();
        phone.Window.Height = 780;
        var page = OpenSchaummittel(phone);

        page.IsRaum = true;
        page.Verschaeumungszahl.Text = "10";
        page.Laenge.Text = "10";
        page.Breite.Text = "5";
        page.Hoehe.Text = "3";
        PhoneRender.Settle();

        var form = phone.Window.GetVisualDescendants().OfType<ScrollViewer>().Single(s => s.Name == "Form");
        Assert.True(
            form.Extent.Height <= form.Viewport.Height + 1,
            $"The form is {form.Extent.Height} high in a viewport of {form.Viewport.Height}");
    }

    private static StrahlenschutzViewModel OpenStrahlenschutz(PhoneRender phone)
    {
        phone.Shell.Open(StrahlenschutzModule.HelperId);
        PhoneRender.Settle();
        return phone.Window.GetVisualDescendants().OfType<Control>()
            .Select(c => c.DataContext).OfType<StrahlenschutzViewModel>().First();
    }

    [AvaloniaFact]
    public void The_strahlenschutz_absperrung_shows_the_absperrgrenze_and_only_its_fields()
    {
        var phone = new PhoneRender();
        var page = OpenStrahlenschutz(phone);

        ((RadioButton)Named(phone, "AbsperrungMode")).IsChecked = true;
        page.Dosisleistung.Text = "400";
        page.Messabstand.Text = "5";
        PhoneRender.Settle();

        Assert.True(page.IsAbsperrung);
        Assert.False(Named(phone, "TkzFields").IsVisible);
        Assert.False(Named(phone, "ZielabstandField").IsVisible);
        Assert.True(Named(phone, "GrenzwertField").IsVisible);
        Assert.Equal(["Absperrgrenze", "20 m"], ResultTexts(phone.Window));

        phone.Capture("20-strahlenschutz.png");
    }

    [AvaloniaFact]
    public void The_strahlenschutz_tkz_shows_the_dose_rate_beside_the_package()
    {
        var phone = new PhoneRender();
        var page = OpenStrahlenschutz(phone);

        page.Tkz.Text = "2";
        page.TkzAbstand.Text = "2";
        PhoneRender.Settle();

        Assert.False(Named(phone, "MessungFields").IsVisible);
        Assert.Equal(["In 1 m", "20 µSv/h", "In 2 m", "5,0 µSv/h"], ResultTexts(phone.Window));
    }

    [AvaloniaFact]
    public void A_bad_strahlenschutz_value_shows_its_error_under_the_field()
    {
        var phone = new PhoneRender();
        var page = OpenStrahlenschutz(phone);

        page.TkzAbstand.Text = "0";
        PhoneRender.Settle();

        var errors = phone.Window.GetVisualDescendants().OfType<TextBlock>()
            .Where(t => t.Classes.Contains("field-error") && t.IsEffectivelyVisible)
            .Select(t => t.Text);
        Assert.Equal(["Von 0,1 bis 10.000 m möglich."], errors);
    }

    [AvaloniaFact]
    public void The_strahlenschutz_modes_and_the_grenzwert_preset_stay_glove_sized_on_a_320_dp_phone()
    {
        var phone = new PhoneRender();
        phone.Window.Width = 320;
        var page = OpenStrahlenschutz(phone);
        page.IsAbsperrung = true;
        PhoneRender.Settle();

        foreach (var name in new[] { "TkzMode", "AbstandMode", "AbsperrungMode", "GrenzwertPreset" })
        {
            var control = Named(phone, name);
            Assert.True(control.Bounds.Height >= 48, $"{name} is {control.Bounds.Height} high");
            Assert.True(control.Bounds.Width >= 48, $"{name} is {control.Bounds.Width} wide");
        }

        var label = Named(phone, "AbsperrungMode").GetVisualDescendants().OfType<TextBlock>().Single(t => t.Text == "ABSPERRUNG");
        Assert.True(label.Bounds.Right <= Named(phone, "AbsperrungMode").Bounds.Width + 1, "ABSPERRUNG is cut off");
    }

    // 780 dp is about what a 915 dp phone leaves between its status and navigation bars.
    [AvaloniaFact]
    public void A_whole_strahlenschutz_calculation_fits_without_scrolling()
    {
        var phone = new PhoneRender();
        phone.Window.Height = 780;
        var page = OpenStrahlenschutz(phone);

        page.IsAbsperrung = true;
        page.Dosisleistung.Text = "400";
        page.Messabstand.Text = "5";
        PhoneRender.Settle();

        var form = phone.Window.GetVisualDescendants().OfType<ScrollViewer>().Single(s => s.Name == "Form");
        Assert.True(
            form.Extent.Height <= form.Viewport.Height + 1,
            $"The form is {form.Extent.Height} high in a viewport of {form.Viewport.Height}");
    }

    private static Control Named(PhoneRender phone, string name) =>
        phone.Window.GetVisualDescendants().OfType<Control>().Single(c => c.Name == name);

    private static List<string> VisibleTexts(Control root) =>
        root.GetVisualDescendants().OfType<TextBlock>()
            .Where(t => t.IsEffectivelyVisible)
            .Select(t => t.Text ?? string.Empty)
            .ToList();

    private static List<string?> DrawingNames(Control root) =>
        root.GetVisualDescendants().OfType<UserControl>()
            .Where(c => c.GetType().Name.EndsWith("Drawing", StringComparison.Ordinal))
            .Select(AutomationProperties.GetName)
            .ToList();

    [AvaloniaFact]
    public void The_rth_landeplatz_shows_only_the_values_of_its_mode()
    {
        var phone = new PhoneRender();
        phone.Shell.Open(RthLandeplatzModule.HelperId);
        PhoneRender.Settle();

        Assert.Equal("TAG", ((TextBlock)Named(phone, "ModeHeading")).Text);
        Assert.Equal(Landeplatzvorgaben.DayDefinition, ((TextBlock)Named(phone, "ModeDefinition")).Text);
        Assert.Contains(Landeplatzvorgaben.PilotDecides, VisibleTexts(phone.Window));
        Assert.DoesNotContain(Landeplatzvorgaben.AbortRule, VisibleTexts(phone.Window));
        Assert.Contains("Fläche: 30 m x 30 m oder größer", VisibleTexts(phone.Window));
        Assert.DoesNotContain(VisibleTexts(phone.Window), t => t.Contains("35 m x 70 m", StringComparison.Ordinal));

        phone.Capture("06-rth-landeplatz-tag.png");

        ((RadioButton)Named(phone, "NachtMode")).IsChecked = true;
        PhoneRender.Settle();

        Assert.Equal("NACHT", ((TextBlock)Named(phone, "ModeHeading")).Text);
        Assert.Equal(Landeplatzvorgaben.NightDefinition, ((TextBlock)Named(phone, "ModeDefinition")).Text);
        Assert.Contains(Landeplatzvorgaben.PilotDecides, VisibleTexts(phone.Window));
        Assert.Contains(Landeplatzvorgaben.AbortRule, VisibleTexts(phone.Window));
        Assert.Contains("Fläche: 35 m x 70 m oder größer", VisibleTexts(phone.Window));
        Assert.DoesNotContain(VisibleTexts(phone.Window), t => t.Contains("30 m x 30 m", StringComparison.Ordinal));

        phone.Capture("07-rth-landeplatz-nacht.png");
    }

    [AvaloniaFact]
    public void The_mode_its_timeframe_the_pilot_line_and_the_abort_rule_stay_outside_the_scrolling_steps()
    {
        var phone = new PhoneRender();
        phone.Shell.Open(RthLandeplatzModule.HelperId);
        PhoneRender.Settle();

        var scroller = Named(phone, "Sections").GetVisualAncestors().OfType<ScrollViewer>().First();

        Assert.False(scroller.IsVisualAncestorOf(Named(phone, "ModeHeading")));
        Assert.False(scroller.IsVisualAncestorOf(Named(phone, "ModeDefinition")));
        Assert.False(scroller.IsVisualAncestorOf(Named(phone, "PilotDecides")));
        Assert.False(scroller.IsVisualAncestorOf(Named(phone, "AbortRule")));
    }

    [AvaloniaFact]
    public void Every_rth_landeplatz_drawing_tells_a_screen_reader_what_it_shows()
    {
        var phone = new PhoneRender();
        phone.Shell.Open(RthLandeplatzModule.HelperId);
        PhoneRender.Settle();

        var tag = DrawingNames(phone.Window);
        ((RadioButton)Named(phone, "NachtMode")).IsChecked = true;
        PhoneRender.Settle();
        var nacht = DrawingNames(phone.Window);

        Assert.Equal(
            Landeplatzvorgaben.Tag.SelectMany(s => s.Drawings).Select(d => d.Description),
            tag);
        Assert.Equal(
            Landeplatzvorgaben.Nacht.SelectMany(s => s.Drawings).Select(d => d.Description),
            nacht);
    }

    [AvaloniaFact]
    public void The_rth_landeplatz_names_its_sources()
    {
        var phone = new PhoneRender();
        phone.Shell.Open(RthLandeplatzModule.HelperId);
        PhoneRender.Settle();

        var texts = Texts(Named(phone, "SourceList"));

        Assert.All(Landeplatzvorgaben.Sources, source => Assert.Contains(source, texts));
        Assert.Contains(Landeplatzvorgaben.SourceNote, texts);
    }

    [AvaloniaFact]
    public void The_dekon_platz_shows_only_the_values_of_its_stufe()
    {
        var phone = new PhoneRender();
        phone.Shell.Open(DekonPlatzModule.HelperId);
        PhoneRender.Settle();

        Assert.Contains("7. Person warm halten", VisibleTexts(phone.Window));
        Assert.DoesNotContain(VisibleTexts(phone.Window), t => t.Contains("10 m x 25 m", StringComparison.Ordinal));
        phone.Capture("08-dekon-stufe-1.png");

        ((RadioButton)Named(phone, "StufeZwei")).IsChecked = true;
        PhoneRender.Settle();

        Assert.Contains("2. Fläche abgrenzen: 9 x 4 m, bei Schwarz-Weiß 8 x 4 m", VisibleTexts(phone.Window));
        Assert.DoesNotContain("7. Person warm halten", VisibleTexts(phone.Window));
        phone.Capture("09-dekon-stufe-2.png");

        ((RadioButton)Named(phone, "StufeDrei")).IsChecked = true;
        PhoneRender.Settle();

        Assert.Contains("Platzbedarf: 10 m x 25 m", VisibleTexts(phone.Window));
        Assert.DoesNotContain(VisibleTexts(phone.Window), t => t.Contains("9 x 4 m", StringComparison.Ordinal));
        phone.Capture("10-dekon-stufe-3.png");
    }

    [AvaloniaFact]
    public void Every_dekon_platz_drawing_tells_a_screen_reader_what_it_shows()
    {
        var phone = new PhoneRender();
        phone.Shell.Open(DekonPlatzModule.HelperId);
        PhoneRender.Settle();

        ((RadioButton)Named(phone, "StufeZwei")).IsChecked = true;
        PhoneRender.Settle();
        var zwei = DrawingNames(phone.Window);
        ((RadioButton)Named(phone, "StufeDrei")).IsChecked = true;
        PhoneRender.Settle();
        var drei = DrawingNames(phone.Window);

        Assert.Equal(Dekonvorgaben.StufeZwei.SelectMany(s => s.Drawings).Select(d => d.Description), zwei);
        Assert.Equal(Dekonvorgaben.StufeDrei.SelectMany(s => s.Drawings).Select(d => d.Description), drei);
    }

    [AvaloniaFact]
    public void Choosing_another_stufe_starts_at_the_top_of_its_page()
    {
        var phone = new PhoneRender();
        phone.Shell.Open(DekonPlatzModule.HelperId);
        PhoneRender.Settle();
        ((RadioButton)Named(phone, "StufeDrei")).IsChecked = true;
        PhoneRender.Settle();
        var scroller = (ScrollViewer)Named(phone, "Scroller");
        scroller.Offset = new Vector(0, 200);
        PhoneRender.Settle();
        Assert.True(scroller.Offset.Y > 100, "The page must have scrolled for this test to mean anything.");

        ((RadioButton)Named(phone, "StufeZwei")).IsChecked = true;
        PhoneRender.Settle();

        Assert.Equal(0, scroller.Offset.Y, precision: 0);
    }

    [AvaloniaFact]
    public void The_dekon_platz_names_its_sources()
    {
        var phone = new PhoneRender();
        phone.Shell.Open(DekonPlatzModule.HelperId);
        PhoneRender.Settle();

        var texts = Texts(Named(phone, "SourceList"));

        Assert.All(Dekonvorgaben.Sources, source => Assert.Contains(source, texts));
        Assert.Contains(Dekonvorgaben.SourceNote, texts);
    }

    private static HydrantenkarteViewModel OpenHydrantenkarte(PhoneRender phone)
    {
        phone.Shell.Open(HydrantenModule.HelperId);
        var vm = Assert.IsType<HydrantenkarteViewModel>(phone.Shell.Current);
        PhoneRender.SettleUntil(() => vm.Hydranten.Count > 0);
        return vm;
    }

    [AvaloniaFact]
    public void The_hydrantenkarte_shows_the_pack_and_asks_for_the_einsatzstelle_first()
    {
        var phone = new PhoneRender();

        var vm = OpenHydrantenkarte(phone);

        Assert.Equal(3, vm.Hydranten.Count);
        Assert.True(Named(phone, "StatusBanner").IsVisible);
        Assert.Equal(HydrantenkarteViewModel.PlacingText, ((TextBlock)Named(phone, "StatusText")).Text);
        Assert.Equal(FictionalRegionPack.Attribution, vm.Map.Attribution);
        Assert.False(Named(phone, "Nearest").IsEffectivelyVisible);
    }

    [AvaloniaFact]
    public void A_tapped_einsatzstelle_lists_the_nearest_hydranten_with_their_b_schlaeuche()
    {
        var phone = new PhoneRender();
        var vm = OpenHydrantenkarte(phone);

        vm.TapCommand.Execute(FictionalRegionPack.Musterstrasse1);
        PhoneRender.Settle();
        Assert.True(Named(phone, "Nearest").IsEffectivelyVisible);
        vm.NaechsteHydrantenCommand.Execute(null);
        PhoneRender.Settle();

        Assert.False(Named(phone, "StatusBanner").IsVisible);
        Assert.Equal(1, Named(phone, "EinsatzstellePin").GetVisualDescendants().OfType<TextBlock>().Count(t => t.Text == "ESt"));
        var rows = VisibleTexts(Named(phone, "NearestRows"));
        Assert.Equal(
            ["Unterflurhydrant", "56 m Luftlinie · 3 B-Schläuche", "Überflurhydrant", "93 m Luftlinie · 5 B-Schläuche", "Unterflurhydrant", "167 m Luftlinie · 9 B-Schläuche"],
            rows);

        phone.Capture("12-hydrantenkarte-naechste.png");

        vm.WaehleCommand.Execute(vm.Nearest[0]);
        PhoneRender.Settle();
        Assert.True(Named(phone, "HydrantCard").IsVisible);
        Assert.Contains("DN 100", VisibleTexts(Named(phone, "HydrantCard")));

        phone.Capture("13-hydrantenkarte-hydrant.png");
    }

    [AvaloniaFact]
    public async Task Einstellungen_list_the_kartenpakete_and_fetch_the_offer_only_on_request()
    {
        var phone = new PhoneRender();
        phone.Shell.Home.OpenSettingsCommand.Execute(null);
        PhoneRender.Settle();

        Assert.Equal("1 installiert", ((TextBlock)Named(phone, "MapPacksSummary")).Text);
        Assert.Equal(["Landkreis Fürstenfeldbruck"], VisibleTexts(Named(phone, "MapPackRows")).Take(1));

        var settings = Assert.IsType<SettingsViewModel>(phone.Shell.Current);
        Assert.NotNull(settings.Maps);
        await settings.Maps.LoadCatalogCommand.ExecuteAsync(null);
        PhoneRender.Settle();

        var rows = VisibleTexts(Named(phone, "MapPackRows"));
        Assert.Contains("Landkreis Musterkreis", rows);
        Assert.Contains("HERUNTERLADEN", rows);
    }

    [AvaloniaFact]
    public async Task The_richtungspfeil_points_from_the_phone_to_the_chosen_hydrant()
    {
        var phone = new PhoneRender();
        var vm = OpenHydrantenkarte(phone);
        vm.TapCommand.Execute(FictionalRegionPack.Musterstrasse1);
        vm.NaechsteHydrantenCommand.Execute(null);
        vm.WaehleCommand.Execute(vm.Nearest[1]);
        PhoneRender.Settle();
        Assert.True(Named(phone, "PfeilStarten").IsEffectivelyVisible);

        await vm.PfeilStartenCommand.ExecuteAsync(null);
        phone.Location.Move(FictionalRegionPack.Musterstrasse1, phone.Time.GetUtcNow(), accuracy: 6);
        phone.Compass.Turn(30);
        PhoneRender.Settle();

        Assert.True(Named(phone, "Arrow").IsEffectivelyVisible);
        Assert.Equal("93 m", ((TextBlock)Named(phone, "Distance")).Text);
        Assert.Equal("Richtung SO (119°)", ((TextBlock)Named(phone, "Direction")).Text);
        Assert.Equal("Richtung SO (119°)", AutomationProperties.GetName(Named(phone, "Arrow")));

        phone.Capture("14-hydrantenkarte-pfeil.png");

        ((Button)Named(phone, "PfeilBeenden")).Command?.Execute(null);
        PhoneRender.Settle();
        Assert.False(phone.Location.IsRunning);
        Assert.False(Named(phone, "Richtungspfeil").IsEffectivelyVisible);
    }

    // At the hydrant: once the signal, AM HYDRANTEN in large letters, and the arrow, smaller, still showing the way (#70).
    [AvaloniaFact]
    public async Task At_the_hydrant_the_phone_signals_once_and_says_so_over_a_smaller_arrow()
    {
        var phone = new PhoneRender();
        var vm = OpenHydrantenkarte(phone);
        vm.TapCommand.Execute(FictionalRegionPack.Musterstrasse1);
        vm.NaechsteHydrantenCommand.Execute(null);
        vm.WaehleCommand.Execute(vm.Nearest[0]);
        await vm.PfeilStartenCommand.ExecuteAsync(null);
        phone.Location.Move(FictionalRegionPack.Musterstrasse1, phone.Time.GetUtcNow());
        phone.Compass.Turn(0);
        PhoneRender.Settle();
        var before = Named(phone, "ArrowBox").Bounds.Height;
        Assert.False(Named(phone, "Angekommen").IsVisible);
        Assert.Equal(0, phone.Signal.Played);

        // About 4 m south of the hydrant.
        Assert.NotNull(vm.Selected);
        var target = vm.Selected.Position;
        phone.Location.Move(new Core.Map.GeoPoint(target.Latitude - 0.00004, target.Longitude), phone.Time.GetUtcNow());
        PhoneRender.Settle();

        Assert.Equal(1, phone.Signal.Played);
        Assert.True(Named(phone, "Angekommen").IsEffectivelyVisible);
        Assert.Equal("AM HYDRANTEN", ((TextBlock)Named(phone, "Angekommen")).Text);
        Assert.True(Named(phone, "Arrow").IsEffectivelyVisible);
        Assert.True(Named(phone, "ArrowBox").Bounds.Height < before, "The arrow shrinks at the hydrant.");
        Assert.True(Named(phone, "Angekommen").Bounds.Bottom <= Named(phone, "ArrowBox").Bounds.Top, "AM HYDRANTEN stands above the arrow.");

        phone.Capture("angekommen.png");
    }

    [AvaloniaFact]
    public async Task Leaving_the_hydrantenkarte_switches_the_gps_off()
    {
        var phone = new PhoneRender();
        var vm = OpenHydrantenkarte(phone);
        vm.TapCommand.Execute(FictionalRegionPack.Musterstrasse1);
        vm.NaechsteHydrantenCommand.Execute(null);
        vm.WaehleCommand.Execute(vm.Nearest[0]);
        await vm.PfeilStartenCommand.ExecuteAsync(null);
        Assert.True(phone.Location.IsRunning);

        phone.Shell.GoHome();

        Assert.False(phone.Location.IsRunning);
        Assert.False(phone.Compass.IsRunning);
    }

    [AvaloniaFact]
    public async Task Mein_standort_sits_above_the_zoom_buttons_and_marks_the_phone_on_the_map()
    {
        var phone = new PhoneRender();
        var vm = OpenHydrantenkarte(phone);
        var standort = Named(phone, "Standort");
        var zoomIn = Named(phone, "ZoomIn");

        Assert.True(standort.IsEffectivelyVisible);
        Assert.Equal("Mein Standort", AutomationProperties.GetName(standort));
        var standortBox = standort.Bounds;
        var zoomBox = zoomIn.Bounds;
        Assert.True(standortBox.Bottom <= zoomBox.Top, "MEIN STANDORT sits above +.");
        Assert.True(Math.Abs(standortBox.Left - zoomBox.Left) <= 0.5, "MEIN STANDORT shares the zoom buttons' column.");

        await vm.StandortZeigenCommand.ExecuteAsync(null);
        phone.Location.Move(FictionalRegionPack.Musterstrasse1, phone.Time.GetUtcNow());
        PhoneRender.Settle();

        Assert.Equal(FictionalRegionPack.Musterstrasse1, vm.Map.Center);
        Assert.Single(Named(phone, "StandortDot").GetVisualDescendants().OfType<Avalonia.Controls.Shapes.Ellipse>());
        Assert.False(phone.Location.IsRunning);

        phone.Capture("standort.png");
    }

    // Tapped where they are drawn, so nothing laid over the map may catch the tap instead (#68).
    [AvaloniaFact]
    public void Tapping_plus_and_minus_zooms_the_map_in_and_out()
    {
        var phone = new PhoneRender();
        var vm = OpenHydrantenkarte(phone);
        var before = vm.Map.Zoom;

        Tap(phone, Named(phone, "ZoomIn"));
        Assert.Equal(before + 1, vm.Map.Zoom);

        Tap(phone, Named(phone, "ZoomOut"));
        Assert.Equal(before, vm.Map.Zoom);
    }

    private static KennzeichnungenViewModel OpenKennzeichnungen(PhoneRender phone)
    {
        phone.Shell.Open(KennzeichnungenModule.HelperId);
        PhoneRender.Settle();
        return phone.Window.GetVisualDescendants().OfType<Control>()
            .Select(c => c.DataContext).OfType<KennzeichnungenViewModel>().First();
    }

    [AvaloniaFact]
    public void The_kennzeichnungen_read_a_gefahrnummer_and_show_every_group()
    {
        var phone = new PhoneRender();
        var vm = OpenKennzeichnungen(phone);

        ((TextBox)Named(phone, "Search")).Text = "X423";
        PhoneRender.Settle();
        Assert.True(Named(phone, "GefahrnummerCard").IsEffectivelyVisible);
        Assert.Contains(Gefahrnummern.WaterNote, VisibleTexts(phone.Window));
        phone.Capture("15-kennzeichnungen-nummer.png");

        vm.ClearCommand.Execute(null);
        ((RadioButton)Named(phone, "GefahrzettelGroup")).IsChecked = true;
        PhoneRender.Settle();
        Assert.False(Named(phone, "GefahrnummerCard").IsEffectivelyVisible);
        Assert.Contains("Entzündbare Gase", VisibleTexts(phone.Window));
        phone.Capture("16-kennzeichnungen-gefahrzettel.png");

        ((RadioButton)Named(phone, "GhsGroup")).IsChecked = true;
        PhoneRender.Settle();
        Assert.Contains("GHS01", VisibleTexts(phone.Window));
        phone.Capture("17-kennzeichnungen-ghs.png");

        ((RadioButton)Named(phone, "RohrleitungGroup")).IsChecked = true;
        PhoneRender.Settle();
        Assert.True(Named(phone, "GroupNote").IsEffectivelyVisible);
        Assert.Contains("Wasserdampf", VisibleTexts(phone.Window));
        phone.Capture("18-kennzeichnungen-rohrleitungen.png");

        ((RadioButton)Named(phone, "GasflascheGroup")).IsChecked = true;
        PhoneRender.Settle();
        Assert.Contains(vm.GroupNote, VisibleTexts(phone.Window));
        phone.Capture("19-kennzeichnungen-gasflaschen.png");
    }

    [AvaloniaFact]
    public void Every_kennzeichnungen_group_button_is_glove_sized()
    {
        var phone = new PhoneRender();
        _ = OpenKennzeichnungen(phone);

        foreach (var group in new[] { "WarntafelGroup", "GefahrzettelGroup", "GhsGroup", "RohrleitungGroup", "GasflascheGroup" })
        {
            var button = Named(phone, group);
            Assert.True(button.Bounds.Height >= 48, $"{group} is {button.Bounds.Height} high");
        }
    }

    [AvaloniaFact]
    public void Every_kennzeichen_shows_its_picture_and_names_it_for_a_screen_reader()
    {
        var phone = new PhoneRender();
        var vm = OpenKennzeichnungen(phone);

        foreach (var group in new[] { "WarntafelGroup", "GefahrzettelGroup", "GhsGroup", "RohrleitungGroup", "GasflascheGroup" })
        {
            ((RadioButton)Named(phone, group)).IsChecked = true;
            PhoneRender.Settle();

            var pictures = Named(phone, "Entries").GetVisualDescendants().OfType<PictureImage>().ToList();
            Assert.Equal(vm.Entries.Select(e => e.Name), pictures.Select(AutomationProperties.GetName));
            Assert.All(pictures, p => Assert.True(p.Source is not null, $"{p.Key} has no picture"));
        }
    }

    [AvaloniaFact]
    public void A_kennzeichnungen_search_without_hits_says_so()
    {
        var phone = new PhoneRender();
        _ = OpenKennzeichnungen(phone);

        ((TextBox)Named(phone, "Search")).Text = "Quarkspeise";
        PhoneRender.Settle();

        Assert.True(Named(phone, "NoHits").IsEffectivelyVisible);
        Assert.Empty(Named(phone, "Entries").GetVisualDescendants().OfType<PictureImage>());
    }

    [AvaloniaFact]
    public void The_kennzeichnungen_name_their_sources()
    {
        var phone = new PhoneRender();
        _ = OpenKennzeichnungen(phone);

        var texts = Texts(Named(phone, "SourceList"));

        Assert.All(Kennzeichenkatalog.Sources, source => Assert.Contains(source, texts));
        Assert.Contains(Kennzeichenkatalog.SourceNote, texts);
    }

    private static void Tap(PhoneRender phone, Control control)
    {
        var middle = control.TranslatePoint(new Point(control.Bounds.Width / 2, control.Bounds.Height / 2), phone.Window)
            ?? throw new InvalidOperationException($"{control.Name} is not in the window.");
        phone.Window.MouseDown(middle, MouseButton.Left);
        phone.Window.MouseUp(middle, MouseButton.Left);
        PhoneRender.Settle();
    }
}
