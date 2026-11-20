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
using FwAssist.Helpers.RthLandeplatz;
using FwAssist.Helpers.Schaummittel;

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
}
