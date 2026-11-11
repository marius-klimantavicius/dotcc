namespace Cui.Managed;

public enum Accent { Neutral = 0, Violet, Blue, Green,
    Amber, Orange, Coral, Rose,
    Count }
public partial struct AgentPlanOpts
{
    public string? Title { get; set; }
}
public partial struct AgentStatusOpts
{
    public TaskStatus Status { get; set; }
    public float Seconds { get; set; }
    public string? Icon { get; set; }
}
public partial struct AlertOpts
{
    public string? Description { get; set; }
    public Tone Tone { get; set; }
    public string? Icon { get; set; }
    public bool Dismissible { get; set; }
}
public enum Align { Default = 0, Start, Center, End, Stretch }
public enum AnnotationKind { X = 0,
    Y,
    Point }
public partial struct ApiEndpoint
{
    public string? Method { get; set; }
    public string? Path { get; set; }
    public string? Summary { get; set; }
    public string? Tag { get; set; }
    public bool Deprecated { get; set; }
    public ReadOnlyMemory<ApiParam> Params { get; set; }
}
public partial struct ApiExplorerOpts
{
    public Size Width { get; set; }
    public float Height { get; set; }
    public State<int>? Selected { get; set; }
}
public partial struct ApiExplorerResult
{
    public int Selected { get; set; }
    public int TryIt { get; set; }
}
public partial struct ApiParam
{
    public string? Name { get; set; }
    public string? In { get; set; }
    public string? Type { get; set; }
    public bool Required { get; set; }
    public string? Description { get; set; }
}
public partial struct AvatarOpts
{
    public float Size { get; set; }
    public Texture? Image { get; set; }
    public string? Status { get; set; }
    public bool Square { get; set; }
}
public enum AxisUnits { Number = 0,
    Hours }
public partial struct BadgeOpts
{
    public Tone Tone { get; set; }
    public bool Dot { get; set; }
    public bool Solid { get; set; }
    public string? Icon { get; set; }
    public ControlSize Size { get; set; }
}
public partial struct BannerOpts
{
    public Tone Tone { get; set; }
    public string? Icon { get; set; }
    public string? Action { get; set; }
    public bool Dismissible { get; set; }
}
public partial struct BarListOpts
{
    public Size Width { get; set; }
    public string? Format { get; set; }
    public int MaxRows { get; set; }
    public bool KeepOrder { get; set; }
    public Color Color { get; set; }
}
public partial struct BlameCommit
{
    public string? Hash { get; set; }
    public string? Author { get; set; }
    public string? Summary { get; set; }
    public long Time { get; set; }
}
public partial struct BlameOpts
{
    public Size Width { get; set; }
    public float Height { get; set; }
    public string? Lang { get; set; }
    public long Now { get; set; }
    public int FirstLine { get; set; }
}
public partial struct BlameResult
{
    public int Clicked { get; set; }
    public int Hovered { get; set; }
}
public partial struct Box
{
    public string? Id { get; set; }
    public Dir Dir { get; set; }
    public Size Width { get; set; }
    public Size Height { get; set; }
    public float MinWidth { get; set; }
    public float MaxWidth { get; set; }
    public float MinHeight { get; set; }
    public float MaxHeight { get; set; }
    public float Pad { get; set; }
    public float PadX { get; set; }
    public float PadY { get; set; }
    public float PadTop { get; set; }
    public float PadRight { get; set; }
    public float PadBottom { get; set; }
    public float PadLeft { get; set; }
    public float Gap { get; set; }
    public int Columns { get; set; }
    public float MinColumnWidth { get; set; }
    public bool Wrap { get; set; }
    public Align Align { get; set; }
    public Justify Justify { get; set; }
    public Color Bg { get; set; }
    public Color BorderColor { get; set; }
    public float Border { get; set; }
    public float Radius { get; set; }
    public float RadiusTl { get; set; }
    public float RadiusTr { get; set; }
    public float RadiusBr { get; set; }
    public float RadiusBl { get; set; }
    public Elevation Elevation { get; set; }
    public bool ScrollX { get; set; }
    public bool ScrollY { get; set; }
    public bool Clip { get; set; }
    public float Opacity { get; set; }
    public Direction Direction { get; set; }
}
public partial struct ButtonOpts
{
    public ButtonVariant Variant { get; set; }
    public ControlSize Size { get; set; }
    public string? Icon { get; set; }
    public string? IconRight { get; set; }
    public bool Disabled { get; set; }
    public bool Loading { get; set; }
    public bool FullWidth { get; set; }
    public bool Selected { get; set; }
    public string? Tooltip { get; set; }
    public string? Shortcut { get; set; }
    public Size Width { get; set; }
    public Color Color { get; set; }
}
public enum ButtonVariant { Default = 0,
    Secondary,
    Primary,
    Outline,
    Ghost,
    Danger,
    Link }
public partial struct CalEvent
{
    public string? Title { get; set; }
    public string? Detail { get; set; }
    public int Day { get; set; }
    public float Start { get; set; }
    public float End { get; set; }
    public Tone Tone { get; set; }
}
public partial struct CalendarHeatmapOpts
{
    public float Cell { get; set; }
    public Color Color { get; set; }
    public string? Format { get; set; }
    public string? Unit { get; set; }
}
public partial struct Candle
{
    public float Open { get; set; }
    public float High { get; set; }
    public float Low { get; set; }
    public float Close { get; set; }
    public float Volume { get; set; }
}
public partial struct CandlestickOpts
{
    public Size Width { get; set; }
    public float Height { get; set; }
    public ReadOnlyMemory<string> Labels { get; set; }
    public string? Format { get; set; }
    public ReadOnlyMemory<Series> Overlays { get; set; }
    public bool NoVolume { get; set; }
    public bool Ohlc { get; set; }
    public Color Up { get; set; }
    public Color Down { get; set; }
}
public partial struct CanvasResult
{
    public Rect Rect { get; set; }
    public bool Hovered { get; set; }
    public bool Pressed { get; set; }
    public bool Clicked { get; set; }
    public Vec2 Mouse { get; set; }
}
public partial struct CardOpts
{
    public string? Id { get; set; }
    public Size Width { get; set; }
    public Size Height { get; set; }
    public float MinWidth { get; set; }
    public float MaxWidth { get; set; }
    public float Pad { get; set; }
    public float PadX { get; set; }
    public float PadY { get; set; }
    public float Gap { get; set; }
    public Dir Dir { get; set; }
    public Align Align { get; set; }
    public bool Wrap { get; set; }
    public bool Hoverable { get; set; }
    public bool Muted { get; set; }
    public bool Selected { get; set; }
}
public partial struct CardStackOpts
{
    public Size Width { get; set; }
    public float Height { get; set; }
    public int Visible { get; set; }
}
public partial struct CarouselOpts
{
    public float Height { get; set; }
    public float AutoAdvance { get; set; }
    public float Gap { get; set; }
    public bool NoArrows { get; set; }
    public bool NoDots { get; set; }
    public bool Loop { get; set; }
}
public partial struct ChartAnnotation
{
    public AnnotationKind Kind { get; set; }
    public double X { get; set; }
    public double Y { get; set; }
    public string? Label { get; set; }
    public string? Detail { get; set; }
    public bool Accent { get; set; }
}
public partial struct ChartOpts
{
    public Size Width { get; set; }
    public float Height { get; set; }
    public ReadOnlyMemory<string> Labels { get; set; }
    public bool Area { get; set; }
    public bool NoGrid { get; set; }
    public bool NoLegend { get; set; }
    public bool Stacked { get; set; }
    public bool Smooth { get; set; }
    public string? Format { get; set; }
    public float Min { get; set; }
    public float Max { get; set; }
    public ReadOnlyMemory<ChartAnnotation> Annotations { get; set; }
}
public partial struct ChatOpts
{
    public bool Mine { get; set; }
    public string? Author { get; set; }
    public string? Time { get; set; }
    public string? Status { get; set; }
    public bool Grouped { get; set; }
    public bool LastInGroup { get; set; }
    public Texture? Avatar { get; set; }
    public bool Markdown { get; set; }
    public State<MarkdownResult>? MarkdownResult { get; set; }
}
public partial struct CheckOpts
{
    public string? Description { get; set; }
    public bool Disabled { get; set; }
    public bool Indeterminate { get; set; }
}
public partial struct CheckStep
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? Action { get; set; }
    public bool Done { get; set; }
}
public partial struct ChecklistOpts
{
    public string? Title { get; set; }
    public string? Description { get; set; }
}
public partial struct ChipOpts
{
    public string? Icon { get; set; }
    public bool Disabled { get; set; }
}
public partial struct Choice
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? Icon { get; set; }
    public string? Badge { get; set; }
}
public partial struct CodeOpts
{
    public string? Lang { get; set; }
    public string? Title { get; set; }
    public bool LineNumbers { get; set; }
    public int HighlightFrom { get; set; }
    public int HighlightTo { get; set; }
    public float MaxHeight { get; set; }
    public float Size { get; set; }
    public bool NoHeader { get; set; }
}
public partial struct Color
{
    public byte R { get; set; }
    public byte G { get; set; }
    public byte B { get; set; }
    public byte A { get; set; }
}
public partial struct Command
{
    public string? Title { get; set; }
    public string? Icon { get; set; }
    public string? Group { get; set; }
    public string? Shortcut { get; set; }
}
public partial struct CommentOpts
{
    public string? Time { get; set; }
    public int Depth { get; set; }
    public State<int>? Likes { get; set; }
    public State<bool>? Liked { get; set; }
    public bool Edited { get; set; }
    public string? Badge { get; set; }
}
public partial struct Commit
{
    public string? Hash { get; set; }
    public string? Message { get; set; }
    public string? Author { get; set; }
    public string? Date { get; set; }
    public string? Refs { get; set; }
    public Memory<int> Parent { get; set; }
}
public partial struct CommitGraphOpts
{
    public Size Width { get; set; }
    public float Height { get; set; }
    public State<int>? Selected { get; set; }
}
public partial struct CommitGraphResult
{
    public int Clicked { get; set; }
    public int Activated { get; set; }
    public int Hovered { get; set; }
}
public partial struct CompareOpts
{
    public Size Width { get; set; }
    public float Height { get; set; }
    public string? BeforeLabel { get; set; }
    public string? AfterLabel { get; set; }
    public float Aspect { get; set; }
}
public partial struct ComposerOpts
{
    public string? Placeholder { get; set; }
    public bool Busy { get; set; }
    public ReadOnlyMemory<string> Attachments { get; set; }
    public State<int>? Removed { get; set; }
    public ReadOnlyMemory<Model> Models { get; set; }
    public State<int>? Model { get; set; }
    public int MaxLines { get; set; }
    public Size Width { get; set; }
}
public partial struct ConfirmOpts
{
    public string? ConfirmLabel { get; set; }
    public string? Icon { get; set; }
    public ButtonVariant Variant { get; set; }
    public ControlSize Size { get; set; }
    public float Timeout { get; set; }
}
public partial struct ConfirmPopoverOpts
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? ConfirmLabel { get; set; }
    public string? CancelLabel { get; set; }
    public bool Danger { get; set; }
    public Placement Placement { get; set; }
}
public partial struct ContextOpts
{
    public ReadOnlyMemory<VizItem> Parts { get; set; }
    public string? Unit { get; set; }
}
public enum ControlSize { Md = 0, Sm, Lg }
public partial struct Corners
{
    public float Tl { get; set; }
    public float Tr { get; set; }
    public float Br { get; set; }
    public float Bl { get; set; }
}
public partial struct CountdownOpts
{
    public bool HideDays { get; set; }
    public bool Compact { get; set; }
}
public partial struct CounterOpts
{
    public string? Format { get; set; }
    public float Size { get; set; }
    public Font Font { get; set; }
    public Color Color { get; set; }
    public bool GroupThousands { get; set; }
}
public partial struct CoverageFile
{
    public string? Path { get; set; }
    public int Lines { get; set; }
    public int LinesHit { get; set; }
    public int Branches { get; set; }
    public int BranchesHit { get; set; }
    public int Functions { get; set; }
    public int FunctionsHit { get; set; }
}
public partial struct CoverageOpts
{
    public Size Width { get; set; }
    public float Height { get; set; }
    public float Low { get; set; }
    public float High { get; set; }
}
public partial struct CoverageResult
{
    public int Activated { get; set; }
    public float Lines { get; set; }
}
public partial struct CropperOpts
{
    public Size Width { get; set; }
    public Size Height { get; set; }
    public float Aspect { get; set; }
    public bool Round { get; set; }
    public float MinSize { get; set; }
}
public partial struct Date
{
    public int Year { get; set; }
    public int Month { get; set; }
    public int Day { get; set; }
}
public partial struct DateOpts
{
    public string? Label { get; set; }
    public string? Placeholder { get; set; }
    public Size Width { get; set; }
}
public partial struct DateRange
{
    public Date Start { get; set; }
    public Date End { get; set; }
}
public partial struct DateRangeOpts
{
    public string? Label { get; set; }
    public string? Placeholder { get; set; }
    public Size Width { get; set; }
    public bool NoPresets { get; set; }
}
public partial struct Diagnostic
{
    public Severity Severity { get; set; }
    public string? Message { get; set; }
    public string? File { get; set; }
    public int Line { get; set; }
    public int Column { get; set; }
    public string? Source { get; set; }
    public string? Code { get; set; }
}
public partial struct DialogOpts
{
    public string? Description { get; set; }
    public float Width { get; set; }
    public bool NoCloseButton { get; set; }
    public bool Persistent { get; set; }
}
public enum Dir { Column = 0, Row, Stack, Grid }
public enum Direction { Inherit = 0, Ltr, Rtl, Auto }
public partial struct DockItemOpts
{
    public Color Color { get; set; }
    public bool Running { get; set; }
    public int Badge { get; set; }
}
public partial struct DockOpts
{
    public float Size { get; set; }
    public float Magnify { get; set; }
}
public partial struct DotChartOpts
{
    public Size Width { get; set; }
    public float Height { get; set; }
    public double XMin { get; set; }
    public double XMax { get; set; }
    public AxisUnits XUnits { get; set; }
    public string? XFormat { get; set; }
    public double XStep { get; set; }
    public float Min { get; set; }
    public float Max { get; set; }
    public string? Format { get; set; }
    public string? Unit { get; set; }
    public float Pitch { get; set; }
    public bool NoGrid { get; set; }
    public bool NoAxis { get; set; }
    public bool NoReadout { get; set; }
    public ReadOnlyMemory<ChartAnnotation> Annotations { get; set; }
}
public partial struct DotChartResult
{
    public int Hovered { get; set; }
    public double X { get; set; }
    public float Value { get; set; }
    public bool Clicked { get; set; }
}
public partial struct DotDisplayOpts
{
    public float Pitch { get; set; }
    public Color Color { get; set; }
    public bool Accent { get; set; }
    public bool NoDim { get; set; }
    public bool Marquee { get; set; }
    public TextAlign Align { get; set; }
    public Size Width { get; set; }
}
public partial struct DotSeries
{
    public ReadOnlyMemory<float> Values { get; set; }
    public string? Name { get; set; }
    public bool Accent { get; set; }
}
public partial struct DropzoneOpts
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? Icon { get; set; }
    public string? Accept { get; set; }
    public float Height { get; set; }
    public Size Width { get; set; }
    public bool Compact { get; set; }
}
public enum Elevation { None = 0, Resting, Raised, Floating }
public partial struct EmojiPickerOpts
{
    public Size Width { get; set; }
    public float Height { get; set; }
    public float Size { get; set; }
    public TextBuffer? Recent { get; set; }
    public State<int>? Tone { get; set; }
    public string? Placeholder { get; set; }
    public string? EmptyText { get; set; }
    public string? Hint { get; set; }
    public ReadOnlyMemory<string> GroupNames { get; set; }
    public bool NoSearch { get; set; }
    public bool NoPreview { get; set; }
    public bool NoTone { get; set; }
}
public partial struct EmptyOpts
{
    public string? Icon { get; set; }
    public string? Description { get; set; }
    public string? Action { get; set; }
}
public partial struct EventTrackOpts
{
    public Size Width { get; set; }
    public double Step { get; set; }
    public AxisUnits Units { get; set; }
    public string? Format { get; set; }
    public double LabelStep { get; set; }
    public State<int>? Selected { get; set; }
    public bool NoAdd { get; set; }
    public bool NoRemove { get; set; }
    public bool NoAxis { get; set; }
}
public partial struct EventTrackResult
{
    public bool Changed { get; set; }
    public int Added { get; set; }
    public int Removed { get; set; }
    public int Selected { get; set; }
    public bool Dragging { get; set; }
}
public partial struct ExpandableCardOpts
{
    public string? Description { get; set; }
    public string? Icon { get; set; }
    public string? Badge { get; set; }
    public Tone BadgeTone { get; set; }
}
public partial struct ExpandableTextOpts
{
    public int Lines { get; set; }
    public float Size { get; set; }
    public Color Color { get; set; }
    public Color FadeTo { get; set; }
}
public partial struct FaderOpts
{
    public string? Label { get; set; }
    public string? Format { get; set; }
    public string? Unit { get; set; }
    public float Length { get; set; }
    public bool Horizontal { get; set; }
    public float Step { get; set; }
    public float Reset { get; set; }
    public ReadOnlyMemory<float> Ticks { get; set; }
    public ReadOnlyMemory<float> Meter { get; set; }
    public bool HideValue { get; set; }
    public bool Disabled { get; set; }
}
public partial struct FeatureFlag
{
    public string? Key { get; set; }
    public string? Description { get; set; }
    public Memory<bool> On { get; set; }
    public Memory<int> Rollout { get; set; }
}
public partial struct FeatureFlagsOpts
{
    public Size Width { get; set; }
    public float Height { get; set; }
}
public partial struct FeatureFlagsResult
{
    public int Flag { get; set; }
    public int Env { get; set; }
}
public partial struct FileBrowserOpts
{
    public Size Width { get; set; }
    public Size Height { get; set; }
    public string? Filter { get; set; }
    public bool FoldersOnly { get; set; }
    public bool Hidden { get; set; }
    public bool NoPlaces { get; set; }
    public ReadOnlyMemory<string> Places { get; set; }
    public FileListHandler? List { get; set; }
    public int MaxEntries { get; set; }
}
public partial struct FileBrowserResult
{
    public string? Picked { get; set; }
    public string? Selected { get; set; }
    public bool Navigated { get; set; }
    public bool Error { get; set; }
}
public partial struct FileEntry
{
    public string? Name { get; set; }
    public bool Dir { get; set; }
    public ulong Size { get; set; }
    public long Modified { get; set; }
}
public partial struct FilePickerOpts
{
    public string? Title { get; set; }
    public bool Save { get; set; }
    public bool Folders { get; set; }
    public string? Start { get; set; }
    public string? Name { get; set; }
    public string? Filter { get; set; }
    public string? FilterLabel { get; set; }
    public FileListHandler? List { get; set; }
}
public partial struct FilterField
{
    public string? Name { get; set; }
    public string? Icon { get; set; }
    public ReadOnlyMemory<string> Options { get; set; }
}
public partial struct FunnelOpts
{
    public Size Width { get; set; }
    public float Height { get; set; }
    public string? Format { get; set; }
    public Color Color { get; set; }
}
public partial struct GanttOpts
{
    public float DayWidth { get; set; }
    public float RowHeight { get; set; }
    public Size Height { get; set; }
    public int Today { get; set; }
    public bool ReadOnly { get; set; }
}
public partial struct GanttTask
{
    public string? Name { get; set; }
    public string? Owner { get; set; }
    public int Start { get; set; }
    public int End { get; set; }
    public float Progress { get; set; }
    public Tone Tone { get; set; }
    public int DependsOn { get; set; }
    public bool Milestone { get; set; }
}
public partial struct GaugeBand
{
    public float To { get; set; }
    public Tone Tone { get; set; }
}
public partial struct GradientTextOpts
{
    public Font Font { get; set; }
    public float Size { get; set; }
    public Color From { get; set; }
    public Color To { get; set; }
    public bool Animate { get; set; }
    public Size Width { get; set; }
}
public partial struct GraphEdge
{
    public int From { get; set; }
    public int To { get; set; }
}
public partial struct GraphNode
{
    public string? Label { get; set; }
    public string? Detail { get; set; }
    public Tone Tone { get; set; }
}
public partial struct GraphOpts
{
    public Size Width { get; set; }
    public float Height { get; set; }
    public bool TopDown { get; set; }
    public State<int>? Selected { get; set; }
}
public partial struct GraphResult
{
    public int Clicked { get; set; }
    public int Activated { get; set; }
    public int Hovered { get; set; }
}
public partial struct GridColumn
{
    public string? Title { get; set; }
    public float Width { get; set; }
    public TextAlign Align { get; set; }
    public bool Sortable { get; set; }
    public bool Fixed { get; set; }
    public int Priority { get; set; }
}
public partial struct GridOpts
{
    public Size Width { get; set; }
    public Size Height { get; set; }
    public float RowHeight { get; set; }
    public Memory<bool> Selected { get; set; }
    public State<TableSort>? Sort { get; set; }
    public bool Striped { get; set; }
    public State<int>? Cursor { get; set; }
    public bool HideCheckboxes { get; set; }
    public State<int>? Anchor { get; set; }
}
public partial struct HeatmapOpts
{
    public ReadOnlyMemory<string?> RowLabels { get; set; }
    public ReadOnlyMemory<string?> ColLabels { get; set; }
    public float Cell { get; set; }
    public float Min { get; set; }
    public float Max { get; set; }
    public Color Color { get; set; }
    public string? Format { get; set; }
    public Size Width { get; set; }
}
public partial struct HistogramOpts
{
    public float Height { get; set; }
    public string? Format { get; set; }
}
public partial struct HistogramSeries
{
    public string? Name { get; set; }
    public ReadOnlyMemory<float> Counts { get; set; }
    public Tone Tone { get; set; }
}
public partial struct HoldOpts
{
    public float Duration { get; set; }
    public string? Icon { get; set; }
    public ButtonVariant Variant { get; set; }
}
public partial struct HoverCardOpts
{
    public Placement Placement { get; set; }
    public float Width { get; set; }
    public float Delay { get; set; }
}
public partial struct ImageOpts
{
    public Size Width { get; set; }
    public Size Height { get; set; }
    public float Radius { get; set; }
    public Color Tint { get; set; }
}
public partial struct ImageView
{
    public float Zoom { get; set; }
    public float Cx { get; set; }
    public float Cy { get; set; }
}
public partial struct ImageViewerOpts
{
    public Size Width { get; set; }
    public Size Height { get; set; }
    public float MaxZoom { get; set; }
    public bool NoToolbar { get; set; }
    public bool Checkerboard { get; set; }
}
public partial struct InlineEditOpts
{
    public Font Font { get; set; }
    public float Size { get; set; }
    public string? Placeholder { get; set; }
    public Size Width { get; set; }
}
public partial struct InputOpts
{
    public string? Label { get; set; }
    public string? Placeholder { get; set; }
    public string? Icon { get; set; }
    public string? Hint { get; set; }
    public string? Error { get; set; }
    public bool Password { get; set; }
    public bool Disabled { get; set; }
    public bool Clearable { get; set; }
    public bool Autofocus { get; set; }
    public bool ReadOnly { get; set; }
    public ControlSize Size { get; set; }
    public Size Width { get; set; }
    public float Height { get; set; }
    public string? Prefix { get; set; }
    public string? Suffix { get; set; }
}
public enum JobStatus { Pending = 0, Running, Passed, Failed, Skipped, Canceled }
public partial struct JsonOpts
{
    public int ExpandDepth { get; set; }
    public float MaxHeight { get; set; }
}
public enum Justify { Start = 0, Center, End, Between, Around }
public partial struct KanbanColumnOpts
{
    public Color Dot { get; set; }
    public int Limit { get; set; }
    public float Width { get; set; }
}
public partial struct KanbanMove
{
    public int Card { get; set; }
    public int FromColumn { get; set; }
    public int ToColumn { get; set; }
    public int ToIndex { get; set; }
}
public enum Key { None = 0,
    Tab, Enter, Escape, Backspace, Delete, Space,
    Left, Right, Up, Down,
    Home, End, PageUp, PageDown,
    A, B, C, D, E, F, G, H, I,
    J, K, L, M, N, O, P, Q, R,
    S, T, U, V, W, X, Y, Z,
    D0, D1, D2, D3, D4, D5, D6, D7, D8, D9,
    Minus, Equal, Comma, Period, Slash,
    F1, F2, F3, F4, F5, F6,
    F7, F8, F9, F10, F11, F12,
    Count }
public partial struct KeyCombo
{
    public int Mods { get; set; }
    public Key Key { get; set; }
}
public partial struct KeymapBinding
{
    public KeyCombo Combo { get; set; }
    public string? Action { get; set; }
    public int Category { get; set; }
}
public partial struct KeymapOpts
{
    public Size Width { get; set; }
    public ReadOnlyMemory<string> Categories { get; set; }
    public State<int>? Mods { get; set; }
    public bool NoFunctionRow { get; set; }
}
public partial struct KeymapResult
{
    public Key Clicked { get; set; }
    public int Conflicts { get; set; }
}
public partial struct KnobOpts
{
    public string? Label { get; set; }
    public string? Format { get; set; }
    public string? Unit { get; set; }
    public float Size { get; set; }
    public float Step { get; set; }
    public float Reset { get; set; }
    public bool Bipolar { get; set; }
    public bool LogScale { get; set; }
    public bool HideValue { get; set; }
    public bool Disabled { get; set; }
    public Color Color { get; set; }
}
public partial struct KvEditorOpts
{
    public string? KeyPlaceholder { get; set; }
    public string? ValuePlaceholder { get; set; }
    public bool NoToggles { get; set; }
    public bool NoSecrets { get; set; }
    public bool ReadOnly { get; set; }
    public Size Width { get; set; }
}
public partial struct KvPair
{
    public string? Key { get; set; }
    public string? Value { get; set; }
    public bool Enabled { get; set; }
    public bool Secret { get; set; }
}
public partial struct LevelMeterOpts
{
    public float MinDb { get; set; }
    public float MaxDb { get; set; }
    public float Length { get; set; }
    public float Thickness { get; set; }
    public bool Horizontal { get; set; }
    public bool Scale { get; set; }
    public bool Segmented { get; set; }
    public float Hold { get; set; }
    public float Decay { get; set; }
}
public partial struct ListItemOpts
{
    public string? Icon { get; set; }
    public string? Subtitle { get; set; }
    public string? Trailing { get; set; }
    public bool Selected { get; set; }
    public string? Avatar { get; set; }
}
public partial struct LoaderOpts
{
    public LoaderStyle Style { get; set; }
    public float Size { get; set; }
    public Color Color { get; set; }
}
public enum LoaderStyle { LoaderDots = 0, LoaderBars, LoaderPulse, LoaderOrbit, LoaderGrid }
public partial struct MarkdownResult
{
    public string? Link { get; set; }
    public string? HoveredLink { get; set; }
    public int TaskOffset { get; set; }
    public int TaskLine { get; set; }
    public bool TaskChecked { get; set; }
    public float Height { get; set; }
}
public partial struct MenuItemOpts
{
    public string? Icon { get; set; }
    public string? Shortcut { get; set; }
    public bool Danger { get; set; }
    public bool Disabled { get; set; }
    public bool Checked { get; set; }
}
public partial struct MenuOpts
{
    public string? Icon { get; set; }
    public ButtonVariant Variant { get; set; }
    public Placement Placement { get; set; }
    public float Width { get; set; }
    public bool IconOnly { get; set; }
}
public partial struct MetricOpts
{
    public float Delta { get; set; }
    public string? DeltaLabel { get; set; }
    public ReadOnlyMemory<float> Spark { get; set; }
    public string? Icon { get; set; }
    public Size Width { get; set; }
    public bool LowerIsBetter { get; set; }
}
public partial struct Model
{
    public string? Name { get; set; }
    public string? Detail { get; set; }
    public string? Badge { get; set; }
    public string? Icon { get; set; }
}
public partial struct MoneyOpts
{
    public string? Label { get; set; }
    public string? Symbol { get; set; }
    public string? Code { get; set; }
    public int Decimals { get; set; }
    public Size Width { get; set; }
    public string? Hint { get; set; }
}
public partial struct MonthEvent
{
    public string? Title { get; set; }
    public Date Start { get; set; }
    public Date End { get; set; }
    public float Time { get; set; }
    public bool AllDay { get; set; }
    public Tone Tone { get; set; }
}
public partial struct MonthOpts
{
    public State<Date>? Selected { get; set; }
    public int FirstWeekday { get; set; }
    public float CellHeight { get; set; }
    public ReadOnlyMemory<string> MonthNames { get; set; }
    public ReadOnlyMemory<string> DayNames { get; set; }
    public bool Hour12 { get; set; }
    public Date Today { get; set; }
}
public partial struct MonthResult
{
    public int Event { get; set; }
    public bool DayClicked { get; set; }
    public bool DayActivated { get; set; }
    public bool MonthChanged { get; set; }
}
public enum MouseButton { Left = 0, Right = 1, Middle = 2 }
public partial struct MultiSelectOpts
{
    public string? Label { get; set; }
    public string? Placeholder { get; set; }
    public ReadOnlyMemory<string> Icons { get; set; }
    public bool Searchable { get; set; }
    public int MaxChips { get; set; }
    public Size Width { get; set; }
}
public partial struct NavOpts
{
    public string? Icon { get; set; }
    public string? Badge { get; set; }
    public bool Selected { get; set; }
    public string? Group { get; set; }
}
public partial struct NetRequest
{
    public string? Name { get; set; }
    public string? Method { get; set; }
    public int Status { get; set; }
    public string? Type { get; set; }
    public double Size { get; set; }
    public double Start { get; set; }
    public Memory<float> Timing { get; set; }
}
public partial struct NodeEditorOpts
{
    public Size Width { get; set; }
    public Size Height { get; set; }
    public float MinZoom { get; set; }
    public float MaxZoom { get; set; }
    public bool NoGrid { get; set; }
    public bool Minimap { get; set; }
}
public partial struct NodeOpts
{
    public float Width { get; set; }
    public string? Icon { get; set; }
}
public partial struct NodeView
{
    public float X { get; set; }
    public float Y { get; set; }
    public float Zoom { get; set; }
}
public partial struct NowPlaying
{
    public string? Title { get; set; }
    public string? Artist { get; set; }
    public Texture? Art { get; set; }
    public Color ArtColor { get; set; }
    public float Position { get; set; }
    public float Duration { get; set; }
    public bool Playing { get; set; }
}
public partial struct NowPlayingOpts
{
    public Size Width { get; set; }
    public bool Compact { get; set; }
}
public partial struct NumberOpts
{
    public string? Label { get; set; }
    public double Step { get; set; }
    public int Decimals { get; set; }
    public bool Disabled { get; set; }
    public Size Width { get; set; }
    public string? Suffix { get; set; }
}
public partial struct PipelineJob
{
    public string? Name { get; set; }
    public int Stage { get; set; }
    public JobStatus Status { get; set; }
    public float Duration { get; set; }
    public bool AllowFailure { get; set; }
}
public partial struct PipelineOpts
{
    public Size Width { get; set; }
    public State<int>? Selected { get; set; }
}
public partial struct PipelineResult
{
    public int Clicked { get; set; }
    public int Hovered { get; set; }
}
public enum Placement { BottomStart = 0, Bottom, BottomEnd,
    TopStart, Top, TopEnd,
    RightStart, LeftStart, Right, Left }
public partial struct PlanStep
{
    public string? Title { get; set; }
    public string? Detail { get; set; }
    public TaskStatus Status { get; set; }
}
public partial struct PopoverOpts
{
    public Placement Placement { get; set; }
    public float Width { get; set; }
    public float Pad { get; set; }
    public float Gap { get; set; }
}
public partial struct PresenceUser
{
    public string? Name { get; set; }
    public Color Color { get; set; }
    public Texture? Avatar { get; set; }
    public bool Idle { get; set; }
}
public partial struct ProblemsOpts
{
    public Size Width { get; set; }
    public float Height { get; set; }
    public bool NoToolbar { get; set; }
}
public partial struct ProblemsResult
{
    public int Selected { get; set; }
    public int Activated { get; set; }
    public int Shown { get; set; }
}
public partial struct ProgressOpts
{
    public bool Indeterminate { get; set; }
    public Tone Tone { get; set; }
    public float Height { get; set; }
    public string? Label { get; set; }
    public bool ShowValue { get; set; }
    public Size Width { get; set; }
}
public partial struct PromptSuggestion
{
    public string? Icon { get; set; }
    public string? Title { get; set; }
    public string? Detail { get; set; }
}
public partial struct PromptSuggestionsOpts
{
    public int Columns { get; set; }
}
public partial struct QueryField
{
    public string? Name { get; set; }
    public QueryType Type { get; set; }
    public ReadOnlyMemory<string> Choices { get; set; }
}
public partial struct QueryNode
{
    public int Depth { get; set; }
    public bool Group { get; set; }
    public bool Any { get; set; }
    public int Field { get; set; }
    public int Op { get; set; }
    public string? Value { get; set; }
}
public partial struct QueryOpts
{
    public Size Width { get; set; }
    public int MaxDepth { get; set; }
    public bool ReadOnly { get; set; }
}
public enum QueryType { QueryText = 0, QueryNumber, QueryChoice, QueryBool }
public partial struct RadarOpts
{
    public float Size { get; set; }
    public float Max { get; set; }
    public string? Format { get; set; }
    public bool NoLegend { get; set; }
}
public partial struct RadialGaugeOpts
{
    public string? Label { get; set; }
    public string? Format { get; set; }
    public string? Unit { get; set; }
    public float Size { get; set; }
    public float Sweep { get; set; }
    public ReadOnlyMemory<GaugeBand> Bands { get; set; }
    public int Ticks { get; set; }
    public float Target { get; set; }
    public bool ShowTarget { get; set; }
    public bool NoNeedle { get; set; }
}
public partial struct RadioCardsOpts
{
    public int Columns { get; set; }
}
public partial struct Reaction
{
    public string? Icon { get; set; }
    public int Count { get; set; }
    public bool Active { get; set; }
}
public partial struct ReadoutOpts
{
    public string? Unit { get; set; }
    public string? Detail { get; set; }
    public ControlSize Size { get; set; }
    public Tone Tone { get; set; }
    public bool Accent { get; set; }
    public TextAlign Align { get; set; }
    public Size Width { get; set; }
}
public partial struct ReasoningOpts
{
    public bool Thinking { get; set; }
    public float Seconds { get; set; }
    public string? Title { get; set; }
}
public partial struct Rect
{
    public float X { get; set; }
    public float Y { get; set; }
    public float W { get; set; }
    public float H { get; set; }
}
public partial struct RegexTesterOpts
{
    public Size Width { get; set; }
    public float Height { get; set; }
    public State<int>? Flags { get; set; }
}
public partial struct RegexTesterResult
{
    public bool Valid { get; set; }
    public int Matches { get; set; }
    public string? Error { get; set; }
}
public partial struct RequestBarOpts
{
    public Size Width { get; set; }
    public bool Loading { get; set; }
    public string? Placeholder { get; set; }
}
public partial struct RequestBarResult
{
    public bool Send { get; set; }
    public bool Cancel { get; set; }
    public bool Changed { get; set; }
}
public partial struct ResponseActionsOpts
{
    public string? CopyText { get; set; }
    public State<int>? Vote { get; set; }
    public bool NoRegenerate { get; set; }
    public bool NoShare { get; set; }
}
public partial struct RingsOpts
{
    public float Size { get; set; }
    public float Thickness { get; set; }
    public string? Center { get; set; }
    public string? Caption { get; set; }
    public bool Legend { get; set; }
}
public partial struct RulerOpts
{
    public float Step { get; set; }
    public int MajorEvery { get; set; }
    public float Spacing { get; set; }
    public string? Format { get; set; }
    public string? Unit { get; set; }
    public Size Width { get; set; }
}
public partial struct SankeyLink
{
    public int From { get; set; }
    public int To { get; set; }
    public float Value { get; set; }
}
public partial struct SankeyOpts
{
    public Size Width { get; set; }
    public float Height { get; set; }
    public string? Format { get; set; }
    public float NodeWidth { get; set; }
}
public partial struct ScatterOpts
{
    public Size Width { get; set; }
    public float Height { get; set; }
    public string? XLabel { get; set; }
    public string? YLabel { get; set; }
    public string? XFormat { get; set; }
    public string? YFormat { get; set; }
    public bool NoLegend { get; set; }
}
public partial struct ScatterSeries
{
    public string? Name { get; set; }
    public ReadOnlyMemory<Vec2> Points { get; set; }
    public ReadOnlyMemory<float> Sizes { get; set; }
    public Color Color { get; set; }
}
public partial struct ScheduleOpts
{
    public Size Width { get; set; }
    public long Now { get; set; }
    public bool Utc { get; set; }
    public int NextCount { get; set; }
}
public partial struct ScheduleResult
{
    public bool Changed { get; set; }
    public bool Valid { get; set; }
    public long Next { get; set; }
}
public partial struct ScrollOpts
{
    public string? Id { get; set; }
    public Size Width { get; set; }
    public Size Height { get; set; }
    public bool Horizontal { get; set; }
    public float Pad { get; set; }
    public float Gap { get; set; }
    public bool FadeEdges { get; set; }
    public Color FadeColor { get; set; }
}
public partial struct ScrubOpts
{
    public float Step { get; set; }
    public string? Format { get; set; }
    public string? Suffix { get; set; }
    public string? Icon { get; set; }
    public Size Width { get; set; }
}
public partial struct SegmentedOpts
{
    public ControlSize Size { get; set; }
    public bool FullWidth { get; set; }
    public ReadOnlyMemory<string> Icons { get; set; }
}
public partial struct SelectOpts
{
    public string? Label { get; set; }
    public string? Placeholder { get; set; }
    public ReadOnlyMemory<string> Icons { get; set; }
    public bool Searchable { get; set; }
    public bool Disabled { get; set; }
    public Size Width { get; set; }
}
public partial struct Series
{
    public string? Name { get; set; }
    public ReadOnlyMemory<float> Values { get; set; }
    public Color Color { get; set; }
}
public enum Severity { Error = 0, Warning, Info, Hint }
public partial struct SheetOpts
{
    public Side Side { get; set; }
    public float Size { get; set; }
    public string? Description { get; set; }
}
public partial struct ShortcutRecorderOpts
{
    public string? Placeholder { get; set; }
    public Size Width { get; set; }
}
public enum Side { Right = 0, Left, Bottom, Top }
public partial struct SignatureOpts
{
    public Size Width { get; set; }
    public float Height { get; set; }
    public string? Placeholder { get; set; }
    public Color Ink { get; set; }
}
public partial struct Size
{
    public SizeKind Kind { get; set; }
    public float Value { get; set; }
}
public enum SizeKind { Fit = 0, Grow, Fixed, Percent }
public partial struct SliderOpts
{
    public float Step { get; set; }
    public bool ShowValue { get; set; }
    public string? Format { get; set; }
    public bool Disabled { get; set; }
    public Size Width { get; set; }
}
public partial struct Source
{
    public string? Title { get; set; }
    public string? Url { get; set; }
    public string? Snippet { get; set; }
}
public partial struct SpeedAction
{
    public string? Icon { get; set; }
    public string? Label { get; set; }
}
public partial struct SpeedDialOpts
{
    public string? Icon { get; set; }
    public Placement Placement { get; set; }
    public string? Tooltip { get; set; }
}
public partial struct SplitBarOpts
{
    public Size Width { get; set; }
    public float Height { get; set; }
    public string? Format { get; set; }
    public bool NoLegend { get; set; }
}
public partial struct SplitButtonOpts
{
    public ButtonVariant Variant { get; set; }
    public ControlSize Size { get; set; }
    public string? Icon { get; set; }
    public ReadOnlyMemory<string> ItemIcons { get; set; }
    public bool Disabled { get; set; }
}
public partial struct SpotlightOpts
{
    public Size Width { get; set; }
    public Size Height { get; set; }
    public float Pad { get; set; }
    public float Gap { get; set; }
    public Color Color { get; set; }
    public bool Beam { get; set; }
}
public partial struct StackFrame
{
    public string? Function { get; set; }
    public string? File { get; set; }
    public int Line { get; set; }
    public int Column { get; set; }
    public bool External { get; set; }
    public ReadOnlyMemory<string> Source { get; set; }
    public int SourceFirst { get; set; }
}
public partial struct StackTraceOpts
{
    public Size Width { get; set; }
    public string? Title { get; set; }
    public string? Message { get; set; }
    public string? Lang { get; set; }
}
public partial struct StackTraceResult
{
    public int Clicked { get; set; }
}
public enum Status { Idle = 0, Busy, Success, Error }
public partial struct StatusButtonOpts
{
    public string? BusyLabel { get; set; }
    public string? SuccessLabel { get; set; }
    public string? ErrorLabel { get; set; }
    public string? Icon { get; set; }
    public ButtonVariant Variant { get; set; }
    public ControlSize Size { get; set; }
}
public partial struct SwipeAction
{
    public string? Icon { get; set; }
    public string? Label { get; set; }
    public Tone Tone { get; set; }
}
public partial struct TabBarItem
{
    public string? Icon { get; set; }
    public string? Label { get; set; }
    public int Badge { get; set; }
}
public partial struct TabBarOpts
{
    public Size Width { get; set; }
    public bool LabelsOnSelectedOnly { get; set; }
}
public partial struct TableColumn
{
    public string? Title { get; set; }
    public Size Width { get; set; }
    public TextAlign Align { get; set; }
    public bool Sortable { get; set; }
}
public partial struct TableOpts
{
    public State<TableSort>? Sort { get; set; }
    public Size Height { get; set; }
    public bool Striped { get; set; }
    public bool Compact { get; set; }
    public bool Selectable { get; set; }
}
public partial struct TableSort
{
    public int Column { get; set; }
    public bool Descending { get; set; }
}
public partial struct TabsOpts
{
    public TabsVariant Variant { get; set; }
    public ReadOnlyMemory<string> Icons { get; set; }
    public bool FullWidth { get; set; }
}
public enum TabsVariant { Underline = 0, Pill }
public partial struct TagOpts
{
    public string? Label { get; set; }
    public string? Placeholder { get; set; }
    public int MaxTags { get; set; }
    public Size Width { get; set; }
}
public enum TaskStatus { Pending = 0, Running, Done, Failed, Skipped }
public partial struct TerminalLine
{
    public uint Offset { get; set; }
    public uint Len { get; set; }
    public uint Cols { get; set; }
}
public partial struct TerminalOpts
{
    public Size Width { get; set; }
    public float Height { get; set; }
    public float Size { get; set; }
    public string? Prompt { get; set; }
    public bool NoInput { get; set; }
}
public partial struct TerminalResult
{
    public string? Command { get; set; }
    public bool Interrupt { get; set; }
}
public partial struct TestExplorerOpts
{
    public Size Width { get; set; }
    public float Height { get; set; }
}
public partial struct TestExplorerResult
{
    public int Selected { get; set; }
    public int Activated { get; set; }
    public int Run { get; set; }
    public bool RunAll { get; set; }
    public int Passed { get; set; }
    public int Failed { get; set; }
    public int Skipped { get; set; }
    public int Total { get; set; }
}
public partial struct TestItem
{
    public string? Name { get; set; }
    public int Depth { get; set; }
    public JobStatus Status { get; set; }
    public float Duration { get; set; }
    public string? Message { get; set; }
}
public enum TextAlign { Start = 0, Center, Right, Left, End }
public partial struct TextFxOpts
{
    public Font Font { get; set; }
    public float Size { get; set; }
    public Color Color { get; set; }
    public Size Width { get; set; }
    public bool Nowrap { get; set; }
    public float Speed { get; set; }
    public float Delay { get; set; }
    public bool Caret { get; set; }
}
public partial struct TextOpts
{
    public Font Font { get; set; }
    public float Size { get; set; }
    public Color Color { get; set; }
    public TextAlign Align { get; set; }
    public bool Nowrap { get; set; }
    public bool Tabular { get; set; }
    public bool Selectable { get; set; }
    public Size Width { get; set; }
    public float LineHeight { get; set; }
    public Direction Direction { get; set; }
}
public enum ThemeMode { Light = 0, Dark = 1 }
public partial struct TickerItem
{
    public string? Label { get; set; }
    public string? Value { get; set; }
    public float Delta { get; set; }
    public bool LowerIsBetter { get; set; }
}
public partial struct TickerOpts
{
    public float Speed { get; set; }
    public float Height { get; set; }
}
public partial struct TileOpts
{
    public string? Icon { get; set; }
    public string? Detail { get; set; }
    public Color Color { get; set; }
    public Size Width { get; set; }
    public bool Wide { get; set; }
}
public partial struct TimeHistogramOpts
{
    public Size Width { get; set; }
    public float Height { get; set; }
    public bool Utc { get; set; }
}
public partial struct TimeHistogramResult
{
    public bool SelectionChanged { get; set; }
    public int Hovered { get; set; }
}
public partial struct TimeOpts
{
    public string? Label { get; set; }
    public bool TwelveHour { get; set; }
    public int MinuteStep { get; set; }
    public Size Width { get; set; }
}
public partial struct TimelineOpts
{
    public string? Time { get; set; }
    public string? Description { get; set; }
    public string? Icon { get; set; }
    public Tone Tone { get; set; }
    public bool Last { get; set; }
}
public partial struct ToastOpts
{
    public string? Description { get; set; }
    public Tone Tone { get; set; }
    public float Duration { get; set; }
    public string? Action { get; set; }
    public bool ShowTimer { get; set; }
}
public partial struct TocItem
{
    public string? Title { get; set; }
    public string? Anchor { get; set; }
    public int Level { get; set; }
}
public partial struct ToggleGroupOpts
{
    public ReadOnlyMemory<string> Icons { get; set; }
    public bool Exclusive { get; set; }
    public bool IconOnly { get; set; }
    public ControlSize Size { get; set; }
}
public enum Token { None = 0,
    Background, Surface, SurfaceRaised, SurfaceMuted,
    Foreground, TextSecondary, TextMuted,
    Border, BorderSubtle, BorderStrong,
    Accent, AccentStrong, AccentSubtle, AccentForeground,
    Success, Warning, Danger, Info,
    ControlOn, ControlGlyph, ControlTrack,
    ControlTrackHover, ControlThumb, ControlThumbOn,
    Hover, Active, Scrim, FocusRing,
    Series1, Series2, Series3, Series4,
    Series5, Series6,
    Count }
public enum Tone { Neutral = 0, Info, Success, Warning, Danger,
    Violet, Teal, Rose, Accent }
public partial struct ToolCallOpts
{
    public TaskStatus Status { get; set; }
    public string? Icon { get; set; }
    public string? Detail { get; set; }
    public string? Input { get; set; }
    public string? Output { get; set; }
    public float Seconds { get; set; }
}
public partial struct TourOpts
{
    public float Width { get; set; }
    public float Padding { get; set; }
    public bool BlockTarget { get; set; }
    public string? NextLabel { get; set; }
    public string? BackLabel { get; set; }
    public string? DoneLabel { get; set; }
    public string? CountFormat { get; set; }
}
public partial struct TourStep
{
    public Id Target { get; set; }
    public string? Title { get; set; }
    public string? Body { get; set; }
    public Placement Placement { get; set; }
}
public partial struct TraceMarker
{
    public string? Name { get; set; }
    public double Time { get; set; }
    public Tone Tone { get; set; }
}
public partial struct TraceOpts
{
    public Size Width { get; set; }
    public float Height { get; set; }
    public float RowHeight { get; set; }
    public ReadOnlyMemory<TraceMarker> Markers { get; set; }
}
public partial struct TraceResult
{
    public bool SelectionChanged { get; set; }
    public bool ViewChanged { get; set; }
    public int Hovered { get; set; }
}
public partial struct TraceSpan
{
    public string? Name { get; set; }
    public double Start { get; set; }
    public double End { get; set; }
    public int Track { get; set; }
    public int Depth { get; set; }
    public uint ColorKey { get; set; }
}
public partial struct TraceTrack
{
    public string? Name { get; set; }
    public bool Collapsed { get; set; }
}
public partial struct TraceView
{
    public double Start { get; set; }
    public double End { get; set; }
    public float Scroll { get; set; }
    public double MarkStart { get; set; }
    public double MarkEnd { get; set; }
}
public partial struct TreeOpts
{
    public string? Icon { get; set; }
    public bool Leaf { get; set; }
    public bool Selected { get; set; }
}
public partial struct TreemapOpts
{
    public Size Width { get; set; }
    public float Height { get; set; }
    public string? Format { get; set; }
}
public partial struct UptimeOpts
{
    public Size Width { get; set; }
    public Date LastDay { get; set; }
}
public partial struct UptimeResult
{
    public int Service { get; set; }
    public int Day { get; set; }
}
public partial struct UptimeService
{
    public string? Name { get; set; }
    public ReadOnlyMemory<float> Days { get; set; }
}
public partial struct UserMenuOpts
{
    public string? Detail { get; set; }
    public string? Status { get; set; }
    public Texture? Avatar { get; set; }
    public bool Compact { get; set; }
    public Placement Placement { get; set; }
    public float Width { get; set; }
}
public partial struct Vec2
{
    public float X { get; set; }
    public float Y { get; set; }
}
public partial struct VizItem
{
    public string? Label { get; set; }
    public float Value { get; set; }
    public Color Color { get; set; }
}
public partial struct VoiceOrbOpts
{
    public float Size { get; set; }
    public Color Color { get; set; }
}
public enum VoiceState { Idle = 0, Listening, Thinking, Speaking }
public partial struct WaffleOpts
{
    public float Cell { get; set; }
    public int Columns { get; set; }
    public int Rows { get; set; }
    public string? Format { get; set; }
}
public partial struct WaterfallOpts
{
    public Size Width { get; set; }
    public float Height { get; set; }
    public State<int>? Selected { get; set; }
    public double DomLoaded { get; set; }
    public double Loaded { get; set; }
}
public partial struct WaterfallResult
{
    public int Clicked { get; set; }
    public int Activated { get; set; }
    public int Hovered { get; set; }
}
public partial struct WaveformOpts
{
    public Size Width { get; set; }
    public float Height { get; set; }
    public float Bar { get; set; }
    public float Gap { get; set; }
    public Color Color { get; set; }
}
public partial struct WeekOpts
{
    public float StartHour { get; set; }
    public float EndHour { get; set; }
    public float HourHeight { get; set; }
    public Size Height { get; set; }
    public bool ReadOnly { get; set; }
}
public partial struct WheelOpts
{
    public int Rows { get; set; }
    public Size Width { get; set; }
    public bool Loop { get; set; }
    public bool JoinLeft { get; set; }
    public bool JoinRight { get; set; }
}
