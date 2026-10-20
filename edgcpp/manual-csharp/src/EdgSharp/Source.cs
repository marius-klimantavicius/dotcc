// SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
// Handwritten adaptation of EDG source-position and lexical-state contracts.
using System.Collections.ObjectModel;

namespace EdgSharp;

/// <summary>Immutable source text. Offsets and columns count UTF-16 code units; ranges are half open.</summary>
public sealed class SourceFile
{
    private readonly int[] lineStarts;
    public string Name { get; }
    public string Text { get; }
    internal SourceFile(string name, string text)
    {
        Name = name;
        Text = text;
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

public enum DiagnosticClass { InvalidSource, UnsupportedFeature }
public enum DiagnosticSeverity { Error, Warning }

public sealed record Diagnostic(string Code, DiagnosticClass Class, string Message, SourceRange Range,
    DiagnosticSeverity Severity = DiagnosticSeverity.Error);

internal sealed class DiagnosticBag
{
    private readonly List<Diagnostic> items = [];
    internal int Count => items.Count;
    internal void Add(string code, string message, SourceRange range, DiagnosticClass kind = DiagnosticClass.InvalidSource)
        => items.Add(new Diagnostic(code, kind, message, range));
    internal void Truncate(int count) => items.RemoveRange(count, items.Count - count);
    internal ReadOnlyCollection<Diagnostic> Snapshot() => Array.AsReadOnly(items.ToArray());
}
