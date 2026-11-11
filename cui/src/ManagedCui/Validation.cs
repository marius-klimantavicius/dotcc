using System.Globalization;

namespace Cui.Managed;

public sealed unsafe partial class Ui
{
    private static T CheckEnum<T>(T value) where T : struct, Enum
    {
        if (!Enum.IsDefined(value)) throw new ArgumentOutOfRangeException(nameof(value));
        return value;
    }
    private static Accent CheckEnum(Accent value)
    {
        if ((uint)value >= (uint)Accent.Count) throw new ArgumentOutOfRangeException(nameof(value));
        return value;
    }
    private static void CheckRange(double minimum, double maximum)
    {
        if (!double.IsFinite(minimum) || !double.IsFinite(maximum) || minimum > maximum)
            throw new ArgumentOutOfRangeException(nameof(minimum));
    }
    private static void CheckOptional(int count, int length, string name)
    {
        if (length != 0) CheckLength(count, length, name);
    }
    private void RequireScope(ScopeKind kind)
    {
        foreach (var scope in scopes) if (scope.Kind == kind) return;
        throw new InvalidOperationException("This component requires its matching container.");
    }
    private static void Validate(ScatterSeries value) => CheckOptional(value.Points.Length, value.Sizes.Length, nameof(value.Sizes));
    private static void Validate(EmojiPickerOpts value) => CheckOptional(10, value.GroupNames.Length, nameof(value.GroupNames));
    private static void Validate(MonthOpts value)
    {
        CheckOptional(12, value.MonthNames.Length, nameof(value.MonthNames));
        CheckOptional(7, value.DayNames.Length, nameof(value.DayNames));
    }
    private static void Validate(FileBrowserOpts value)
    {
        if (value.MaxEntries < 0 || value.MaxEntries > 65536) throw new ArgumentOutOfRangeException(nameof(value.MaxEntries));
    }
    private static void Validate(Date value)
    {
        if (value.Year == 0 && value.Month == 0 && value.Day == 0) return;
        if (value.Year < 1 || value.Year > 9999 || value.Month < 1 || value.Month > 12 || value.Day < 0 || value.Day > DateTime.DaysInMonth(value.Year, value.Month))
            throw new ArgumentOutOfRangeException(nameof(value));
    }
    private static void Validate(Size value)
    {
        if (!float.IsFinite(value.Value) || value.Value < 0) throw new ArgumentOutOfRangeException(nameof(value.Value));
    }
    private static void ValidateDotChart(ReadOnlySpan<DotSeries> series, int seriesCount, int count)
    {
        CheckLength(seriesCount, series.Length, nameof(seriesCount));
        if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));
        for (int i = 0; i < seriesCount; i++) CheckOptional(count, series[i].Values.Length, nameof(series));
    }
    private static void ValidateHistogram(ReadOnlySpan<HistogramSeries> series, int seriesCount, int buckets)
    {
        CheckLength(seriesCount, series.Length, nameof(seriesCount));
        if (buckets < 0) throw new ArgumentOutOfRangeException(nameof(buckets));
        for (int i = 0; i < seriesCount; i++) CheckOptional(buckets, series[i].Counts.Length, nameof(series));
    }
    private static void ValidateGrid(GridOpts opts, int rows)
    {
        if (rows < 0) throw new ArgumentOutOfRangeException(nameof(rows));
        CheckOptional(rows, opts.Selected.Length, nameof(opts.Selected));
    }
    private static void ValidateHeatmap(HeatmapOpts opts, int rows, int cols)
    {
        if (rows < 0 || cols < 0) throw new ArgumentOutOfRangeException(nameof(rows));
        CheckOptional(rows, opts.RowLabels.Length, nameof(opts.RowLabels));
        CheckOptional(cols, opts.ColLabels.Length, nameof(opts.ColLabels));
    }
    private static void ValidateEdges(ReadOnlySpan<GraphEdge> edges, int count, int nodes)
    {
        CheckLength(count, edges.Length, nameof(count));
        for (int i = 0; i < count; i++)
            if ((uint)edges[i].From >= (uint)nodes || (uint)edges[i].To >= (uint)nodes) throw new ArgumentOutOfRangeException(nameof(edges));
    }
    private static void ValidateLinks(ReadOnlySpan<SankeyLink> edges, int count, int nodes)
    {
        CheckLength(count, edges.Length, nameof(count));
        for (int i = 0; i < count; i++)
            if ((uint)edges[i].From >= (uint)nodes || (uint)edges[i].To >= (uint)nodes) throw new ArgumentOutOfRangeException(nameof(edges));
    }
    private static void ValidateTracks(ReadOnlySpan<TraceSpan> spans, int count, int tracks)
    {
        CheckLength(count, spans.Length, nameof(count));
        for (int i = 0; i < count; i++)
            if ((uint)spans[i].Track >= (uint)tracks || spans[i].Depth < 0) throw new ArgumentOutOfRangeException(nameof(spans));
    }
    private static void ValidateOrder(ReadOnlySpan<int> order, int count)
    {
        if (count > ManagedCuiInfo.ReorderCapacity) throw new ArgumentOutOfRangeException(nameof(count), "CUI supports at most 256 reorder items.");
        CheckLength(count, order.Length, nameof(count));
        for (int i = 0; i < count; i++)
        {
            for (int j = 0; j < i; j++) if (order[i] == order[j]) throw new ArgumentException("Reorder entries must be a permutation.", nameof(order));
        }
    }
    private static void ValidateQueryNodes(ReadOnlySpan<QueryNode> nodes, int fieldCount)
    {
        foreach (var node in nodes)
            if (!node.Group && (node.Field < 0 || (fieldCount > 0 && node.Field >= fieldCount)))
                throw new ArgumentOutOfRangeException(nameof(nodes), "A query rule must reference an available field.");
    }
    private static string? CheckFormat(string? format, int arguments)
    {
        if (format == null) return null;
        int conversions = 0;
        for (int i = 0; i < format.Length; i++)
        {
            if (format[i] != '%') continue;
            if (++i < format.Length && format[i] == '%') continue;
            if (++conversions > arguments) throw new ArgumentException("Too many printf conversions.", nameof(format));
            int width = 0;
            while (i < format.Length && "-+ #0.123456789".Contains(format[i]))
            {
                if (char.IsAsciiDigit(format[i])) width = checked(width * 10 + format[i] - '0');
                else width = 0;
                if (width > 1024) throw new ArgumentException("Format width or precision exceeds 1024.", nameof(format));
                i++;
            }
            if (i >= format.Length || !(arguments == 2 ? "di" : "fFeEgGaAdiuoxX").Contains(format[i]))
                throw new ArgumentException("Only bounded numeric printf conversions are supported.", nameof(format));
        }
        return format;
    }
}
