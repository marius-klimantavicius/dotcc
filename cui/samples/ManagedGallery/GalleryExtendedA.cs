using System.Globalization;
using System.Text;
using Cui.Managed;

namespace Cui.ManagedGallery;

public sealed partial class GalleryDemo
{
    partial void DrawExtendedA(Ui ui, int page)
    {
        switch (page)
        {
            case 9: AActions(ui); break;
            case 10: ASmartInputs(ui); break;
            case 11: APickers(ui); break;
            case 12: ACapture(ui); break;
            case 13: ADisclosure(ui); break;
            case 14: AReader(ui); break;
            case 15: ADataGrid(ui); break;
            case 16: ACode(ui); break;
            case 17: ASocial(ui); break;
            case 18: APlanning(ui); break;
        }
    }

    private readonly bool[] aFormat = [true, false, false, false];
    private readonly bool[] aAlignment = [true, false, false];
    private Status aSaveStatus;
    private double aStatusAt;

    private readonly Reaction[] aReactions =
    [
        new Reaction { Icon = Icons.ThumbsUp, Count = 12, Active = true },
        new Reaction { Icon = Icons.Heart, Count = 4 },
        new Reaction { Icon = Icons.Rocket, Count = 7 },
        new Reaction { Icon = Icons.Smile, Count = 2 }
    ];

    private int aDialChoice = -1, aThemeMode = -1, aDeletedProjects, aSplitChoice = -1;
    private static readonly string[] aSaveItems = ["Save as draft", "Save and publish", "Schedule…"];
    private static readonly string[] aSaveIcons = [Icons.FileText, Icons.Send, Icons.Clock];
    private static readonly string[] aFormatLabels = ["Bold", "Italic", "Underline", "Code"];
    private static readonly string[] aFormatIcons = [Icons.Bold, Icons.Italic, Icons.Underline, Icons.Code];
    private static readonly string[] aAlignmentLabels = ["Left", "Center", "Right"];
    private static readonly string[] aAlignmentIcons = [Icons.AlignLeft, Icons.List, Icons.Menu];
    private static readonly SwipeAction[] aSwipeLeft = [new SwipeAction { Icon = Icons.Check, Label = "Done", Tone = Tone.Success }];

    private static readonly SwipeAction[] aSwipeRight =
    [
        new SwipeAction { Icon = Icons.Archive, Label = "Archive", Tone = Tone.Info },
        new SwipeAction { Icon = Icons.Trash2, Label = "Delete", Tone = Tone.Danger }
    ];

    private static readonly string[] aSwipeTitles = ["Review the design tokens", "Reply to Jonas about the release", "Book the offsite venue"];

    private static readonly SpeedAction[] aSpeedActions =
    [
        new SpeedAction { Icon = Icons.FileText, Label = "New note" },
        new SpeedAction { Icon = Icons.Image, Label = "Upload image" },
        new SpeedAction { Icon = Icons.Mic, Label = "Record memo" }
    ];

    private void AActions(Ui ui)
    {
        Section(ui, "Split button", "A main action with related actions one click away.");
        using (ui.RowBegin(new Box { Gap = 12, Wrap = true }))
        {
            var result = ui.SplitButton("Save", aSaveItems, 3, new SplitButtonOpts { ItemIcons = aSaveIcons, Icon = Icons.Save });
            if (result >= 0) aSplitChoice = result;
            ui.SplitButton("Export", aSaveItems, 3, new SplitButtonOpts { Variant = ButtonVariant.Outline, Icon = Icons.Download });
            if (aSplitChoice >= 0) ui.Muted(aSplitChoice == 0 ? "Saved" : aSaveItems[aSplitChoice - 1]);
        }

        Section(ui, "Toggle group", "Independent toggles for toolbars, or exclusive like radio buttons.");
        using (ui.RowBegin(new Box { Gap = 12, Wrap = true }))
        {
            ui.ToggleGroup("format", aFormatLabels, 4, aFormat, new ToggleGroupOpts { Icons = aFormatIcons, IconOnly = true });
            ui.ToggleGroup("align", aAlignmentLabels, 3, aAlignment, new ToggleGroupOpts { Icons = aAlignmentIcons, Exclusive = true });
        }

        Section(ui, "Morphing buttons", "Confirm before destroying, and show the result of work in place.");
        using (ui.RowBegin(new Box { Gap = 12, Wrap = true }))
        {
            if (ui.ConfirmButton("Delete project", new ConfirmOpts { Icon = Icons.Trash2 })) aDeletedProjects++;
            if (ui.StatusButton("Save changes", aSaveStatus, new StatusButtonOpts { Icon = Icons.Save, SuccessLabel = "Saved" }))
            {
                aSaveStatus = Status.Busy;
                aStatusAt = ui.Time();
            }

            var age = ui.Time() - aStatusAt;
            if (aSaveStatus == Status.Busy && age > 1.4)
            {
                aSaveStatus = Status.Success;
                aStatusAt = ui.Time();
            }
            else if (aSaveStatus == Status.Success && age > 1.6) aSaveStatus = Status.Idle;

            if (aSaveStatus != Status.Idle) ui.RequestRedraw();
            if (aDeletedProjects != 0) ui.Muted("Project deleted");
        }

        Section(ui, "Reactions", "Counts roll when they change.");
        ui.Reactions("reactions", aReactions, 4);
        Section(ui, "Theme", "A compact switcher and a single toggle that morphs between sun and moon.");
        using (ui.RowBegin(new Box { Gap = 16, Wrap = true }))
        {
            ui.ThemeSwitcher(ref aThemeMode, true);
            ui.ThemeToggle();
        }

        Section(ui, "User menu", "Account trigger for sidebars and headers.");
        using (ui.BoxBegin(new Box { Width = Size.Px(260) }))
        using (var menu = ui.UserMenuBegin("Maya Okafor", new UserMenuOpts { Detail = "maya@fieldwork.studio", Status = "online", Placement = Placement.BottomStart }))
        {
            if (menu.IsOpen)
            {
                ui.MenuItem("Profile", new MenuItemOpts { Icon = Icons.User, Shortcut = "Ctrl P" });
                ui.MenuItem("Billing", new MenuItemOpts { Icon = Icons.CreditCard });
                ui.MenuItem("Settings", new MenuItemOpts { Icon = Icons.Settings, Shortcut = "Ctrl ," });
                ui.MenuSeparator();
                ui.MenuItem("Log out", new MenuItemOpts { Icon = Icons.LogOut });
            }
        }

        Section(ui, "Swipe actions", "Drag a row sideways or swipe with two fingers.");
        using (ui.CardBegin(new CardOpts { Pad = 6, Gap = 6 }))
            for (var i = 0; i < 3; i++)
            {
                using var id = ui.PushIdInt(i);
                using var swipe = ui.SwipeItemBegin("row", aSwipeLeft, 1, aSwipeRight, 2);
                ui.Icon(Icons.Circle, 16, Color.TextMuted);
                ui.Text(aSwipeTitles[i], new TextOpts { Width = Size.Fill, Nowrap = true });
                ui.Text("Today", new TextOpts { Size = 12, Color = Color.TextMuted });
                var action = ui.SwipeItemEnd();
                if (action >= 0) ui.Toast(action == 0 ? "Marked as done" : action == 100 ? "Archived" : "Deleted", new ToastOpts { Tone = action == 101 ? Tone.Danger : Tone.Success });
            }

        Section(ui, "Speed dial", "A floating action button that fans out related actions.");
        using (ui.RowBegin(new Box { Gap = 16, Align = Align.Center, Height = Size.Px(80) }))
        {
            var choice = ui.SpeedDial("dial", aSpeedActions, 3, new SpeedDialOpts { Tooltip = "Create" });
            if (choice >= 0) aDialChoice = choice;
            if (aDialChoice >= 0) ui.Muted(aSpeedActions[aDialChoice].Label!);
        }
    }

    private readonly TextBuffer aTitle = new TextBuffer(128, "Q3 roadmap");
    private readonly TextBuffer aOwner = new TextBuffer(64, "Jonas Berg");
    private readonly TextBuffer aUrl = new TextBuffer(96, "cui.dev");
    private readonly TextBuffer aWeight = new TextBuffer(16, "72");
    private readonly TextBuffer aMessage = new TextBuffer(160, "Release notes are up ");
    private readonly TextBuffer aRecent = new TextBuffer(256);
    private readonly State<int> aEmojiTone = new State<int>();
    private float aOpacity = 80, aRadius = 12, aBlur = 24;
    private double aAmount = 1249.5, aSalary = 98000;
    private KeyCombo aPaletteKey = new KeyCombo(Mod.Primary | Mod.Shift, Key.P), aSearchKey;

    private void ASmartInputs(Ui ui)
    {
        Section(ui, "Inline edit", "Click the text to edit it in place. Enter saves, Esc cancels.");
        using (ui.ColumnBegin(new Box { Gap = 6 }))
        {
            ui.InlineEdit("title", aTitle, new InlineEditOpts { Font = Font.Display, Size = 26 });
            ui.InlineEdit("owner", aOwner, new InlineEditOpts { Placeholder = "Add an owner" });
        }

        Section(ui, "Scrub fields", "Drag sideways to change the value, Shift for fine steps, click to type.");
        using (ui.RowBegin(new Box { Gap = 12, Wrap = true }))
        {
            ui.Scrub("Opacity", ref aOpacity, 0, 100, new ScrubOpts { Suffix = "%" });
            ui.Scrub("Radius", ref aRadius, 0, 48, new ScrubOpts { Step = .5f, Format = "%.1f", Suffix = "px" });
            ui.Scrub("Blur", ref aBlur, 0, 80, new ScrubOpts { Suffix = "px", Width = Size.Px(140) });
        }

        Section(ui, "Money", "Grouped digits at rest, plain digits while typing.");
        using (ui.RowBegin(new Box { Gap = 12, Align = Align.Start }))
        {
            using (ui.BoxBegin(new Box { Width = Size.Px(240) })) ui.MoneyInput("amount", ref aAmount, new MoneyOpts { Label = "Amount", Code = "USD" });
            using (ui.BoxBegin(new Box { Width = Size.Px(240) })) ui.MoneyInput("salary", ref aSalary, new MoneyOpts { Label = "Salary", Symbol = "€", Code = "EUR", Decimals = -1, Hint = "Gross per year" });
        }

        Section(ui, "Prefix and suffix", "Any text field can carry muted text before and after the value.");
        using (ui.RowBegin(new Box { Gap = 12, Wrap = true }))
        {
            ui.Input("url", aUrl, new InputOpts { Prefix = "https://", Width = Size.Px(260) });
            ui.Input("weight", aWeight, new InputOpts { Suffix = "kg", Width = Size.Px(120) });
        }

        Section(ui, "Shortcut recorder", "Click, then press a combination. Backspace clears.");
        using (ui.RowBegin(new Box { Gap = 12, Wrap = true }))
        {
            ui.ShortcutRecorder("palette-key", ref aPaletteKey);
            ui.ShortcutRecorder("search-key", ref aSearchKey, new ShortcutRecorderOpts { Placeholder = "Record shortcut" });
        }

        Section(ui, "Emoji picker", "Search by name, jump by group, pick a skin tone. Recent emoji come first.");
        using (ui.RowBegin(new Box { Gap = 8, Align = Align.Center, Width = Size.Fill, MaxWidth = 480 }))
        {
            ui.Input("message", aMessage, new InputOpts { Width = Size.Fill });
            if (ui.IconButton(Icons.Smile, new ButtonOpts { Tooltip = "Emoji" })) ui.PopoverToggle("emoji-pop");
            string? emoji = null;
            using (var popover = ui.PopoverBegin("emoji-pop", new PopoverOpts { Placement = Placement.BottomEnd, Width = 340, Pad = 0 }))
                if (popover.IsOpen)
                    emoji = ui.EmojiPicker("emoji", new EmojiPickerOpts { Recent = aRecent, Tone = aEmojiTone, Height = 260 });
            // Close in the trigger's ID scope, matching the existing gallery's emoji repair.
            if (emoji is not null && aMessage.Length + Encoding.UTF8.GetByteCount(emoji) < aMessage.Capacity)
            {
                aMessage.Set(aMessage.Text + emoji);
                ui.PopoverClose("emoji-pop");
            }
        }
    }

    private readonly bool[] aLanguages = [true, false, true, false, false, true, false, false];
    private DateRange aRange = new DateRange(new Date(2026, 11, 3), new Date(2026, 11, 14));
    private DateRange aStay = new DateRange(new Date(2026, 12, 18), new Date(2026, 12, 27));
    private int aHour = 9, aMinute = 30, aEndHour = 18, aEndMinute = 45;
    private int aDay = 13, aMonth = 10, aYear = 6, aDialHour = 7, aDialMinute = 25;
    private float aHeight = 172, aTemperature = 21.5f, aPriceLow = 40, aPriceHigh = 160;
    private static readonly float[] aPriceBins = [2, 4, 7, 12, 18, 26, 31, 29, 34, 40, 37, 30, 24, 22, 18, 15, 11, 9, 7, 5, 4, 3, 2, 1];
    private static readonly string[] aLanguageNames = ["C", "C++", "Rust", "Zig", "Go", "Swift", "Kotlin", "Odin"];
    private static readonly string[] aDays = ALabels(1, 31);
    private static readonly string[] aYears = ALabels(2020, 12);
    private static readonly string[] aMonths = ["January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December"];

    private static string[] ALabels(int first, int count)
    {
        var labels = new string[count];
        for (var k = 0; k < count; k++) labels[k] = (first + k).ToString(CultureInfo.InvariantCulture);
        return labels;
    }

    private void APickers(Ui ui)
    {
        Section(ui, "Multi-select", "Chips for the chosen items, checkboxes in the list.");
        using (ui.RowBegin(new Box { Gap = 12, Align = Align.Start }))
        using (ui.BoxBegin(new Box { Width = Size.Px(320) }))
            ui.MultiSelect("langs", aLanguageNames, 8, aLanguages, new MultiSelectOpts { Label = "Languages", Searchable = true });
        Section(ui, "Date range", "Presets on the left, two months to pick a start and an end.");
        using (ui.RowBegin(new Box { Gap = 12, Wrap = true }))
        {
            ui.DateRangePicker("range", ref aRange, new DateRangeOpts { Width = Size.Px(300) });
            ui.DateRangePicker("stay", ref aStay, new DateRangeOpts { Width = Size.Px(300), NoPresets = true });
        }

        using (ui.RowBegin())
        using (ui.CardBegin(new CardOpts { Pad = 16 }))
            ui.RangeCalendar("inline-range", ref aStay, 2);
        Section(ui, "Time", "A field with hour and minute columns, 24 hour or 12 hour.");
        using (ui.RowBegin(new Box { Gap = 12, Wrap = true }))
        {
            ui.TimePicker("start", ref aHour, ref aMinute);
            ui.TimePicker("end", ref aEndHour, ref aEndMinute, new TimeOpts { TwelveHour = true, MinuteStep = 15 });
        }

        Section(ui, "Wheels and dial", "Drag, scroll or use the arrow keys. Several wheels make a date reel.");
        using (ui.RowBegin(new Box { Gap = 32, Align = Align.Center }))
        {
            using (ui.CardBegin(new CardOpts { Dir = Dir.Row, Gap = 0, Pad = 8 }))
            {
                ui.WheelPicker("day", aDays, 31, ref aDay, new WheelOpts { Width = Size.Px(64), Loop = true, JoinRight = true });
                ui.WheelPicker("month", aMonths, 12, ref aMonth, new WheelOpts { Width = Size.Px(128), Loop = true, JoinLeft = true, JoinRight = true });
                ui.WheelPicker("year", aYears, 12, ref aYear, new WheelOpts { Width = Size.Px(80), JoinLeft = true });
            }

            ui.TimeDial("dial", ref aDialHour, ref aDialMinute, 232);
        }

        Section(ui, "Ruler and histogram", "A tape that slides under a fixed marker, and a range over its data.");
        using (ui.RowBegin(new Box { Gap = 24, Align = Align.Start }))
        {
            using (ui.ColumnBegin(new Box { Width = Size.Fill, Gap = 20 }))
            {
                ui.RulerSlider("Height", ref aHeight, 120, 220, new RulerOpts { Unit = "cm" });
                ui.RulerSlider("Temperature", ref aTemperature, 10, 30, new RulerOpts { Step = .5f, MajorEvery = 4, Spacing = 14, Format = "%.1f", Unit = "°C" });
            }

            using (ui.CardBegin(new CardOpts { Width = Size.Fill, Gap = 6 }))
            {
                ui.HistogramSlider("Price", aPriceBins, 24, ref aPriceLow, ref aPriceHigh, 0, 240, new HistogramOpts { Format = "$%.0f" });
                ui.Muted("1,204 listings in range");
            }
        }
    }

    private Texture? aAvatar;
    private string aLastDrop = "";

    private void ACapture(Ui ui)
    {
        Section(ui, "Dropzone", "Drop files from your desktop, or click to browse.");
        var count = ui.Dropzone("files", new DropzoneOpts { Description = "PNG, JPG or PDF, up to 25 MB", Accept = "png;jpg;jpeg;pdf" });
        if (count > 0) aLastDrop = AUtf8Prefix(ui.DroppedFile(0) ?? "", 255);
        using (ui.RowBegin(new Box { Gap = 12, Wrap = true }))
        {
            using (ui.BoxBegin(new Box { Width = Size.Fill })) ui.Dropzone("compact", new DropzoneOpts { Compact = true, Title = "Attach a file", Description = "Any type" });
            using (ui.BoxBegin(new Box { Width = Size.Fill })) ui.Dropzone("images", new DropzoneOpts { Compact = true, Title = "Add images", Description = "PNG or JPG", Icon = Icons.Image, Accept = "png;jpg;jpeg" });
        }

        if (aLastDrop.Length != 0) ui.Muted(aLastDrop);
        Section(ui, "Avatar upload", "Drop an image on the avatar.");
        using (ui.RowBegin(new Box { Gap = 20, Align = Align.Center }))
        {
            var previous = aAvatar;
            ui.AvatarUpload("avatar", "Maya Okafor", ref aAvatar, 88);
            if (aAvatar is not null && !ReferenceEquals(previous, aAvatar)) Own(aAvatar);
            using (ui.ColumnBegin(new Box { Gap = 4 }))
            {
                ui.Label("Profile photo");
                ui.Secondary("At least 256 by 256 pixels.");
            }
        }

        Section(ui, "Signature", "Draw with the mouse or a pen. Faster strokes are thinner, like ink.");
        using (ui.BoxBegin(new Box { Width = Size.Px(520) })) ui.SignaturePad("signature", new SignatureOpts { Height = 180 });
    }

    private static string AUtf8Prefix(string value, int byteCapacity)
    {
        if (Encoding.UTF8.GetByteCount(value) <= byteCapacity) return value;

        int bytes = 0, chars = 0;
        foreach (var rune in value.EnumerateRunes())
        {
            if (bytes + rune.Utf8SequenceLength > byteCapacity) break;

            bytes += rune.Utf8SequenceLength;
            chars += rune.Utf16SequenceLength;
        }

        return value[..chars];
    }

    private bool aBanner = true, aBilling = true, aSecurity;
    private int aDeletedBranches;
    private double aCountdownTarget;

    private readonly CheckStep[] aOnboarding =
    [
        new CheckStep { Title = "Create your workspace", Description = "Pick a name and invite your team.", Action = "Create", Done = true },
        new CheckStep { Title = "Connect a repository", Description = "Link GitHub or GitLab to sync issues and pull requests.", Action = "Connect" },
        new CheckStep { Title = "Import your data", Description = "Bring tasks over from a CSV or another tool.", Action = "Import" },
        new CheckStep { Title = "Turn on notifications", Description = "Get pinged when something needs you.", Action = "Enable" }
    ];

    private void ADisclosure(Ui ui)
    {
        if (ui.Banner("CUI 1.1 is out: data grid, kanban and forty new components.", ref aBanner, new BannerOpts { Action = "Read the notes", Dismissible = true }))
            ui.Toast("Opening release notes");
        if (!aBanner && ui.Button("Show the banner again", new ButtonOpts { Size = ControlSize.Sm })) aBanner = true;
        Section(ui, "Hover card", "Rest the pointer on a name.");
        using (ui.RowBegin(new Box { Gap = 6, Wrap = true }))
        {
            ui.Text("Assigned to");
            ui.Link("@jonas");
            using (var hover = ui.HoverCardBegin("grace-card", new HoverCardOpts { Width = 300 }))
            {
                if (hover.IsOpen)
                {
                    using var row = ui.RowBegin(new Box { Gap = 12, Align = Align.Start });
                    ui.Avatar("Jonas Berg", new AvatarOpts { Size = 44, Status = "online" });
                    using var column = ui.ColumnBegin(new Box { Gap = 4, Width = Size.Fill });
                    ui.Text("Jonas Berg", new TextOpts { Font = Font.Medium });
                    ui.Text("Engineer on the rendering team. Joined March 2024.", new TextOpts { Size = 13, Color = Color.TextSecondary });
                    using var detail = ui.RowBegin(new Box { Gap = 6, PadTop = 4, Wrap = true });
                    ui.Icon(Icons.Calendar, 12, Color.TextMuted);
                    ui.Text("Usually replies within an hour", new TextOpts { Size = 12, Color = Color.TextMuted });
                }
            }

            ui.Text("and due Friday.");
        }

        Section(ui, "Confirm popover", "Ask right where the action is.");
        using (ui.RowBegin(new Box { Gap = 12, Wrap = true }))
        {
            if (ui.Button("Delete branch", new ButtonOpts { Icon = Icons.Trash2 })) ui.PopoverOpen("confirm-delete");
            if (ui.ConfirmPopover("confirm-delete", new ConfirmPopoverOpts { Title = "Delete feature/grid?", Description = "The branch and its 12 commits are removed from the remote.", ConfirmLabel = "Delete branch", Danger = true })) aDeletedBranches++;
            if (aDeletedBranches != 0) ui.Muted("Branch deleted");
        }

        Section(ui, "Expandable text", "Long text clamps to a few lines with a fade.");
        ui.ExpandableText("about",
            "CUI is a component library for C. It draws everything itself, so an app looks the same on Windows, macOS and Linux, and it " +
            "ships the parts most apps need: inputs, pickers, menus, dialogs, tables, charts and a command palette. Every component " +
            "supports the keyboard, light and dark themes and reduced motion. The core is plain C99 with no dependencies, and it " +
            "renders through OpenGL or a built-in software rasterizer, so it also runs in virtual machines and over remote desktop.", new ExpandableTextOpts { Lines = 2 });
        Section(ui, "Expandable cards", "A header that opens a body with a spring.");
        using (ui.ColumnBegin(new Box { Gap = 10 }))
        {
            using (var billing = ui.ExpandableCardBegin("Billing", ref aBilling, new ExpandableCardOpts { Description = "Pro plan, renews on December 1", Icon = Icons.CreditCard, Badge = "Active", BadgeTone = Tone.Success }))
            {
                if (billing.IsOpen)
                {
                    using (ui.RowBegin(new Box { Gap = 40, Wrap = true }))
                    {
                        using (ui.ColumnBegin(new Box { Gap = 2 }))
                        {
                            ui.Muted("This month");
                            ui.Text("$84.00", new TextOpts { Font = Font.Display, Size = 22, Tabular = true });
                        }

                        using (ui.ColumnBegin(new Box { Gap = 2 }))
                        {
                            ui.Muted("Seats");
                            ui.Text("7 of 10", new TextOpts { Font = Font.Display, Size = 22, Tabular = true });
                        }
                    }

                    using (ui.RowBegin(new Box { Gap = 8, Wrap = true }))
                    {
                        ui.Button("Change plan", new ButtonOpts { Size = ControlSize.Sm });
                        ui.Button("Invoices", new ButtonOpts { Size = ControlSize.Sm, Variant = ButtonVariant.Ghost });
                    }
                }
            }

            using (var security = ui.ExpandableCardBegin("Security", ref aSecurity, new ExpandableCardOpts { Description = "Two-factor authentication is on", Icon = Icons.Shield }))
                if (security.IsOpen)
                    ui.Text("Recovery codes were generated 3 months ago.", new TextOpts { Color = Color.TextSecondary });
        }

        Section(ui, "Countdown", "Digits roll as the time passes.");
        if (aCountdownTarget == 0) aCountdownTarget = ui.Time() + 2 * 86400 + 5 * 3600 + 42 * 60 + 18;
        using (ui.RowBegin(new Box { Gap = 24, Align = Align.Center }))
        {
            ui.Countdown("launch", aCountdownTarget - ui.Time());
            ui.Countdown("sale", aCountdownTarget - ui.Time() - 86400 * 2, new CountdownOpts { Compact = true, HideDays = true });
        }

        Section(ui, "Onboarding checklist", "Progress, with the next step open.");
        using (ui.BoxBegin(new Box { Width = Size.Px(460) }))
        {
            var action = ui.Checklist("onboarding", aOnboarding, 4, new ChecklistOpts { Title = "Set up your workspace" });
            if (action >= 0)
            {
                aOnboarding[action].Done = true;
                ui.Toast(aOnboarding[action].Title!, new ToastOpts { Tone = Tone.Success });
            }
        }

        Section(ui, "Undo toast", "A draining line shows how long Undo stays.");
        if (ui.Button("Archive 3 conversations", new ButtonOpts { Icon = Icons.Archive })) ui.Toast("3 conversations archived", new ToastOpts { Action = "Undo", ShowTimer = true });
    }

    private static readonly TocItem[] aToc =
    [
        new TocItem { Title = "Introduction", Anchor = "intro" }, new TocItem { Title = "Install", Anchor = "install" },
        new TocItem { Title = "With CMake", Anchor = "cmake", Level = 1 }, new TocItem { Title = "Without CMake", Anchor = "nocmake", Level = 1 },
        new TocItem { Title = "First window", Anchor = "window" }, new TocItem { Title = "Immediate mode", Anchor = "imm" },
        new TocItem { Title = "Layout", Anchor = "layout" }, new TocItem { Title = "Next steps", Anchor = "next" }
    ];

    private void AReader(Ui ui)
    {
        const string paragraph = "Components are functions. They draw themselves, handle input and return what happened. Your variables are the " +
            "source of truth, so there is nothing to keep in sync, and conditional interface is just an if statement. ";
        using var row = ui.RowBegin(new Box { Gap = 28, Align = Align.Start, Width = Size.Fill });
        using (ui.ScrollBegin(new ScrollOpts { Id = "reader", Height = Size.Px(560), Width = Size.Fill, Gap = 14, FadeEdges = true }))
        {
            for (var k = 0; k < 8; k++)
            {
                ui.TocAnchor(aToc[k].Anchor!);
                ui.Heading(aToc[k].Title!, aToc[k].Level != 0 ? 4 : 3);
                for (var p = 0; p < 3; p++)
                {
                    using var id = ui.PushIdInt(k * 10 + p);
                    ui.Text(paragraph, new TextOpts { Color = Color.TextSecondary });
                }
            }

            ui.Gap(200);
        }

        using (ui.ColumnBegin(new Box { Width = Size.Px(200), Gap = 10 }))
        {
            ui.Text("On this page", new TextOpts { Size = 12, Font = Font.Medium, Color = Color.TextMuted });
            ui.Toc("reader", aToc, 8);
        }
    }

    private sealed class APerson(string name, string email, string team, int commits, float score)
    {
        public string Name = name;
        public readonly string Email = email;
        public readonly string Team = team;
        public readonly int Commits = commits;
        public readonly float Score = score;
        public readonly string CommitText = commits.ToString(CultureInfo.InvariantCulture);
        public readonly string ScoreText = score.ToString("F1", CultureInfo.InvariantCulture);
        public TextBuffer? NameBuffer;
    }

    private const int aPeopleCount = 2000;
    private APerson[]? aPeople;

    private readonly int[] aPeopleOrder = new int[aPeopleCount];

    // Upstream attaches selection to displayed row positions, including across sorts.
    private readonly bool[] aPeopleSelected = new bool[aPeopleCount];
    private readonly State<TableSort> aPeopleSort = new State<TableSort>();
    private TableSort aLastPeopleSort = new TableSort { Column = -1 };
    private readonly TextBuffer aPeopleSearch = new TextBuffer(64);
    private readonly uint[] aFilters = [1u << 0 | 1u << 2, 0, 0];
    private bool aTreeSource = true, aTreeUi;
    private static readonly string[] aFirstNames = ["Maya", "Priya", "Jonas", "Lena", "Sam", "Tomas", "Aiko", "Daniel", "Rosa", "Omar", "Elif", "Ben", "Nora", "Kwame", "Hana", "Felix"];
    private static readonly string[] aLastNames = ["Okafor", "Nair", "Berg", "Fischer", "Whitfield", "Rivera", "Tanaka", "Mensah", "Delgado", "Haddad", "Kaya", "Carter", "Lindqvist", "Asante", "Novak", "Moreau"];
    private static readonly string[] aTeams = ["Engine", "Design", "Platform", "Docs", "Growth"];
    private static readonly string[] aStatusOptions = ["Active", "Away", "Invited"];
    private static readonly string[] aRoleOptions = ["Owner", "Admin", "Member"];

    private static readonly FilterField[] aFilterFields =
    [
        new FilterField { Name = "Team", Icon = Icons.Users, Options = aTeams },
        new FilterField { Name = "Status", Icon = Icons.CircleDot, Options = aStatusOptions },
        new FilterField { Name = "Role", Icon = Icons.KeyRound, Options = aRoleOptions }
    ];

    private static readonly GridColumn[] aGridColumns =
    [
        new GridColumn { Title = "Name", Width = 200, Align = TextAlign.Left, Sortable = true },
        new GridColumn { Title = "Email", Width = 196, Align = TextAlign.Left },
        new GridColumn { Title = "Team", Width = 110, Align = TextAlign.Left, Sortable = true },
        new GridColumn { Title = "Commits", Width = 96, Align = TextAlign.Right, Sortable = true },
        new GridColumn { Title = "Score", Width = 130, Align = TextAlign.Right, Sortable = true }
    ];

    private static readonly TableColumn[] aTreeColumns =
    [
        new TableColumn { Title = "Name", Width = Size.Fill, Align = TextAlign.Left },
        new TableColumn { Title = "Files", Width = Size.Px(80), Align = TextAlign.Right },
        new TableColumn { Title = "Size", Width = Size.Px(100), Align = TextAlign.Right }
    ];

    private void AInitializePeople()
    {
        if (aPeople is not null) return;

        var people = new APerson[aPeopleCount];
        for (var k = 0; k < aPeopleCount; k++)
        {
            var hash = unchecked((uint)k * 2654435761u);
            hash ^= hash >> 15;
            hash = unchecked(hash * 2246822519u);
            hash ^= hash >> 13;
            string first = aFirstNames[hash % 16], last = aLastNames[(hash >> 4) % 16];
            var email = string.Create(CultureInfo.InvariantCulture, $"{first}.{last[0]}{(hash >> 8) % 90 + 10}@cui.dev").ToLowerInvariant();
            people[k] = new APerson(first + " " + last, email, aTeams[(hash >> 12) % 5], (int)((hash >> 16) % 1400) + 12, (hash >> 3) % 1000 / 10.0f);
            aPeopleOrder[k] = k;
        }

        aPeople = people;
        aPeopleSort.Value = new TableSort { Column = 3, Descending = true };
    }

    private int AComparePeople(int left, int right, TableSort sort)
    {
        var people = aPeople!;
        APerson x = people[left], y = people[right];
        int result;
        switch (sort.Column)
        {
            case 0:
                // UTF-8 byte order matches strcmp even for edited non-ASCII names.
                Span<byte> xBytes = stackalloc byte[48];
                Span<byte> yBytes = stackalloc byte[48];
                int xCount = Encoding.UTF8.GetBytes(x.Name, xBytes), yCount = Encoding.UTF8.GetBytes(y.Name, yBytes);
                result = xBytes[..xCount].SequenceCompareTo(yBytes[..yCount]);
                break;
            case 2: result = string.CompareOrdinal(x.Team, y.Team); break;
            case 3: result = x.Commits - y.Commits; break;
            default: result = x.Score < y.Score ? -1 : x.Score > y.Score ? 1 : 0; break;
        }

        return sort.Descending ? -result : result;
    }

    private void ASortPeople(TableSort sort)
    {
        // The translated qsort uses stable insertion sort. Preserve ties and prior order.
        for (var i = 1; i < aPeopleCount; i++)
        {
            int value = aPeopleOrder[i], j = i - 1;
            while (j >= 0 && AComparePeople(aPeopleOrder[j], value, sort) > 0)
            {
                aPeopleOrder[j + 1] = aPeopleOrder[j];
                j--;
            }

            aPeopleOrder[j + 1] = value;
        }
    }

    private static void ARefreshPersonName(APerson person)
    {
        Span<byte> edited = stackalloc byte[48];
        Span<byte> cached = stackalloc byte[48];
        var buffer = person.NameBuffer!;
        var editedLength = buffer.CopyUtf8To(edited);
        var cachedLength = Encoding.UTF8.GetBytes(person.Name, cached);
        if (!edited[..editedLength].SequenceEqual(cached[..cachedLength])) person.Name = buffer.Text;
    }

    private void ADataGrid(Ui ui)
    {
        AInitializePeople();
        var current = aPeopleSort.Value;
        if (current.Column != aLastPeopleSort.Column || current.Descending != aLastPeopleSort.Descending)
        {
            ASortPeople(current);
            aLastPeopleSort = current;
        }

        Section(ui, "Filter bar", "Chips for active filters, values in a list, and a search.");
        ui.FilterBar("filters", aFilterFields, 3, aFilters, aPeopleSearch);
        Section(ui, "Data grid", "2,000 rows, virtualized. Sort, resize columns, select with Shift and Ctrl, edit names with a double click.");
        var selected = 0;
        for (var k = 0; k < aPeopleCount; k++)
            if (aPeopleSelected[k])
                selected++;
        int first = 0, last = 0;
        using (var grid = ui.DataGridBegin("people", aGridColumns, 5, aPeopleCount, ref first, ref last, new GridOpts { Height = Size.Px(420), Selected = aPeopleSelected, Sort = aPeopleSort }))
        {
            if (grid.IsOpen)
                for (var row = first; row < last; row++)
                {
                    var person = aPeople![aPeopleOrder[row]];
                    ui.DataGridRow(row);
                    ui.DataGridCell();
                    ui.Avatar(person.Name, new AvatarOpts { Size = 24 });
                    person.NameBuffer ??= Own(new TextBuffer(48, person.Name));
                    ui.DataGridEdit(person.NameBuffer);
                    ARefreshPersonName(person);
                    ui.DataGridCellText(person.Email);
                    ui.DataGridCell();
                    ui.Badge(person.Team);
                    ui.DataGridCellText(person.CommitText);
                    ui.DataGridCell();
                    ui.Progress(person.Score / 100, new ProgressOpts { Width = Size.Px(70), Height = 5 });
                    ui.Text(person.ScoreText, new TextOpts { Tabular = true, Size = 13, Color = Color.TextSecondary });
                }
        }

        ui.Muted(selected != 0 ? "Rows selected. Press Esc to clear." : "Click a row, Shift+click to select a range, Ctrl+A for all.");
        Section(ui, "Tree table", "Tree cells inside a table: disclosure, indentation, totals.");
        using (ui.TableBegin("tree", aTreeColumns, 3))
        {
            ui.TableRow(false);
            ui.TableCell();
            ui.TreeCell("src", 0, true, ref aTreeSource, Icons.Folder);
            ui.TableCellText("42");
            ui.TableCellText("1.2 MB");
            if (aTreeSource)
            {
                ui.TableRow(false);
                ui.TableCell();
                ui.TreeCell("ui", 1, true, ref aTreeUi, Icons.Folder);
                ui.TableCellText("18");
                ui.TableCellText("640 KB");
                if (aTreeUi)
                {
                    ui.TableRow(false);
                    ui.TableCell();
                    ui.TreeCell("button.c", 2, false, Icons.File);
                    ui.TableCellText("1");
                    ui.TableCellText("24 KB");
                }

                ui.TableRow(false);
                ui.TableCell();
                ui.TreeCell("main.c", 1, false, Icons.File);
                ui.TableCellText("1");
                ui.TableCellText("8 KB");
            }

            ui.TableRow(false);
            ui.TableCell();
            ui.TreeCell("README.md", 0, false, Icons.FileText);
            ui.TableCellText("1");
            ui.TableCellText("12 KB");
        }
    }

    private const string CodeSample =
            "#include <cui/cui.h>\n" +
            "\n" +
            "/* A counter with a button */\n" +
            "static int clicks = 0;\n" +
            "\n" +
            "void frame(cui_ctx *ui) {\n" +
            "    if (cui_button(ui, \"Click me\", .variant = CUI_BUTTON_PRIMARY)) clicks++;\n" +
            "    cui_textf(ui, \"Clicked %d times\", clicks);\n" +
            "    float ratio = clicks / 10.0f;   // grows with each click\n" +
            "}\n";

    private readonly TextBuffer _codeSource = new(64 * 1024, CodeSample);
    private CodeEditorResult _codeEditorResult;
    private string _codeStatus = "";

    private void DrawCodeEditor(Ui ui)
    {
        Section(ui, "Editable code", "Type here. Tab indents, Ctrl+Z undoes, Ctrl+F finds and Ctrl+G goes to a line.");
        using (ui.RowBegin(new Box { Width = Size.Fill, Align = Align.Center }))
        {
            ui.Text("counter.c", new TextOpts { Font = Font.Mono });
            ui.Spacer();
            if (ui.Button("Reset code", new ButtonOpts { Size = ControlSize.Sm })) _codeSource.Set(CodeSample);
        }
        var editor = ui.CodeEditor("editable-code"u8, _codeSource,
            new CodeEditorOpts { Lang = "c", Height = 320, Placeholder = "Write some C code…" });
        if (_codeStatus.Length == 0 || editor.Line != _codeEditorResult.Line || editor.Column != _codeEditorResult.Column ||
            editor.LineCount != _codeEditorResult.LineCount || editor.Length != _codeEditorResult.Length || editor.Refused != _codeEditorResult.Refused)
            _codeStatus = editor.Refused ? "Buffer full — edit was not inserted." :
                $"Ln {editor.Line}, Col {editor.Column}  ·  {editor.LineCount} lines  ·  {editor.Length:N0} / {_codeSource.Capacity - 1:N0} UTF-8 bytes";
        _codeEditorResult = editor;
        ui.Text(_codeStatus, new TextOpts { Size = 12, Color = Color.TextSecondary });

    }

    private void ACode(Ui ui)
    {
        if (IncludeManagedExamples) DrawCodeEditor(ui);

        Section(ui, "Code block", "Highlighting for C, C++, JavaScript, TypeScript, Python, Rust, Go, JSON and shell.");
        ui.CodeBlock("c-sample", CodeSample, new CodeOpts { Lang = "c", Title = "counter.c", LineNumbers = true, HighlightFrom = 7 });
        using (ui.RowBegin(new Box { Gap = 16, Align = Align.Start, Width = Size.Fill }))
        {
            using (ui.BoxBegin(new Box { Width = Size.Grow(1) }))
                ui.CodeBlock("py", "def fib(n: int) -> int:\n    # the slow way, on purpose\n    if n < 2:\n        return n\n    return fib(n - 1) + fib(n - 2)\n\nprint([fib(k) for k in range(10)])\n", new CodeOpts { Lang = "python", Title = "fib.py" });
            using (ui.BoxBegin(new Box { Width = Size.Grow(1) }))
                ui.CodeBlock("sh", "git clone https://github.com/yappologistic/CUI.git\ncmake -S CUI -B build\ncmake --build build\n./build/examples/gallery  # run it\n", new CodeOpts { Lang = "shell" });
        }

        Section(ui, "JSON viewer", "Collapsible, with counts. Click a brace to fold it.");
        ui.JsonView("json",
            "{\"name\": \"cui\", \"version\": \"1.1.0\", \"private\": false, \"downloads\": 128400,\n" +
            " \"platforms\": [\"windows\", \"macos\", \"linux\"],\n" +
            " \"renderer\": {\"backend\": \"opengl\", \"fallback\": \"software\", \"vsync\": true, \"msaa\": null},\n" +
            " \"authors\": [{\"name\": \"Maya\", \"commits\": 412}, {\"name\": \"Jonas\", \"commits\": 388}]}", new JsonOpts { ExpandDepth = 2 });
        ui.JsonView("broken", "{\"ok\": true, \"oops\": [1, 2,}");
    }

    private readonly State<int>[] aLikes = [new State<int>(12), new State<int>(3), new State<int>(0)];
    private readonly State<bool>[] aLiked = [new State<bool>(true), new State<bool>(false), new State<bool>(false)];
    private readonly State<int> aSlide = new State<int>(1);
    private Texture? aBefore, aAfter;
    private float aImageSplit = .45f;
    private int aDeck;
    private double aRevenue = 48210;
    private ImageView aImageView;
    private Rect aCrop;

    private static readonly PresenceUser[] aPresence =
    [
        new PresenceUser { Name = "Maya Okafor", Color = new Color(0x7c, 0x5c, 0xff) },
        new PresenceUser { Name = "Jonas Berg", Color = new Color(0x22, 0xc5, 0x8b) },
        new PresenceUser { Name = "Priya Nair", Color = new Color(0xf5, 0x9e, 0x0b) },
        new PresenceUser { Name = "Lena Fischer", Color = new Color(0x38, 0xbd, 0xf8), Idle = true },
        new PresenceUser { Name = "Aiko Tanaka", Color = new Color(0xec, 0x48, 0x99) },
        new PresenceUser { Name = "Daniel Mensah", Color = new Color(0x10, 0xb9, 0x81) },
        new PresenceUser { Name = "Rosa Delgado", Color = new Color(0xa8, 0x55, 0xf7) }
    ];

    private static readonly TickerItem[] aTicker =
    [
        new TickerItem { Label = "Revenue", Value = "$48.2k", Delta = 6.9f },
        new TickerItem { Label = "Active users", Value = "12,408", Delta = 3.1f },
        new TickerItem { Label = "Churn", Value = "1.7%", Delta = -.6f, LowerIsBetter = true },
        new TickerItem { Label = "Latency", Value = "182 ms", Delta = -8.4f, LowerIsBetter = true },
        new TickerItem { Label = "Builds", Value = "1,204", Delta = 12 },
        new TickerItem { Label = "Uptime", Value = "99.98%" }
    ];

    private static readonly string[] aSlideTitles = ["Faster exports", "ARM64 runners", "Shortcuts everywhere", "Smaller downloads"];

    private static readonly string[] aSlideDescriptions =
    [
        "A 900 file project now exports in 40 seconds, down from four minutes.",
        "Linux and macOS jobs can run on ARM64 machines in every plan.",
        "Every action in the editor has a key, listed under Help, Shortcuts.",
        "The desktop app is 38 MB lighter after dropping the bundled browser."
    ];

    private static readonly uint[] aSlideColors = [0x1f2a37, 0x0f3d3e, 0x3a2e22, 0x2b2f3a];
    private static readonly string[] aDeckNames = ["Kyoto", "Lisbon", "Reykjavik", "Oaxaca", "Hanoi"];
    private static readonly uint[] aDeckColors = [0xc8553d, 0x2d3047, 0x5b8e7d, 0xb7791f, 0x1f3a5f];

    private static void AMakeScene(Span<byte> pixels, int width, int height, bool color)
    {
        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
        {
            float fx = (float)x / width, fy = (float)y / height;
            float r = .56f + fy * .34f, g = .72f + fy * .2f, b = .88f + fy * .06f;
            var sun = (fx - .72f) * (fx - .72f) * 15.4f + (fy - .24f) * (fy - .24f) * 6.0f;
            if (sun < .024f)
            {
                r = 1.0f;
                g = .97f;
                b = .86f;
            }

            var m1 = .5f + .1f * MathF.Sin(fx * 6.0f + .6f) + .03f * MathF.Sin(fx * 23.0f);
            var m2 = .68f + .06f * MathF.Sin(fx * 4.0f + 2.1f) + .02f * MathF.Sin(fx * 31.0f);
            if (fy > m1)
            {
                r = .5f;
                g = .6f;
                b = .62f - fy * .08f;
            }

            if (fy > m2)
            {
                r = .3f - (fy - m2) * .2f;
                g = .45f - (fy - m2) * .25f;
                b = .3f;
            }

            if (!color)
            {
                var luminance = r * .3f + g * .59f + b * .11f;
                luminance = luminance * .85f + .05f;
                r = g = b = luminance;
            }

            var offset = (y * width + x) * 4;
            // C's unsigned-char conversion truncates after clamping; it does not round.
            pixels[offset] = (byte)(MathF.Min(1, MathF.Max(0, r)) * 255);
            pixels[offset + 1] = (byte)(MathF.Min(1, MathF.Max(0, g)) * 255);
            pixels[offset + 2] = (byte)(MathF.Min(1, MathF.Max(0, b)) * 255);
            pixels[offset + 3] = 255;
        }
    }

    private void ASceneTextures(Ui ui)
    {
        if (aBefore is not null && aAfter is not null) return;

        var pixels = new byte[640 * 400 * 4];
        if (aBefore is null)
        {
            AMakeScene(pixels, 640, 400, false);
            aBefore = Own(ui.ImageCreate(640, 400, pixels));
        }

        if (aAfter is null)
        {
            AMakeScene(pixels, 640, 400, true);
            aAfter = Own(ui.ImageCreate(640, 400, pixels));
        }
    }

    private void ASocial(Ui ui)
    {
        ASceneTextures(ui);
        using (ui.RowBegin(new Box { Gap = 24, Align = Align.Start, Width = Size.Fill }))
        {
            using (ui.ColumnBegin(new Box { Width = Size.Grow(1), Gap = 10 }))
            {
                Section(ui, "Chat", "Grouped bubbles, read receipts and a typing indicator.");
                using var chat = ui.CardBegin(new CardOpts { Gap = 0, Pad = 16 });
                ui.ChatDivider("Today");
                ui.ChatMessage("Did the data grid make it into 1.1?", new ChatOpts { Author = "Jonas Berg", Time = "9:41", LastInGroup = false });
                ui.ChatMessage("I need it for the billing page.", new ChatOpts { Author = "Jonas Berg", Grouped = true, LastInGroup = true });
                ui.ChatMessage("It did. 2,000 rows scroll at 60 fps on my laptop.", new ChatOpts { Mine = true });
                ui.ChatMessage("Shift and Ctrl clicks select like a spreadsheet.", new ChatOpts { Mine = true, Grouped = true, LastInGroup = true, Status = "Read", Time = "9:43" });
                ui.TypingIndicator("Jonas");
            }

            using (ui.ColumnBegin(new Box { Width = Size.Grow(1), Gap = 10 }))
            {
                Section(ui, "Comments", "Threads with replies, likes and badges.");
                using var comments = ui.CardBegin(new CardOpts { Gap = 4, Pad = 14 });
                ui.Comment("c1", "Tomas Rivera", "The new numbers match the staging run. I would still test the import on a large file before Friday.", new CommentOpts { Time = "2h", Likes = aLikes[0], Liked = aLiked[0], Badge = "Author" });
                ui.Comment("c2", "Priya Nair", "Agreed. I have a 2 GB export we can use.", new CommentOpts { Time = "1h", Depth = 1, Likes = aLikes[1], Liked = aLiked[1] });
                ui.Comment("c3", "Maya Okafor", "Scheduled it for 3 pm.", new CommentOpts { Time = "40m", Depth = 1, Likes = aLikes[2], Liked = aLiked[2], Edited = true });
            }
        }

        Section(ui, "Presence", "Who is here now. Hover to spread them out.");
        using (ui.RowBegin(new Box { Gap = 24, Align = Align.Center }))
        {
            ui.Presence("here", aPresence, 7, 5, 30);
            ui.Counter("rev", aRevenue, new CounterOpts { Format = "$%.0f", Size = 34, GroupThousands = true });
            if (ui.Button("Add a sale", new ButtonOpts { Size = ControlSize.Sm, Icon = Icons.Plus })) aRevenue += 1287;
        }

        Section(ui, "Ticker", "A continuous strip of figures. It pauses under the pointer.");
        ui.Ticker("tick", aTicker, 6);
        Section(ui, "Carousel", "Drag, flick, scroll sideways or use the arrows and dots.");
        using (ui.CarouselBegin("tour", 4, aSlide, new CarouselOpts { Height = 200, AutoAdvance = 6 }))
            for (var k = 0; k < 4; k++)
            {
                using var slide = ui.CarouselSlideBegin();
                using var box = ui.BoxBegin(new Box { Width = Size.Fill, Height = Size.Fill, Bg = Color.Rgb(aSlideColors[k]), Radius = 16, Pad = 28, Gap = 8, Justify = Justify.End });
                ui.Text(aSlideTitles[k], new TextOpts { Font = Font.Display, Size = 24, Color = Color.Rgb(0xffffff) });
                ui.Text(aSlideDescriptions[k], new TextOpts { Color = Color.Rgba(0xffffffc0) });
            }

        using (ui.RowBegin(new Box { Gap = 24, Align = Align.Start, Width = Size.Fill, Wrap = true }))
        {
            using (ui.ColumnBegin(new Box { Width = Size.Grow(3), MinWidth = 280, Gap = 10 }))
            {
                Section(ui, "Image viewer", "Wheel to zoom around the pointer, drag to pan, double-click for 100%.");
                ui.ImageViewer("viewer", aAfter, 640, 400, ref aImageView, new ImageViewerOpts { Height = Size.Px(260) });
            }

            using (ui.ColumnBegin(new Box { Width = Size.Grow(2), MinWidth = 240, Gap = 10 }))
            {
                Section(ui, "Cropper", "Drag the corners; the square keeps its shape.");
                ui.Cropper("cropper", aAfter, 640, 400, ref aCrop, new CropperOpts { Aspect = 1, Height = Size.Px(260) });
            }
        }

        using (ui.RowBegin(new Box { Gap = 24, Align = Align.Start, Width = Size.Fill }))
        {
            using (ui.ColumnBegin(new Box { Width = Size.Grow(1), Gap = 10 }))
            {
                Section(ui, "Before and after", "Drag the divider.");
                ui.ImageCompare("cmp", aBefore, aAfter, ref aImageSplit, new CompareOpts { Height = 230, Aspect = 640.0f / 400.0f });
            }

            using (ui.ColumnBegin(new Box { Width = Size.Grow(1), Gap = 10, Align = Align.Center }))
            {
                Section(ui, "Card stack", "Flick the top card away, left or right.");
                using var stack = ui.CardStackBegin("deck", 3, new CardStackOpts { Width = Size.Px(260), Height = 200 });
                for (var k = 0; k < 3; k++)
                {
                    var index = (aDeck + k) % 5;
                    using var item = ui.CardStackItemBegin(k);
                    using var box = ui.BoxBegin(new Box { Width = Size.Fill, Height = Size.Fill, Bg = Color.Rgb(aDeckColors[index]), Radius = 25, Pad = 20, Justify = Justify.End, Gap = 4 });
                    ui.Text(aDeckNames[index], new TextOpts { Font = Font.Display, Size = 22, Color = Color.Rgb(0xffffff) });
                    ui.Text("Swipe right to save", new TextOpts { Size = 13, Font = Font.Medium, Color = Color.Rgba(0xfffffff0) });
                }

                var direction = ui.CardStackEnd();
                if (direction != 0)
                {
                    aDeck = (aDeck + 1) % 5;
                    ui.Toast(direction > 0 ? "Saved" : "Skipped");
                }
            }
        }
    }

    private readonly record struct AKanbanCard(int Id, string Title, string Tag, string Who, Tone Tone, int Comments, int Files, float Progress);

    private static readonly AKanbanCard[] aKanbanCards =
    [
        new AKanbanCard(1, "Audit the color tokens for contrast", "Design", "Maya Okafor", Tone.Violet, 3, 0, 0),
        new AKanbanCard(2, "Virtualize the activity feed", "Perf", "Jonas Berg", Tone.Teal, 1, 2, 0),
        new AKanbanCard(3, "Dark mode screenshots for docs", "Docs", "Aiko Tanaka", Tone.Rose, 0, 4, 0),
        new AKanbanCard(4, "Keyboard nav in the date range picker", "A11y", "Priya Nair", Tone.Info, 5, 0, .6f),
        new AKanbanCard(5, "IME composition on Windows", "Platform", "Daniel Mensah", Tone.Warning, 2, 1, .3f),
        new AKanbanCard(6, "Spring presets", "Motion", "Rosa Delgado", Tone.Violet, 4, 0, 1),
        new AKanbanCard(7, "Glyph atlas eviction", "Perf", "Lena Fischer", Tone.Teal, 0, 0, 1),
        new AKanbanCard(8, "Retina screenshots", "Docs", "Aiko Tanaka", Tone.Rose, 1, 3, 1)
    ];

    private readonly int[][] aKanbanColumns =
    [
        [1, 2, 3, 0, 0, 0, 0, 0], [4, 5, 0, 0, 0, 0, 0, 0],
        [6, 0, 0, 0, 0, 0, 0, 0], [7, 8, 0, 0, 0, 0, 0, 0]
    ];

    private readonly int[] aKanbanLengths = [3, 2, 1, 2];
    private static readonly string[] aKanbanNames = ["Backlog", "In progress", "Review", "Done"];
    private static readonly uint[] aKanbanDots = [0x94a3b8, 0x3b82f6, 0xf59e0b, 0x22c55e];
    private static readonly string[] aDigits = ["0", "1", "2", "3", "4", "5", "6", "7", "8", "9"];

    private readonly CalEvent[] aWeekEvents =
    [
        new CalEvent { Title = "Standup", Detail = "Zoom", Day = 0, Start = 9.5f, End = 9.75f, Tone = Tone.Info },
        new CalEvent { Title = "Design review", Detail = "Room 2", Day = 0, Start = 11, End = 12.5f, Tone = Tone.Violet },
        new CalEvent { Title = "Lunch with Maya", Day = 1, Start = 12.5f, End = 13.5f, Tone = Tone.Success },
        new CalEvent { Title = "Standup", Detail = "Zoom", Day = 1, Start = 9.5f, End = 9.75f, Tone = Tone.Info },
        new CalEvent { Title = "Release planning", Detail = "Room 1", Day = 2, Start = 10, End = 11.5f, Tone = Tone.Warning },
        new CalEvent { Title = "1:1 Jonas", Day = 2, Start = 10.5f, End = 11, Tone = Tone.Rose },
        new CalEvent { Title = "Focus time", Detail = "No meetings", Day = 2, Start = 14, End = 17, Tone = Tone.Teal },
        new CalEvent { Title = "Standup", Detail = "Zoom", Day = 3, Start = 9.5f, End = 9.75f, Tone = Tone.Info },
        new CalEvent { Title = "Customer call", Detail = "Northlight", Day = 3, Start = 15, End = 16, Tone = Tone.Danger },
        new CalEvent { Title = "Demo day", Detail = "All hands", Day = 4, Start = 16, End = 17.5f, Tone = Tone.Violet },
        new CalEvent { Title = "Gym", Day = 5, Start = 10, End = 11.5f, Tone = Tone.Success }
    ];

    private readonly GanttTask[] aGanttTasks =
    [
        new GanttTask { Name = "Research", Owner = "Maya Okafor", Start = 0, End = 5, Progress = 1, Tone = Tone.Teal, DependsOn = -1 },
        new GanttTask { Name = "Wireframes", Owner = "Aiko Tanaka", Start = 4, End = 10, Progress = 1, Tone = Tone.Teal, DependsOn = 0 },
        new GanttTask { Name = "Design system", Owner = "Rosa Delgado", Start = 8, End = 17, Progress = .7f, Tone = Tone.Teal, DependsOn = 1 },
        new GanttTask { Name = "Core engine", Owner = "Jonas Berg", Start = 6, End = 20, Progress = .55f, Tone = Tone.Info, DependsOn = 0 },
        new GanttTask { Name = "Platform backends", Owner = "Daniel Mensah", Start = 14, End = 26, Progress = .2f, Tone = Tone.Teal, DependsOn = 3 },
        new GanttTask { Name = "Beta", Start = 27, End = 27, Tone = Tone.Warning, DependsOn = 4, Milestone = true },
        new GanttTask { Name = "Docs and launch", Owner = "Priya Nair", Start = 28, End = 36, Tone = Tone.Success, DependsOn = 5 }
    ];

    private Date aCalendarMonth;
    private readonly State<Date> aPickedDate = new State<Date>();
    private readonly MonthEvent[] aMonthEvents = new MonthEvent[12];
    private static readonly string[] aMonthTitles = ["Standup", "Design review", "Offsite", "1:1", "Release", "Planning", "Customer call", "Lunch", "Retro", "Holiday", "Demo", "Board meeting"];

    private static AKanbanCard? AFindKanbanCard(int id)
    {
        for (var k = 0; k < aKanbanCards.Length; k++)
            if (aKanbanCards[k].Id == id)
                return aKanbanCards[k];

        return null;
    }

    private void AApplyKanbanMove(KanbanMove move)
    {
        int[] from = aKanbanColumns[move.FromColumn], to = aKanbanColumns[move.ToColumn];
        var at = -1;
        for (var k = 0; k < aKanbanLengths[move.FromColumn]; k++)
            if (from[k] == move.Card)
                at = k;
        if (at < 0 || (move.ToColumn != move.FromColumn && aKanbanLengths[move.ToColumn] >= 8)) return;

        for (var k = at; k < aKanbanLengths[move.FromColumn] - 1; k++) from[k] = from[k + 1];
        aKanbanLengths[move.FromColumn]--;
        var index = move.ToIndex < 0 ? 0 : move.ToIndex > aKanbanLengths[move.ToColumn] ? aKanbanLengths[move.ToColumn] : move.ToIndex;
        for (var k = aKanbanLengths[move.ToColumn]; k > index; k--) to[k] = to[k - 1];
        to[index] = move.Card;
        aKanbanLengths[move.ToColumn]++;
    }

    private void APlanning(Ui ui)
    {
        Section(ui, "Kanban", "Drag cards within and between columns. Columns can carry a work-in-progress limit.");
        using (ui.KanbanBegin("board"))
        {
            for (var c = 0; c < 4; c++)
            {
                using var column = ui.KanbanColumnBegin(aKanbanNames[c], aKanbanLengths[c], new KanbanColumnOpts { Dot = Color.Rgb(aKanbanDots[c]), Limit = c == 1 ? 3 : 0, Width = 264 });
                if (column.Result != 0) ui.Toast("New card", new ToastOpts { Description = aKanbanNames[c] });
                for (var k = 0; k < aKanbanLengths[c]; k++)
                {
                    var found = AFindKanbanCard(aKanbanColumns[c][k]);
                    if (found is not { } card) continue;

                    using var cardScope = ui.KanbanCardBegin(card.Id);
                    if (cardScope.Result != 0) ui.Toast(card.Title);
                    ui.Badge(card.Tag, new BadgeOpts { Tone = card.Tone, Size = ControlSize.Sm });
                    ui.Text(card.Title, new TextOpts { Font = Font.Medium, Size = 13.5f, LineHeight = 1.35f });
                    if (card.Progress > 0 && card.Progress < 1) ui.Progress(card.Progress, new ProgressOpts { Height = 4 });
                    using (ui.RowBegin(new Box { Gap = 10, Align = Align.Center, Width = Size.Fill }))
                    {
                        ui.Avatar(card.Who, new AvatarOpts { Size = 22 });
                        ui.Spacer();
                        if (card.Files != 0)
                        {
                            ui.Icon(Icons.Paperclip, 12, Color.TextMuted);
                            ui.Text(aDigits[card.Files % 10], new TextOpts { Size = 12, Color = Color.TextMuted });
                        }

                        if (card.Comments != 0)
                        {
                            ui.Icon(Icons.MessageSquare, 12, Color.TextMuted);
                            ui.Text(aDigits[card.Comments % 10], new TextOpts { Size = 12, Color = Color.TextMuted });
                        }

                        if (card.Progress >= 1) ui.Icon(Icons.CircleCheck, 14, Color.Success);
                    }
                }
            }

            KanbanMove move = default;
            if (ui.KanbanEnd(ref move)) AApplyKanbanMove(move);
        }

        Section(ui, "Week calendar", "Drag events to move them in 15 minute steps, or pull the bottom edge to resize.");
        var today = ui.DateToday();
        var todayDays = ui.DateToDays(today);
        var monday = todayDays - ((todayDays + 3) % 7 + 7) % 7;
        var ev = ui.WeekCalendar("week", ui.DateFromDays(monday), aWeekEvents, aWeekEvents.Length, new WeekOpts { StartHour = 8, EndHour = 19, Height = Size.Px(420) });
        if (ev >= 0) ui.Toast(aWeekEvents[ev].Title!, new ToastOpts { Description = aWeekEvents[ev].Detail });
        Section(ui, "Gantt", "Drag bars to reschedule, pull either end to change the length. Arrows follow dependencies.");
        var origin = ui.DateFromDays(monday - 7);
        var task = ui.Gantt("plan", aGanttTasks, aGanttTasks.Length, origin, 42, new GanttOpts { Today = 7 + (todayDays - monday) });
        if (task >= 0)
        {
            var duration = string.Create(CultureInfo.InvariantCulture, $"{aGanttTasks[task].End - aGanttTasks[task].Start} days");
            ui.Toast(aGanttTasks[task].Name!, new ToastOpts { Description = aGanttTasks[task].Milestone ? "Milestone" : duration });
        }

        Section(ui, "Month calendar", "Bars for days off and trips, timed events in time order, +N more when a day is full.");
        var now = ui.DateToday();
        var firstDay = ui.DateToDays(new Date(now.Year, now.Month, 1));
        for (var k = 0; k < 12; k++)
        {
            var span = k == 2 || k == 9;
            aMonthEvents[k] = new MonthEvent
            {
                Title = aMonthTitles[k], Start = ui.DateFromDays(firstDay + (k * 5) % 27),
                End = span ? ui.DateFromDays(firstDay + (k * 5) % 27 + 2) : default,
                Time = 9.0f + k % 8, AllDay = span || k == 4, Tone = (Tone)(1 + k % 8)
            };
        }

        var result = ui.MonthCalendar("month", ref aCalendarMonth, aMonthEvents, 12, new MonthOpts { Selected = aPickedDate });
        if (result.Event >= 0) ui.Toast(aMonthEvents[result.Event].Title!, new ToastOpts { Description = "Opened from the month calendar" });
    }

    partial void DisposeExtendedA()
    {
        _codeSource.Dispose();
        aTitle.Dispose();
        aOwner.Dispose();
        aUrl.Dispose();
        aWeight.Dispose();
        aMessage.Dispose();
        aRecent.Dispose();
        aPeopleSearch.Dispose();
    }
}