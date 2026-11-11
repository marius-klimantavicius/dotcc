using System.Buffers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Unicode;

namespace Cui.Managed;

internal static class Utf8Validation
{
    internal static void Check(ReadOnlySpan<byte> text)
    {
        if (!Utf8.IsValid(text) || text.Contains((byte)0))
            throw new ArgumentException("Supply valid UTF-8 without embedded NUL.", nameof(text));
    }
}

public sealed unsafe partial class Ui
{
    // Stack-only carriers share the reviewed implementation between mixed
    // string/UTF-8/buffer overloads. They never escape the public call.
    internal readonly ref struct Utf8Argument
    {
        internal readonly string? Text;
        internal readonly ReadOnlySpan<byte> Bytes;
        internal readonly TextBuffer? Buffer;
        private readonly bool _present;

        internal Utf8Argument(string text)
        {
            Text = text;
            _present = text != null;
        }

        internal Utf8Argument(ReadOnlySpan<byte> bytes)
        {
            Bytes = bytes;
            _present = true;
        }

        internal Utf8Argument(TextBuffer buffer)
        {
            Buffer = buffer;
            _present = buffer != null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void ThrowIfNull(string name)
        {
            if (!_present)
                Throw(name);

            [MethodImpl(MethodImplOptions.NoInlining)]
            static void Throw(string name) => throw new ArgumentNullException(name);
        }

        internal ReadOnlySpan<byte> GetBytes() => Buffer is null ? Bytes : Buffer.ReadUtf8();

        public static implicit operator Utf8Argument(string text) => new Utf8Argument(text);
    }

    // Only used for entrypoints whose scalar input lifetime has been audited.
    // The caller owns the stack scratch and pins Bytes across the native call.
    private readonly ref struct ScopedUtf8
    {
        private readonly byte[]? _rented;
        private readonly TextBuffer? _owner;
        internal readonly ReadOnlySpan<byte> Bytes;

        internal ScopedUtf8(string text, Span<byte> scratch, bool nullTerminated = true)
            : this(new Utf8Argument(text), scratch, nullTerminated)
        {
        }

        internal ScopedUtf8(Utf8Argument text, Span<byte> scratch, bool nullTerminated = true)
        {
            text.ThrowIfNull(nameof(text));
            if (text.Text is not null)
            {
                Bytes = Encode(text.Text.AsSpan(), scratch, out _rented);
                return;
            }

            if (text.Buffer is not null)
            {
                // A callback may dispose the owner during the call. A temporary
                // read lease keeps its native storage alive without binding it
                // to this UI or extending the lease until the next frame.
                Bytes = text.Buffer.AcquireReadUtf8();
                try { Utf8Validation.Check(Bytes[..^1]); }
                catch
                {
                    text.Buffer.Release();
                    throw;
                }

                _owner = text.Buffer;
                return;
            }

            var bytes = text.Bytes;
            var terminated = !bytes.IsEmpty && bytes[^1] == 0;
            Utf8Validation.Check(terminated ? bytes[..^1] : bytes);
            if (!nullTerminated || terminated)
            {
                Bytes = bytes;
                return;
            }

            var required = checked(bytes.Length + 1);
            var destination = required <= scratch.Length ? scratch : (_rented = ArrayPool<byte>.Shared.Rent(required));
            bytes.CopyTo(destination);
            destination[bytes.Length] = 0;
            Bytes = destination[..required];
        }

        internal ScopedUtf8(ReadOnlySpan<char> text, Span<byte> scratch)
        {
            Bytes = Encode(text, scratch, out _rented);
        }

        private static ReadOnlySpan<byte> Encode(ReadOnlySpan<char> text, Span<byte> scratch, out byte[]? rented)
        {
            if (text.Contains('\0')) throw new ArgumentException("CUI strings cannot contain NUL.", nameof(text));

            var count = Encoding.UTF8.GetByteCount(text);
            var required = checked(count + 1);
            rented = required <= scratch.Length ? null : ArrayPool<byte>.Shared.Rent(required);
            var destination = rented ?? scratch;
            Encoding.UTF8.GetBytes(text, destination);
            destination[count] = 0;
            return destination[..required];
        }

        public void Dispose()
        {
            _owner?.Release();
            if (_rented is not null) ArrayPool<byte>.Shared.Return(_rented);
        }
    }

    private readonly ref struct Utf8Arguments
    {
        private readonly ReadOnlySpan<string> _strings;
        private readonly ReadOnlySpan<ReadOnlyMemory<byte>> _bytes;
        private readonly ReadOnlySpan<TextBuffer> _buffers;
        private readonly byte _kind;
        internal Utf8Arguments(ReadOnlySpan<string> values) { _strings = values; }

        internal Utf8Arguments(ReadOnlySpan<ReadOnlyMemory<byte>> values)
        {
            _bytes = values;
            _kind = 1;
        }

        internal Utf8Arguments(ReadOnlySpan<TextBuffer> values)
        {
            _buffers = values;
            _kind = 2;
        }

        internal int Length => _kind switch { 0 => _strings.Length, 1 => _bytes.Length, _ => _buffers.Length };

        internal Utf8Argument this[int index] => _kind switch
        {
            0 => new Utf8Argument(_strings[index]), 1 => new Utf8Argument(_bytes[index].Span), _ => new Utf8Argument(_buffers[index]),
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
        var result = (byte**)_arena.Allocate(checked(values.Length * sizeof(byte*)));
        for (var i = 0; i < values.Length; i++) result[i] = Utf8(values[i]);
        return result;
    }

    private static void CheckUtf8Length(Utf8Argument text, int length)
    {
        text.ThrowIfNull(nameof(text));
        var available = text.Text is not null ? Encoding.UTF8.GetByteCount(text.Text) : ContentBytes(text.GetBytes()).Length;
        if (length < -1 || length > available) throw new ArgumentOutOfRangeException(nameof(length));
    }

    private byte* CopyUtf8(ReadOnlySpan<byte> text)
    {
        text = ContentBytes(text);
        Utf8Validation.Check(text);
        var result = (byte*)_arena.Allocate(checked(text.Length + 1));
        text.CopyTo(new Span<byte>(result, text.Length));
        result[text.Length] = 0;
        return result;
    }

    private static ReadOnlySpan<byte> ContentBytes(ReadOnlySpan<byte> text) =>
        !text.IsEmpty && text[^1] == 0 ? text[..^1] : text;

    /// <summary>Submits validated UTF-8; CUI owns the text copy needed for rendering.</summary>
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