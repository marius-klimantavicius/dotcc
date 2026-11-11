using System.Buffers;
using System.Text;

namespace Cui.Managed;

internal static class Utf8Validation
{
    internal static void Check(ReadOnlySpan<byte> text)
    {
        while (!text.IsEmpty)
        {
            if (text[0] == 0 || Rune.DecodeFromUtf8(text, out _, out var consumed) != OperationStatus.Done)
                throw new ArgumentException("Supply valid UTF-8 without embedded NUL.", nameof(text));
            text = text[consumed..];
        }
    }
}

public sealed unsafe partial class Ui
{
    // Stack-only carriers share the reviewed implementation between mixed
    // string/UTF-8/buffer overloads. They never escape the public call.
    private readonly ref struct Utf8Argument
    {
        internal readonly string? Text;
        internal readonly ReadOnlySpan<byte> Bytes;
        internal readonly TextBuffer? Buffer;
        private readonly bool present;
        internal Utf8Argument(string text) { Text = text; present = text != null; }
        internal Utf8Argument(ReadOnlySpan<byte> bytes) { Bytes = bytes; present = true; }
        internal Utf8Argument(TextBuffer buffer) { Buffer = buffer; present = buffer != null; }
        internal void ThrowIfNull(string name) { if (!present) throw new ArgumentNullException(name); }
        internal ReadOnlySpan<byte> GetBytes() => Buffer is null ? Bytes : Buffer.ReadUtf8();
    }

    private readonly ref struct Utf8Arguments
    {
        private readonly ReadOnlySpan<string> strings;
        private readonly ReadOnlySpan<ReadOnlyMemory<byte>> bytes;
        private readonly ReadOnlySpan<TextBuffer> buffers;
        private readonly byte kind;
        internal Utf8Arguments(ReadOnlySpan<string> values) { strings = values; }
        internal Utf8Arguments(ReadOnlySpan<ReadOnlyMemory<byte>> values) { bytes = values; kind = 1; }
        internal Utf8Arguments(ReadOnlySpan<TextBuffer> values) { buffers = values; kind = 2; }
        internal int Length => kind switch { 0 => strings.Length, 1 => bytes.Length, _ => buffers.Length };
        internal Utf8Argument this[int index] => kind switch
        {
            0 => new(strings[index]), 1 => new(bytes[index].Span), _ => new(buffers[index])
        };
    }

    private byte* Utf8(Utf8Argument text)
    {
        text.ThrowIfNull(nameof(text));
        return text.Text is not null ? Utf8(text.Text) : CopyUtf8(text.GetBytes());
    }
    private byte* Utf8Required(Utf8Argument text) => Utf8(text);
    private byte** Strings(Utf8Arguments values)
    {
        var result = (byte**)arena.Allocate(checked(values.Length * sizeof(byte*)));
        for (int i = 0; i < values.Length; i++) result[i] = Utf8(values[i]);
        return result;
    }
    private static void CheckUtf8Length(Utf8Argument text, int length)
    {
        text.ThrowIfNull(nameof(text));
        int available = text.Text is not null ? Encoding.UTF8.GetByteCount(text.Text) : text.GetBytes().Length;
        if (length < -1 || length > available) throw new ArgumentOutOfRangeException(nameof(length));
    }
    private byte* CopyUtf8(ReadOnlySpan<byte> text)
    {
        Utf8Validation.Check(text);
        var result = (byte*)arena.Allocate(checked(text.Length + 1));
        text.CopyTo(new Span<byte>(result, text.Length)); result[text.Length] = 0;
        return result;
    }
    /// <summary>Copies validated UTF-8 into stable frame storage, with no managed encoding allocation.</summary>
    public void TextUtf8(ReadOnlySpan<byte> text, TextOpts opts = default)
    {
        Text(text, opts);
    }
    /// <summary>Builds a button from validated UTF-8. Input storage may be stack-backed.</summary>
    public bool ButtonUtf8(ReadOnlySpan<byte> label, ButtonOpts opts = default)
    {
        return Button(label, opts);
    }
}
