using Cui.Managed;

namespace Cui.ManagedGallery;

/// <summary>The authored component gallery. Dispose before its UI context.</summary>
public sealed partial class GalleryDemo : IDisposable
{
    public const int PageCount = 27;
    /// <summary>Include added managed examples. Disable only when comparing the
    /// original gallery layout against its upstream reference.</summary>
    public bool IncludeManagedExamples { get; init; } = true;
    private static readonly string versionBadge = "v" + ManagedCuiInfo.UpstreamVersion[..ManagedCuiInfo.UpstreamVersion.LastIndexOf('.')];

    private static readonly string[] pageNames =
    [
        "Overview", "Buttons", "Inputs", "Selection", "Navigation", "Overlays", "Feedback", "Data display", "Charts",
        "Actions", "Smart inputs", "Pickers", "Capture", "Disclosure", "Reader", "Data grid", "Code", "Social and media",
        "Planning", "More charts", "Motion", "AI", "Audio", "Files", "Developer tools", "More developer tools", "Instruments"
    ];

    private static readonly string[] pageIcons =
    [
        Icons.LayoutDashboard, Icons.Zap, Icons.Type, Icons.SlidersHorizontal, Icons.Compass, Icons.Layers,
        Icons.Bell, Icons.Database, Icons.ChartLine, Icons.Command, Icons.SquarePen, Icons.Calendar, Icons.Upload,
        Icons.Layers, Icons.FileText, Icons.Grid3x3, Icons.Code, Icons.MessageSquare, Icons.ListChecks,
        Icons.ChartPie, Icons.Activity, Icons.MessageSquare, Icons.AudioLines, Icons.Folder, Icons.SquareTerminal,
        Icons.Bug, Icons.ChartLine
    ];

    public static IReadOnlyList<string> PageNames { get; } = Array.AsReadOnly(pageNames);
    public static string PageName(int index) => pageNames[CheckedPage(index)];
    public static string PageIcon(int index) => pageIcons[CheckedPage(index)];
    private static int CheckedPage(int value) => (uint)value < PageCount ? value : throw new ArgumentOutOfRangeException(nameof(value));

    private readonly List<IDisposable> owned = new List<IDisposable>();
    private readonly int ownerThread = Environment.CurrentManagedThreadId;
    private bool disposed;
    private int page;

    public int Page
    {
        get => page;
        set
        {
            CheckAlive();
            page = CheckedPage(value);
        }
    }

    public TextBuffer Name { get; }
    public TextBuffer Email { get; }
    public TextBuffer Password { get; }
    public TextBuffer Search { get; }
    public TextBuffer Bio { get; }
    public TextBuffer Tags { get; }
    public TextBuffer Otp { get; }

    private bool loading;
    private int segView, segSize;
    private double quantity = 3;
    private Date date = new Date(2026, 11, 14);
    private Color color = Color.Rgb(0x7747ff);
    private bool terms, newsletter = true, marketing, wifi = true, bluetooth, airplane;
    private Rect bluetoothRect;
    private int plan = 1;
    private float volume = 64, brightness = 40, lo = 20, hi = 75;
    private int rating = 4;
    private readonly bool[] chips = [true, false, true, false, false, false];
    private int framework = -1, planCard = 1, country = 2;
    private int tab, tab2, pageIndex, step = 1;
    private bool dialogOpen, sheetOpen, paletteOpen, confirmOpen, drawer;
    private readonly bool[] acc = [true, false, false];
    private bool treeSrc = true, treeUi = true, treeAssets;
    private int selectedRow = 1, selectedFile = 2;
    private readonly State<TableSort> sort = new State<TableSort>(new TableSort { Column = 0 });
    private readonly int[] taskOrder = [0, 1, 2, 3, 4];
    private bool dark;
    private int accent;

    // Observable state for input and selection qualification, without exposing native state.
    public bool Loading => loading;
    public int SegmentedView => segView;
    public int SegmentedRange => segSize;
    public double Quantity => quantity;
    public Date DueDate => date;
    public Color BrandColor => color;
    public bool Terms => terms;
    public bool Newsletter => newsletter;
    public bool Marketing => marketing;
    public bool Wifi => wifi;
    public bool Bluetooth => bluetooth;
    public bool Airplane => airplane;
    public Rect BluetoothRect => bluetoothRect;
    public int Plan => plan;
    public float Volume => volume;
    public float Brightness => brightness;
    public float RangeLow => lo;
    public float RangeHigh => hi;
    public int Rating => rating;
    public ReadOnlySpan<bool> Chips => chips;
    public int Framework => framework;
    public int PlanCard => planCard;
    public int Country => country;
    public int Tab => tab;
    public int SecondaryTab => tab2;
    public int PaginationIndex => pageIndex;
    public int Step => step;
    public bool DialogOpen => dialogOpen;
    public bool SheetOpen => sheetOpen;
    public bool PaletteOpen => paletteOpen;
    public bool ConfirmOpen => confirmOpen;
    public bool DrawerOpen => drawer;
    public int SelectedRow => selectedRow;
    public TableSort Sort => sort.Value;
    public int SelectedFile => selectedFile;
    public ReadOnlySpan<int> TaskOrder => taskOrder;
    public bool Dark => dark;
    public Accent SelectedAccent => (Accent)accent;

    private static readonly string[] themeModes = ["Light", "Dark"];
    private static readonly string[] themeIcons = [Icons.Sun, Icons.Moon];
    private static readonly uint[] accentHex = [0x555555, 0x7747ff, 0x0562ef, 0x00864b, 0xf3ad20, 0xc25300, 0xd1403a, 0xd23286];
    private readonly Swatch[] swatches = new Swatch[(int)Accent.Count];
    private Ui? swatchUi;

    private static readonly Command[] commands =
    [
        new Command { Title = "Go to Overview", Icon = Icons.LayoutDashboard, Group = "Navigation" },
        new Command { Title = "Go to Buttons", Icon = Icons.Zap, Group = "Navigation" },
        new Command { Title = "Go to Inputs", Icon = Icons.Type, Group = "Navigation" },
        new Command { Title = "Go to Charts", Icon = Icons.ChartLine, Group = "Navigation" },
        new Command { Title = "Toggle dark mode", Icon = Icons.Moon, Group = "Preferences", Shortcut = "Ctrl J" },
        new Command { Title = "Change accent color", Icon = Icons.Palette, Group = "Preferences" },
        new Command { Title = "New project", Icon = Icons.Plus, Group = "Actions", Shortcut = "Ctrl N" },
        new Command { Title = "Invite teammate", Icon = Icons.UserPlus, Group = "Actions" },
        new Command { Title = "Open settings", Icon = Icons.Settings, Group = "Actions", Shortcut = "Ctrl ," }
    ];

    public GalleryDemo(ThemeMode theme = ThemeMode.Light, Accent accent = Accent.Neutral)
    {
        if (!Enum.IsDefined(theme) || (uint)accent >= (uint)Accent.Count) throw new ArgumentOutOfRangeException(nameof(theme));

        dark = theme == ThemeMode.Dark;
        this.accent = (int)accent;
        try
        {
            Name = Own(new TextBuffer(64, "Maya Okafor"));
            Email = Own(new TextBuffer(96, "maya@fieldwork.studio"));
            Password = Own(new TextBuffer(64));
            Search = Own(new TextBuffer(64));
            Bio = Own(new TextBuffer(512, "Mathematician and writer. I like engines that compute and interfaces that stay out of the way."));
            Tags = Own(new TextBuffer(256, "design, c99, native"));
            Otp = Own(new TextBuffer(8, "418"));
            for (var i = 0; i < swatches.Length; i++) swatches[i] = new Swatch(i, Color.Rgb(accentHex[i]));
        }
        catch
        {
            Dispose();
            throw;
        }
    }

    private void CheckAlive()
    {
        if (ownerThread != Environment.CurrentManagedThreadId) throw new InvalidOperationException("Use the gallery on its creating thread.");

        ObjectDisposedException.ThrowIf(disposed, this);
    }

    private T Own<T>(T resource) where T : IDisposable
    {
        CheckAlive();
        try { owned.Add(resource); }
        catch
        {
            resource.Dispose();
            throw;
        }

        return resource;
    }

    private static void Section(Ui ui, string title, string? description)
    {
        using var column = ui.ColumnBegin(new Box { Gap = 4, PadTop = 18 });
        ui.Heading(title, 4);
        if (description is not null) ui.Secondary(description);
    }

    private sealed class Swatch(int index, Color color)
    {
        public readonly string Id = $"accent-{index}";
        public bool Selected;
        public CanvasPainter? Painter;

        public void Paint(PaintContext paint)
        {
            var r = paint.Rect;
            var center = new Vec2(r.X + r.W / 2, r.Y + r.H / 2);
            if (Selected) paint.DrawCircle(center, r.W / 2, paint.ColorOf(Color.Foreground));
            paint.DrawCircle(center, r.W / 2 - (Selected ? 2 : 0), paint.ColorOf(Color.SurfaceMuted));
            paint.DrawCircle(center, r.W / 2 - 4, color);
        }
    }

    private void Sidebar(Ui ui, bool inDrawer, ReadOnlySpan<byte> frameRate)
    {
        if (swatchUi is not null && !ReferenceEquals(swatchUi, ui)) throw new InvalidOperationException("A gallery instance belongs to one UI context.");

        swatchUi = ui;
        using var sidebar = ui.ColumnBegin(new Box
        {
            Width = inDrawer ? Size.Fill : Size.Px(240), Height = Size.Fill,
            Bg = inDrawer ? Color.Transparent : Color.SurfaceMuted, Pad = inDrawer ? 0 : 12, Gap = 2
        });
        if (!inDrawer)
        {
            using var brand = ui.RowBegin(new Box { Gap = 10, PadX = 8, PadTop = 6, PadBottom = 18 });
            using (ui.BoxBegin(new Box { Width = Size.Px(28), Height = Size.Px(28), Radius = 9, Bg = Color.Foreground, Dir = Dir.Stack, Align = Align.Center, Justify = Justify.Center }))
                ui.Text("C", new TextOpts { Font = Font.Display, Size = 16, Color = Color.Background, LineHeight = 1 });
            ui.Text("CUI", new TextOpts { Font = Font.Medium, Size = 15, Nowrap = true });
            ui.Spacer();
            ui.Badge(versionBadge, new BadgeOpts { Size = ControlSize.Sm });
        }

        using (ui.ScrollBegin(new ScrollOpts { Id = "nav", Height = Size.Fill, Gap = 2 }))
        {
            ui.NavSection("Components");
            for (var i = 0; i < 9; i++) NavigationItem(ui, i);
            ui.Gap(10);
            ui.NavSection("Extended");
            for (var i = 9; i < PageCount; i++) NavigationItem(ui, i);
        }

        using (ui.ColumnBegin(new Box { Gap = 12, Pad = 8 }))
        {
            var mode = dark ? 1 : 0;
            if (ui.Segmented("theme", themeModes, 2, ref mode, new SegmentedOpts { FullWidth = true, Icons = themeIcons, Size = ControlSize.Sm }))
            {
                dark = mode == 1;
                ui.SetTheme(dark ? ThemeMode.Dark : ThemeMode.Light);
            }

            using var row = ui.RowBegin(new Box { Justify = Justify.Between });
            for (var a = 0; a < swatches.Length; a++)
            {
                var swatch = swatches[a];
                swatch.Painter ??= Own(ui.CreateCanvasPainter(swatch.Paint));
                swatch.Selected = accent == a;
                var result = ui.Canvas(swatch.Id, Size.Px(22), Size.Px(22), swatch.Painter);
                ui.Tooltip(ui.AccentName((Accent)a)!);
                if (result.Clicked)
                {
                    accent = a;
                    ui.SetAccent((Accent)a);
                }
            }
        }

        if (!frameRate.IsEmpty) DrawFrameRate(ui, frameRate);
    }

    private void NavigationItem(Ui ui, int index)
    {
        if (!ui.NavItem(pageNames[index], new NavOpts { Icon = pageIcons[index], Selected = page == index, Group = "pages" })) return;

        page = index;
        drawer = false;
        ui.ScrollTo("main", 0);
    }

    public void Frame(Ui ui) => Frame(ui, default);

    public void Frame(Ui ui, ReadOnlySpan<byte> frameRate)
    {
        CheckAlive();
        ArgumentNullException.ThrowIfNull(ui);
        if (!ui.WantsKeyboard())
        {
            if (ui.Shortcut(Mod.Primary, Key.K)) paletteOpen = !paletteOpen;
            if (ui.Shortcut(Mod.Primary, Key.J))
            {
                dark = !dark;
                ui.SetTheme(dark ? ThemeMode.Dark : ThemeMode.Light);
            }
        }

        var narrow = ui.ViewportSize().X < 760;
        if (!narrow) drawer = false;
        using (ui.RowBegin(new Box { Width = Size.Fill, Height = Size.Fill, Align = Align.Stretch, Bg = Color.Background }))
        {
            if (!narrow)
            {
                Sidebar(ui, false, frameRate);
                ui.Separator();
            }

            using var scroll = ui.ScrollBegin(new ScrollOpts { Id = "main", Width = Size.Fill, FadeEdges = true });
            using var row = ui.RowBegin(new Box { Justify = Justify.Center, Align = Align.Start });
            using var column = ui.ColumnBegin(new Box { Width = Size.Fill, MaxWidth = 920, PadX = narrow ? 16 : 40, PadTop = narrow ? 12 : 36, PadBottom = 64, Gap = 20 });
            if (narrow)
            {
                using var header = ui.RowBegin(new Box { Gap = 8, Width = Size.Fill });
                if (ui.IconButton(Icons.Menu, new ButtonOpts { Tooltip = "Components" })) drawer = true;
                ui.Text("CUI", new TextOpts { Font = Font.Medium, Size = 15 });
                if (!frameRate.IsEmpty)
                {
                    ui.Spacer();
                    DrawFrameRate(ui, frameRate);
                }
            }

            if (page != 0)
            {
                using var heading = ui.ColumnBegin(new Box { Gap = 6, PadBottom = 8 });
                ui.Heading(pageNames[page], 2);
            }

            if (page >= 9)
            {
                ui.PushId(pageNames[page]);
                try
                {
                    DrawExtendedA(ui, page);
                    DrawExtendedB(ui, page);
                }
                finally { ui.PopId(); }
            }

            switch (page)
            {
                case 0: DrawOverview(ui); break;
                case 1: DrawButtons(ui); break;
                case 2: DrawInputs(ui); break;
                case 3: DrawSelection(ui); break;
                case 4: DrawNavigation(ui); break;
                case 5: DrawOverlays(ui); break;
                case 6: DrawFeedback(ui); break;
                case 7: DrawData(ui); break;
                case 8: DrawCharts(ui); break;
            }
        }

        using (var sheet = ui.SheetBegin("CUI", ref drawer, new SheetOpts { Side = Side.Left, Size = 280 }))
            if (sheet.IsOpen)
                Sidebar(ui, true, frameRate);
        Dialogs(ui);
    }

    private static void DrawFrameRate(Ui ui, ReadOnlySpan<byte> frameRate) =>
        ui.Text(frameRate, new TextOpts { Size = 12, Color = Color.TextSecondary, Nowrap = true });

    private void Dialogs(Ui ui)
    {
        using (var dialog = ui.DialogBegin("Edit profile"u8, ref dialogOpen, new DialogOpts { Description = "Changes are visible to your team." }))
        {
            if (dialog.IsOpen)
            {
                ui.Input("dlg-name", Name, new InputOpts { Label = "Name" });
                ui.Input("dlg-email", Email, new InputOpts { Label = "Email", Icon = Icons.Mail });
                ui.Switch("Public profile", ref wifi);
                using var footer = ui.DialogFooterBegin();
                if (ui.Button("Cancel")) dialogOpen = false;
                if (ui.Button("Save changes", new ButtonOpts { Variant = ButtonVariant.Primary }))
                {
                    dialogOpen = false;
                    ui.Toast("Profile updated", new ToastOpts { Tone = Tone.Success });
                }
            }
        }

        using (var dialog = ui.DialogBegin("Delete this project?", ref confirmOpen, new DialogOpts { Description = "This removes the project and its history for everyone. You cannot undo this.", Width = 400 }))
        {
            if (dialog.IsOpen)
            {
                using var footer = ui.DialogFooterBegin();
                if (ui.Button("Cancel##confirm")) confirmOpen = false;
                if (ui.Button("Delete project", new ButtonOpts { Variant = ButtonVariant.Danger }))
                {
                    confirmOpen = false;
                    ui.Toast("Project deleted", new ToastOpts { Tone = Tone.Danger, Action = "Undo" });
                }
            }
        }

        using (var sheet = ui.SheetBegin("Notifications", ref sheetOpen, new SheetOpts { Description = "Choose what we tell you about." }))
        {
            if (sheet.IsOpen)
            {
                ui.Switch("Comments", ref newsletter, new CheckOpts { Description = "When someone replies to you." });
                ui.Switch("Mentions", ref wifi, new CheckOpts { Description = "When someone mentions you." });
                ui.Switch("Weekly digest", ref bluetooth, new CheckOpts { Description = "A summary every Monday." });
                ui.Separator();
                ui.Label("Recent");
                ui.ListItem("Jonas commented on Release 1.0", new ListItemOpts { Avatar = "Jonas Berg", Subtitle = "2 minutes ago" });
                ui.ListItem("Priya invited you to Engines", new ListItemOpts { Avatar = "Priya Nair", Subtitle = "1 hour ago" });
            }
        }

        var chosen = ui.CommandPalette(ref paletteOpen, commands, commands.Length);
        switch (chosen)
        {
            case 0: page = 0; break;
            case 1: page = 1; break;
            case 2: page = 2; break;
            case 3: page = 8; break;
            case 4:
                dark = !dark;
                ui.SetTheme(dark ? ThemeMode.Dark : ThemeMode.Light);
                break;
            case 5:
                accent = (accent + 1) % (int)Accent.Count;
                ui.SetAccent((Accent)accent);
                break;
            case 6: page = 6; break;
            case 7: dialogOpen = true; break;
            case 8: sheetOpen = true; break;
        }
    }

    partial void DrawExtendedA(Ui ui, int page);
    partial void DrawExtendedB(Ui ui, int page);
    partial void DisposeExtendedA();
    partial void DisposeExtendedB();

    public void Dispose()
    {
        if (ownerThread != Environment.CurrentManagedThreadId) throw new InvalidOperationException("Dispose the gallery on its creating thread.");

        if (disposed) return;

        disposed = true;
        List<Exception>? failures = null;
        try { DisposeExtendedA(); }
        catch (Exception error) { (failures ??= new List<Exception>()).Add(error); }

        try { DisposeExtendedB(); }
        catch (Exception error) { (failures ??= new List<Exception>()).Add(error); }

        for (var i = owned.Count - 1; i >= 0; i--)
            try { owned[i].Dispose(); }
            catch (Exception error) { (failures ??= new List<Exception>()).Add(error); }

        owned.Clear();
        if (failures is not null) throw new AggregateException("Gallery resource cleanup failed.", failures);
    }
}