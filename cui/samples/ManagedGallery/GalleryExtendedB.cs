using System.Globalization;
using System.Text;
using Cui.Managed;
using TaskStatus = Cui.Managed.TaskStatus;

namespace Cui.ManagedGallery;

public sealed partial class GalleryDemo
{
    partial void DrawExtendedB(Ui ui, int page)
    {
        switch (page)
        {
            case 19: BMoreCharts(ui); break;
            case 20: BMotion(ui); break;
            case 21: BAi(ui); break;
            case 22: BAudio(ui); break;
            case 23: BFiles(ui); break;
            case 24: BDeveloperTools(ui); break;
            case 25: BMoreDeveloperTools(ui); break;
            case 26: BInstruments(ui); break;
        }
    }

    private int bReplay, bMotionTab;
    private readonly float[] bPeaks = new float[96];
    private float bWaveProgress = .36f;
    private bool bWifi = true, bHotspot, bFocus = true, bAirShare, bDarkTile = true;
    private NowPlaying bNowPlaying = new() { Title = "Low Tide", Artist = "Marlow Avenue", ArtColor = new(0xc8, 0x55, 0x3d), Position = 76, Duration = 222, Playing = true };
    private static readonly string[] bRotatingWords = ["staging.", "eu-west.", "us-east.", "all regions."];
    private static readonly string[] bSpotTitles = ["Software", "OpenGL 3.3", "Your own"];
    private static readonly string[] bSpotBlurbs = ["Plain C into a pixel buffer. No GPU needed.", "The default on desktop. Deploying now.", "The core emits triangles; bring any GPU API."];
    private static readonly TabBarItem[] bMotionTabs =
    [
        new() { Icon = Icons.House, Label = "Home" }, new() { Icon = Icons.Search, Label = "Search" },
        new() { Icon = Icons.Inbox, Label = "Inbox", Badge = 4 }, new() { Icon = Icons.User, Label = "Profile" }
    ];

    private void BMotion(Ui ui)
    {
        if (bPeaks[0] == 0)
            for (int k = 0; k < 96; k++) bPeaks[k] = .18f + .8f * MathF.Abs(MathF.Sin(k * .37f) * MathF.Sin(k * .11f + 1.3f));
        Section(ui, "Text effects", "A shimmer for pending work, a typewriter for streamed text, words that settle into place.");
        using (ui.CardBegin(new() { Gap = 18, Pad = 20, Width = Size.Fill }))
        {
            using (ui.RowBegin(new() { Gap = 10, Align = Align.Center }))
            {
                ui.Loader(new() { Style = LoaderStyle.LoaderDots, Size = 22 });
                ui.ShimmerText("Thinking about your question…", new() { Size = 15 });
            }
            using (ui.PushIdInt(bReplay))
            {
                ui.Typewriter("tw", "CUI draws a full frame in a fraction of a millisecond, so text can stream in as fast as it arrives.", new() { Caret = true, Size = 15 });
                ui.TextReveal("rv", "The release build finished in 3 minutes 42 seconds.", new() { Size = 20, Font = Font.Display });
            }
            using (ui.RowBegin(new() { Gap = 8, Align = Align.Center }))
            {
                ui.Text("Deploying to", new() { Font = Font.Display, Size = 26 });
                ui.WordRotate("rot", bRotatingWords, 4, 2.2f, new() { Font = Font.Display, Size = 26 });
            }
            if (ui.Button("Replay", new() { Size = ControlSize.Sm, Icon = Icons.RotateCcw, Variant = ButtonVariant.Outline })) bReplay++;
        }
        Section(ui, "Gradient text and spotlight cards", "A light follows the pointer across a card and along its edge. A beam marks the one card that is live.");
        ui.GradientText("Kiln 2.4", new() { Font = Font.Display, Size = 34, Animate = true });
        using (ui.RowBegin(new() { Gap = 14, Align = Align.Stretch, Width = Size.Fill }))
            for (int k = 0; k < 3; k++)
            {
                using var id = ui.PushIdInt(k);
                using var spot = ui.SpotlightBegin("spot", new() { Width = Size.Grow(1), Beam = k == 1 });
                ui.Text(bSpotTitles[k], new() { Font = Font.Medium, Size = 15 });
                ui.Text(bSpotBlurbs[k], new() { Size = 13, Color = Color.TextSecondary });
            }
        Section(ui, "Loaders", "Dots, bars, pulse, orbit and grid.");
        using (ui.RowBegin(new() { Gap = 36, Align = Align.Center }))
            for (int k = 0; k < 5; k++) ui.Loader(new() { Style = (LoaderStyle)k, Size = 28 });
        Section(ui, "Dock", "Icons grow as the pointer passes.");
        using (ui.RowBegin(new() { Width = Size.Fill, Justify = Justify.Center, Height = Size.Px(110), Align = Align.End }))
        using (ui.DockBegin("dock"))
        {
            if (ui.DockItem(Icons.Folder, "Files", new() { Color = Color.Rgb(0x3b82f6), Running = true })) ui.Toast("Files");
            ui.DockItem(Icons.Mail, "Mail", new() { Color = Color.Rgb(0x0ea5e9), Badge = 3, Running = true });
            ui.DockItem(Icons.Calendar, "Calendar", new() { Color = Color.Rgb(0xef4444) });
            ui.DockItem(Icons.Music, "Music", new() { Color = Color.Rgb(0xec4899), Running = true });
            ui.DockItem(Icons.SquareTerminal, "Terminal", new() { Color = Color.Rgb(0x27272a) });
            ui.DockItem(Icons.Palette, "Design", new() { Color = Color.Rgb(0x8b5cf6) });
            ui.DockSeparator();
            ui.DockItem(Icons.Settings, "Settings");
            ui.DockItem(Icons.Trash2, "Trash");
        }
        using (ui.RowBegin(new() { Gap = 28, Align = Align.Start, Width = Size.Fill }))
        {
            using (ui.ColumnBegin(new() { Width = Size.Grow(1), Gap = 10 }))
            {
                Section(ui, "Control tiles", "Big toggles for quick settings.");
                using (ui.BoxBegin(new() { Dir = Dir.Grid, Columns = 2, Gap = 10, Width = Size.Px(300) }))
                {
                    ui.ControlTile("Wi-Fi", ref bWifi, new() { Icon = Icons.Wifi, Detail = bWifi ? "Home" : "Off" });
                    ui.ControlTile("Hotspot", ref bHotspot, new() { Icon = Icons.Radio });
                    ui.ControlTile("Focus", ref bFocus, new() { Icon = Icons.Moon, Detail = bFocus ? "Until 6 pm" : "Off" });
                    ui.ControlTile("AirShare", ref bAirShare, new() { Icon = Icons.Share2, Detail = bAirShare ? "Everyone" : "Contacts only" });
                }
                ui.ControlTile("Dark appearance", ref bDarkTile, new() { Icon = Icons.Sun, Wide = true, Width = Size.Px(300) });
            }
            using (ui.ColumnBegin(new() { Width = Size.Grow(1), Gap = 10 }))
            {
                Section(ui, "Now playing", "Artwork, a scrubber and transport.");
                int ev = ui.NowPlaying("np", ref bNowPlaying);
                if ((ev & (int)PlayerActions.Next) != 0) ui.Toast("Next track");
                if (bNowPlaying.Playing)
                {
                    bNowPlaying.Position += ui.DeltaTime();
                    if (bNowPlaying.Position > bNowPlaying.Duration) bNowPlaying.Position = 0;
                }
                Section(ui, "Waveform", "Click or drag to seek.");
                using (ui.CardBegin(new() { Pad = 14, Gap = 8, Width = Size.Px(360) }))
                using (ui.RowBegin(new() { Gap = 12, Align = Align.Center, Width = Size.Fill }))
                {
                    ui.IconButton(Icons.Play, new() { Size = ControlSize.Sm, Variant = ButtonVariant.Secondary });
                    using (ui.BoxBegin(new() { Width = Size.Fill })) ui.Waveform("wave", bPeaks, 96, ref bWaveProgress, new() { Height = 34 });
                    ui.Text("0:42", new() { Size = 12, Tabular = true, Color = Color.TextMuted });
                }
                Section(ui, "Tab bar", "For compact layouts.");
                ui.TabBar("tabs", bMotionTabs, 4, ref bMotionTab, new() { Width = Size.Px(360) });
            }
        }
    }

    private readonly TextBuffer bDraft = new(512);
    private readonly State<int> bModel = new(), bVote = new(), bRemoved = new(-1);
    private int bOrbState = 1, bAttachmentCount = 2;
    private bool bBusy;
    private double bStarted = -1;
    private readonly string[] bAttachments = ["quarterly-report.pdf", "metrics.csv"];
    private static readonly Model[] bModels =
    [
        new() { Name = "Atlas", Detail = "Longer answers, reads files and code", Badge = "New", Icon = Icons.Layers },
        new() { Name = "Atlas Fast", Detail = "Short answers in about a second", Icon = Icons.Zap },
        new() { Name = "Atlas Mini", Detail = "Runs on your own machine", Icon = Icons.Box }
    ];
    private static readonly Source[] bSources =
    [
        new() { Title = "Immediate mode GUIs and why they keep coming back", Url = "https://fieldnotes.dev/immediate-mode", Snippet = "A look at retained and immediate mode toolkits and the trade offs between them." },
        new() { Title = "Notes on subpixel text positioning", Url = "https://glyphlab.io/notes/subpixel", Snippet = "Shaping, hinting and subpixel positioning explained with examples." },
        new() { Title = "Spring physics for interface motion", Url = "https://motionbook.dev/springs", Snippet = "Why spring physics feel more natural than cubic beziers for interface motion." }
    ];
    private static readonly VizItem[] bContextParts =
    [
        new() { Label = "System", Value = 3200 }, new() { Label = "Files", Value = 41800 }, new() { Label = "Messages", Value = 68400 }
    ];
    private readonly PlanStep[] bPlanSteps = new PlanStep[5];
    private static readonly string[] bVoiceStates = ["Idle", "Listening", "Thinking", "Speaking"];
    private static readonly PromptSuggestion[] bSuggestions =
    [
        new() { Icon = Icons.Code, Title = "Explain this code", Detail = "Walk through what it does" },
        new() { Icon = Icons.ChartBar, Title = "Analyze a dataset", Detail = "Find trends and outliers" },
        new() { Icon = Icons.Pencil, Title = "Draft an email", Detail = "Clear and friendly" },
        new() { Icon = Icons.Bug, Title = "Debug an error", Detail = "Paste the stack trace" }
    ];

    private void BAi(Ui ui)
    {
        double now = ui.Time();
        if (bBusy && bStarted < 0) bStarted = now;
        if (!bBusy) bStarted = -1;
        float elapsed = bBusy ? (float)(now - bStarted) : 0;
        if (bBusy && elapsed > 9) bBusy = false;
        using var row = ui.RowBegin(new() { Gap = 28, Align = Align.Start, Width = Size.Fill });
        using (ui.ColumnBegin(new() { Width = Size.Grow(1.6f), Gap = 14 }))
        {
            Section(ui, "Conversation", "Reasoning, tool calls, an answer with sources, and the composer.");
            ui.ChatMessage("Why do immediate mode toolkits feel so fast? Cite a few sources.", new() { Mine = true });
            using (var reasoning = ui.ReasoningBegin("think", new() { Thinking = bBusy, Seconds = bBusy ? elapsed : 8 }))
                if (reasoning.IsOpen)
                {
                    ui.Text("The question is about perceived speed. Two parts: how little work a frame does, and how quickly input shows up on screen.", new() { Size = 13.5f, Color = Color.TextSecondary });
                    ui.Text("Look for sources on retained versus immediate mode, text rendering cost and motion.", new() { Size = 13.5f, Color = Color.TextSecondary });
                }
            ui.ToolCall("search", "web_search", new() { Icon = Icons.Globe, Detail = "immediate mode gui performance", Status = TaskStatus.Done, Seconds = 1.4f,
                Input = "{\"query\": \"immediate mode gui performance\", \"limit\": 5}",
                Output = "{\"results\": [{\"title\": \"Immediate mode GUIs\", \"url\": \"https://fieldnotes.dev/immediate-mode\"}], \"took_ms\": 1380}" });
            ui.ToolCall("read", "read_file", new() { Icon = Icons.FileText, Detail = "quarterly-report.pdf", Status = bBusy ? TaskStatus.Running : TaskStatus.Done, Seconds = bBusy ? elapsed : 2.1f });
            if (bBusy) ui.AgentStatus("Reading quarterly-report.pdf", new() { Status = TaskStatus.Running, Seconds = elapsed });
            ui.Typewriter("answer", "They do very little work per frame: the interface is rebuilt from your data, laid out once and drawn in a handful of batches, so there is no tree of objects to keep in sync. Text is the expensive part, which is why glyphs are cached in an atlas, and motion feels quick when it is driven by springs instead of fixed curves.", new() { Size = 15 });
            using (ui.RowBegin(new() { Gap = 6, Align = Align.Center }))
            {
                ui.Text("Sources", new() { Size = 12, Color = Color.TextMuted });
                for (int k = 0; k < 3; k++) ui.Citation(k + 1, bSources[k]);
            }
            ui.SourceList("sources", bSources, 3);
            using (ui.RowBegin(new() { Gap = 8, Align = Align.Center, Width = Size.Fill }))
            {
                int actions = ui.ResponseActions("actions", new() { CopyText = "They do very little work per frame.", Vote = bVote });
                if ((actions & (int)ResponseAction.Regenerate) != 0) bBusy = true;
                ui.Spacer();
                ui.ContextMeter("ctx", 113400, 200000, new() { Parts = bContextParts });
            }
            int ev = ui.ChatComposer("composer", bDraft, new() { Placeholder = "Ask a follow up", Busy = bBusy,
                Attachments = bAttachments.AsMemory(0, bAttachmentCount), Removed = bRemoved, Models = bModels, Model = bModel });
            if ((ev & (int)ComposerActions.Send) != 0) { bDraft.Set(""); bBusy = true; }
            if ((ev & (int)ComposerActions.Stop) != 0) bBusy = false;
            int removed = bRemoved.Value;
            if ((ev & (int)ComposerActions.Remove) != 0 && removed >= 0 && removed < bAttachmentCount)
            {
                for (int k = removed; k < bAttachmentCount - 1; k++) bAttachments[k] = bAttachments[k + 1];
                bAttachmentCount--;
            }
            if ((ev & (int)ComposerActions.Attach) != 0) ui.Toast("Attach files", new() { Description = "Drop files on the window, too." });
        }
        using (ui.ColumnBegin(new() { Width = Size.Grow(1), Gap = 14 }))
        {
            Section(ui, "Agent plan", "Steps and their status.");
            bPlanSteps[0] = new() { Title = "Search for sources", Detail = "3 results", Status = TaskStatus.Done };
            bPlanSteps[1] = new() { Title = "Read the attached report", Detail = bBusy ? "Page 12 of 40" : "40 pages", Status = bBusy ? TaskStatus.Running : TaskStatus.Done };
            bPlanSteps[2] = new() { Title = "Compare the numbers with last quarter", Status = bBusy ? TaskStatus.Pending : TaskStatus.Done };
            bPlanSteps[3] = new() { Title = "Draft the summary", Status = bBusy ? TaskStatus.Pending : TaskStatus.Running };
            bPlanSteps[4] = new() { Title = "Check the citations", Status = TaskStatus.Pending };
            ui.AgentPlan("plan", bPlanSteps, 5, new() { Title = "Research plan" });
            Section(ui, "Voice", "Idle, listening, thinking and speaking.");
            using (ui.ColumnBegin(new() { Align = Align.Center, Gap = 12, Width = Size.Fill }))
            {
                float level = .35f + .35f * MathF.Sin((float)now * 5.1f) * MathF.Sin((float)now * 1.7f);
                if (ui.VoiceOrb("orb", (VoiceState)bOrbState, level, new() { Size = 170 })) bOrbState = bOrbState == 1 ? 0 : 1;
                ui.Segmented("orb-state", bVoiceStates, 4, ref bOrbState, new() { Size = ControlSize.Sm });
            }
            Section(ui, "Suggestions", "Ways to start.");
            int suggestion = ui.PromptSuggestions("sugg", bSuggestions, 4, new() { Columns = 1 });
            if (suggestion >= 0) bDraft.Set(bSuggestions[suggestion].Title + ": ");
        }
    }

    private float bCutoff = 1800, bResonance = .3f, bAttack = 12, bPan = .15f;
    private readonly float[] bFaders = [-4, -9, 0, -15, -6, -2];
    private readonly float[] bAudioLevels = new float[12], bMasterLevels = new float[2];
    private int bAudioTick, bAudioTour = -1;
    private static readonly string[] bChannelNames = ["Kick", "Snare", "Bass", "Keys", "Vocals", "FX"];
    private static readonly float[] bFaderMarks = [6, 0, -6, -12, -24, -48];
    private readonly TourStep[] bAudioSteps =
    [
        new() { Title = "Turn a knob", Body = "Drag up and down. Hold Shift for fine steps, double-click to reset.", Placement = Placement.BottomStart },
        new() { Title = "Watch the levels", Body = "Bars fall back after each peak; the mark above them holds the peak a moment.", Placement = Placement.TopStart },
        new() { Title = "Ride the faders", Body = "Click a fader, then use the wheel or the arrow keys.", Placement = Placement.LeftStart }
    ];

    private void BAudio(Ui ui)
    {
        bAudioTick++;
        for (int k = 0; k < 6; k++)
            for (int c = 0; c < 2; c++)
            {
                float t = (bAudioTick + c * 5) / 60f;
                float beat = (t * (1.8f + .2f * k) + k * .31f) % 1f;
                bAudioLevels[k * 2 + c] = (.3f + .1f * k) * (.2f + .8f * MathF.Exp(-beat * (2.5f + k)));
            }
        if (ui.Button("Take the tour", new() { Icon = Icons.Sparkles, Variant = ButtonVariant.Outline })) bAudioTour = 0;
        using (ui.RowBegin(new() { Gap = 16, Wrap = true, Align = Align.Start }))
        {
            using (ui.CardBegin(new() { Gap = 16, MinWidth = 280 }))
            {
                ui.Heading("Synth", 4);
                using (ui.RowBegin(new() { Gap = 14, Wrap = true }))
                {
                    ui.Knob("Cutoff", ref bCutoff, 20, 20000, new() { Unit = "Hz", LogScale = true, Reset = 1000 });
                    bAudioSteps[0].Target = ui.LastId();
                    ui.Knob("Resonance", ref bResonance, 0, 1, new() { Format = "%.2f" });
                    ui.Knob("Attack", ref bAttack, 1, 2000, new() { Unit = "ms", LogScale = true, Reset = 10 });
                    ui.Knob("Pan", ref bPan, -1, 1, new() { Bipolar = true, Format = "%+.2f" });
                }
                bMasterLevels[0] = bAudioLevels[0] * 1.4f;
                bMasterLevels[1] = bAudioLevels[9] * 1.3f;
                ui.LevelMeter("master", bMasterLevels, 2, new() { Horizontal = true, Length = 240, Scale = true, Segmented = true });
                bAudioSteps[1].Target = ui.LastId();
            }
            using (ui.CardBegin(new() { Dir = Dir.Row, Gap = 8, Wrap = true }))
                for (int k = 0; k < 6; k++)
                {
                    using var id = ui.PushIdInt(k);
                    ui.Fader(bChannelNames[k], ref bFaders[k], -48, 6, new() { Unit = "dB", Length = 180,
                        Ticks = k == 0 ? bFaderMarks : default, Meter = bAudioLevels.AsMemory(k * 2, 2) });
                    if (k == 2) bAudioSteps[2].Target = ui.LastId();
                }
        }
        if (bAudioTour >= 0) ui.Tour("audio-tour", bAudioSteps, 3, ref bAudioTour);
    }

    private readonly TextBuffer bFolder = new(1024, "."), bChosenFile = new(1024);
    private bool bOpenFileDialog, bSaveFileDialog;
    private TerminalBuffer? bTerminal;
    private readonly FileEntry[] bShellEntries = new FileEntry[256];
    private readonly FileListHandler bFileList = BListFiles;
    private static readonly string[] bDemoFiles = ["assets", "src", "README.md", "notes.txt", "cover.png", "report.pdf"];

    private static int BListFiles(string path, Span<FileEntry> entries, bool hidden) =>
        BListFilesCore(path, entries, hidden, checked(entries.Length * 40));

    private static int BListFilesCore(string path, Span<FileEntry> entries, bool hidden, int nameBytes)
    {
        int n = BReadDirectory(path, entries, hidden, nameBytes);
        if (n >= 0 || path != ".") return n;
        for (int k = 0; k < 6 && k < entries.Length; k++)
            entries[k] = new() { Name = bDemoFiles[k], Dir = k < 2, Size = (ulong)(k * 13100), Modified = 1730000000 + k * 86400 };
        return 6;
    }

    // Authored safe counterpart of the filesystem callback, including stat-like
    // metadata for symlinks and an entry with zero metadata if stat fails.
    private static int BReadDirectory(string path, Span<FileEntry> entries, bool hidden, int nameBytes)
    {
        try
        {
            int count = 0, used = 0;
            foreach (FileSystemInfo entry in new DirectoryInfo(path.Length == 0 ? "." : path).EnumerateFileSystemInfos())
            {
                if (!hidden && (entry.Name.StartsWith('.') || (OperatingSystem.IsWindows() && (entry.Attributes & FileAttributes.Hidden) != 0))) continue;
                if (count >= entries.Length) break;
                int bytes = Encoding.UTF8.GetByteCount(entry.Name) + 1;
                if (bytes > nameBytes - used) break;
                bool directory = false;
                ulong size = 0;
                long modified = 0;
                try
                {
                    FileSystemInfo? metadata = (entry.Attributes & FileAttributes.ReparsePoint) != 0 ? entry.ResolveLinkTarget(true) : entry;
                    if (metadata is not null && metadata.Exists)
                    {
                        directory = (metadata.Attributes & FileAttributes.Directory) != 0;
                        size = directory ? 0 : (ulong)((FileInfo)metadata).Length;
                        modified = new DateTimeOffset(metadata.LastWriteTimeUtc).ToUnixTimeSeconds();
                    }
                }
                catch (Exception error) when (error is IOException or UnauthorizedAccessException)
                { directory = false; size = 0; modified = 0; }
                entries[count++] = new() { Name = entry.Name, Dir = directory, Size = size, Modified = modified };
                used += bytes;
            }
            return count;
        }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException or ArgumentException or NotSupportedException)
        { return -1; }
    }

    private void BFiles(Ui ui)
    {
        Section(ui, "File browser", "Double-click to open; type to jump; Backspace goes up.");
        FileBrowserResult result = ui.FileBrowser("files", bFolder, new() { List = bFileList, Height = Size.Px(380) });
        if (result.Picked is not null) bChosenFile.Set(result.Picked);
        using (ui.RowBegin(new() { Gap = 8, Align = Align.Center, Wrap = true }))
        {
            if (ui.Button("Open…", new() { Icon = Icons.FolderOpen })) bOpenFileDialog = true;
            if (ui.Button("Save as…", new() { Icon = Icons.Download })) bSaveFileDialog = true;
            string chosen = bChosenFile.Text;
            if (chosen.Length != 0) ui.Muted(chosen);
        }
        ui.FilePicker("open", ref bOpenFileDialog, bChosenFile, new() { List = bFileList });
        ui.FilePicker("save", ref bSaveFileDialog, bChosenFile, new() { List = bFileList, Save = true, Name = "untitled.txt" });
        if (bTerminal is null)
        {
            bTerminal = Own(new TerminalBuffer(64 * 1024, 1024));
            bTerminal.Write("Type \u001b[1mhelp\u001b[0m for the commands.\n");
        }
        Section(ui, "Terminal", "Colors, \\r progress, history on Up and Down, Ctrl+C, Ctrl+L, drag to select.");
        TerminalResult terminal = ui.Terminal("shell", bTerminal, new() { Height = 300, Prompt = "> " });
        if (terminal.Command is not null) BShellCommand(terminal.Command);
        if (terminal.Interrupt) ui.Toast("Interrupted");
    }

    private void BShellCommand(string command)
    {
        TerminalBuffer terminal = bTerminal!;
        switch (command)
        {
            case "help":
                terminal.Write("  \u001b[36mls\u001b[0m      the folder above\n  \u001b[36mcolors\u001b[0m  the 16 colors\n  \u001b[36mbuild\u001b[0m   a progress bar\n  \u001b[36mecho\u001b[0m    prints its words\n  \u001b[36mclear\u001b[0m   empties the screen\n");
                break;
            case "ls":
                int count = BListFilesCore(bFolder.Text, bShellEntries, false, 16384);
                for (int k = 0; k < count; k++)
                    terminal.Write(bShellEntries[k].Dir ? "\u001b[1;34m" + bShellEntries[k].Name + "/\u001b[0m\n" : bShellEntries[k].Name + "\n");
                if (count < 0) terminal.Write("\u001b[31mls: cannot read this folder\u001b[0m\n");
                break;
            case "colors":
                for (int k = 0; k < 16; k++)
                    terminal.Write(string.Create(CultureInfo.InvariantCulture, $"\u001b[{(k < 8 ? 30 + k : 82 + k)}m██\u001b[0m{(k == 7 || k == 15 ? "\n" : " ")}"));
                break;
            case "build":
                for (int k = 0; k <= 20; k++)
                    terminal.Write(string.Create(CultureInfo.InvariantCulture, $"\r\u001b[32m{new string('█', k)}{new string('░', 20 - k)}\u001b[0m {k * 5,3}%"));
                terminal.Write("\n\u001b[32mdone\u001b[0m\n");
                break;
            case "clear": terminal.Clear(); break;
            default:
                if (command == "echo" || command.StartsWith("echo ", StringComparison.Ordinal)) terminal.Write((command.Length > 4 ? command[5..] : "") + "\n");
                else if (command.Length != 0) terminal.Write("\u001b[31m" + command + ": command not found\u001b[0m\n");
                break;
        }
    }

    private readonly KvPair[] bHeaderPairs = BCreateHeaders();
    private int bHeaderCount = 4;
    private static KvPair[] BCreateHeaders()
    {
        var pairs = new KvPair[12];
        pairs[0] = new() { Key = "Authorization", Value = "Bearer 4f9c2e81d0b7a6", Enabled = true, Secret = true };
        pairs[1] = new() { Key = "Accept", Value = "application/json", Enabled = true };
        pairs[2] = new() { Key = "X-Request-Id", Value = "7d1c0a", Enabled = false };
        pairs[3] = new() { Key = "Accept", Value = "text/html", Enabled = true };
        return pairs;
    }
    private static readonly Diagnostic[] bDiagnostics =
    [
        new() { Severity = Severity.Error, Message = "use of undeclared identifier 'frame_time'", File = "src/render/timing.c", Line = 42, Column = 17, Source = "clang", Code = "undeclared-var" },
        new() { Severity = Severity.Warning, Message = "unused variable 'scale'", File = "src/render/timing.c", Line = 88, Column = 11, Source = "clang", Code = "-Wunused-variable" },
        new() { Severity = Severity.Error, Message = "incompatible pointer types passing 'float *' to 'double *'", File = "src/core/layout.c", Line = 210, Column = 33, Source = "clang", Code = "-Wincompatible-pointer-types" },
        new() { Severity = Severity.Warning, Message = "implicit conversion loses floating-point precision", File = "src/core/layout.c", Line = 318, Column = 22, Source = "clang", Code = "-Wfloat-conversion" },
        new() { Severity = Severity.Info, Message = "function 'measure' could be declared static", File = "src/core/layout.c", Line = 402, Column = 6, Source = "clang-tidy", Code = "misc-use-internal-linkage" },
        new() { Severity = Severity.Hint, Message = "loop variable copied on each iteration; consider a pointer", File = "src/app/main.c", Line = 51, Column = 9, Source = "clang-tidy", Code = "performance-for-range-copy" },
        new() { Severity = Severity.Warning, Message = "comparison of integers of different signs", File = "src/app/main.c", Line = 77, Column = 18, Source = "gcc", Code = "-Wsign-compare" }
    ];
    private readonly State<int> bPipelineSelected = new(3), bCommitSelected = new(1), bGraphSelected = new(1), bWaterfallSelected = new(2), bKeymapMods = new((int)Mod.Primary);
    private static readonly string[] bPipelineStages = ["Build", "Test", "Package", "Deploy"];
    private static readonly PipelineJob[] bPipelineJobs =
    [
        new() { Name = "linux-x64", Stage = 0, Status = JobStatus.Passed, Duration = 142 },
        new() { Name = "macos-arm64", Stage = 0, Status = JobStatus.Passed, Duration = 201 },
        new() { Name = "windows-x64", Stage = 0, Status = JobStatus.Passed, Duration = 236 },
        new() { Name = "unit", Stage = 1, Status = JobStatus.Passed, Duration = 64 },
        new() { Name = "integration", Stage = 1, Status = JobStatus.Running, Duration = 95 },
        new() { Name = "lint", Stage = 1, Status = JobStatus.Failed, Duration = 12, AllowFailure = true },
        new() { Name = "docker image", Stage = 2, Status = JobStatus.Pending, Duration = -1 },
        new() { Name = "release notes", Stage = 2, Status = JobStatus.Skipped, Duration = -1 },
        new() { Name = "staging", Stage = 3, Status = JobStatus.Pending, Duration = -1 },
        new() { Name = "production", Stage = 3, Status = JobStatus.Pending, Duration = -1 }
    ];
    private static readonly QueryField[] bQueryFields =
    [
        new() { Name = "Status", Type = QueryType.QueryChoice, Choices = new string[] { "Open", "In progress", "Closed" } },
        new() { Name = "Assignee", Type = QueryType.QueryText }, new() { Name = "Priority", Type = QueryType.QueryNumber },
        new() { Name = "Has PR", Type = QueryType.QueryBool }
    ];
    private readonly QueryNode[] bQueryNodes = BCreateQuery();
    private int bQueryCount = 5;
    private readonly TextBuffer bQueryText = new(256), bCron = new(64, "30 9 * * 1-5");
    private static QueryNode[] BCreateQuery()
    {
        var nodes = new QueryNode[16];
        nodes[0] = new() { Group = true };
        nodes[1] = new() { Depth = 1, Field = 0, Op = 0, Value = "Open" };
        nodes[2] = new() { Depth = 1, Group = true, Any = true };
        nodes[3] = new() { Depth = 2, Field = 1, Op = 0, Value = "me" };
        nodes[4] = new() { Depth = 2, Field = 2, Op = 2, Value = "2" };
        return nodes;
    }
    private static readonly KeymapBinding[] bKeyBindings =
    [
        BBinding(Mod.Primary, Key.S, "Save", 0), BBinding(Mod.Primary, Key.O, "Open", 0), BBinding(Mod.Primary, Key.N, "New file", 0),
        BBinding(Mod.Primary, Key.W, "Close tab", 0), BBinding(Mod.Primary, Key.P, "Go to file", 1), BBinding(Mod.Primary, Key.G, "Go to line", 1),
        BBinding(Mod.Primary, Key.F, "Find", 2), BBinding(Mod.Primary, Key.H, "Replace", 2), BBinding(Mod.Primary, Key.Z, "Undo", 2),
        BBinding(Mod.Primary, Key.Y, "Redo", 2), BBinding(Mod.Primary, Key.B, "Build", 3), BBinding(Mod.Primary, Key.R, "Run", 3),
        BBinding(Mod.Primary, Key.D, "Debug", 3), BBinding(Mod.Primary, Key.Slash, "Comment", 2), BBinding(Mod.Primary, Key.E, "Recent files", 1),
        BBinding(Mod.Primary, Key.R, "Rename symbol", 2), BBinding(Mod.None, Key.F5, "Continue", 3), BBinding(Mod.None, Key.F9, "Breakpoint", 3),
        BBinding(Mod.None, Key.F10, "Step over", 3), BBinding(Mod.None, Key.F11, "Step into", 3), BBinding(Mod.None, Key.F2, "Rename", 2),
        BBinding(Mod.None, Key.F12, "Definition", 1), BBinding(Mod.Primary | Mod.Shift, Key.P, "Commands", 1), BBinding(Mod.Primary | Mod.Shift, Key.F, "Find in files", 2)
    ];
    private static KeymapBinding BBinding(Mod mods, Key key, string action, int category) => new() { Combo = new() { Mods = (int)mods, Key = key }, Action = action, Category = category };
    private static readonly string[] bKeyCategories = ["File", "Navigate", "Edit", "Run and debug"];
    private static readonly Commit[] bCommits =
    [
        new() { Hash = "a91f3c2e", Message = "Merge branch 'feature/trace-viewer'", Author = "Maya Okafor", Date = "2 hours ago", Refs = "main, origin/main", Parent = new int[] { 1, 4 } },
        new() { Hash = "7be04d19", Message = "Docs: the trace viewer and the waterfall", Author = "Maya Okafor", Date = "3 hours ago", Parent = new int[] { 2, -1 } },
        new() { Hash = "c3d8a7f0", Message = "Fix the ruler's last tick on narrow panels", Author = "Jonas Berg", Date = "5 hours ago", Parent = new int[] { 3, -1 } },
        new() { Hash = "1f22b9e4", Message = "Release 1.2", Author = "Jonas Berg", Date = "yesterday", Refs = "tag: v1.2.0", Parent = new int[] { 6, -1 } },
        new() { Hash = "e8a0c55b", Message = "Trace viewer: measure a range with Shift+drag", Author = "Priya Nair", Date = "yesterday", Refs = "feature/trace-viewer", Parent = new int[] { 5, -1 } },
        new() { Hash = "4d9e1a77", Message = "Trace viewer: lanes, spans and markers", Author = "Priya Nair", Date = "2 days ago", Parent = new int[] { 6, -1 } },
        new() { Hash = "90b7f3c1", Message = "Merge pull request #21 from fix/atlas", Author = "Maya Okafor", Date = "3 days ago", Parent = new int[] { 7, 8 } },
        new() { Hash = "52ce8d0a", Message = "Faster soft shadows in software", Author = "Lena Fischer", Date = "3 days ago", Parent = new int[] { 9, -1 } },
        new() { Hash = "b6f1e2d3", Message = "Glyph atlas: compact after a zoom", Author = "Aiko Tanaka", Date = "4 days ago", Refs = "fix/atlas", Parent = new int[] { 9, -1 } },
        new() { Hash = "0aa4c9e8", Message = "Right to left layout", Author = "Omid Saadat", Date = "5 days ago", Parent = new int[] { -1, -1 } }
    ];
    private static readonly GraphNode[] bGraphNodes =
    [
        new() { Label = "app", Detail = "executable", Tone = Tone.Violet }, new() { Label = "ui", Detail = "libcui.a", Tone = Tone.Info },
        new() { Label = "net", Detail = "http client", Tone = Tone.Info }, new() { Label = "storage", Detail = "sqlite", Tone = Tone.Teal },
        new() { Label = "render", Detail = "software, OpenGL", Tone = Tone.Info }, new() { Label = "text", Detail = "fonts, shaping", Tone = Tone.Info },
        new() { Label = "tls", Detail = "mbedtls", Tone = Tone.Warning }, new() { Label = "zlib", Tone = Tone.Neutral },
        new() { Label = "platform", Detail = "X11, Win32, Cocoa", Tone = Tone.Info }
    ];
    private static readonly GraphEdge[] bGraphEdges =
    [
        new() { From = 0, To = 1 }, new() { From = 0, To = 2 }, new() { From = 0, To = 3 }, new() { From = 1, To = 4 },
        new() { From = 1, To = 5 }, new() { From = 1, To = 8 }, new() { From = 2, To = 6 }, new() { From = 2, To = 7 },
        new() { From = 3, To = 7 }, new() { From = 5, To = 7 }, new() { From = 4, To = 8 }
    ];
    private static readonly NetRequest[] bNetworkRequests =
    [
        new() { Name = "https://shop.example.com/", Method = "GET", Status = 200, Type = "document", Size = 18400, Start = .000, Timing = new float[] { .002f, .018f, .024f, .031f, .142f, .012f } },
        new() { Name = "https://shop.example.com/assets/app.css", Method = "GET", Status = 200, Type = "stylesheet", Size = 42100, Start = .232, Timing = new float[] { .004f, 0, 0, 0, .061f, .022f } },
        new() { Name = "https://shop.example.com/assets/app.js", Method = "GET", Status = 200, Type = "script", Size = 186000, Start = .233, Timing = new float[] { .005f, 0, 0, 0, .068f, .094f } },
        new() { Name = "https://cdn.example.net/fonts/inter.woff2", Method = "GET", Status = 200, Type = "font", Size = 48200, Start = .241, Timing = new float[] { .003f, .021f, .019f, .028f, .044f, .018f } },
        new() { Name = "https://shop.example.com/api/session", Method = "POST", Status = 401, Type = "fetch", Size = 312, Start = .402, Timing = new float[] { .002f, 0, 0, 0, .083f, .001f } },
        new() { Name = "https://shop.example.com/api/products?page=1", Method = "GET", Status = 200, Type = "fetch", Size = 9600, Start = .404, Timing = new float[] { .002f, 0, 0, 0, .121f, .006f } },
        new() { Name = "https://shop.example.com/img/hero.avif", Method = "GET", Status = 200, Type = "image", Size = 212000, Start = .410, Timing = new float[] { .012f, 0, 0, 0, .052f, .188f } },
        new() { Name = "https://shop.example.com/img/p/1042.webp", Method = "GET", Status = 304, Type = "image", Size = 0, Start = .538, Timing = new float[] { .006f, 0, 0, 0, .038f, 0 } },
        new() { Name = "https://shop.example.com/img/p/1043.webp", Method = "GET", Status = 200, Type = "image", Size = 31200, Start = .540, Timing = new float[] { .008f, 0, 0, 0, .041f, .024f } },
        new() { Name = "https://analytics.example.org/collect", Method = "POST", Status = 0, Type = "ping", Size = 0, Start = .702, Timing = new float[] { .004f, .026f, .031f, .036f, .12f, 0 } }
    ];
    private readonly TraceTrack[] bTraceTracks = [new() { Name = "Main thread" }, new() { Name = "Render" }, new() { Name = "Worker 1" }, new() { Name = "Worker 2" }, new() { Name = "GPU" }];
    private readonly TraceSpan[] bTraceSpans = new TraceSpan[400];
    private int bTraceCount, bTraceSelected = -1;
    private TraceView bTraceView = new() { Start = .0042, End = .0418, Scroll = 0, MarkStart = .0161, MarkEnd = .0236 };
    private static readonly TraceMarker[] bTraceMarkers =
    [
        new() { Name = "vsync", Time = 0, Tone = Tone.Neutral }, new() { Name = "vsync", Time = .0166, Tone = Tone.Neutral },
        new() { Name = "GC pause", Time = .0273, Tone = Tone.Warning }, new() { Name = "vsync", Time = .0332, Tone = Tone.Neutral }
    ];

    private void BAddTrace(string name, double start, double end, int track, int depth)
    {
        if (bTraceCount < 400) bTraceSpans[bTraceCount++] = new() { Name = name, Start = start, End = end, Track = track, Depth = depth, ColorKey = 0 };
    }

    private void BBuildTrace(Ui ui)
    {
        for (int f = 0; f < 3; f++)
        {
            double t = f * .0166;
            BAddTrace("Frame", t + .0004, t + .0151, 0, 0);
            BAddTrace("Input", t + .0006, t + .0019, 0, 1);
            BAddTrace("Update", t + .0021, t + .0092, 0, 1);
            BAddTrace("Physics", t + .0023, t + .0051, 0, 2);
            BAddTrace("Animation", t + .0053, t + .0068, 0, 2);
            BAddTrace("Scripts", t + .0070, t + .0090, 0, 2);
            BAddTrace("Layout", t + .0094, t + .0121, 0, 1);
            BAddTrace("Measure", t + .0095, t + .0108, 0, 2);
            BAddTrace("Paint", t + .0123, t + .0149, 0, 1);
            BAddTrace("Record", t + .0124, t + .0139, 0, 2);
            BAddTrace("Submit", t + .0152, t + .0163, 1, 0);
            BAddTrace("Upload", t + .0153, t + .0157, 1, 1);
            BAddTrace("Encode", t + .0157, t + .0162, 1, 1);
            BAddTrace("Decode image", t + .0012, t + .0074, 2, 0);
            BAddTrace("Inflate", t + .0013, t + .0049, 2, 1);
            BAddTrace("Parse JSON", t + .0081, t + .0118, 3, 0);
            BAddTrace("Draw", t + .0164, t + .0166 + .0105, 4, 0);
        }
        ui.TraceSort(bTraceSpans, bTraceCount);
    }

    private void BDeveloperTools(Ui ui)
    {
        Section(ui, "Trace viewer", "Wheel zooms, drag pans, Shift+drag measures, a lane's name folds it.");
        if (bTraceCount == 0) BBuildTrace(ui);
        if (bTraceSelected < 0)
            for (int i = 0; i < bTraceCount; i++)
                if (bTraceSpans[i].Name == "Update" && bTraceSpans[i].Start > .016 && bTraceSpans[i].Start < .03) bTraceSelected = i;
        using (ui.ColumnBegin(new() { Gap = 10, Width = Size.Fill, MaxWidth = 1000 }))
            ui.TraceView("trace", bTraceTracks, 5, bTraceSpans, bTraceCount, ref bTraceView, ref bTraceSelected, new() { Markers = bTraceMarkers, Height = 320 });
        Section(ui, "Network waterfall", "Hover a bar for its timing; Up and Down move the selection.");
        using (ui.ColumnBegin(new() { Gap = 10, Width = Size.Fill, MaxWidth = 1000 }))
            ui.Waterfall("net", bNetworkRequests, 10, new() { Selected = bWaterfallSelected, Height = 340, DomLoaded = .395, Loaded = .68 });
        Section(ui, "Dependency graph", "Hover a node to light up its neighbours; wheel zooms, F fits.");
        using (ui.ColumnBegin(new() { Gap = 10, Width = Size.Fill, MaxWidth = 1000 }))
            ui.GraphView("deps", bGraphNodes, 9, bGraphEdges, 11, new() { Selected = bGraphSelected, Height = 340 });
        Section(ui, "Commit graph", "Branch lanes, merges and refs; only the rows in view are built.");
        using (ui.ColumnBegin(new() { Gap = 10, Width = Size.Fill, MaxWidth = 920 }))
            ui.CommitGraph("history", bCommits, 10, new() { Selected = bCommitSelected, Height = 320 });
        Section(ui, "Pipeline", "Stages of jobs; a stage sums up its jobs.");
        using (ui.ColumnBegin(new() { Gap = 10, Width = Size.Fill, MaxWidth = 920 }))
            ui.Pipeline("run", bPipelineStages, 4, bPipelineJobs, 10, new() { Selected = bPipelineSelected });
        Section(ui, "Problems", "Grouped by file; filter by severity or text, Enter opens.");
        using (ui.ColumnBegin(new() { Gap = 10, Width = Size.Fill, MaxWidth = 760 }))
            ui.Problems("problems", bDiagnostics, 7, new() { Height = 300 });
        Section(ui, "Key-value editor", "Type in the empty row to add a pair; the lock hides a value.");
        using (ui.ColumnBegin(new() { Gap = 10, Width = Size.Fill, MaxWidth = 680 }))
        {
            ui.Text("Headers", new() { Font = Font.Medium });
            ui.KvEditor("headers", bHeaderPairs, ref bHeaderCount, 12);
        }
        Section(ui, "Query builder", "Nested all and any groups.");
        using (ui.ColumnBegin(new() { Gap = 10, Width = Size.Fill, MaxWidth = 720 }))
        {
            ui.QueryBuilder("filter", bQueryFields, 4, bQueryNodes, ref bQueryCount, 16);
            ui.QueryText(bQueryFields, 4, bQueryNodes, bQueryCount, bQueryText);
            ui.Text(bQueryText.Text, new() { Size = 12, Color = Color.TextMuted });
        }
        Section(ui, "Schedule picker", "Writes a cron expression and shows the next runs.");
        using (ui.ColumnBegin(new() { Gap = 10, Width = Size.Fill, MaxWidth = 560 }))
            ui.SchedulePicker("backup", bCron, new() { Now = 1794211200, Utc = true, NextCount = 3 });
        Section(ui, "Keymap", "Shortcuts on a keyboard; keys bound twice are marked.");
        using (ui.ColumnBegin(new() { Gap = 10, Width = Size.Fill, MaxWidth = 900 }))
            ui.Keymap("keys", bKeyBindings, 24, new() { Categories = bKeyCategories, Mods = bKeymapMods });
    }

    private readonly TextBuffer bRegexPattern = new(128, @"(?<user>[\w.]+)@(?<host>[\w-]+)\.(com|org)");
    private readonly TextBuffer bRegexText = new(1024, "Contact maya.okafor@example.com or jonas@cui-lib.org for access.\nOld address: priya@legacy.net (no longer read).\nSupport: help@example.com");
    private static readonly TestItem[] bTests =
    [
        new() { Name = "layout", Depth = 0 },
        new() { Name = "row distributes extra space", Depth = 1, Status = JobStatus.Passed, Duration = .004f },
        new() { Name = "wraps to the next line", Depth = 1, Status = JobStatus.Passed, Duration = .006f },
        new() { Name = "mirrors under right to left", Depth = 1, Status = JobStatus.Failed, Duration = .012f, Message = "expected x = 412, got 388 (padding not mirrored)" },
        new() { Name = "text", Depth = 0 },
        new() { Name = "shapes Persian", Depth = 1, Status = JobStatus.Passed, Duration = .210f },
        new() { Name = "ellipsis keeps whole characters", Depth = 1, Status = JobStatus.Passed, Duration = .003f },
        new() { Name = "bidi brackets", Depth = 1, Status = JobStatus.Skipped, Duration = -1 },
        new() { Name = "renderer", Depth = 0 }, new() { Name = "software", Depth = 1 },
        new() { Name = "rounded corners", Depth = 2, Status = JobStatus.Passed, Duration = .031f },
        new() { Name = "soft shadows", Depth = 2, Status = JobStatus.Running, Duration = -1 },
        new() { Name = "OpenGL", Depth = 1 },
        new() { Name = "atlas growth", Depth = 2, Status = JobStatus.Pending, Duration = -1 },
        new() { Name = "slow gradient path", Depth = 2, Status = JobStatus.Passed, Duration = 1.42f }
    ];
    private static readonly string[] bStackSource1 = ["function total(cart) {", "  let sum = 0;", "  for (const item of cart.items) {", "    sum += item.price * item.qty;", "  }", "  return sum;"];
    private static readonly string[] bStackSource2 = ["export function checkout(user) {", "  const cart = carts.get(user.id);", "  const amount = total(cart);", "  return charge(user, amount);"];
    private static readonly StackFrame[] bStackFrames =
    [
        new() { Function = "total", File = "src/cart.js", Line = 3, Column = 29, Source = bStackSource1, SourceFirst = 1 },
        new() { Function = "checkout", File = "src/checkout.js", Line = 3, Column = 18, Source = bStackSource2, SourceFirst = 1 },
        new() { Function = "handle", File = "node_modules/express/lib/router/layer.js", Line = 95, Column = 5, External = true },
        new() { Function = "next", File = "node_modules/express/lib/router/route.js", Line = 149, Column = 13, External = true },
        new() { Function = "dispatch", File = "node_modules/express/lib/router/index.js", Line = 284, Column = 15, External = true },
        new() { Function = "onRequest", File = "src/server.js", Line = 41, Column = 9 }
    ];
    private int bRequestMethod = 1, bRequestParamCount = -1;
    private readonly TextBuffer bRequestUrl = new(256, "https://api.example.com/v1/orders?status=open&limit=20");
    private readonly KvPair[] bRequestParams = new KvPair[8];
    private readonly float[] bUptimeDays = new float[360], bHistogramInfo = new float[96], bHistogramWarning = new float[96], bHistogramError = new float[96];
    private readonly UptimeService[] bUptimeServices = new UptimeService[4];
    private readonly HistogramSeries[] bHistogramSeries = new HistogramSeries[3];
    private bool bDevDataInitialized;
    private double bHistogramStart = 1794240000 + 3600 * 5, bHistogramEnd = 1794240000 + 3600 * 7.5;
    private static readonly CoverageFile[] bCoverageFiles =
    [
        new() { Path = "src/core/layout.c", Lines = 1840, LinesHit = 1702, Branches = 620, BranchesHit = 541, Functions = 88, FunctionsHit = 86 },
        new() { Path = "src/core/state.c", Lines = 420, LinesHit = 401, Branches = 96, BranchesHit = 90, Functions = 31, FunctionsHit = 31 },
        new() { Path = "src/core/draw.c", Lines = 960, LinesHit = 610, Branches = 300, BranchesHit = 160, Functions = 54, FunctionsHit = 40 },
        new() { Path = "src/text/shape.c", Lines = 2210, LinesHit = 1490, Branches = 802, BranchesHit = 411, Functions = 120, FunctionsHit = 88 },
        new() { Path = "src/text/atlas.c", Lines = 640, LinesHit = 598, Branches = 140, BranchesHit = 131, Functions = 30, FunctionsHit = 30 },
        new() { Path = "src/render/gl.c", Lines = 1300, LinesHit = 410, Branches = 260, BranchesHit = 60, Functions = 70, FunctionsHit = 22 },
        new() { Path = "src/render/software.c", Lines = 2400, LinesHit = 2210, Branches = 900, BranchesHit = 799, Functions = 140, FunctionsHit = 136 },
        new() { Path = "src/main.c", Lines = 120, LinesHit = 120, Branches = 10, BranchesHit = 10, Functions = 4, FunctionsHit = 4 }
    ];
    private static readonly string[] bBlameLines =
    [
        "#include \"cui_internal.h\"", "", "/* the width a component asks for */", "static float measure(cui_node *n) {", "    float w = n->pad[1] + n->pad[3];",
        "    for (int c = n->first; c >= 0; c = next(c))", "        w += width_of(c) + n->gap;", "    return w - n->gap;", "}", "",
        "void cui__layout(cui_ctx *ui) {", "    for (int i = 0; i < ui->node_count; i++)", "        measure(&ui->nodes[i]);", "    place(ui, 0);  /* the root */", "}"
    ];
    private static readonly int[] bLineCommits = [0, 0, 1, 1, 2, 2, 2, 1, 1, 0, 3, 3, 4, 3, 3];
    private static readonly BlameCommit[] bBlameCommits =
    [
        new() { Hash = "a1b2c3d4e5", Author = "Maya Okafor", Summary = "First layout pass", Time = 1730000000 },
        new() { Hash = "b2c3d4e5f6", Author = "Jonas Berg", Summary = "Measure: count padding once", Time = 1760000000 },
        new() { Hash = "c3d4e5f6a7", Author = "Priya Nair", Summary = "Gaps between children", Time = 1790000000 },
        new() { Hash = "d4e5f6a7b8", Author = "Omid Saadat", Summary = "Layout entry point", Time = 1793000000 },
        new() { Hash = "e5f6a7b8c9", Author = "Aiko Tanaka", Summary = "Measure every node", Time = 1794100000 }
    ];
    private readonly FeatureFlag[] bFeatureFlags =
    [
        BFlag("new-checkout", "Single-page checkout with saved cards", true, true, false, 0, 25, 0),
        BFlag("dark-mode-default", "Start new accounts in dark mode", true, false, false),
        BFlag("search-v2", "Ranked search with typo tolerance", true, true, true, 0, 100, 5),
        BFlag("export-pdf", "Export reports as PDF", false, false, false)
    ];
    private static FeatureFlag BFlag(string key, string description, bool development, bool staging, bool production, int developmentShare = 0, int stagingShare = 0, int productionShare = 0) =>
        new() { Key = key, Description = description, On = new bool[] { development, staging, production, false, false, false, false, false },
            Rollout = new int[] { developmentShare, stagingShare, productionShare, 0, 0, 0, 0, 0 } };
    private static readonly string[] bEnvironments = ["Development", "Staging", "Production"];
    private static readonly ApiParam[] bApiUserParams =
    [
        new() { Name = "id", In = "path", Type = "string", Required = true, Description = "The user's id" },
        new() { Name = "fields", In = "query", Type = "string", Required = false, Description = "Comma separated fields to return" }
    ];
    private static readonly ApiParam[] bApiListParams =
    [
        new() { Name = "limit", In = "query", Type = "integer", Required = false, Description = "At most this many, default 20" },
        new() { Name = "cursor", In = "query", Type = "string", Required = false, Description = "From a previous page" }
    ];
    private static readonly ApiEndpoint[] bApiEndpoints =
    [
        new() { Method = "GET", Path = "/users", Summary = "List users", Tag = "Users", Params = bApiListParams },
        new() { Method = "POST", Path = "/users", Summary = "Create a user", Tag = "Users" },
        new() { Method = "GET", Path = "/users/{id}", Summary = "Get a user", Tag = "Users", Params = bApiUserParams },
        new() { Method = "PATCH", Path = "/users/{id}", Summary = "Update a user", Tag = "Users", Params = bApiUserParams.AsMemory(0, 1) },
        new() { Method = "DELETE", Path = "/users/{id}", Summary = "Delete a user", Tag = "Users", Params = bApiUserParams.AsMemory(0, 1) },
        new() { Method = "GET", Path = "/orders", Summary = "List orders", Tag = "Orders", Params = bApiListParams },
        new() { Method = "PUT", Path = "/orders/{id}/status", Summary = "Set an order's status", Tag = "Orders" },
        new() { Method = "GET", Path = "/v1/orders", Summary = "List orders (old)", Tag = "Orders", Deprecated = true }
    ];
    private readonly State<int> bApiSelected = new(2);

    private void BInitializeDevData()
    {
        if (bDevDataInitialized) return;
        for (int i = 0; i < 90; i++)
        {
            bUptimeDays[i] = bUptimeDays[90 + i] = bUptimeDays[180 + i] = 1;
            bUptimeDays[270 + i] = i < 20 ? -1 : 1;
            if (i % 17 == 5) bUptimeDays[i] = .9985f;
            if (i == 61) bUptimeDays[i] = .962f;
            if (i == 44) bUptimeDays[90 + i] = .93f;
            if (i % 23 == 9) bUptimeDays[180 + i] = .995f;
            if (i == 88) bUptimeDays[180 + i] = .97f;
        }
        bUptimeServices[0] = new() { Name = "API", Days = bUptimeDays.AsMemory(0, 90) };
        bUptimeServices[1] = new() { Name = "Website", Days = bUptimeDays.AsMemory(90, 90) };
        bUptimeServices[2] = new() { Name = "Database", Days = bUptimeDays.AsMemory(180, 90) };
        bUptimeServices[3] = new() { Name = "CDN", Days = bUptimeDays.AsMemory(270, 90) };
        for (int i = 0; i < 96; i++)
        {
            double x = i / 96.0;
            bHistogramInfo[i] = (float)(120 + 80 * Math.Sin(x * 6.28 * 2) + i * 37 % 23);
            bHistogramWarning[i] = 12 + i * 13 % 9 + (i > 60 && i < 70 ? 40 : 0);
            bHistogramError[i] = i * 7 % 5 + (i > 62 && i < 67 ? 30 : 0);
        }
        bHistogramSeries[0] = new() { Name = "Info", Counts = bHistogramInfo, Tone = Tone.Info };
        bHistogramSeries[1] = new() { Name = "Warning", Counts = bHistogramWarning, Tone = Tone.Warning };
        bHistogramSeries[2] = new() { Name = "Error", Counts = bHistogramError, Tone = Tone.Danger };
        bDevDataInitialized = true;
    }

    private void BMoreDeveloperTools(Ui ui)
    {
        Section(ui, "Test explorer", "Suites fold; hover a row to run it; filter by status or name.");
        using (ui.ColumnBegin(new() { Gap = 10, Width = Size.Fill, MaxWidth = 760 }))
            ui.TestExplorer("tests", bTests, 15, new() { Height = 500 });
        Section(ui, "Stack trace", "Library frames fold; a frame with source opens; copy the trace.");
        using (ui.ColumnBegin(new() { Gap = 10, Width = Size.Fill, MaxWidth = 760 }))
            ui.StackTrace("trace", bStackFrames, 6, new() { Title = "TypeError", Message = "Cannot read properties of undefined (reading 'items')", Lang = "js" });
        Section(ui, "Blame", "Hover the gutter to mark a commit's lines; click it to open the commit.");
        using (ui.ColumnBegin(new() { Gap = 10, Width = Size.Fill, MaxWidth = 900 }))
            ui.Blame("blame", bBlameLines, bLineCommits, 15, bBlameCommits, 5, new() { Lang = "c", Now = 1794211200, Height = 320, FirstLine = 120 });
        Section(ui, "Coverage", "Folders sum up their files; click a column to sort.");
        using (ui.ColumnBegin(new() { Gap = 10, Width = Size.Fill, MaxWidth = 820 }))
            ui.Coverage("cov", bCoverageFiles, 8, new() { Height = 372 });
        Section(ui, "Regex tester", "Matches and groups are marked as you type.");
        using (ui.ColumnBegin(new() { Gap = 10, Width = Size.Fill, MaxWidth = 760 }))
            ui.RegexTester("rx", bRegexPattern, bRegexText, new() { Height = 110 });
        Section(ui, "HTTP request bar", "The parameters table and the URL stay in step.");
        if (bRequestParamCount < 0) bRequestParamCount = ui.UrlParams(bRequestUrl.Text, bRequestParams);
        using (ui.ColumnBegin(new() { Gap = 12, Width = Size.Fill, MaxWidth = 760 }))
        {
            RequestBarResult request = ui.RequestBar("req", ref bRequestMethod, bRequestUrl);
            if (request.Changed) bRequestParamCount = ui.UrlParams(bRequestUrl.Text, bRequestParams);
            if (ui.KvEditor("params", bRequestParams, ref bRequestParamCount, 8)) ui.UrlSetParams(bRequestUrl, bRequestParams, bRequestParamCount);
            ui.ResponseStatus(201, .142, 18400);
        }
        Section(ui, "API explorer", "Click an endpoint for its parameters.");
        using (ui.ColumnBegin(new() { Gap = 10, Width = Size.Fill, MaxWidth = 820 }))
            ui.ApiExplorer("api", bApiEndpoints, 8, new() { Selected = bApiSelected });
        Section(ui, "Feature flags", "A switch per environment; click a share to change it.");
        using (ui.ColumnBegin(new() { Gap = 10, Width = Size.Fill, MaxWidth = 860 }))
            ui.FeatureFlags("flags", bFeatureFlags, 4, bEnvironments, 3);
        BInitializeDevData();
        Section(ui, "Uptime", "One bar per day; hover for the day.");
        using (ui.ColumnBegin(new() { Gap = 10, Width = Size.Fill, MaxWidth = 860 }))
            ui.Uptime("status", bUptimeServices, 4, new() { LastDay = new() { Year = 2026, Month = 11, Day = 9 } });
        Section(ui, "Time histogram", "Drag to select a range; click to clear it.");
        using (ui.ColumnBegin(new() { Gap = 10, Width = Size.Fill, MaxWidth = 900 }))
            ui.TimeHistogram("logs", 1794240000, 900, 96, bHistogramSeries, 3, ref bHistogramStart, ref bHistogramEnd, new() { Utc = true, Height = 150 });
    }

    private readonly double[] bCups = [8, 10.75, 12.25, 0, 0, 0, 0, 0];
    private int bCupCount = 3;
    private readonly State<int> bSelectedCup = new(2);
    private readonly float[] bCaffeineCurves = new float[8 * 97];
    private readonly DotSeries[] bCaffeineSeries = new DotSeries[8];
    private readonly ChartAnnotation[] bCaffeineNotes = new ChartAnnotation[3];

    private static float BDose(double t)
    {
        if (t <= 0) return 0;
        double ke = Math.Log(2) / 5, ka = Math.Log(2) / .25;
        return (float)(95 * ka / (ka - ke) * (Math.Exp(-ke * t) - Math.Exp(-ka * t)));
    }

    private void BInstruments(Ui ui)
    {
        double peak = 0, peakAt = 6, bed = 0;
        for (int k = 0; k < 97; k++)
        {
            double at = 6 + 24.0 * k / 96, total = 0;
            for (int c = 0; c < bCupCount; c++)
            {
                float dose = BDose(at - bCups[c]);
                bCaffeineCurves[c * 97 + k] = dose;
                total += dose;
            }
            if (total > peak) { peak = total; peakAt = at; }
        }
        for (int c = 0; c < bCupCount; c++) bed += BDose(23 - bCups[c]);
        string peakMg = peak.ToString("F0", CultureInfo.InvariantCulture);
        string peakText = "PEAK " + peakMg + " MG";
        // C's lround rounds halfway away from zero; the hour is deliberately
        // not carried when the minute wraps, matching the upstream readout.
        string peakTime = string.Create(CultureInfo.InvariantCulture, $"{(int)peakAt:00}:{(int)Math.Round((peakAt - (int)peakAt) * 60, MidpointRounding.AwayFromZero) % 60:00}");
        string big = bed.ToString("F0", CultureInfo.InvariantCulture);
        string bedText = big + " MG STILL IN YOU";
        Section(ui, "Readouts", "A label, a value with its unit, and a line of context, in the monospaced font.");
        using (ui.RowBegin(new() { Gap = 40, Wrap = true, Align = Align.End }))
        {
            ui.Readout("AT BED", big, new() { Unit = "MG", Size = ControlSize.Lg, Accent = bed > 50, Detail = bed > 50 ? "OVER THE 50 MG LINE" : "UNDER THE 50 MG LINE" });
            ui.Readout("PEAK", peakMg, new() { Unit = "MG", Detail = peakTime });
            ui.Readout("CUPS", bCupCount == 1 ? "1" : bCupCount == 2 ? "2" : bCupCount == 3 ? "3" : bCupCount == 4 ? "4" : "5+", new() { Size = ControlSize.Sm, Detail = "95 MG EACH" });
        }
        Section(ui, "Dot-matrix chart and event track", "Drag a cup along the axis, click the chart to add one, double-click a cup to remove it.");
        for (int c = 0; c < bCupCount; c++)
            bCaffeineSeries[c] = new() { Values = bCaffeineCurves.AsMemory(c * 97, 97), Accent = c == bSelectedCup.Value };
        bCaffeineNotes[0] = new() { Kind = AnnotationKind.Y, X = 0, Y = 50, Label = "50" };
        bCaffeineNotes[1] = new() { Kind = AnnotationKind.X, X = 23, Y = 0, Label = "BED 23:00", Detail = bedText };
        bCaffeineNotes[2] = new() { Kind = AnnotationKind.X, X = peakAt, Y = 0, Label = peakText, Detail = peakTime };
        using (ui.ColumnBegin(new() { Width = Size.Fill, Gap = 0 }))
        {
            DotChartResult chart = ui.DotChart("caffeine", bCaffeineSeries.AsSpan(0, bCupCount), bCupCount, 97,
                new() { Height = 240, XMin = 6, XMax = 30, XUnits = AxisUnits.Hours, Min = 0, Max = 260, Unit = "MG", NoAxis = true,
                    Annotations = bCaffeineNotes.AsMemory(0, bCupCount != 0 ? 3 : 1) });
            if (chart.Clicked && bCupCount < 8)
            {
                double t = Math.Floor(chart.X * 4 + .5) / 4;
                int at = bCupCount++;
                while (at > 0 && bCups[at - 1] > t) { bCups[at] = bCups[at - 1]; at--; }
                bCups[at] = t;
                bSelectedCup.Value = at;
            }
            ui.EventTrack("cups", bCups, ref bCupCount, 8, 6, 30, new() { Step = .25, Units = AxisUnits.Hours, LabelStep = 6, Selected = bSelectedCup });
        }
        Section(ui, "Dot-matrix display", "Text in a 5 by 7 dot font; a long line can scroll.");
        ui.DotDisplay(bed > 50 ? "OVER THE LINE AT BEDTIME" : "UNDER THE LINE AT BEDTIME", new() { Pitch = 3, Accent = bed > 50 });
    }

    partial void DisposeExtendedB()
    {
        bDraft.Dispose();
        bFolder.Dispose();
        bChosenFile.Dispose();
        bQueryText.Dispose();
        bCron.Dispose();
        bRegexPattern.Dispose();
        bRegexText.Dispose();
        bRequestUrl.Dispose();
    }

    private readonly float[] bTraffic = new float[7 * 24], bDays = new float[364];
    private bool bChartsInitialized;
    private readonly Random bRandom = new(1);
    private static readonly string[] bWeekdays = ["Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun"];
    // Empty entries represent upstream's NULL optional labels. Exact layout and
    // tooltip fallback require heatmap-specific marshalling to omit these entries.
    private static readonly string?[] bHours = ["0h", null, null, "3h", null, null, "6h", null, null, "9h", null, null, "12h", null, null, "15h", null, null, "18h", null, null, "21h", null, null];
    private static readonly VizItem[] bModules =
    [
        new() { Label = "Layout", Value = 55 }, new() { Label = "Text", Value = 42 },
        new() { Label = "Draw", Value = 35 }, new() { Label = "Controls", Value = 31 },
        new() { Label = "Charts", Value = 23 }, new() { Label = "Overlays", Value = 19 },
        new() { Label = "Platform", Value = 16 }, new() { Label = "Theme", Value = 9 },
        new() { Label = "Icons", Value = 7 }, new() { Label = "Grid", Value = 6 }
    ];
    private static readonly Series[] bRadar =
    [
        new() { Name = "1.0", Values = new float[] { 82, 64, 90, 58, 75, 88 } },
        new() { Name = "1.1", Values = new float[] { 70, 85, 72, 80, 60, 79 } }
    ];
    private static readonly string[] bRadarAxes = ["Speed", "Size", "Polish", "Docs", "Platforms", "Tests"];
    private static readonly VizItem[] bFunnel =
    [
        new() { Label = "Visited", Value = 12840 }, new() { Label = "Signed up", Value = 4210 },
        new() { Label = "Activated", Value = 2380 }, new() { Label = "Subscribed", Value = 860 }
    ];
    private static readonly VizItem[] bSplit =
    [
        new() { Label = "Layout", Value = 38 }, new() { Label = "Text", Value = 24 },
        new() { Label = "Paint", Value = 21 }, new() { Label = "Input", Value = 9 }, new() { Label = "Other", Value = 8 }
    ];
    private static readonly VizItem[] bPlatforms =
    [
        new() { Label = "Windows", Value = 46 }, new() { Label = "macOS", Value = 31 }, new() { Label = "Linux", Value = 23 }
    ];
    private static readonly string[] bSankeyNodes = ["Search", "Social", "Referral", "Sign up", "Bounced", "Trial", "Paid", "Churned"];
    private static readonly SankeyLink[] bSankeyLinks =
    [
        new() { From = 0, To = 3, Value = 420 }, new() { From = 1, To = 3, Value = 260 },
        new() { From = 2, To = 3, Value = 180 }, new() { From = 0, To = 4, Value = 300 },
        new() { From = 1, To = 4, Value = 220 }, new() { From = 3, To = 5, Value = 860 },
        new() { From = 5, To = 6, Value = 580 }, new() { From = 5, To = 7, Value = 280 }
    ];
    private readonly VizItem[] bChartPages =
    [
        new() { Label = "/docs", Value = 4820 }, new() { Label = "/", Value = 3920 },
        new() { Label = "/gallery", Value = 2410 }, new() { Label = "/blog/fast-ui-in-c", Value = 1830 },
        new() { Label = "/download", Value = 1210 }, new() { Label = "/pricing", Value = 640 }
    ];
    private static readonly VizItem[] bRings =
    [
        new() { Label = "Move", Value = .82f }, new() { Label = "Exercise", Value = 1.24f }, new() { Label = "Stand", Value = .58f }
    ];
    private readonly Vec2[] bScatterA = new Vec2[40], bScatterB = new Vec2[40];
    private readonly float[] bScatterSizesA = new float[40], bScatterSizesB = new float[40];
    private readonly ScatterSeries[] bScatter = new ScatterSeries[2];
    private static readonly GaugeBand[] bGaugeBands =
    [
        new() { To = 60, Tone = Tone.Success }, new() { To = 85, Tone = Tone.Warning }, new() { To = 100, Tone = Tone.Danger }
    ];
    private readonly Candle[] bCandles = new Candle[2000];

    private void BInitializeCharts(Ui ui)
    {
        if (bChartsInitialized) return;
        for (int d = 0; d < 7; d++)
            for (int h = 0; h < 24; h++)
            {
                float work = d < 5 ? 1 : .45f;
                float day = MathF.Exp(-MathF.Pow((h - 13.5f) / 4.2f, 2)) * 90 * work
                    + MathF.Exp(-MathF.Pow((h - 20.5f) / 2, 2)) * 35;
                bTraffic[d * 24 + h] = day + (d * 31 + h * 17) % 11;
            }
        uint s = 7;
        int today = ui.DateToDays(ui.DateToday());
        for (int k = 0; k < 364; k++)
        {
            s = unchecked(s * 1103515245u + 12345u);
            int wd = (k + ((today - 363) % 7 + 10) % 7) % 7;
            float basis = (wd >= 5 ? .6f : 3.2f) * (.6f + .4f * MathF.Sin(k / 40f) * MathF.Sin(k / 40f));
            float r = (s >> 16) % 1000 / 1000f;
            bDays[k] = r < .18f ? 0 : MathF.Floor(basis * r * 4);
        }
        for (int k = 0; k < 40; k++)
        {
            float n = 200 + k * 95f;
            bScatterA[k] = new(n, .08f + n * .00006f + (k * 37 % 13) * .004f);
            bScatterB[k] = new(n * .9f + 150, .15f + n * .00011f + (k * 53 % 17) * .006f);
            bScatterSizesA[k] = 10 + k * 7 % 40;
            bScatterSizesB[k] = 20 + k * 11 % 50;
        }
        bScatter[0] = new() { Name = "Software", Points = bScatterA, Sizes = bScatterSizesA };
        bScatter[1] = new() { Name = "OpenGL", Points = bScatterB, Sizes = bScatterSizesB };
        float price = 64;
        for (int k = 0; k < bCandles.Length; k++)
        {
            float o = price, c = o * (1 + MathF.Sin(k * .37f) * .012f + MathF.Cos(k * .043f) * .008f);
            bCandles[k] = new() { Open = o, High = MathF.Max(o, c) * 1.006f, Low = MathF.Min(o, c) * .994f, Close = c, Volume = 2000 + (k * 7919 % 900) * 10f };
            price = c;
        }
        bChartsInitialized = true;
    }

    private void BMoreCharts(Ui ui)
    {
        BInitializeCharts(ui);
        using (ui.RowBegin(new() { Gap = 24, Align = Align.Start, Width = Size.Fill }))
        using (ui.ColumnBegin(new() { Width = Size.Grow(1), Gap = 10 }))
        {
            Section(ui, "Heatmap", "Requests by weekday and hour.");
            using var card = ui.CardBegin(new() { Pad = 18, Width = Size.Fill });
            ui.Heatmap("traffic", bTraffic, 7, 24, new() { RowLabels = bWeekdays, ColLabels = bHours, Format = "%.0f req" });
        }
        Section(ui, "Calendar heatmap", "A year of activity, one square a day.");
        using (ui.CardBegin(new() { Pad = 18 }))
            ui.CalendarHeatmap("activity", ui.DateFromDays(ui.DateToDays(ui.DateToday()) - 363), bDays, 364, new() { Unit = "contributions" });
        using (ui.RowBegin(new() { Gap = 24, Align = Align.Start, Width = Size.Fill }))
        {
            using (ui.ColumnBegin(new() { Width = Size.Grow(1.3f), Gap = 10 }))
            {
                Section(ui, "Treemap", "Bundle size by module.");
                ui.Treemap("bundle", bModules, 10, new() { Height = 280, Format = "%.0f KB" });
            }
            using (ui.ColumnBegin(new() { Width = Size.Grow(1), Gap = 10, Align = Align.Center }))
            {
                Section(ui, "Radar", "Two releases across six qualities.");
                ui.RadarChart("radar", bRadar, 2, bRadarAxes, 6, new() { Size = 220, Max = 100 });
            }
        }
        using (ui.RowBegin(new() { Gap = 24, Align = Align.Start, Width = Size.Fill }))
        {
            using (ui.ColumnBegin(new() { Width = Size.Grow(1), Gap = 10 }))
            {
                Section(ui, "Funnel", "Sign up conversion.");
                ui.Funnel("funnel", bFunnel, 4);
            }
            using (ui.ColumnBegin(new() { Width = Size.Grow(1), Gap = 10 }))
            {
                Section(ui, "Split bar", "Where the time goes.");
                ui.SplitBar("split", bSplit, 5, new() { Format = "%.0f ms" });
                ui.Gap(18);
                Section(ui, "Waffle", "Platforms of the last 100 installs.");
                ui.Waffle("waffle", bPlatforms, 3, new() { Cell = 14 });
            }
        }
        Section(ui, "Sankey", "Where sign ups come from and where they end up.");
        using (ui.CardBegin(new() { Pad = 18, Width = Size.Fill }))
            ui.Sankey("flow", bSankeyNodes, 8, bSankeyLinks, 8, new() { Height = 280 });
        using (ui.RowBegin(new() { Gap = 24, Align = Align.Start, Width = Size.Fill }))
        {
            using (ui.ColumnBegin(new() { Width = Size.Grow(1.4f), Gap = 10 }))
            {
                Section(ui, "Bar list", "Ranked, keyed by label: press Shuffle and the rows slide to their new rank.");
                ui.BarList("pages", bChartPages, 6);
                if (ui.Button("Shuffle", new() { Size = ControlSize.Sm, Icon = Icons.Shuffle, Variant = ButtonVariant.Outline }))
                    for (int k = 0; k < 6; k++) bChartPages[k].Value = 500 + bRandom.Next(0, 32768) % 5000;
            }
            using (ui.ColumnBegin(new() { Width = Size.Grow(1), Gap = 10, Align = Align.Center }))
            {
                Section(ui, "Activity rings", "Past one, a ring keeps winding.");
                ui.ActivityRings("rings", bRings, 3, new() { Size = 170, Center = "82%", Caption = "of goal" });
            }
        }
        Section(ui, "Scatter", "Frame time against node count, sized by draw calls.");
        using (ui.CardBegin(new() { Pad = 18, Width = Size.Fill }))
            ui.ScatterChart("scatter", bScatter, 2, new() { Height = 300, XLabel = "Nodes", YLabel = "Frame time (ms)", YFormat = "%.2f" });
        using (ui.CardBegin(new() { Pad = 18, Width = Size.Fill, Dir = Dir.Row, Wrap = true, Gap = 24, Align = Align.Center }))
        {
            ui.RadialGauge("cpu", 64, 0, 100, new() { Unit = "%", Label = "CPU", Bands = bGaugeBands, Size = 180 });
            ui.RadialGauge("temp", 71, 20, 100, new() { Unit = "C", Label = "GPU temperature", ShowTarget = true, Target = 85, Size = 180 });
            ui.RadialGauge("mem", .62f * 32, 0, 32, new() { Format = "%.1f", Unit = "GB", Label = "Memory", NoNeedle = true, Ticks = -1, Size = 150 });
        }
        using (ui.CardBegin(new() { Pad = 18, Width = Size.Fill, Gap = 10 }))
        {
            ui.Label("Price, 2,000 periods");
            ui.Candlestick("price", bCandles, 2000, new() { Height = 280 });
        }
    }
}
