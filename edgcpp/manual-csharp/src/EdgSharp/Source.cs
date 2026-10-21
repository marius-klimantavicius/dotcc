// SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
// Handwritten adaptation of EDG source-position and lexical-state contracts.
using System.Collections.ObjectModel;

namespace EdgSharp;

public enum SourceFileKind { Physical, LogicalInput, PreprocessedPresentation, SynthesizedToken, Configuration }
public enum OriginalSourceEncoding { ManagedUtf16, Utf8, Utf16LittleEndian, Utf16BigEndian }
public enum UnicodeSourcePolicy { Unicode, LegacyNonUnicode, NativeMultibyte }
public enum ExecutionCharacterEncoding { Utf8, LegacyNative }

/// <summary>Immutable interpretation of decoded source scalars, separate from the original file encoding and target character widths.</summary>
public sealed class SourceInputOptions
{
    public UnicodeSourcePolicy UnicodePolicy { get; }
    /// <summary>The implemented ordinary-character execution encoding; string-literal value conversion remains unsupported.</summary>
    public ExecutionCharacterEncoding ExecutionEncoding { get; }
    public SourceInputOptions(UnicodeSourcePolicy unicodePolicy = UnicodeSourcePolicy.Unicode,
        ExecutionCharacterEncoding executionEncoding = ExecutionCharacterEncoding.Utf8)
    {
        if (!Enum.IsDefined(unicodePolicy)) throw new ArgumentOutOfRangeException(nameof(unicodePolicy));
        if (!Enum.IsDefined(executionEncoding)) throw new ArgumentOutOfRangeException(nameof(executionEncoding));
        (UnicodePolicy, ExecutionEncoding) = (unicodePolicy, executionEncoding);
    }
}

/// <summary>Retained per-source provenance. Managed strings have no original byte encoding or byte-order mark; their UTF-16 storage is decoded Unicode source.</summary>
public sealed class SourceEncodingInfo
{
    public OriginalSourceEncoding OriginalEncoding { get; }
    public bool HasByteOrderMark { get; }
    public UnicodeSourcePolicy UnicodePolicy { get; }
    /// <summary>Configured ordinary-character execution policy, separate from original input encoding. This does not promise string-literal value support.</summary>
    public ExecutionCharacterEncoding ExecutionEncoding { get; }
    public SourceEncodingInfo(OriginalSourceEncoding originalEncoding = OriginalSourceEncoding.ManagedUtf16,
        bool hasByteOrderMark = false, UnicodeSourcePolicy unicodePolicy = UnicodeSourcePolicy.Unicode,
        ExecutionCharacterEncoding executionEncoding = ExecutionCharacterEncoding.Utf8)
    {
        if (!Enum.IsDefined(originalEncoding)) throw new ArgumentOutOfRangeException(nameof(originalEncoding));
        if (!Enum.IsDefined(unicodePolicy)) throw new ArgumentOutOfRangeException(nameof(unicodePolicy));
        if (!Enum.IsDefined(executionEncoding)) throw new ArgumentOutOfRangeException(nameof(executionEncoding));
        if (originalEncoding == OriginalSourceEncoding.ManagedUtf16 && hasByteOrderMark)
            throw new ArgumentException("Managed strings do not carry a byte-order mark.", nameof(hasByteOrderMark));
        (OriginalEncoding, HasByteOrderMark, UnicodePolicy, ExecutionEncoding) = (originalEncoding, hasByteOrderMark, unicodePolicy, executionEncoding);
    }
    internal static SourceEncodingInfo Managed(SourceInputOptions options)
        => new(unicodePolicy: options.UnicodePolicy, executionEncoding: options.ExecutionEncoding);
    internal bool Validate(SourceFile source, DiagnosticBag diagnostics)
    {
        if (UnicodePolicy != UnicodeSourcePolicy.Unicode || ExecutionEncoding != ExecutionCharacterEncoding.Utf8)
        {
            diagnostics.Add("UNS030", "Legacy non-Unicode/native multibyte source or execution encoding is not implemented.", source.Range(0, 0), DiagnosticClass.UnsupportedFeature);
            return false;
        }
        bool valid = true;
        for (int offset = 0; offset < source.Text.Length;)
        {
            if (!UnicodeIdentifiers.ReadScalar(source.Text, offset, out _, out int length))
            {
                diagnostics.Add("SRC001", "Managed source text contains an unpaired UTF-16 surrogate.", source.Range(offset, 1));
                valid = false;
            }
            offset += Math.Max(1, length);
        }
        return valid;
    }
}
public readonly record struct PresumedPosition(string FileName, int Line, int Column);
public enum TokenOriginKind { Direct, MacroReplacement, MacroArgument, TokenPaste, Stringization, Predefined }

public sealed class IncludeFrame
{
    public SourceFile File { get; }
    public TokenOrigin Directive { get; }
    public IncludeFrame? Parent { get; }
    internal IncludeFrame(SourceFile file, TokenOrigin directive, IncludeFrame? parent)
        => (File, Directive, Parent) = (file, directive, parent);
}
public sealed class MacroExpansionFrame
{
    public string Name { get; }
    public SourceRange Definition { get; }
    public TokenOrigin Invocation { get; }
    public MacroExpansionFrame? Parent { get; }
    internal MacroExpansionFrame(string name, SourceRange definition, TokenOrigin invocation, MacroExpansionFrame? parent)
        => (Name, Definition, Invocation, Parent) = (name, definition, invocation, parent);
}

/// <summary>Immutable spelling contributors, expansion history, include activation and presumed location; none is a presentation offset.</summary>
public sealed class TokenOrigin
{
    public ReadOnlyCollection<SourceRange> SpellingRanges { get; }
    public TokenOriginKind Kind { get; }
    public MacroExpansionFrame? Expansion { get; }
    public IncludeFrame? Include { get; }
    public PresumedPosition Presumed { get; }
    public ReadOnlyCollection<TokenOrigin> RelatedOrigins { get; }
    public SourceRange PrimaryPhysicalRange => Expansion?.Invocation.PrimaryPhysicalRange ?? SpellingRanges[0];
    internal TokenOrigin(IEnumerable<SourceRange> ranges, PresumedPosition presumed, IncludeFrame? include = null,
        MacroExpansionFrame? expansion = null, TokenOriginKind kind = TokenOriginKind.Direct, IEnumerable<TokenOrigin>? related = null)
    {
        SourceRange[] array = ranges.ToArray();
        if (array.Length == 0) throw new ArgumentException("Every origin requires a retained source contributor.", nameof(ranges));
        (SpellingRanges, Presumed, Include, Expansion, Kind) = (Array.AsReadOnly(array), presumed, include, expansion, kind);
        RelatedOrigins = Array.AsReadOnly((related ?? []).ToArray());
    }
    internal static TokenOrigin Direct(SourceRange range)
    {
        SourcePosition position = range.StartPosition;
        return new([range], new PresumedPosition(range.File.Name, position.Line, position.Column));
    }
    internal TokenOrigin WithContext(PresumedPosition position, IncludeFrame? include)
        => new(SpellingRanges, position, include, Expansion, Kind, RelatedOrigins);
    internal TokenOrigin Expanded(MacroExpansionFrame frame, TokenOriginKind kind = TokenOriginKind.MacroReplacement)
        => new(SpellingRanges, frame.Invocation.Presumed, Include, frame, kind, [this]);
    public ReadOnlyCollection<MacroExpansionFrame> ExpansionHistory
    {
        get
        {
            var result = new List<MacroExpansionFrame>();
            var seenFrames = new HashSet<MacroExpansionFrame>();
            var seenOrigins = new HashSet<TokenOrigin>();
            var pending = new Stack<TokenOrigin>();
            pending.Push(this);
            while (pending.TryPop(out TokenOrigin? origin))
            {
                if (!seenOrigins.Add(origin)) continue;
                for (MacroExpansionFrame? frame = origin.Expansion; frame != null; frame = frame.Parent)
                    if (seenFrames.Add(frame)) { result.Add(frame); pending.Push(frame.Invocation); }
                for (int i = origin.RelatedOrigins.Count - 1; i >= 0; --i) pending.Push(origin.RelatedOrigins[i]);
            }
            return result.AsReadOnly();
        }
    }
    public ReadOnlyCollection<string> ExpansionNotes
    {
        get
        {
            var notes = new List<string>();
            foreach (MacroExpansionFrame frame in ExpansionHistory)
                notes.Add($"expanded from macro '{frame.Name}' at {frame.Invocation.Presumed.FileName}:{frame.Invocation.Presumed.Line}:{frame.Invocation.Presumed.Column}");
            var seenOrigins = new HashSet<TokenOrigin>();
            var seenIncludes = new HashSet<IncludeFrame>();
            var pending = new Stack<TokenOrigin>();
            pending.Push(this);
            while (pending.TryPop(out TokenOrigin? origin))
            {
                if (!seenOrigins.Add(origin)) continue;
                for (IncludeFrame? frame = origin.Include; frame != null; frame = frame.Parent)
                    if (seenIncludes.Add(frame))
                    {
                        notes.Add($"included from {frame.Directive.Presumed.FileName}:{frame.Directive.Presumed.Line}:{frame.Directive.Presumed.Column}");
                        pending.Push(frame.Directive);
                    }
                for (MacroExpansionFrame? frame = origin.Expansion; frame != null; frame = frame.Parent) pending.Push(frame.Invocation);
                for (int i = origin.RelatedOrigins.Count - 1; i >= 0; --i) pending.Push(origin.RelatedOrigins[i]);
            }
            return notes.AsReadOnly();
        }
    }
}
internal readonly record struct SourceMapSegment(int Start, int Length, TokenOrigin Origin);

/// <summary>Immutable source text. Offsets and columns count UTF-16 code units; ranges are half open.</summary>
public sealed class SourceFile
{
    private readonly int[] lineStarts;
    private readonly SourceMapSegment[] originMap;
    public string Name { get; }
    public string Text { get; }
    public SourceFileKind Kind { get; }
    public SourceEncodingInfo Encoding { get; }
    internal SourceFile(string name, string text, SourceFileKind kind = SourceFileKind.Physical, SourceMapSegment[]? origins = null,
        SourceEncodingInfo? encoding = null)
    {
        Name = name;
        Text = text;
        Kind = kind;
        Encoding = encoding ?? new SourceEncodingInfo();
        originMap = origins == null ? [] : (SourceMapSegment[])origins.Clone();
        var starts = new List<int> { 0 };
        for (int i = 0; i < text.Length; ++i)
        {
            if (text[i] == '\r')
            {
                if (i + 1 < text.Length && text[i + 1] == '\n') ++i;
                starts.Add(i + 1);
            }
            else if (text[i] == '\n') starts.Add(i + 1);
        }
        lineStarts = starts.ToArray();
    }
    public SourcePosition PositionAt(int offset)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(offset);
        if (offset > Text.Length) throw new ArgumentOutOfRangeException(nameof(offset));
        int line = Array.BinarySearch(lineStarts, offset);
        if (line < 0) line = ~line - 1;
        return new SourcePosition(this, offset, line + 1, offset - lineStarts[line] + 1);
    }
    public SourceRange Range(int start, int length) => new(this, start, length);
    internal int OffsetFromPhysical(SourceFile physical, int offset)
    {
        if (ReferenceEquals(this, physical)) return offset;
        foreach (SourceMapSegment segment in originMap)
            foreach (SourceRange contributor in segment.Origin.SpellingRanges)
                if (ReferenceEquals(contributor.File, physical) && offset >= contributor.Start && offset <= contributor.End && contributor.Length == segment.Length)
                    return segment.Start + offset - contributor.Start;
        if (originMap.Length != 0 && offset == physical.Text.Length) return Text.Length;
        throw new InvalidOperationException("Physical position is not represented in this immutable logical buffer.");
    }
    /// <summary>Maps a range in this buffer to retained source contributors. A presentation range is never a physical source range.</summary>
    public TokenOrigin OriginForRange(SourceRange range)
    {
        if (!ReferenceEquals(range.File, this)) throw new ArgumentException("Range belongs to another source buffer.", nameof(range));
        if (originMap.Length == 0) return TokenOrigin.Direct(range);
        if (range.Length == 0)
        {
            // Boundary anchors have forward bias: a splice boundary belongs to
            // the next retained run, while the terminal map anchors physical EOF.
            SourceMapSegment selected = originMap[0];
            foreach (SourceMapSegment segment in originMap)
            { if (segment.Start > range.Start) break; selected = segment; }
            if (Kind == SourceFileKind.LogicalInput && selected.Origin.SpellingRanges.Count == 1 &&
                selected.Origin.SpellingRanges[0].Length == selected.Length)
            {
                SourceRange physical = selected.Origin.SpellingRanges[0];
                int offset = Math.Clamp(range.Start - selected.Start, 0, selected.Length);
                return TokenOrigin.Direct(physical.File.Range(physical.Start + offset, 0));
            }
            return selected.Origin;
        }
        var contributors = new List<SourceRange>();
        var coveredOrigins = new List<TokenOrigin>();
        TokenOrigin? first = null;
        foreach (SourceMapSegment segment in originMap)
        {
            if (segment.Start > range.End || range.Length != 0 && segment.Start == range.End) break;
            if (segment.Start + segment.Length < range.Start || segment.Start + segment.Length == range.Start && range.Length != 0) continue;
            first ??= segment.Origin;
            coveredOrigins.Add(segment.Origin);
            if (segment.Start == range.Start && segment.Length == range.Length) return segment.Origin;
            if (Kind == SourceFileKind.LogicalInput && segment.Origin.SpellingRanges.Count == 1)
            {
                SourceRange physical = segment.Origin.SpellingRanges[0];
                int from = Math.Max(range.Start, segment.Start), to = Math.Min(range.End, segment.Start + segment.Length);
                if (to >= from) contributors.Add(physical.Length == segment.Length ? physical.File.Range(physical.Start + from - segment.Start, to - from) : physical);
            }
            else contributors.AddRange(segment.Origin.SpellingRanges);
        }
        if (first == null)
        {
            SourceMapSegment nearest = originMap.Last(segment => segment.Start <= range.Start);
            return nearest.Origin;
        }
        PresumedPosition presumed = first.Presumed;
        if (Kind == SourceFileKind.LogicalInput && contributors.Count != 0)
        {
            SourcePosition physical = contributors[0].StartPosition;
            presumed = new(physical.File.Name, physical.Line, physical.Column);
        }
        return new TokenOrigin(contributors.Count == 0 ? first.SpellingRanges : contributors,
            presumed, first.Include, first.Expansion, first.Kind, coveredOrigins.Distinct());
    }
}

public readonly record struct SourcePosition(SourceFile File, int Offset, int Line, int Column);

public readonly record struct SourceRange
{
    public SourceFile File { get; }
    public int Start { get; }
    public int Length { get; }
    public int End => Start + Length;
    public SourcePosition StartPosition => File.PositionAt(Start);
    public SourcePosition EndPosition => File.PositionAt(End);
    public string Spelling => File.Text.Substring(Start, Length);
    public SourceRange(SourceFile file, int start, int length)
    {
        ArgumentNullException.ThrowIfNull(file);
        if (start < 0 || length < 0 || start > file.Text.Length - length)
            throw new ArgumentOutOfRangeException(nameof(start));
        File = file;
        Start = start;
        Length = length;
    }
    internal static SourceRange Cover(SourceRange first, SourceRange last)
    {
        if (!ReferenceEquals(first.File, last.File)) throw new InvalidOperationException("Cross-file range.");
        return new SourceRange(first.File, first.Start, Math.Max(first.End, last.End) - first.Start);
    }
}

public enum DiagnosticClass { InvalidSource, UnsupportedFeature, ResourceLimit, InputFailure }
public enum DiagnosticSeverity { Error, Warning }

public sealed record Diagnostic(string Code, DiagnosticClass Class, string Message, SourceRange Range,
    DiagnosticSeverity Severity = DiagnosticSeverity.Error)
{
    public TokenOrigin Origin => Range.File.OriginForRange(Range);
    public PresumedPosition PrimaryLocation => Origin.Presumed;
    public ReadOnlyCollection<string> ExpansionNotes => Origin.ExpansionNotes;
}

internal sealed class DiagnosticBag
{
    private readonly List<Diagnostic> items = [];
    internal int Count => items.Count;
    internal int ErrorCount => items.Count(item => item.Severity == DiagnosticSeverity.Error);
    internal void Add(string code, string message, SourceRange range, DiagnosticClass kind = DiagnosticClass.InvalidSource, DiagnosticSeverity severity = DiagnosticSeverity.Error)
        => items.Add(new Diagnostic(code, kind, message, range, severity));
    internal void Truncate(int count) => items.RemoveRange(count, items.Count - count);
    internal ReadOnlyCollection<Diagnostic> Snapshot() => Array.AsReadOnly(items.ToArray());
}
