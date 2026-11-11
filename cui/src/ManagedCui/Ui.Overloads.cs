using Raw = global::Managed.Cui.Cui;
using static global::Managed.Cui.Cui;

namespace Cui.Managed;

public sealed unsafe partial class Ui
{
    public ulong FrameCount() { Check(); return Raw.cui_frame_count(ctx); }
    public UiScope Box(Box opts = default) => BoxBegin(opts);
    public UiScope Accordion(string title, ref bool open) => AccordionBegin(title, ref open);
    public UiScope TreeBegin(string label, TreeOpts opts = default)
    {
        CheckFrame(); CheckBegin(); ArgumentNullException.ThrowIfNull(label);
        var native = (cui_tree_opts*)arena.Allocate(sizeof(cui_tree_opts)); *native = ToNative(opts);
        bool opened = Raw.cui_tree_begin(ctx, Utf8(label), null, native) != 0;
        SyncState(); return TrackScope(ScopeKind.Tree, opened, opened ? 1 : 0);
    }
    public UiScope Tree(string label, TreeOpts opts = default) => TreeBegin(label, opts);
    public bool TreeCell(string label, int depth, bool hasChildren, string icon)
    {
        CheckFrame(); ArgumentNullException.ThrowIfNull(label); ArgumentNullException.ThrowIfNull(icon);
        bool result = Raw.cui_tree_cell(ctx, Utf8(label), depth, hasChildren, null, Utf8(icon)) != 0;
        SyncState(); return result;
    }
    public UiScope Tree(string label, ref bool open, TreeOpts opts = default) => TreeBegin(label, ref open, opts);
    public UiScope Carousel(string id, int count, State<int> index, CarouselOpts opts = default) => CarouselBegin(id, count, index, opts);
    public UiScope Reorder(string id, Memory<int> order) => ReorderBegin(id, order, order.Length);
    public UiScope ButtonGroup() => ButtonGroupBegin();
    public UiScope DialogFooter() => DialogFooterBegin();
    public bool Segmented(string id, ReadOnlySpan<string> items, ref int index, SegmentedOpts opts = default) => Segmented(id, items, items.Length, ref index, opts);
    public bool Select(string id, ReadOnlySpan<string> items, ref int index, SelectOpts opts = default) => Select(id, items, items.Length, ref index, opts);
    public bool Tabs(string id, ReadOnlySpan<string> items, ref int index, TabsOpts opts = default) => Tabs(id, items, items.Length, ref index, opts);
    public bool RadioCards(string id, ReadOnlySpan<Choice> items, ref int index, RadioCardsOpts opts = default) => RadioCards(id, items, items.Length, ref index, opts);
    public bool RadioGroup(string id, ReadOnlySpan<string> items, ref int index)
    {
        CheckFrame();
        int* state = (int*)arena.Allocate(sizeof(int)); *state = index;
        bool changed = Raw.cui_radio_group(ctx, Utf8Required(id), Strings(items), items.Length, state) != 0;
        index = *state; SyncState(); return changed;
    }
    public bool MultiSelect(string id, ReadOnlySpan<string> items, Span<bool> selected, MultiSelectOpts opts = default) => MultiSelect(id, items, items.Length, selected, opts);
    public bool ToggleGroup(string id, ReadOnlySpan<string> labels, Span<bool> selected, ToggleGroupOpts opts = default) => ToggleGroup(id, labels, labels.Length, selected, opts);
    public bool TabBar(string id, ReadOnlySpan<TabBarItem> items, ref int selected, TabBarOpts opts = default) => TabBar(id, items, items.Length, ref selected, opts);
    public int Reactions(string id, Span<Reaction> items) => Reactions(id, items, items.Length);
    public int Breadcrumb(ReadOnlySpan<string> items) => Breadcrumb(items, items.Length);
    public void Stepper(ReadOnlySpan<string> steps, int current) => Stepper(steps, steps.Length, current);
    public void LineChart(string id, ReadOnlySpan<Series> series, ChartOpts opts = default) => LineChart(id, series, series.Length, opts);
    public void BarChart(string id, ReadOnlySpan<Series> series, ChartOpts opts = default) => BarChart(id, series, series.Length, opts);
    public void DonutChart(string id, ReadOnlySpan<float> values, ReadOnlySpan<string> labels, ChartOpts opts = default) => DonutChart(id, values, labels, values.Length, opts);
    public int ScatterChart(string id, ReadOnlySpan<ScatterSeries> series, ScatterOpts opts = default) => ScatterChart(id, series, series.Length, opts);
    public DotChartResult DotChart(string id, ReadOnlySpan<DotSeries> series, int points, DotChartOpts opts = default) => DotChart(id, series, series.Length, points, opts);
    public int Treemap(string id, ReadOnlySpan<VizItem> items, TreemapOpts opts = default) => Treemap(id, items, items.Length, opts);
    public int Funnel(string id, ReadOnlySpan<VizItem> items, FunnelOpts opts = default) => Funnel(id, items, items.Length, opts);
    public int SplitBar(string id, ReadOnlySpan<VizItem> items, SplitBarOpts opts = default) => SplitBar(id, items, items.Length, opts);
    public int BarList(string id, ReadOnlySpan<VizItem> items, BarListOpts opts = default) => BarList(id, items, items.Length, opts);
    public int Candlestick(string id, ReadOnlySpan<Candle> candles, CandlestickOpts opts = default) => Candlestick(id, candles, candles.Length, opts);
    public int Waffle(string id, ReadOnlySpan<VizItem> items, WaffleOpts opts = default) => Waffle(id, items, items.Length, opts);
    public int ActivityRings(string id, ReadOnlySpan<VizItem> items, RingsOpts opts = default) => ActivityRings(id, items, items.Length, opts);
    public int CalendarHeatmap(string id, Date start, ReadOnlySpan<float> days, CalendarHeatmapOpts opts = default) => CalendarHeatmap(id, start, days, days.Length, opts);
    public bool Waveform(string id, ReadOnlySpan<float> peaks, ref float progress, WaveformOpts opts = default) => Waveform(id, peaks, peaks.Length, ref progress, opts);
    public bool LevelMeter(string id, ReadOnlySpan<float> levels, LevelMeterOpts opts = default) => LevelMeter(id, levels, levels.Length, opts);
    public int AgentPlan(string id, ReadOnlySpan<PlanStep> steps, AgentPlanOpts opts = default) => AgentPlan(id, steps, steps.Length, opts);
    public int SourceList(string id, ReadOnlySpan<Source> items) => SourceList(id, items, items.Length);
    public ProblemsResult Problems(string id, ReadOnlySpan<Diagnostic> items, ProblemsOpts opts = default) => Problems(id, items, items.Length, opts);
    public TestExplorerResult TestExplorer(string id, ReadOnlySpan<TestItem> items, TestExplorerOpts opts = default) => TestExplorer(id, items, items.Length, opts);
    public StackTraceResult StackTrace(string id, ReadOnlySpan<StackFrame> items, StackTraceOpts opts = default) => StackTrace(id, items, items.Length, opts);
    public WaterfallResult Waterfall(string id, ReadOnlySpan<NetRequest> items, WaterfallOpts opts = default) => Waterfall(id, items, items.Length, opts);
    public CoverageResult Coverage(string id, ReadOnlySpan<CoverageFile> items, CoverageOpts opts = default) => Coverage(id, items, items.Length, opts);
    public CommitGraphResult CommitGraph(string id, ReadOnlySpan<Commit> items, CommitGraphOpts opts = default) => CommitGraph(id, items, items.Length, opts);
    public ApiExplorerResult ApiExplorer(string id, ReadOnlySpan<ApiEndpoint> items, ApiExplorerOpts opts = default) => ApiExplorer(id, items, items.Length, opts);
    public bool QueryBuilder(string id, ReadOnlySpan<QueryField> fields, Span<QueryNode> nodes, ref int count, QueryOpts opts = default) => QueryBuilder(id, fields, fields.Length, nodes, ref count, nodes.Length, opts);
    public bool KvEditor(string id, Span<KvPair> pairs, ref int count, KvEditorOpts opts = default) => KvEditor(id, pairs, ref count, pairs.Length, opts);
    public EventTrackResult EventTrack(string id, Span<double> times, ref int count, double min, double max, EventTrackOpts opts = default) => EventTrack(id, times, ref count, times.Length, min, max, opts);
    public UiScope DataGrid(string id, ReadOnlySpan<GridColumn> columns, int rows, ref int first, ref int last, GridOpts opts = default) => DataGridBegin(id, columns, columns.Length, rows, ref first, ref last, opts);
    public UiScope VirtualList(string id, int count, float rowHeight, Size height, ref int first, ref int last) => VirtualListBegin(id, count, rowHeight, height, ref first, ref last);
    public bool Shortcut(Mod mods, Key key) => Shortcut((int)mods, key);
    public void InputKey(Key key, bool down, Mod mods) => InputKey(key, down, (int)mods);
    public int SignaturePoints(string id, Span<Vec2> points, Span<int> strokeEnds, out int strokeCount)
    {
        CheckFrame(); cui_vec2* rawPoints; int* rawEnds; int strokes;
        int count = Raw.cui_signature_points(ctx, Utf8Required(id), &rawPoints, &rawEnds, &strokes);
        strokeCount = strokes;
        CheckLength(count, points.Length, nameof(points)); CheckLength(strokes, strokeEnds.Length, nameof(strokeEnds));
        for (int i = 0; i < count; i++) points[i] = FromNative(rawPoints[i]);
        for (int i = 0; i < strokes; i++) strokeEnds[i] = rawEnds[i];
        return count;
    }
    public void SignatureClear(string id) { CheckFrame(); Raw.cui_signature_clear(ctx, Utf8Required(id)); }
}
