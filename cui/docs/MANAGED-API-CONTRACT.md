# Managed CUI API contract

Namespace `Cui.Managed`, project `src/ManagedCui/ManagedCui.csproj`. The wrapper references only the matching generated core; it never references the translated gallery. Select raw with `-p:CuiVariant=Raw`, or override `CuiProject` with an ordinary project path.

## Frozen naming and data contract

The authoritative public value/option definitions are `src/ManagedCui/Types.cs`; exact widget signatures are below and in `Ui.Api.cs`. Names derive directly from C: remove `cui_`, convert snake case to PascalCase, remove `_ex`. Options keep the `Opts` suffix. Fields become PascalCase properties. Zero/default options preserve upstream defaults. Counts remain explicit on widget methods (spans validate them); option array counts derive from memory lengths. No raw context, addresses, function pointers or raw translated structs cross this boundary. Top-level string arguments and array entries must be non-null. Optional string fields in options retain the C null/default convention; all supplied strings reject embedded NUL.

`TextBuffer(int capacity, string text = "")` owns persistent UTF-8 edit storage; capacity includes the terminator. Use `Text`, `Set(string)`, `Set(ReadOnlySpan<char>)`, `CopyUtf8To(Span<byte>)`, and `Dispose`. Pass it directly to Input/Textarea/Search/InlineEdit/ChatComposer/RegexTester and other editors; omit C buffer capacity arguments.

Mutable scalar pointers inside options become `State<T>` (construct once; read/write `Value`), e.g. `new State<int>(-1)` for Selected and `new State<TableSort>()` for Sort. Mutable option arrays are `Memory<T>`, immutable arrays `ReadOnlyMemory<T>`. Function arguments use spans, except retained reorder order uses `Memory<int>`. Ordinary scalar in/out arguments use `ref T`. Build controls with structs, cached arrays and buffers; there are no widget objects or per-widget delegates.

Container Begin methods return `UiScope`, with `IsOpen` and `Result` (original event/id). Use `using var scope = ui.MenuBegin(...); if (scope.IsOpen) { ... }`. Every successful begin needs disposal, including ordinary Box/Row/Column/Card/Table/Carousel/Kanban containers. Scope copies share an id/generation; repeated disposal is harmless, out-of-order disposal fails, and frame completion unwinds outstanding scopes. Explicit End methods remain available with checked nesting. `Row`, `Column`, `Card`, `Scroll`, `Table`, `Popover`, `Dialog`, `Sheet`, `Menu`, `ContextMenu` aliases remove Begin for convenience.

`OffscreenSurface(width,height, scale=1)` owns `Ui`, software renderer and RGBA pixels. `BeginFrame(float dt)` returns a disposable frame; `Render()` finishes it; `Pixels`/`CopyPixelsTo` expose safe bytes. `Application(ApplicationOptions)` owns a desktop window; `NextFrame`, `Ui`, `Render`, `Quit`, `CapturePixels` and `Dispose` follow the upstream app loop. All context/resource/frame calls require the owner thread. No nested frames, stale scopes, cross-context Id/Texture/State, widget calls outside a frame, or calls after disposal are accepted. Read-only context queries also work outside frames. Offscreen scale must be finite, positive and at most 64; pixel dimensions and buffer byte counts are checked.

`Texture` is a context-owned IDisposable resource, created through `Ui.ImageCreate(width,height,ReadOnlySpan<byte>)` or `Ui.ImageLoad`. `TerminalBuffer(textCapacity,lineCapacity)` owns terminal parser storage; `Write(string/ReadOnlySpan<byte>)`, `Clear`, and `Ui.Terminal` preserve upstream terminal behavior. `Ui.CreateCanvasPainter(Action<PaintContext>)` roots a reusable `CanvasPainter`; `Ui.Canvas(id,width,height,painter)` invokes it after layout. PaintContext exposes safe draw methods and expires on callback exit. FileBrowserOpts.List and FilePickerOpts.List accept `FileListHandler(string path, Span<FileEntry> entries, bool hidden)`; return the written count. Callback exceptions are contained at the raw boundary and surfaced after the engine returns.

## Retention audit and implementation rules

Audited upstream `cui_core.c` begin/end frame, `cui_internal.h` nodes/build state, `cui_textedit.c`, `cui_widgets.c`, `cui_data.c`, `cui_grid.c`, `cui_extras.c`, `cui_charts.c`, `cui_instrument.c`, `cui_devtools.c`, `cui_devpanels.c`, `cui_devviews.c`, `cui_terminal.c`, `cui_overlay.c`, `cui_files.c` and gallery source consumers. `cui_end_frame` runs widgets-end, layout, paint, state GC and font upload. Nodes and paint data can reference inputs after the widget returns. Tables retain columns in their build stack; grids retain columns and options; reorder retains its writable order until end. Editors consume mutable buffers synchronously; textedit paint duplicates the text but can retain option strings. Terminal paint consumes parser storage later. The wrapper conservatively leases all edit/terminal owners until native begin discards previous frame pointers. Carousel retains its writable index until end; NodeEditor retains its view until end, so both require State<T>. Terminal paint consumes its parser buffer later. File-list callback registrations are context roots, reused for identical delegates; the safe bridge supports up to 65,536 entries, including upstream’s 20,000 default. Callback exceptions are captured, the native callback returns failure, and the exception is rethrown after the outer engine call. Widget and lifecycle reentry from callbacks is rejected; paint callbacks receive an expiring drawing capability. Toasts copy title/description/action into context-owned fixed arrays; tooltip/text and most chart/instrument payloads copy to C frame memory, but the wrapper conservatively retains *all* marshalled inputs through end-frame and renderer consumption. Context-owned settings/callback roots outlive frames. Image textures are context resources.

Implementation: stable segmented native frame arena, reset only at the next frame after rendering, with a configurable byte cap (default 16 MiB). UTF-8 strings, arrays and nested options copy into it. The arena never reallocates a live segment. Mutable option State slots and editable buffers have independent persistent ownership; memory-array journals synchronize after engine calls and scope ends. Disposed resources remain leased until the current frame completes. Edit/terminal backing storage stays leased through paint, renderer consumption and native begin of the next frame. Dispose marks an owner unusable immediately; its backing storage is released after the last lease. Context destruction drains current leases after native destruction. Callback roots remain alive through native destruction. Texture disposal drains at the next frame or context destruction, allowing repeat render/capture of the current draw data. Callback roots are bounded and remain stable until context destruction. No borrowed fixed block backs retained CUI input. Public spans can be stack-backed because CUI receives copies. Unicode uses UTF-8 with rejected embedded NUL and no truncation of editable text.

## Exact widget signatures

```csharp
public string? AccentName(Accent accent);
public UiScope AccordionBegin(string title, ref bool open);
public void AccordionEnd();
public int ActivityRings(string id, ReadOnlySpan<VizItem> rings, int count, RingsOpts opts = default);
public int AgentPlan(string id, ReadOnlySpan<PlanStep> steps, int count, AgentPlanOpts opts = default);
public void AgentStatus(string text, AgentStatusOpts opts = default);
public bool Alert(string title, AlertOpts opts = default);
public ApiExplorerResult ApiExplorer(string id, ReadOnlySpan<ApiEndpoint> endpoints, int count, ApiExplorerOpts opts = default);
public void Avatar(string name, AvatarOpts opts = default);
public void AvatarGroup(ReadOnlySpan<string> names, int count, int max_visible, float size);
public int AvatarUpload(string id, string name, ref Texture? image, float size);
public void Badge(string text, BadgeOpts opts = default);
public bool Banner(string text, ref bool open, BannerOpts opts = default);
public void BarChart(string id, ReadOnlySpan<Series> series, int count, ChartOpts opts = default);
public int BarList(string id, ReadOnlySpan<VizItem> items, int count, BarListOpts opts = default);
public BlameResult Blame(string id, ReadOnlySpan<string> lines, ReadOnlySpan<int> line_commit, int line_count, ReadOnlySpan<BlameCommit> commits, int commit_count, BlameOpts opts = default);
public UiScope BoxBegin(Box opts = default);
public void BoxEnd();
public int Breadcrumb(ReadOnlySpan<string> items, int count);
public bool Button(string label, ButtonOpts opts = default);
public UiScope ButtonGroupBegin();
public void ButtonGroupEnd();
public int CalendarHeatmap(string id, Date start, ReadOnlySpan<float> values, int days, CalendarHeatmapOpts opts = default);
public int Candlestick(string id, ReadOnlySpan<Candle> candles, int count, CandlestickOpts opts = default);
public UiScope CardBegin(CardOpts opts = default);
public void CardEnd();
public UiScope CardStackBegin(string id, int count, CardStackOpts opts = default);
public int CardStackEnd();
public UiScope CardStackItemBegin(int depth);
public void CardStackItemEnd();
public UiScope CarouselBegin(string id, int count, State<int> index, CarouselOpts opts = default);
public void CarouselEnd();
public UiScope CarouselSlideBegin();
public void CarouselSlideEnd();
public int ChatComposer(string id, TextBuffer buf, ComposerOpts opts = default);
public void ChatDivider(string label);
public void ChatMessage(string text, ChatOpts opts = default);
public bool Checkbox(string label, ref bool value, CheckOpts opts = default);
public int Checklist(string id, Span<CheckStep> steps, int count, ChecklistOpts opts = default);
public bool Chip(string label, ref bool selected, ChipOpts opts = default);
public bool Citation(int number, Source source);
public void CodeBlock(string id, string code, CodeOpts opts = default);
public Color ColorOf(Color c);
public bool ColorPicker(string id, ref Color value);
public UiScope ColumnBegin(Box opts = default);
public int CommandPalette(ref bool open, ReadOnlySpan<Command> commands, int count);
public int Comment(string id, string author, string text, CommentOpts opts = default);
public CommitGraphResult CommitGraph(string id, ReadOnlySpan<Commit> commits, int count, CommitGraphOpts opts = default);
public bool ConfirmButton(string label, ConfirmOpts opts = default);
public bool ConfirmPopover(string id, ConfirmPopoverOpts opts = default);
public UiScope ContextMenuBegin(string id);
public void ContextMenuEnd();
public void ContextMeter(string id, float used, float limit, ContextOpts opts = default);
public bool ControlTile(string label, ref bool on, TileOpts opts = default);
public bool CopyButton(string id, string text);
public void Countdown(string id, double seconds_left, CountdownOpts opts = default);
public void Counter(string id, double value, CounterOpts opts = default);
public CoverageResult Coverage(string id, ReadOnlySpan<CoverageFile> files, int count, CoverageOpts opts = default);
public bool Cropper(string id, Texture? tex, int image_w, int image_h, ref Rect crop, CropperOpts opts = default);
public UiScope DataGridBegin(string id, ReadOnlySpan<GridColumn> columns, int column_count, int row_count, ref int first, ref int last, GridOpts opts = default);
public void DataGridCell();
public void DataGridCellText(string text);
public bool DataGridEdit(TextBuffer buf);
public void DataGridEnd();
public bool DataGridRow(int row);
public Date DateFromDays(int days);
public bool DatePicker(string id, ref Date value, DateOpts opts = default);
public bool DateRangePicker(string id, ref DateRange range, DateRangeOpts opts = default);
public int DateToDays(Date d);
public Date DateToday();
public float DeltaTime();
public UiScope DialogBegin(string title, ref bool open, DialogOpts opts = default);
public void DialogEnd();
public UiScope DialogFooterBegin();
public void DialogFooterEnd();
public UiScope DockBegin(string id, DockOpts opts = default);
public void DockEnd();
public bool DockItem(string icon, string label, DockItemOpts opts = default);
public void DockSeparator();
public void DonutChart(string id, ReadOnlySpan<float> values, ReadOnlySpan<string> labels, int count, ChartOpts opts = default);
public DotChartResult DotChart(string id, ReadOnlySpan<DotSeries> series, int series_count, int count, DotChartOpts opts = default);
public void DotDisplay(string text, DotDisplayOpts opts = default);
public float DpiScale();
public void DrawArc(Vec2 center, float radius, float a0, float a1, float thickness, Color color);
public void DrawCircle(Vec2 center, float radius, Color color);
public void DrawEmoji(string utf8, int len, Rect r);
public void DrawIcon(string icon, Rect r, float size, Color color);
public void DrawImage(Texture? tex, Rect r, Rect uv, Corners radius, Color tint);
public void DrawLine(Vec2 a, Vec2 b, float thickness, Color color);
public void DrawPolygon(ReadOnlySpan<Vec2> points, int count, Color color);
public void DrawPolyline(ReadOnlySpan<Vec2> points, int count, float thickness, Color color, bool closed);
public void DrawRect(Rect r, Corners radius, Color fill);
public void DrawRectBorder(Rect r, Corners radius, Color fill, Color border, float border_width);
public void DrawRectGradient(Rect r, Corners radius, Color top, Color bottom);
public void DrawShadow(Rect r, Corners radius, float blur, float spread, Vec2 offset, Color color);
public void DrawText(Font font, float size, Vec2 pos, Color color, string text, int len);
public string? DroppedFile(int index);
public int Dropzone(string id, DropzoneOpts opts = default);
public void Emoji(string utf8, float size);
public bool EmojiFontLoadFile(string path);
public bool EmojiFontLoadSystem();
public string? EmojiPicker(string id, EmojiPickerOpts opts = default);
public bool EmptyState(string title, EmptyOpts opts = default);
public EventTrackResult EventTrack(string id, Span<double> times, ref int count, int capacity, double min, double max, EventTrackOpts opts = default);
public UiScope ExpandableCardBegin(string title, ref bool open, ExpandableCardOpts opts = default);
public void ExpandableCardEnd();
public bool ExpandableText(string id, string text, ExpandableTextOpts opts = default);
public bool Fader(string id, ref float value, float min, float max, FaderOpts opts = default);
public FeatureFlagsResult FeatureFlags(string id, Span<FeatureFlag> flags, int count, ReadOnlySpan<string> envs, int env_count, FeatureFlagsOpts opts = default);
public FileBrowserResult FileBrowser(string id, TextBuffer path, FileBrowserOpts opts = default);
public bool FilePicker(string id, ref bool open, TextBuffer @out, FilePickerOpts opts = default);
public bool FilterBar(string id, ReadOnlySpan<FilterField> fields, int count, Span<uint> selected, TextBuffer search);
public int Funnel(string id, ReadOnlySpan<VizItem> stages, int count, FunnelOpts opts = default);
public int Gantt(string id, Span<GanttTask> tasks, int count, Date origin, int days, GanttOpts opts = default);
public void Gap(float size);
public void Gauge(string label, float value, float size);
public Accent GetAccent();
public Rect GetRect(Id id);
public ThemeMode GetThemeMode();
public void GradientText(string text, GradientTextOpts opts = default);
public GraphResult GraphView(string id, ReadOnlySpan<GraphNode> nodes, int node_count, ReadOnlySpan<GraphEdge> edges, int edge_count, GraphOpts opts = default);
public void Heading(string text, int level);
public int Heatmap(string id, ReadOnlySpan<float> values, int rows, int cols, HeatmapOpts opts = default);
public bool HistogramSlider(string label, ReadOnlySpan<float> bins, int bin_count, ref float lo, ref float hi, float min, float max, HistogramOpts opts = default);
public bool HoldButton(string label, HoldOpts opts = default);
public UiScope HoverCardBegin(string id, HoverCardOpts opts = default);
public void HoverCardEnd();
public void Icon(string icon, float size, Color color);
public bool IconButton(string icon, ButtonOpts opts = default);
public bool ImageCompare(string id, Texture? before, Texture? after, ref float pos, CompareOpts opts = default);
public void Image(Texture? tex, ImageOpts opts = default);
public bool ImageViewer(string id, Texture? tex, int image_w, int image_h, ref ImageView view, ImageViewerOpts opts = default);
public int InlineEdit(string id, TextBuffer buf, InlineEditOpts opts = default);
public int Input(string id, TextBuffer buf, InputOpts opts = default);
public void InputFileDrop(ReadOnlySpan<string> paths, int count, float x, float y);
public void InputFileHover(bool hovering, float x, float y);
public void InputFocus(bool window_focused);
public void InputKey(Key key, bool down, int mods);
public void InputMouseButton(MouseButton button, bool down);
public void InputMouseLeave();
public void InputMouseMove(float x, float y);
public void InputScroll(float dx, float dy);
public void InputText(string utf8);
public bool JsonView(string id, string json, JsonOpts opts = default);
public UiScope KanbanBegin(string id);
public UiScope KanbanCardBegin(int card_id);
public void KanbanCardEnd();
public UiScope KanbanColumnBegin(string title, int card_count, KanbanColumnOpts opts = default);
public void KanbanColumnEnd();
public bool KanbanEnd(ref KanbanMove move);
public bool KeyDown(Key key);
public string? KeyName(Key key);
public bool KeyPressed(Key key);
public KeymapResult Keymap(string id, ReadOnlySpan<KeymapBinding> bindings, int count, KeymapOpts opts = default);
public bool Knob(string id, ref float value, float min, float max, KnobOpts opts = default);
public bool KvEditor(string id, Span<KvPair> pairs, ref int count, int capacity, KvEditorOpts opts = default);
public void Label(string text);
public bool LastHovered();
public Id LastId();
public Rect LastRect();
public bool LevelMeter(string id, ReadOnlySpan<float> levels, int channels, LevelMeterOpts opts = default);
public void LineChart(string id, ReadOnlySpan<Series> series, int count, ChartOpts opts = default);
public bool Link(string text);
public bool ListItem(string title, ListItemOpts opts = default);
public void Loader(LoaderOpts opts = default);
public Id MakeId(string str);
public UiScope MenuBegin(string label, MenuOpts opts = default);
public void MenuEnd();
public bool MenuItem(string label, MenuItemOpts opts = default);
public void MenuLabel(string text);
public void MenuSeparator();
public void Meter(string label, float used, float total, string unit);
public void Metric(string label, string value, MetricOpts opts = default);
public bool MoneyInput(string id, ref double value, MoneyOpts opts = default);
public MonthResult MonthCalendar(string id, ref Date month, ReadOnlySpan<MonthEvent> events, int count, MonthOpts opts = default);
public bool MouseClicked(MouseButton b);
public bool MouseDown(MouseButton b);
public Vec2 MousePos();
public bool MultiSelect(string id, ReadOnlySpan<string> items, int count, Span<bool> selected, MultiSelectOpts opts = default);
public void Muted(string text);
public bool NavItem(string label, NavOpts opts = default);
public void NavSection(string title);
public bool NeedsRedraw();
public float NextWakeup();
public UiScope NodeBegin(int node_id, ref float x, ref float y, string title, NodeOpts opts = default);
public UiScope NodeEditorBegin(string id, State<NodeView> view, NodeEditorOpts opts = default);
public void NodeEditorEnd();
public void NodeEnd();
public int NowPlaying(string id, ref NowPlaying np, NowPlayingOpts opts = default);
public bool Number(string id, ref double value, double min, double max, NumberOpts opts = default);
public bool OtpInput(string id, TextBuffer code, int length);
public bool Pagination(string id, ref int page, int page_count);
public int PasswordStrength(string password);
public PipelineResult Pipeline(string id, ReadOnlySpan<string> stages, int stage_count, ReadOnlySpan<PipelineJob> jobs, int job_count, PipelineOpts opts = default);
public void PopDisabled();
public void PopId();
public UiScope PopoverBegin(string id, PopoverOpts opts = default);
public void PopoverClose(string id);
public void PopoverEnd();
public bool PopoverIsOpen(string id);
public void PopoverOpen(string id);
public void PopoverToggle(string id);
public void Presence(string id, ReadOnlySpan<PresenceUser> users, int count, int max_visible, float size);
public ProblemsResult Problems(string id, ReadOnlySpan<Diagnostic> diags, int count, ProblemsOpts opts = default);
public void Progress(float value, ProgressOpts opts = default);
public void ProgressRing(string label, float value, float size);
public int PromptSuggestions(string id, ReadOnlySpan<PromptSuggestion> items, int count, PromptSuggestionsOpts opts = default);
public UiScope PushDisabled(bool disabled);
public UiScope PushId(string str);
public UiScope PushIdInt(int value);
public bool QueryBuilder(string id, ReadOnlySpan<QueryField> fields, int field_count, Span<QueryNode> nodes, ref int count, int capacity, QueryOpts opts = default);
public int RadarChart(string id, ReadOnlySpan<Series> series, int count, ReadOnlySpan<string> axes, int axis_count, RadarOpts opts = default);
public void RadialGauge(string id, float value, float min, float max, RadialGaugeOpts opts = default);
public bool RadioCards(string id, ReadOnlySpan<Choice> choices, int count, ref int index, RadioCardsOpts opts = default);
public bool Radio(string label, ref int value, int option, CheckOpts opts = default);
public bool RangeCalendar(string id, ref DateRange range, int months);
public bool RangeSlider(string label, ref float lo, ref float hi, float min, float max);
public bool Rating(string id, ref int value, int max);
public int Reactions(string id, Span<Reaction> items, int count);
public void Readout(string label, string value, ReadoutOpts opts = default);
public UiScope ReasoningBegin(string id, ReasoningOpts opts = default);
public void ReasoningEnd();
public RegexTesterResult RegexTester(string id, TextBuffer pattern, TextBuffer text, RegexTesterOpts opts = default);
public UiScope ReorderBegin(string id, Memory<int> order, int count);
public bool ReorderEnd();
public UiScope ReorderItemBegin(int position);
public void ReorderItemEnd();
public RequestBarResult RequestBar(string id, ref int method, TextBuffer url, RequestBarOpts opts = default);
public void RequestRedraw();
public void RequestWakeup(float seconds);
public int ResponseActions(string id, ResponseActionsOpts opts = default);
public void ResponseStatus(int status, double seconds, double bytes);
public UiScope RowBegin(Box opts = default);
public bool RulerSlider(string label, ref float value, float min, float max, RulerOpts opts = default);
public int Sankey(string id, ReadOnlySpan<string> nodes, int node_count, ReadOnlySpan<SankeyLink> links, int link_count, SankeyOpts opts = default);
public int ScatterChart(string id, ReadOnlySpan<ScatterSeries> series, int count, ScatterOpts opts = default);
public ScheduleResult SchedulePicker(string id, TextBuffer cron, ScheduleOpts opts = default);
public UiScope ScrollBegin(ScrollOpts opts = default);
public void ScrollEnd();
public void ScrollTo(string scroll_id, float y);
public bool Scrub(string label, ref float value, float min, float max, ScrubOpts opts = default);
public int Search(string id, TextBuffer buf, InputOpts opts = default);
public void Secondary(string text);
public bool Segmented(string id, ReadOnlySpan<string> items, int count, ref int index, SegmentedOpts opts = default);
public bool Select(string id, ReadOnlySpan<string> items, int count, ref int index, SelectOpts opts = default);
public void Separator();
public void SetAccent(Accent accent);
public void SetReducedMotion(bool reduced);
public void SetTheme(ThemeMode mode);
public UiScope SheetBegin(string title, ref bool open, SheetOpts opts = default);
public void SheetEnd();
public void ShimmerText(string text, TextFxOpts opts = default);
public bool Shortcut(int mods, Key key);
public string? ShortcutLabel(KeyCombo combo);
public bool ShortcutRecorder(string id, ref KeyCombo combo, ShortcutRecorderOpts opts = default);
public bool SignaturePad(string id, SignatureOpts opts = default);
public void Skeleton(Size width, float height);
public bool Slider(string label, ref float value, float min, float max, SliderOpts opts = default);
public int SourceList(string id, ReadOnlySpan<Source> sources, int count);
public void Spacer();
public int SpeedDial(string id, ReadOnlySpan<SpeedAction> actions, int count, SpeedDialOpts opts = default);
public void Spinner(float size);
public int SplitBar(string id, ReadOnlySpan<VizItem> items, int count, SplitBarOpts opts = default);
public int SplitButton(string label, ReadOnlySpan<string> items, int count, SplitButtonOpts opts = default);
public UiScope SpotlightBegin(string id, SpotlightOpts opts = default);
public void SpotlightEnd();
public StackTraceResult StackTrace(string id, ReadOnlySpan<StackFrame> frames, int count, StackTraceOpts opts = default);
public bool StatusButton(string label, Status status, StatusButtonOpts opts = default);
public void Stepper(ReadOnlySpan<string> steps, int count, int current);
public UiScope SwipeItemBegin(string id, ReadOnlySpan<SwipeAction> left, int left_count, ReadOnlySpan<SwipeAction> right, int right_count);
public int SwipeItemEnd();
public bool Switch(string label, ref bool value, CheckOpts opts = default);
public bool TabBar(string id, ReadOnlySpan<TabBarItem> items, int count, ref int selected, TabBarOpts opts = default);
public UiScope TableBegin(string id, ReadOnlySpan<TableColumn> columns, int column_count, TableOpts opts = default);
public void TableCell();
public void TableCellText(string text);
public void TableEnd();
public bool TableRow(bool selected);
public bool Tabs(string id, ReadOnlySpan<string> items, int count, ref int index, TabsOpts opts = default);
public bool TagInput(string id, TextBuffer buf, TagOpts opts = default);
public TestExplorerResult TestExplorer(string id, ReadOnlySpan<TestItem> items, int count, TestExplorerOpts opts = default);
public void Text(string text, TextOpts opts = default);
public void TextReveal(string id, string text, TextFxOpts opts = default);
public int Textarea(string id, TextBuffer buf, InputOpts opts = default);
public bool ThemeSwitcher(ref int mode, bool with_system);
public bool ThemeToggle();
public void Ticker(string id, ReadOnlySpan<TickerItem> items, int count, TickerOpts opts = default);
public double Time();
public bool TimeDial(string id, ref int hour, ref int minute, float size);
public TimeHistogramResult TimeHistogram(string id, double start, double bucket_seconds, int buckets, ReadOnlySpan<HistogramSeries> series, int series_count, ref double sel_start, ref double sel_end, TimeHistogramOpts opts = default);
public bool TimePicker(string id, ref int hour, ref int minute, TimeOpts opts = default);
public void TimelineItem(string title, TimelineOpts opts = default);
public bool ToastActionClicked(int handle);
public void ToastDismiss(int handle);
public int Toast(string title, ToastOpts opts = default);
public int Toc(string scroll_id, ReadOnlySpan<TocItem> items, int count);
public void TocAnchor(string anchor);
public bool ToggleGroup(string id, ReadOnlySpan<string> labels, int count, Span<bool> values, ToggleGroupOpts opts = default);
public void ToolCall(string id, string name, ToolCallOpts opts = default);
public void Tooltip(string text);
public int Tour(string id, ReadOnlySpan<TourStep> steps, int count, ref int step, TourOpts opts = default);
public void TraceSort(Span<TraceSpan> spans, int count);
public TraceResult TraceView(string id, Span<TraceTrack> tracks, int track_count, ReadOnlySpan<TraceSpan> spans, int span_count, ref TraceView view, ref int selected, TraceOpts opts = default);
public UiScope TreeBegin(string label, ref bool open, TreeOpts opts = default);
public bool TreeCell(string label, int depth, bool has_children, ref bool open, string icon);
public bool TreeClicked();
public void TreeEnd();
public int Treemap(string id, ReadOnlySpan<VizItem> items, int count, TreemapOpts opts = default);
public bool Typewriter(string id, string text, TextFxOpts opts = default);
public void TypingIndicator(string name);
public UptimeResult Uptime(string id, ReadOnlySpan<UptimeService> services, int count, UptimeOpts opts = default);
public UiScope UserMenuBegin(string name, UserMenuOpts opts = default);
public void UserMenuEnd();
public Vec2 ViewportSize();
public UiScope VirtualListBegin(string id, int count, float row_height, Size height, ref int first, ref int last);
public void VirtualListEnd();
public bool VoiceOrb(string id, VoiceState state, float level, VoiceOrbOpts opts = default);
public int Waffle(string id, ReadOnlySpan<VizItem> items, int count, WaffleOpts opts = default);
public bool WantsKeyboard();
public bool WantsMouse();
public WaterfallResult Waterfall(string id, ReadOnlySpan<NetRequest> requests, int count, WaterfallOpts opts = default);
public bool Waveform(string id, ReadOnlySpan<float> peaks, int count, ref float progress, WaveformOpts opts = default);
public int WeekCalendar(string id, Date week_start, Span<CalEvent> events, int count, WeekOpts opts = default);
public bool WheelPicker(string id, ReadOnlySpan<string> items, int count, ref int index, WheelOpts opts = default);
public void WordRotate(string id, ReadOnlySpan<string> words, int count, float interval, TextFxOpts opts = default);
```

## Gallery coverage inventory

All 266 distinct actual upstream gallery call names resolve to direct typed engine calls or deliberate managed ownership/formatting replacements (static source audit: no missing calls). Every `cui_*` identifier referenced by both pinned gallery files is mapped below. Header types map to the corresponding safe PascalCase definition; macros resolve to their explicit underlying API.

| Upstream identifier | Managed API / treatment |
| --- | --- |
| `cui__layout` | Displayed source identifier in the blame example; no internal-layout API |
| `cui_accent` | `Accent` |
| `cui_accent_name` | `Ui.AccentName` |
| `cui_accordion_begin` | `Ui.AccordionBegin` |
| `cui_accordion_end` | `Ui.AccordionEnd` |
| `cui_activity_rings` | `Ui.ActivityRings` |
| `cui_agent_plan` | `Ui.AgentPlan` |
| `cui_agent_status` | `Ui.AgentStatus` |
| `cui_alert` | `Ui.Alert` |
| `cui_api_endpoint` | `ApiEndpoint` |
| `cui_api_explorer` | `Ui.ApiExplorer` |
| `cui_api_param` | `ApiParam` |
| `cui_avatar` | `Ui.Avatar` |
| `cui_avatar_group` | `Ui.AvatarGroup` |
| `cui_avatar_upload` | `Ui.AvatarUpload` |
| `cui_badge` | `Ui.Badge` |
| `cui_banner` | `Ui.Banner` |
| `cui_bar_chart` | `Ui.BarChart` |
| `cui_bar_list` | `Ui.BarList` |
| `cui_blame` | `Ui.Blame` |
| `cui_blame_commit` | `BlameCommit` |
| `cui_box` | `Box` |
| `cui_breadcrumb` | `Ui.Breadcrumb` |
| `cui_button` | `Ui.Button` |
| `cui_button_group` | `Ui.ButtonGroupBegin` |
| `cui_cal_event` | `CalEvent` |
| `cui_calendar_heatmap` | `Ui.CalendarHeatmap` |
| `cui_candle` | `Candle` |
| `cui_candlestick` | `Ui.Candlestick` |
| `cui_canvas` | Ui.Canvas and rooted CanvasPainter |
| `cui_canvas_result` | `CanvasResult` |
| `cui_card` | `Ui.CardBegin` |
| `cui_card_stack_begin` | `Ui.CardStackBegin` |
| `cui_card_stack_end` | `Ui.CardStackEnd` |
| `cui_card_stack_item` | `Ui.CardStackItemBegin` |
| `cui_card_stack_opts` | `CardStackOpts` |
| `cui_carousel` | `Ui.CarouselBegin` |
| `cui_carousel_slide` | `Ui.CarouselSlideBegin` |
| `cui_chart_annotation` | `ChartAnnotation` |
| `cui_chat_composer` | `Ui.ChatComposer` |
| `cui_chat_divider` | `Ui.ChatDivider` |
| `cui_chat_message` | `Ui.ChatMessage` |
| `cui_check_step` | `CheckStep` |
| `cui_checkbox` | `Ui.Checkbox` |
| `cui_checklist` | `Ui.Checklist` |
| `cui_chip` | `Ui.Chip` |
| `cui_choice` | `Choice` |
| `cui_citation` | `Ui.Citation` |
| `cui_code_block` | `Ui.CodeBlock` |
| `cui_color` | `Color` |
| `cui_color_of` | `Ui.ColorOf` |
| `cui_color_picker` | `Ui.ColorPicker` |
| `cui_column` | `Ui.ColumnBegin` |
| `cui_command` | `Command` |
| `cui_command_palette` | `Ui.CommandPalette` |
| `cui_comment` | `Ui.Comment` |
| `cui_commit` | `Commit` |
| `cui_commit_graph` | `Ui.CommitGraph` |
| `cui_confirm_button` | `Ui.ConfirmButton` |
| `cui_confirm_popover` | `Ui.ConfirmPopover` |
| `cui_context_menu` | `Ui.ContextMenuBegin` |
| `cui_context_meter` | `Ui.ContextMeter` |
| `cui_control_tile` | `Ui.ControlTile` |
| `cui_copy_button` | `Ui.CopyButton` |
| `cui_countdown` | `Ui.Countdown` |
| `cui_counter` | `Ui.Counter` |
| `cui_coverage` | `Ui.Coverage` |
| `cui_coverage_file` | `CoverageFile` |
| `cui_cropper` | `Ui.Cropper` |
| `cui_ctx` | Ui owner |
| `cui_data_grid_begin` | `Ui.DataGridBegin` |
| `cui_data_grid_cell` | `Ui.DataGridCell` |
| `cui_data_grid_cell_text` | `Ui.DataGridCellText` |
| `cui_data_grid_edit` | `Ui.DataGridEdit` |
| `cui_data_grid_end` | `Ui.DataGridEnd` |
| `cui_data_grid_row` | `Ui.DataGridRow` |
| `cui_date` | `Date` |
| `cui_date_from_days` | `Ui.DateFromDays` |
| `cui_date_picker` | `Ui.DatePicker` |
| `cui_date_range` | `DateRange` |
| `cui_date_range_picker` | `Ui.DateRangePicker` |
| `cui_date_to_days` | `Ui.DateToDays` |
| `cui_date_today` | `Ui.DateToday` |
| `cui_delta_time` | `Ui.DeltaTime` |
| `cui_diagnostic` | `Diagnostic` |
| `cui_dialog` | `Ui.DialogBegin` |
| `cui_dialog_footer` | `Ui.DialogFooterBegin` |
| `cui_dock` | `Ui.DockBegin` |
| `cui_dock_item` | `Ui.DockItem` |
| `cui_dock_separator` | `Ui.DockSeparator` |
| `cui_donut_chart` | `Ui.DonutChart` |
| `cui_dot_chart` | `Ui.DotChart` |
| `cui_dot_chart_result` | `DotChartResult` |
| `cui_dot_display` | `Ui.DotDisplay` |
| `cui_dot_series` | `DotSeries` |
| `cui_draw_circle` | `Ui.DrawCircle` |
| `cui_dropped_file` | `Ui.DroppedFile` |
| `cui_dropzone` | `Ui.Dropzone` |
| `cui_emoji_picker` | `Ui.EmojiPicker` |
| `cui_empty_state` | `Ui.EmptyState` |
| `cui_event_track` | `Ui.EventTrack` |
| `cui_expandable_card` | `Ui.ExpandableCardBegin` |
| `cui_expandable_text` | `Ui.ExpandableText` |
| `cui_fader` | `Ui.Fader` |
| `cui_feature_flag` | `FeatureFlag` |
| `cui_feature_flags` | `Ui.FeatureFlags` |
| `cui_file_browser` | `Ui.FileBrowser` |
| `cui_file_browser_result` | `FileBrowserResult` |
| `cui_file_entry` | `FileEntry` |
| `cui_file_picker` | `Ui.FilePicker` |
| `cui_filter_bar` | `Ui.FilterBar` |
| `cui_filter_field` | `FilterField` |
| `cui_frame` | Displayed diagnostic string; no function call |
| `cui_fs_list` | Ui.FsList with Span<FileEntry> |
| `cui_funnel` | `Ui.Funnel` |
| `cui_gantt` | `Ui.Gantt` |
| `cui_gantt_task` | `GanttTask` |
| `cui_gap` | `Ui.Gap` |
| `cui_gauge` | `Ui.Gauge` |
| `cui_gauge_band` | `GaugeBand` |
| `cui_gradient_text` | `Ui.GradientText` |
| `cui_graph_edge` | `GraphEdge` |
| `cui_graph_node` | `GraphNode` |
| `cui_graph_view` | `Ui.GraphView` |
| `cui_grid_column` | `GridColumn` |
| `cui_heading` | `Ui.Heading` |
| `cui_heatmap` | `Ui.Heatmap` |
| `cui_histogram_series` | `HistogramSeries` |
| `cui_histogram_slider` | `Ui.HistogramSlider` |
| `cui_hold_button` | `Ui.HoldButton` |
| `cui_hover_card` | `Ui.HoverCardBegin` |
| `cui_icon` | `Ui.Icon` |
| `cui_icon_button` | `Ui.IconButton` |
| `cui_id` | Id bound to Ui |
| `cui_image_compare` | `Ui.ImageCompare` |
| `cui_image_create` | Ui.ImageCreate |
| `cui_image_destroy` | Texture.Dispose |
| `cui_image_view` | `ImageView` |
| `cui_image_viewer` | `Ui.ImageViewer` |
| `cui_inline_edit` | `Ui.InlineEdit` |
| `cui_input` | `Ui.Input` |
| `cui_internal` | No public/internal header dependency |
| `cui_json_view` | `Ui.JsonView` |
| `cui_kanban_begin` | `Ui.KanbanBegin` |
| `cui_kanban_card_begin` | `Ui.KanbanCardBegin` |
| `cui_kanban_card_end` | `Ui.KanbanCardEnd` |
| `cui_kanban_column_begin` | `Ui.KanbanColumnBegin` |
| `cui_kanban_column_end` | `Ui.KanbanColumnEnd` |
| `cui_kanban_end` | `Ui.KanbanEnd` |
| `cui_kanban_move` | `KanbanMove` |
| `cui_key_combo` | `KeyCombo` |
| `cui_keymap` | `Ui.Keymap` |
| `cui_keymap_binding` | `KeymapBinding` |
| `cui_knob` | `Ui.Knob` |
| `cui_kv_editor` | `Ui.KvEditor` |
| `cui_kv_pair` | `KvPair` |
| `cui_label` | `Ui.Label` |
| `cui_last_id` | `Ui.LastId` |
| `cui_last_rect` | `Ui.LastRect` |
| `cui_level_meter` | `Ui.LevelMeter` |
| `cui_line_chart` | `Ui.LineChart` |
| `cui_link` | `Ui.Link` |
| `cui_list_item` | `Ui.ListItem` |
| `cui_loader` | `Ui.Loader` |
| `cui_loader_style` | `LoaderStyle` |
| `cui_make_id` | `Ui.MakeId` |
| `cui_menu` | `Ui.MenuBegin` |
| `cui_menu_item` | `Ui.MenuItem` |
| `cui_menu_label` | `Ui.MenuLabel` |
| `cui_menu_separator` | `Ui.MenuSeparator` |
| `cui_meter` | `Ui.Meter` |
| `cui_metric` | `Ui.Metric` |
| `cui_model` | `Model` |
| `cui_money_input` | `Ui.MoneyInput` |
| `cui_month_calendar` | `Ui.MonthCalendar` |
| `cui_month_event` | `MonthEvent` |
| `cui_month_result` | `MonthResult` |
| `cui_multi_select` | `Ui.MultiSelect` |
| `cui_muted` | `Ui.Muted` |
| `cui_nav_item` | `Ui.NavItem` |
| `cui_nav_section` | `Ui.NavSection` |
| `cui_net_request` | `NetRequest` |
| `cui_node` | Displayed source type in blame example; independent safe Node/NodeEditor scopes are additionally supplied |
| `cui_now_playing` | `NowPlaying` |
| `cui_number` | `Ui.Number` |
| `cui_otp_input` | `Ui.OtpInput` |
| `cui_pagination` | `Ui.Pagination` |
| `cui_password_strength` | `Ui.PasswordStrength` |
| `cui_pipeline` | `Ui.Pipeline` |
| `cui_pipeline_job` | `PipelineJob` |
| `cui_plan_step` | `PlanStep` |
| `cui_pop_id` | `Ui.PopId` |
| `cui_popover` | `Ui.PopoverBegin` |
| `cui_popover_close` | `Ui.PopoverClose` |
| `cui_popover_open` | `Ui.PopoverOpen` |
| `cui_popover_toggle` | `Ui.PopoverToggle` |
| `cui_presence` | `Ui.Presence` |
| `cui_presence_user` | `PresenceUser` |
| `cui_problems` | `Ui.Problems` |
| `cui_progress` | `Ui.Progress` |
| `cui_progress_ring` | `Ui.ProgressRing` |
| `cui_prompt_suggestion` | `PromptSuggestion` |
| `cui_prompt_suggestions` | `Ui.PromptSuggestions` |
| `cui_push_id` | `Ui.PushId` |
| `cui_push_id_int` | `Ui.PushIdInt` |
| `cui_query_builder` | `Ui.QueryBuilder` |
| `cui_query_field` | `QueryField` |
| `cui_query_node` | `QueryNode` |
| `cui_query_text` | Ui.QueryText |
| `cui_radar_chart` | `Ui.RadarChart` |
| `cui_radial_gauge` | `Ui.RadialGauge` |
| `cui_radio` | `Ui.Radio` |
| `cui_radio_cards` | `Ui.RadioCards` |
| `cui_range_calendar` | `Ui.RangeCalendar` |
| `cui_range_slider` | `Ui.RangeSlider` |
| `cui_rating` | `Ui.Rating` |
| `cui_reaction` | `Reaction` |
| `cui_reactions` | `Ui.Reactions` |
| `cui_readout` | `Ui.Readout` |
| `cui_reasoning` | `Ui.ReasoningBegin` |
| `cui_rect` | `Rect` |
| `cui_regex_tester` | `Ui.RegexTester` |
| `cui_reorder_begin` | `Ui.ReorderBegin` |
| `cui_reorder_end` | `Ui.ReorderEnd` |
| `cui_reorder_item` | `Ui.ReorderItemBegin` |
| `cui_request_bar` | `Ui.RequestBar` |
| `cui_request_bar_result` | `RequestBarResult` |
| `cui_request_redraw` | `Ui.RequestRedraw` |
| `cui_response_actions` | `Ui.ResponseActions` |
| `cui_response_status` | `Ui.ResponseStatus` |
| `cui_row` | `Ui.RowBegin` |
| `cui_ruler_slider` | `Ui.RulerSlider` |
| `cui_sankey` | `Ui.Sankey` |
| `cui_sankey_link` | `SankeyLink` |
| `cui_scatter_chart` | `Ui.ScatterChart` |
| `cui_scatter_series` | `ScatterSeries` |
| `cui_schedule_picker` | `Ui.SchedulePicker` |
| `cui_scroll` | `Ui.ScrollBegin` |
| `cui_scroll_to` | `Ui.ScrollTo` |
| `cui_scrub` | `Ui.Scrub` |
| `cui_search` | `Ui.Search` |
| `cui_secondary` | `Ui.Secondary` |
| `cui_segmented` | `Ui.Segmented` |
| `cui_select` | `Ui.Select` |
| `cui_separator` | `Ui.Separator` |
| `cui_series` | `Series` |
| `cui_set_accent` | `Ui.SetAccent` |
| `cui_set_theme` | `Ui.SetTheme` |
| `cui_sheet` | `Ui.SheetBegin` |
| `cui_shimmer_text` | `Ui.ShimmerText` |
| `cui_shortcut` | `Ui.Shortcut` |
| `cui_shortcut_recorder` | `Ui.ShortcutRecorder` |
| `cui_signature_pad` | `Ui.SignaturePad` |
| `cui_skeleton` | `Ui.Skeleton` |
| `cui_slider` | `Ui.Slider` |
| `cui_source` | `Source` |
| `cui_source_list` | `Ui.SourceList` |
| `cui_spacer` | `Ui.Spacer` |
| `cui_speed_action` | `SpeedAction` |
| `cui_speed_dial` | `Ui.SpeedDial` |
| `cui_spinner` | `Ui.Spinner` |
| `cui_split_bar` | `Ui.SplitBar` |
| `cui_split_button` | `Ui.SplitButton` |
| `cui_spotlight` | `Ui.SpotlightBegin` |
| `cui_stack_frame` | `StackFrame` |
| `cui_stack_trace` | `Ui.StackTrace` |
| `cui_state` | Consumer-owned typed fields/State<T>; no byte-sized state escape |
| `cui_state_on_free` | IDisposable resource owners attached to Ui lifetime |
| `cui_status` | `Status` |
| `cui_status_button` | `Ui.StatusButton` |
| `cui_stepper` | `Ui.Stepper` |
| `cui_swipe_action` | `SwipeAction` |
| `cui_swipe_item_begin` | `Ui.SwipeItemBegin` |
| `cui_swipe_item_end` | `Ui.SwipeItemEnd` |
| `cui_switch` | `Ui.Switch` |
| `cui_tab_bar` | `Ui.TabBar` |
| `cui_tab_bar_item` | `TabBarItem` |
| `cui_table` | `Ui.TableBegin` |
| `cui_table_cell` | `Ui.TableCell` |
| `cui_table_cell_text` | `Ui.TableCellText` |
| `cui_table_column` | `TableColumn` |
| `cui_table_row` | `Ui.TableRow` |
| `cui_table_sort` | `TableSort` |
| `cui_tabs` | `Ui.Tabs` |
| `cui_tag_input` | `Ui.TagInput` |
| `cui_terminal` | `Ui.Terminal` |
| `cui_terminal_buffer` | `TerminalBuffer` |
| `cui_terminal_clear` | TerminalBuffer.Clear |
| `cui_terminal_init` | TerminalBuffer constructor |
| `cui_terminal_line` | `TerminalLine` |
| `cui_terminal_printf` | TerminalBuffer.Write with managed formatting |
| `cui_terminal_result` | `TerminalResult` |
| `cui_terminal_write` | TerminalBuffer.Write |
| `cui_test_explorer` | `Ui.TestExplorer` |
| `cui_test_item` | `TestItem` |
| `cui_text` | `Ui.Text` |
| `cui_text_reveal` | `Ui.TextReveal` |
| `cui_textarea` | `Ui.Textarea` |
| `cui_textf` | Ui.Text with managed string formatting or span |
| `cui_texture` | Texture owner |
| `cui_theme_switcher` | `Ui.ThemeSwitcher` |
| `cui_theme_toggle` | `Ui.ThemeToggle` |
| `cui_ticker` | `Ui.Ticker` |
| `cui_ticker_item` | `TickerItem` |
| `cui_time` | `Ui.Time` |
| `cui_time_dial` | `Ui.TimeDial` |
| `cui_time_histogram` | `Ui.TimeHistogram` |
| `cui_time_picker` | `Ui.TimePicker` |
| `cui_timeline_item` | `Ui.TimelineItem` |
| `cui_toast` | `Ui.Toast` |
| `cui_toc` | `Ui.Toc` |
| `cui_toc_anchor` | `Ui.TocAnchor` |
| `cui_toc_item` | `TocItem` |
| `cui_toggle_group` | `Ui.ToggleGroup` |
| `cui_tone` | `Tone` |
| `cui_tool_call` | `Ui.ToolCall` |
| `cui_tooltip` | `Ui.Tooltip` |
| `cui_tour` | `Ui.Tour` |
| `cui_tour_step` | `TourStep` |
| `cui_trace_marker` | `TraceMarker` |
| `cui_trace_sort` | `Ui.TraceSort` |
| `cui_trace_span` | `TraceSpan` |
| `cui_trace_track` | `TraceTrack` |
| `cui_trace_view` | `TraceView` |
| `cui_tree_begin` | `Ui.TreeBegin` |
| `cui_tree_cell` | `Ui.TreeCell` |
| `cui_tree_clicked` | `Ui.TreeClicked` |
| `cui_tree_end` | `Ui.TreeEnd` |
| `cui_tree_opts` | `TreeOpts` |
| `cui_treemap` | `Ui.Treemap` |
| `cui_typewriter` | `Ui.Typewriter` |
| `cui_typing_indicator` | `Ui.TypingIndicator` |
| `cui_uptime` | `Ui.Uptime` |
| `cui_uptime_service` | `UptimeService` |
| `cui_url_params` | Ui.UrlParams |
| `cui_url_set_params` | Ui.UrlSetParams |
| `cui_user_menu` | `Ui.UserMenuBegin` |
| `cui_vec2` | `Vec2` |
| `cui_viewport_size` | `Ui.ViewportSize` |
| `cui_virtual_list` | `Ui.VirtualListBegin` |
| `cui_viz_item` | `VizItem` |
| `cui_voice_orb` | `Ui.VoiceOrb` |
| `cui_voice_state` | `VoiceState` |
| `cui_waffle` | `Ui.Waffle` |
| `cui_wants_keyboard` | `Ui.WantsKeyboard` |
| `cui_waterfall` | `Ui.Waterfall` |
| `cui_waveform` | `Ui.Waveform` |
| `cui_week_calendar` | `Ui.WeekCalendar` |
| `cui_wheel_picker` | `Ui.WheelPicker` |
| `cui_word_rotate` | `Ui.WordRotate` |

## Build and audit status

Full library implementation is present. `Ui.Api.cs` supplies 325 typed C-derived entry points, `Ui.Overloads.cs` adds count-inferred and scope aliases, and `Types.cs` supplies safe options/results. Complex devtools and instrument APIs call the translated core directly. `Values.cs` and `Icons.cs` provide safe size/color/geometry constructors, semantic colors, action flags and upstream glyph constants. `Ui.SignaturePoints` copies signature data to caller spans. TreeBegin/TreeCell additionally provide overloads for leaf nodes with no writable open state; optional options are value defaults.

Steady-state build paths reuse segmented native frame storage, typed memory journals, scope stacks and state leases. They create no widget objects or per-widget delegates. Input strings and spans encode/copy without temporary managed arrays. String results reuse an existing string during mutable-array/state writeback when text is unchanged; APIs returning strings can allocate. Cache caller arrays, TextBuffer, State<T>, TerminalBuffer and CanvasPainter registrations. Owner creation, first arena growth and registration can allocate. Frame arena limits, 32 managed scopes, 1,024 callback/painter registrations each, 65,536 state leases and checked counted spans bound wrapper-managed retention. The translated engine still owns its own allocations.

The following contract refinements followed direct retention inspection: CarouselBegin uses State<int>, NodeEditorBegin uses State<NodeView>, Pagination uses ref int, and Citation takes one Source value. Mutable inline arrays (FeatureFlag.On/Rollout, Commit.Parent, NetRequest.Timing) use Memory<T> with exact upstream lengths; empty/default memory produces zeroed fields. FeatureFlag has eight environment slots. Copyback preserves edits to inline arrays. Public scalar refs and option structs are also marshalled into stable arena slots, never borrowed stack locals. Only synchronous image upload, terminal byte parsing, and copied canvas user-data use fixed/local call-time storage.

Processed and raw Release library builds pass with zero compiler warnings/errors, using pre-existing matching core/GLFW binaries copied to isolated caller-selected artifact directories and --no-dependencies. Neither core was rebuilt or retranslated. NativeAOT compatibility analyzers are enabled on the library. Runtime, allocation measurement, unsafe-disabled consumer compilation, complete gallery compilation, pixel comparisons and executable NativeAOT tests remain coordinator integration gates; library compilation alone does not claim them.

Reproduce a normal build with `TMPDIR=$MANAGED_CUI_TEMP dotnet build cui/src/ManagedCui/ManagedCui.csproj --artifacts-path "$MANAGED_CUI_ARTIFACTS" -c Release`, adding `-p:CuiVariant=Raw` for raw selection. When core rebuilding is excluded, supply existing core/GLFW project outputs under the selected artifact layout and add `--no-dependencies`.

Coordinator integration adds `TextUtf8`/`ButtonUtf8`, `TextBuffer.SetUtf8` and a
copied `GetDiagnostics` snapshot. `Input` enforces widget frame validity. Grid
rows reject out-of-range indices before native access. Resource disposal remains
idempotent after context destruction. Application presentation is performed once
per constructed frame. Retained-array synchronization runs at audited native
mutation points rather than after every cell/text call.

The Linux library raw/processed × JIT/NativeAOT matrix now passes ordinary
project-reference builds, whole-wrapper AOT rooting, recursive public-surface
audits, 69 safety assertions per variant and three equivalent-work benchmark
trials per variant. All three workloads add zero steady-state managed bytes,
with exact software RGBA equality, stable retained storage and the documented
timing gates. See MANAGED-LIBRARY-VALIDATION.json and MANAGED-API-GUIDE.md.
Full authored-gallery and actual Windows acceptance remain separate gates.

Reviewed guards also reject short candlestick label arrays, table/grid text
cells before a row, invalid active query field indexes, and reorder counts above
the upstream capacity of 256. Gantt's day range is validated independently of
task count. File-list callbacks preserve the ordinary `-1` failure result.
Heatmap row/column labels deliberately allow null entries for skipped ticks.
