using Cui.Managed;

namespace Cui.ManagedGallery;

public sealed partial class GalleryDemo
{
    private static readonly float[] frameSpark = [9, 7, 8, 6, 5, 6, 4, 4, 3];
    private static readonly string[] overviewNames = ["Jonas Berg", "Priya Nair", "Tomas Rivera", "Lena Fischer"];
    private static readonly string[] overviewEmails = ["jonas@northlight.io", "priya@harbor.dev", "tomas@kiln.app", "lena@fischer.dev"];
    private static readonly string[] overviewRoles = ["Owner", "Admin", "Editor", "Viewer"];
    private static readonly string[] views = ["List", "Board", "Timeline"];
    private static readonly string[] viewIcons = [Icons.List, Icons.LayoutDashboard, Icons.History];
    private static readonly string[] ranges = ["Day", "Week", "Month", "Year"];
    private static readonly string[] countries = ["Canada", "Denmark", "Germany", "Iran", "Japan", "Kenya", "Netherlands", "Portugal", "United States"];
    private static readonly string[] frameworks = ["C99", "C11", "C17", "C23", "C++17", "Objective-C", "Zig", "Odin", "Rust"];

    private static readonly Choice[] places =
    [
        new Choice { Title = "This computer", Description = "Saved next to the project. Nothing leaves the machine.", Icon = Icons.Monitor },
        new Choice { Title = "Team server", Description = "Synced to your company server every few minutes.", Icon = Icons.Server, Badge = "Default" },
        new Choice { Title = "Both", Description = "A local copy, uploaded to the server every night.", Icon = Icons.Layers }
    ];

    private static readonly string[] chipLabels = ["Design", "Engineering", "Marketing", "Sales", "Support", "Finance"];
    private static readonly string[] tabs = ["Overview", "Analytics", "Reports", "Settings"];
    private static readonly string[] secondaryTabs = ["Account", "Password", "Billing"];
    private static readonly string[] tabIcons = [Icons.User, Icons.Lock, Icons.CreditCard];
    private static readonly string[] crumbs = ["Home", "Projects", "CUI", "Settings"];
    private static readonly string[] steps = ["Account", "Workspace", "Invite team", "Done"];
    private static readonly string[] avatarTeam = ["Kwame Asante", "Hana Novak", "Elif Kaya", "Ben Carter", "Omar Haddad", "Felix Moreau", "Iris Coleman"];

    private void DrawOverview(Ui ui)
    {
        using (ui.ColumnBegin(new Box { Gap = 14, PadBottom = 8 }))
        {
            ui.Heading("Components for C", 1);
            ui.Text("Forms, tables, charts, planning boards and chat pieces that draw themselves the same way on Windows, macOS and Linux. Every one works from the keyboard.",
                new TextOpts { Color = Color.TextSecondary, Size = 15 });
            using var row = ui.RowBegin(new Box { Gap = 8, PadTop = 6, Wrap = true });
            if (ui.Button("Browse components", new ButtonOpts { Variant = ButtonVariant.Primary, IconRight = Icons.ArrowRight })) page = 1;
            if (ui.Button("Open command palette", new ButtonOpts { Variant = ButtonVariant.Outline, Icon = Icons.Command })) paletteOpen = true;
        }

        using (ui.BoxBegin(new Box { Dir = Dir.Grid, Columns = 3, MinColumnWidth = 200, Gap = 16 }))
        {
            ui.Metric("Components", "150", new MetricOpts { Icon = Icons.Box });
            ui.Metric("Frame time", "0.4 ms", new MetricOpts { Delta = -18, DeltaLabel = "-18% vs v0.9", LowerIsBetter = true, Spark = frameSpark, Icon = Icons.Zap });
            ui.Metric("Dependencies", "0", new MetricOpts { Icon = Icons.Package });
        }

        using (ui.BoxBegin(new Box { Dir = Dir.Grid, Columns = 2, MinColumnWidth = 260, Gap = 16 }))
        {
            using (ui.CardBegin(new CardOpts { Gap = 16 }))
            {
                using (ui.ColumnBegin(new Box { Gap = 4 }))
                {
                    ui.Heading("Create a workspace", 4);
                    ui.Secondary("You can invite people once it exists.");
                }

                ui.Input("ov-name", Name, new InputOpts { Label = "Name", Placeholder = "Your name" });
                ui.Input("ov-email", Email, new InputOpts { Label = "Email", Icon = Icons.Mail, Placeholder = "you@company.com" });
                ui.Checkbox("Email me a weekly summary", ref terms);
                if (ui.Button("Create workspace", new ButtonOpts { Variant = ButtonVariant.Primary, FullWidth = true }))
                    ui.Toast("Workspace created", new ToastOpts { Description = "It is empty for now. Start with a project.", Tone = Tone.Success });
            }

            using (ui.CardBegin(new CardOpts { Gap = 12 }))
            {
                using (ui.RowBegin())
                {
                    using (ui.ColumnBegin(new Box { Gap = 4, Width = Size.Fill }))
                    {
                        ui.Heading("Team", 4);
                        ui.Secondary("Four people, one owner.");
                    }

                    ui.Button("Invite", new ButtonOpts { Icon = Icons.UserPlus, Size = ControlSize.Sm });
                }

                for (var i = 0; i < overviewNames.Length; i++)
                {
                    using var row = ui.RowBegin(new Box { Gap = 12, PadY = 4 });
                    ui.Avatar(overviewNames[i], new AvatarOpts { Size = 34, Status = i == 0 ? "online" : i == 1 ? "away" : null });
                    using (ui.ColumnBegin(new Box { Width = Size.Fill, Gap = 1 }))
                    {
                        ui.Text(overviewNames[i], new TextOpts { Font = Font.Medium, Nowrap = true });
                        ui.Text(overviewEmails[i], new TextOpts { Size = 12, Color = Color.TextSecondary, Nowrap = true });
                    }

                    ui.Badge(overviewRoles[i], new BadgeOpts { Tone = i == 0 ? Tone.Accent : Tone.Neutral });
                }
            }
        }
    }

    private void DrawButtons(Ui ui)
    {
        Section(ui, "Variants", "One primary action per view. Secondary for everything else.");
        using (ui.CardBegin())
        using (ui.RowBegin(new Box { Gap = 8, Wrap = true }))
        {
            ui.Button("Primary", new ButtonOpts { Variant = ButtonVariant.Primary });
            ui.Button("Secondary");
            ui.Button("Outline", new ButtonOpts { Variant = ButtonVariant.Outline });
            ui.Button("Ghost", new ButtonOpts { Variant = ButtonVariant.Ghost });
            ui.Button("Danger", new ButtonOpts { Variant = ButtonVariant.Danger });
            ui.Button("Link", new ButtonOpts { Variant = ButtonVariant.Link });
        }

        Section(ui, "Sizes and icons", null);
        using (ui.CardBegin(new CardOpts { Gap = 16 }))
        {
            using (ui.RowBegin(new Box { Gap = 8, Wrap = true }))
            {
                ui.Button("Small", new ButtonOpts { Size = ControlSize.Sm });
                ui.Button("Medium");
                ui.Button("Large", new ButtonOpts { Size = ControlSize.Lg });
            }

            using (ui.RowBegin(new Box { Gap = 8, Wrap = true }))
            {
                ui.Button("New project", new ButtonOpts { Variant = ButtonVariant.Primary, Icon = Icons.Plus });
                ui.Button("Download", new ButtonOpts { Icon = Icons.Download });
                ui.Button("Continue", new ButtonOpts { Variant = ButtonVariant.Outline, IconRight = Icons.ArrowRight });
                ui.IconButton(Icons.Copy, new ButtonOpts { Tooltip = "Copy", Shortcut = "Ctrl C" });
                ui.IconButton(Icons.Share2, new ButtonOpts { Tooltip = "Share" });
                ui.IconButton(Icons.Heart, new ButtonOpts { Tooltip = "Like", Variant = ButtonVariant.Outline });
                ui.IconButton(Icons.Trash2, new ButtonOpts { Tooltip = "Delete", Variant = ButtonVariant.Secondary });
            }
        }

        Section(ui, "States", "Loading keeps the label so the layout does not jump.");
        using (ui.CardBegin())
        using (ui.RowBegin(new Box { Gap = 8, Wrap = true }))
        {
            if (ui.Button(loading ? "Saving" : "Save changes", new ButtonOpts { Variant = ButtonVariant.Primary, Loading = loading })) loading = true;
            if (ui.Button("Reset##loading")) loading = false;
            ui.Button("Disabled", new ButtonOpts { Disabled = true });
            ui.Button("Selected", new ButtonOpts { Variant = ButtonVariant.Ghost, Selected = true, Icon = Icons.Check });
        }

        Section(ui, "Groups, copy and hold to confirm", "Hold the button until it fills to confirm a destructive action.");
        using (ui.CardBegin())
        using (ui.RowBegin(new Box { Gap = 12, Wrap = true }))
        {
            using (ui.ButtonGroupBegin())
            {
                ui.Button("Bold##group", new ButtonOpts { Icon = Icons.Bold });
                ui.Button("Italic##group", new ButtonOpts { Icon = Icons.Italic });
                ui.Button("Underline##group", new ButtonOpts { Icon = Icons.Underline });
            }

            using (ui.RowBegin(new Box { Gap = 6, Bg = Color.SurfaceMuted, Radius = 18, PadLeft = 14, PadRight = 4, PadY = 2 }))
            {
                ui.Text("npm i cui", new TextOpts { Font = Font.Mono, Nowrap = true });
                ui.CopyButton("install", "npm i cui");
            }

            if (ui.HoldButton("Hold to delete", new HoldOpts { Icon = Icons.Trash2 })) ui.Toast("Deleted", new ToastOpts { Tone = Tone.Danger });
        }

        Section(ui, "Segmented control", "A highlight glides to the selected option.");
        using (ui.CardBegin(new CardOpts { Gap = 16 }))
        {
            ui.Segmented("view", views, 3, ref segView, new SegmentedOpts { Icons = viewIcons });
            ui.Segmented("range", ranges, 4, ref segSize, new SegmentedOpts { Size = ControlSize.Sm });
        }

        Section(ui, "Dropdown menu", null);
        using (ui.CardBegin())
        using (ui.RowBegin(new Box { Gap = 8, Wrap = true }))
        {
            using (var menu = ui.MenuBegin("Options", new MenuOpts { Icon = Icons.Settings }))
            {
                if (menu.IsOpen)
                {
                    ui.MenuLabel("Project");
                    ui.MenuItem("Rename", new MenuItemOpts { Icon = Icons.Pencil, Shortcut = "F2" });
                    ui.MenuItem("Duplicate", new MenuItemOpts { Icon = Icons.Copy, Shortcut = "Ctrl D" });
                    ui.MenuItem("Share", new MenuItemOpts { Icon = Icons.Share2 });
                    ui.MenuSeparator();
                    if (ui.MenuItem("Delete", new MenuItemOpts { Icon = Icons.Trash2, Danger = true })) confirmOpen = true;
                }
            }

            using (var menu = ui.MenuBegin("More actions", new MenuOpts { IconOnly = true, Icon = Icons.Ellipsis }))
            {
                if (menu.IsOpen)
                {
                    ui.MenuItem("Archive", new MenuItemOpts { Icon = Icons.Archive });
                    ui.MenuItem("Export", new MenuItemOpts { Icon = Icons.Download });
                }
            }
        }
    }

    private void DrawInputs(Ui ui)
    {
        Section(ui, "Text fields", "Labels above, hints and errors below.");
        using (ui.CardBegin(new CardOpts { Gap = 16 }))
        {
            using (ui.BoxBegin(new Box { Dir = Dir.Grid, Columns = 2, MinColumnWidth = 260, Gap = 16 }))
            {
                ui.Input("name", Name, new InputOpts { Label = "Full name", Placeholder = "Maya Okafor" });
                ui.Input("email", Email, new InputOpts { Label = "Email", Icon = Icons.AtSign, Hint = "We never share your email." });
                ui.Input("password", Password, new InputOpts { Label = "Password", Password = true, Placeholder = "At least 8 characters", Error = Password.Length is > 0 and < 8 ? "Use 8 or more characters." : null });
                ui.Search("search", Search, new InputOpts { Label = "Search", Placeholder = "Search projects" });
            }

            ui.Textarea("bio", Bio, new InputOpts { Label = "About", Height = 96, Hint = "Markdown is supported." });
        }

        Section(ui, "Tags, codes and password strength", null);
        using (ui.CardBegin(new CardOpts { Gap = 16 }))
        {
            ui.TagInput("tags", Tags, new TagOpts { Label = "Topics", Placeholder = "Add a topic" });
            using (ui.ColumnBegin(new Box { Gap = 8 }))
            {
                ui.Label("Verification code");
                if (ui.OtpInput("otp", Otp, 6)) ui.Toast("Code entered", new ToastOpts { Tone = Tone.Success });
            }

            using (ui.ColumnBegin(new Box { Gap = 8 }))
            {
                ui.Input("new-pw", Password, new InputOpts { Label = "New password", Password = true, Placeholder = "Try a long passphrase" });
                ui.PasswordStrength(Password.Text);
            }
        }

        Section(ui, "Pickers", null);
        using (ui.CardBegin(new CardOpts { Gap = 16 }))
        {
            using (ui.BoxBegin(new Box { Dir = Dir.Grid, Columns = 2, MinColumnWidth = 260, Gap = 16 }))
            {
                ui.Select("country", countries, 9, ref country, new SelectOpts { Label = "Country" });
                ui.Select("framework", frameworks, 9, ref framework, new SelectOpts { Label = "Language", Searchable = true, Placeholder = "Pick a language" });
                ui.DatePicker("due", ref date, new DateOpts { Label = "Due date" });
                ui.Number("qty", ref quantity, 0, 99, new NumberOpts { Label = "Seats", Width = Size.Fill });
            }

            using (ui.ColumnBegin(new Box { Gap = 6 }))
            {
                ui.Label("Brand color");
                ui.ColorPicker("brand", ref color);
            }
        }
    }

    private void DrawSelection(Ui ui)
    {
        using (ui.BoxBegin(new Box { Dir = Dir.Grid, Columns = 2, MinColumnWidth = 260, Gap = 16 }))
        {
            using (ui.ColumnBegin(new Box { Gap = 12 }))
            {
                Section(ui, "Checkbox", null);
                using var card = ui.CardBegin(new CardOpts { Gap = 14 });
                ui.Checkbox("Email notifications", ref newsletter, new CheckOpts { Description = "Product news, at most once a month." });
                ui.Checkbox("Marketing", ref marketing, new CheckOpts { Description = "Offers from our partners." });
                ui.Checkbox("Disabled option", ref terms, new CheckOpts { Disabled = true });
            }

            using (ui.ColumnBegin(new Box { Gap = 12 }))
            {
                Section(ui, "Switch", null);
                using var card = ui.CardBegin(new CardOpts { Gap = 14 });
                ui.Switch("Wi-Fi", ref wifi);
                ui.Switch("Bluetooth", ref bluetooth);
                bluetoothRect = ui.LastRect();
                ui.Switch("Airplane mode", ref airplane, new CheckOpts { Description = "Turns off all radios." });
            }
        }

        Section(ui, "Radio group", null);
        using (ui.CardBegin(new CardOpts { Gap = 12 }))
        {
            ui.Radio("Every change", ref plan, 0, new CheckOpts { Description = "Save as you type. Uses the most disk." });
            ui.Radio("Every 5 minutes", ref plan, 1, new CheckOpts { Description = "A good balance for most projects." });
            ui.Radio("Only when I save", ref plan, 2, new CheckOpts { Description = "Nothing is written until you press Ctrl S." });
        }

        Section(ui, "Radio cards", "For choices that need a sentence of explanation.");
        ui.RadioCards("places", places, 3, ref planCard);
        Section(ui, "Sliders", "Drag past either end: the track stretches like rubber.");
        using (ui.CardBegin(new CardOpts { Gap = 20 }))
        {
            ui.Slider("Volume", ref volume, 0, 100, new SliderOpts { ShowValue = true });
            ui.Slider("Brightness", ref brightness, 0, 100, new SliderOpts { Step = 10, ShowValue = true, Format = "%.0f%%" });
            ui.RangeSlider("Price range", ref lo, ref hi, 0, 100);
        }

        Section(ui, "Chips and rating", null);
        using (ui.CardBegin(new CardOpts { Gap = 16 }))
        {
            using (ui.RowBegin(new Box { Gap = 8, Wrap = true }))
                for (var i = 0; i < chips.Length; i++)
                    ui.Chip(chipLabels[i], ref chips[i]);
            ui.Rating("rating", ref rating, 5);
        }
    }

    private void DrawNavigation(Ui ui)
    {
        Section(ui, "Tabs", "The indicator slides to the active tab.");
        using (ui.CardBegin(new CardOpts { Gap = 16 }))
        {
            ui.Tabs("tabs", tabs, 4, ref tab);
            using (ui.ColumnBegin(new Box { Gap = 6, PadY = 4 }))
            {
                ui.Label(tabs[tab]);
                ui.Secondary("Tabs switch between views of the same thing. Content changes in place.");
            }

            ui.Tabs("tabs2", secondaryTabs, 3, ref tab2, new TabsOpts { Variant = TabsVariant.Pill, Icons = tabIcons });
        }

        Section(ui, "Breadcrumb and pagination", null);
        using (ui.CardBegin(new CardOpts { Gap = 18 }))
        {
            var c = ui.Breadcrumb(crumbs, 4);
            if (c >= 0) ui.Toast(crumbs[c]);
            ui.Pagination("pages", ref pageIndex, 12);
        }

        Section(ui, "Stepper", null);
        using (ui.CardBegin(new CardOpts { Gap = 18 }))
        {
            ui.Stepper(steps, 4, step);
            using var row = ui.RowBegin(new Box { Gap = 8, Wrap = true });
            if (ui.Button("Back", new ButtonOpts { Disabled = step == 0 })) step--;
            if (ui.Button(step >= 3 ? "Finish" : "Next", new ButtonOpts { Variant = ButtonVariant.Primary, IconRight = Icons.ArrowRight }))
            {
                if (step < 3) step++;
                else ui.Toast("All set", new ToastOpts { Tone = Tone.Success });
            }
        }
    }

    private void DrawOverlays(Ui ui)
    {
        Section(ui, "Dialogs and sheets", "Esc or a click outside closes them. Focus stays inside.");
        using (ui.CardBegin())
        using (ui.RowBegin(new Box { Gap = 8, Wrap = true }))
        {
            if (ui.Button("Edit profile", new ButtonOpts { Icon = Icons.Pencil })) dialogOpen = true;
            if (ui.Button("Open sheet", new ButtonOpts { Icon = Icons.PanelLeft })) sheetOpen = true;
            if (ui.Button("Delete project", new ButtonOpts { Variant = ButtonVariant.Danger, Icon = Icons.Trash2 })) confirmOpen = true;
            if (ui.Button("Command palette", new ButtonOpts { Variant = ButtonVariant.Outline, Icon = Icons.Command, Tooltip = "Search everything", Shortcut = "Ctrl K" })) paletteOpen = true;
        }

        Section(ui, "Popover and context menu", null);
        using (ui.CardBegin(new CardOpts { Gap = 16 }))
        {
            using (ui.RowBegin(new Box { Gap = 8, Wrap = true }))
            {
                if (ui.Button("Filters", new ButtonOpts { Icon = Icons.Filter })) ui.PopoverToggle("filters");
                using var popover = ui.PopoverBegin("filters", new PopoverOpts { Width = 280 });
                if (popover.IsOpen)
                {
                    ui.Label("Filter results");
                    ui.Checkbox("Only active", ref newsletter);
                    ui.Checkbox("Include archived", ref marketing);
                    ui.Slider("Minimum score", ref brightness, 0, 100, new SliderOpts { ShowValue = true });
                    using var row = ui.RowBegin(new Box { Gap = 8, Justify = Justify.End });
                    if (ui.Button("Apply", new ButtonOpts { Variant = ButtonVariant.Primary, Size = ControlSize.Sm })) ui.PopoverClose("filters");
                }
            }

            using (ui.CardBegin(new CardOpts { Muted = true, Id = "ctx-area", Pad = 28, Align = Align.Center, Hoverable = true }))
            {
                ui.Icon(Icons.Ellipsis, 18, Color.TextMuted);
                ui.Text("Right click this area", new TextOpts { Color = Color.TextSecondary, Align = TextAlign.Center });
            }

            using var contextMenu = ui.ContextMenuBegin("ctx");
            if (contextMenu.IsOpen)
            {
                ui.MenuItem("Cut", new MenuItemOpts { Icon = Icons.Scissors, Shortcut = "Ctrl X" });
                ui.MenuItem("Copy", new MenuItemOpts { Icon = Icons.Copy, Shortcut = "Ctrl C" });
                ui.MenuItem("Paste", new MenuItemOpts { Icon = Icons.Clipboard, Shortcut = "Ctrl V" });
                ui.MenuSeparator();
                ui.MenuItem("Delete", new MenuItemOpts { Icon = Icons.Trash2, Danger = true });
            }
        }

        Section(ui, "Toasts", "They stack, expand on hover and pause while you read.");
        using (ui.CardBegin())
        using (ui.RowBegin(new Box { Gap = 8, Wrap = true }))
        {
            if (ui.Button("Save profile")) ui.Toast("Changes saved", new ToastOpts { Description = "Your profile is up to date.", Tone = Tone.Success });
            if (ui.Button("Upload a 30 MB file")) ui.Toast("Upload failed", new ToastOpts { Description = "The file is larger than 25 MB.", Tone = Tone.Danger });
            if (ui.Button("Archive a message")) ui.Toast("Message archived", new ToastOpts { Action = "Undo" });
            if (ui.Button("Check for updates")) ui.Toast("Update available", new ToastOpts { Description = "Version 1.1 is ready to install.", Tone = Tone.Info });
        }

        Section(ui, "Tooltips", null);
        using (ui.CardBegin())
        using (ui.RowBegin(new Box { Gap = 4, Wrap = true }))
        {
            ui.IconButton(Icons.Bold, new ButtonOpts { Tooltip = "Bold", Shortcut = "Ctrl B" });
            ui.IconButton(Icons.Italic, new ButtonOpts { Tooltip = "Italic", Shortcut = "Ctrl I" });
            ui.IconButton(Icons.Underline, new ButtonOpts { Tooltip = "Underline", Shortcut = "Ctrl U" });
            ui.Separator();
            ui.IconButton(Icons.AlignLeft, new ButtonOpts { Tooltip = "Align left" });
            ui.IconButton(Icons.List, new ButtonOpts { Tooltip = "Bulleted list" });
            ui.IconButton(Icons.Link, new ButtonOpts { Tooltip = "Insert link" });
        }
    }

    private void DrawFeedback(Ui ui)
    {
        Section(ui, "Alerts", "Color only when something succeeded, needs attention or failed.");
        using (ui.ColumnBegin(new Box { Gap = 10 }))
        {
            ui.Alert("A new version is available", new AlertOpts { Description = "Restart to update to 1.1." });
            ui.Alert("Payment received", new AlertOpts { Description = "Your invoice was paid on November 5.", Tone = Tone.Success });
            ui.Alert("Storage almost full", new AlertOpts { Description = "You have used 92% of your plan.", Tone = Tone.Warning, Dismissible = true });
            ui.Alert("Build failed", new AlertOpts { Description = "Missing symbol cui_frame in main.c line 42.", Tone = Tone.Danger });
        }

        using (ui.BoxBegin(new Box { Dir = Dir.Grid, Columns = 2, MinColumnWidth = 260, Gap = 16 }))
        {
            using (ui.CardBegin(new CardOpts { Gap = 18 }))
            {
                ui.Heading("Progress", 4);
                ui.Progress(0.68f, new ProgressOpts { Label = "Uploading assets", ShowValue = true });
                ui.Progress(0, new ProgressOpts { Label = "Indexing", Indeterminate = true });
                ui.Meter("Storage", 18.4f, 20, "GB");
                using var row = ui.RowBegin(new Box { Gap = 16, Wrap = true });
                ui.Spinner(16);
                ui.Spinner(22);
                ui.Spinner(28);
            }

            using (ui.CardBegin(new CardOpts { Gap = 14 }))
            {
                ui.Heading("Loading", 4);
                using (ui.RowBegin(new Box { Gap = 12, Wrap = true }))
                {
                    ui.Skeleton(Size.Px(40), 40);
                    using var column = ui.ColumnBegin(new Box { Gap = 8, Width = Size.Fill });
                    ui.Skeleton(Size.Percent(70), 12);
                    ui.Skeleton(Size.Percent(45), 12);
                }

                ui.Skeleton(Size.Fill, 88);
                ui.Skeleton(Size.Percent(90), 12);
                ui.Skeleton(Size.Percent(60), 12);
            }
        }

        using (ui.BoxBegin(new Box { Dir = Dir.Grid, Columns = 2, MinColumnWidth = 260, Gap = 16 }))
        {
            using (ui.CardBegin(new CardOpts { Gap = 14 }))
            {
                ui.Heading("Activity", 4);
                ui.TimelineItem("Deployed to production", new TimelineOpts { Time = "2m ago", Description = "Release 1.0.3 is live in all regions.", Icon = Icons.Rocket, Tone = Tone.Success });
                ui.TimelineItem("Review requested", new TimelineOpts { Time = "1h ago", Description = "Jonas asked you to review the theming changes.", Icon = Icons.MessageSquare, Tone = Tone.Info });
                ui.TimelineItem("Build failed", new TimelineOpts { Time = "3h ago", Icon = Icons.CircleAlert, Tone = Tone.Danger, Last = true });
            }

            using (ui.CardBegin(new CardOpts { Gap = 14 }))
            {
                ui.Heading("Goals", 4);
                using var row = ui.RowBegin(new Box { Gap = 24, Justify = Justify.Around });
                ui.ProgressRing("Onboarding", 0.72f, 104);
                ui.ProgressRing("Quota", 0.35f, 104);
            }
        }

        Section(ui, "Badges and avatars", null);
        using (ui.CardBegin(new CardOpts { Gap = 16 }))
        {
            using (ui.RowBegin(new Box { Gap = 8, Wrap = true }))
            {
                ui.Badge("Draft");
                ui.Badge("Recommended", new BadgeOpts { Tone = Tone.Accent });
                ui.Badge("Active", new BadgeOpts { Tone = Tone.Success, Dot = true });
                ui.Badge("Pending", new BadgeOpts { Tone = Tone.Warning, Dot = true });
                ui.Badge("Overdue", new BadgeOpts { Tone = Tone.Danger, Dot = true });
                ui.Badge("Beta", new BadgeOpts { Tone = Tone.Violet });
                ui.Badge("New", new BadgeOpts { Tone = Tone.Info, Solid = true });
            }

            using (ui.RowBegin(new Box { Gap = 16, Wrap = true }))
            {
                ui.Avatar("Sam Whitfield", new AvatarOpts { Size = 40, Status = "online" });
                ui.Avatar("Daniel Mensah", new AvatarOpts { Size = 40, Status = "busy" });
                ui.Avatar("Rosa Delgado", new AvatarOpts { Size = 40, Square = true });
                ui.AvatarGroup(avatarTeam, 7, 4, 32);
            }
        }

        using (ui.CardBegin(new CardOpts { Pad = 0 }))
        {
            if (ui.EmptyState("No projects yet", new EmptyOpts { Icon = Icons.FolderOpen, Description = "A project holds your screens, their builds and who can see them.", Action = "New project" }))
                ui.Toast("Project created", new ToastOpts { Tone = Tone.Success });
        }
    }

    private readonly record struct PersonRow(string Name, string Email, string Role, string Status, Tone StatusTone, string Amount);

    private static readonly PersonRow[] people =
    [
        new PersonRow("Jonas Berg", "jonas@northlight.io", "Owner", "Active", Tone.Success, "$12,384"),
        new PersonRow("Priya Nair", "priya@harbor.dev", "Admin", "Active", Tone.Success, "$9,846"),
        new PersonRow("Tomas Rivera", "tomas@kiln.app", "Editor", "Invited", Tone.Warning, "$7,215"),
        new PersonRow("Lena Fischer", "lena@fischer.dev", "Viewer", "Active", Tone.Success, "$3,127"),
        new PersonRow("Aiko Tanaka", "aiko@tanaka.design", "Editor", "Suspended", Tone.Danger, "$1,962")
    ];

    private static readonly TableColumn[] peopleColumns =
    [
        new TableColumn { Title = "Name", Width = Size.Fill, Align = TextAlign.Left, Sortable = true },
        new TableColumn { Title = "Email", Width = Size.Grow(1.2f), Align = TextAlign.Left },
        new TableColumn { Title = "Role", Width = Size.Px(110), Align = TextAlign.Left, Sortable = true },
        new TableColumn { Title = "Status", Width = Size.Px(120), Align = TextAlign.Left, Sortable = true },
        new TableColumn { Title = "Revenue", Width = Size.Px(110), Align = TextAlign.Right }
    ];

    private readonly int[] peopleOrder = new int[5];
    private static readonly string[] treeFiles = ["button.c", "dialog.c", "slider.c", "table.c"];
    private static readonly string[] tasks = ["Write the release notes", "Record the demo video", "Update the screenshots", "Tag version 1.0", "Post the announcement"];
    private static readonly Tone[] taskTones = [Tone.Info, Tone.Neutral, Tone.Info, Tone.Warning, Tone.Teal];
    private static readonly string[] taskLabels = ["Docs", "Video", "Docs", "Release", "Blog"];
    private static readonly string[] accordionQuestions = ["Is it really dependency free?", "Can I use my own renderer?", "Does it respect reduced motion?"];

    private static readonly string[] accordionAnswers =
    [
        "Yes. The core is plain C99. Windows uses Win32, macOS uses Cocoa and Linux uses X11. Fonts and icons are embedded.",
        "CUI outputs vertices and draw commands. Port the 60 line shader to Vulkan, Metal or Direct3D and you are done.",
        "Every animation has a reduced motion path that keeps the final state and feedback."
    ];

    private readonly string?[] invoiceLabels = new string?[10000];
    private readonly string?[] invoiceAmounts = new string?[10000];

    private void DrawData(Ui ui)
    {
        Section(ui, "Table", "Sortable columns, row selection and components in cells.");
        // Upstream sorts before the header processes this frame's click.
        var currentSort = sort.Value;
        for (var i = 0; i < peopleOrder.Length; i++) peopleOrder[i] = i;
        for (var i = 1; i < peopleOrder.Length; i++)
        {
            int v = peopleOrder[i], j = i - 1;
            while (j >= 0)
            {
                PersonRow a = people[peopleOrder[j]], b = people[v];
                var aKey = currentSort.Column == 0 ? a.Name : currentSort.Column == 2 ? a.Role : a.Status;
                var bKey = currentSort.Column == 0 ? b.Name : currentSort.Column == 2 ? b.Role : b.Status;
                var c = string.CompareOrdinal(aKey, bKey);
                if (currentSort.Descending ? c < 0 : c > 0)
                {
                    peopleOrder[j + 1] = peopleOrder[j];
                    j--;
                }
                else break;
            }

            peopleOrder[j + 1] = v;
        }

        using (ui.TableBegin("people", peopleColumns, 5, new TableOpts { Sort = sort, Selectable = true }))
        {
            for (var k = 0; k < peopleOrder.Length; k++)
            {
                var r = people[peopleOrder[k]];
                if (ui.TableRow(selectedRow == peopleOrder[k])) selectedRow = peopleOrder[k];
                ui.TableCell();
                ui.Avatar(r.Name, new AvatarOpts { Size = 26 });
                ui.Text(r.Name, new TextOpts { Font = Font.Medium, Nowrap = true });
                ui.TableCell();
                ui.Text(r.Email, new TextOpts { Color = Color.TextSecondary, Nowrap = true });
                ui.TableCellText(r.Role);
                ui.TableCell();
                ui.Badge(r.Status, new BadgeOpts { Tone = r.StatusTone, Dot = true });
                ui.TableCellText(r.Amount);
            }
        }

        using (ui.BoxBegin(new Box { Dir = Dir.Grid, Columns = 2, MinColumnWidth = 260, Gap = 16 }))
        {
            using (ui.ColumnBegin(new Box { Gap = 12 }))
            {
                Section(ui, "Tree view", null);
                using var card = ui.CardBegin(new CardOpts { Pad = 10, Gap = 1 });
                using var src = ui.TreeBegin("src", ref treeSrc);
                if (src.IsOpen)
                {
                    using (var components = ui.TreeBegin("components", ref treeUi))
                    {
                        if (components.IsOpen)
                            for (var i = 0; i < treeFiles.Length; i++)
                            {
                                using var leaf = ui.TreeBegin(treeFiles[i], new TreeOpts { Leaf = true, Selected = selectedFile == i });
                                if (ui.TreeClicked()) selectedFile = i;
                            }
                    }

                    using (var assets = ui.TreeBegin("assets", ref treeAssets))
                    {
                        if (assets.IsOpen)
                        {
                            using var leaf = ui.TreeBegin("logo.png", new TreeOpts { Leaf = true, Icon = Icons.Image });
                        }
                    }

                    using var main = ui.TreeBegin("main.c", new TreeOpts { Leaf = true });
                }
            }

            using (ui.ColumnBegin(new Box { Gap = 12 }))
            {
                Section(ui, "List", null);
                using var card = ui.CardBegin(new CardOpts { Pad = 8, Gap = 2 });
                ui.ListItem("Design review", new ListItemOpts { Avatar = "Jonas Berg", Subtitle = "Tomorrow at 10:00", Trailing = "45m" });
                ui.ListItem("Release 1.0", new ListItemOpts { Avatar = "Priya Nair", Subtitle = "Ship it", Trailing = "2h", Selected = true });
                ui.ListItem("Planning", new ListItemOpts { Avatar = "Tomas Rivera", Subtitle = "Q1 roadmap", Trailing = "1h" });
            }
        }

        Section(ui, "Reorderable list", "Drag a row. The others make room as you go.");
        using (ui.ColumnBegin(new Box { Width = Size.Fill }))
        {
            using var reorder = ui.ReorderBegin("tasks", taskOrder, 5);
            for (var i = 0; i < taskOrder.Length; i++)
            {
                using var item = ui.ReorderItemBegin(i);
                var t = taskOrder[i];
                ui.Text(tasks[t], new TextOpts { Nowrap = true, Width = Size.Fill });
                ui.Badge(taskLabels[t], new BadgeOpts { Tone = taskTones[t] });
            }

            if (ui.ReorderEnd()) ui.Toast("Order saved");
        }

        Section(ui, "Accordion", null);
        using (ui.CardBegin(new CardOpts { PadY = 4, Gap = 0 }))
        {
            for (var i = 0; i < acc.Length; i++)
            {
                using var accordion = ui.AccordionBegin(accordionQuestions[i], ref acc[i]);
                if (accordion.IsOpen) ui.Secondary(accordionAnswers[i]);
            }
        }

        Section(ui, "Virtual list", "Ten thousand rows. Only the visible ones are built.");
        using (ui.CardBegin(new CardOpts { Pad = 6 }))
        {
            int first = 0, last = 0;
            using var list = ui.VirtualListBegin("big", 10000, 36, Size.Px(220), ref first, ref last);
            for (var i = first; i < last; i++)
            {
                var h = unchecked((uint)(i + 1) * 2654435761u);
                var label = invoiceLabels[i] ??= FormattableString.Invariant($"Invoice 2026-{10000 - i:00000}");
                var amount = invoiceAmounts[i] ??= FormattableString.Invariant($"${40 + (h >> 8) % 2400}.{(h >> 4) % 100:00}");
                using var row = ui.RowBegin(new Box { Height = Size.Px(36), PadX = 12, Gap = 12 });
                ui.Icon(Icons.FileText, 16, Color.TextMuted);
                ui.Text(label, new TextOpts { Nowrap = true, Width = Size.Fill, Tabular = true });
                ui.Text(amount, new TextOpts { Size = 13, Tabular = true, Nowrap = true });
                ui.Text((h >> 20) % 5 == 0 ? "Due" : "Paid", new TextOpts { Size = 12, Color = Color.TextMuted, Nowrap = true, Width = Size.Px(32) });
            }
        }
    }

    private static readonly string[] months = ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"];

    private static readonly Series[] revenueSeries =
    [
        new Series { Name = "Revenue", Values = new float[] { 18, 22, 21, 27, 31, 29, 36, 41, 39, 46, 52, 58 } },
        new Series { Name = "Costs", Values = new float[] { 12, 13, 15, 14, 18, 19, 21, 22, 24, 25, 27, 29 } }
    ];

    private static readonly Series[] visitSeries =
    [
        new Series { Name = "Desktop", Values = new float[] { 42, 51, 38, 64, 58, 71 } },
        new Series { Name = "Mobile", Values = new float[] { 28, 33, 30, 41, 46, 49 } }
    ];

    private static readonly float[] platformShares = [46, 27, 17, 10];
    private static readonly string[] platformLabels = ["Windows", "macOS", "Linux", "Other"];

    private static readonly Series[] runSeries =
    [
        new Series { Name = "Passed", Values = new float[] { 112, 124, 131, 128, 140, 152, 149, 161 } },
        new Series { Name = "Retried", Values = new float[] { 9, 11, 8, 12, 10, 9, 13, 8 } },
        new Series { Name = "Failed", Values = new float[] { 4, 6, 3, 5, 7, 4, 6, 3 } }
    ];

    private static readonly string[] weeks = ["W1", "W2", "W3", "W4", "W5", "W6", "W7", "W8"];

    private void DrawCharts(Ui ui)
    {
        Section(ui, "Line chart", "Hover to inspect values. The crosshair glides between points.");
        using (ui.CardBegin())
            ui.LineChart("revenue", revenueSeries, 2, new ChartOpts { Labels = months, Area = true, Smooth = true, Height = 240, Format = "$%.0fk" });
        using (ui.BoxBegin(new Box { Dir = Dir.Grid, Columns = 2, MinColumnWidth = 260, Gap = 16 }))
        {
            using (ui.ColumnBegin(new Box { Gap = 12 }))
            {
                Section(ui, "Bar chart", null);
                using var card = ui.CardBegin();
                ui.BarChart("visits", visitSeries, 2, new ChartOpts { Labels = months.AsMemory(0, 6), Height = 200 });
            }

            using (ui.ColumnBegin(new Box { Gap = 12 }))
            {
                Section(ui, "Donut chart", null);
                using var card = ui.CardBegin(new CardOpts { Align = Align.Center });
                ui.DonutChart("platforms", platformShares, platformLabels, 4, new ChartOpts { Height = 180, Format = "%.0f%%" });
            }
        }

        Section(ui, "Stacked bars and gauges", null);
        using (ui.BoxBegin(new Box { Dir = Dir.Grid, Columns = 2, MinColumnWidth = 260, Gap = 16 }))
        {
            using (ui.CardBegin())
                ui.BarChart("runs", runSeries, 3, new ChartOpts { Labels = weeks, Stacked = true, Height = 200 });
            using (ui.CardBegin(new CardOpts { Dir = Dir.Row, Gap = 24, Align = Align.Center }))
            {
                ui.Gauge("CPU", 0.42f, 130);
                ui.Gauge("Memory", 0.81f, 130);
            }
        }
    }
}