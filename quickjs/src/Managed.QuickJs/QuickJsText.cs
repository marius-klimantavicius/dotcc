using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Managed.Interpreters;

internal static unsafe class QuickJsText
{
    public ref struct ManagedToUnmanagedIn
    {
        public static int BufferSize => 0x100;

        private byte* _unmanagedValue;
        private int _count;
        private bool _allocated;

        public int Count => _count;

        public void FromManaged(string? managed, Span<byte> buffer)
        {
            _allocated = false;

            if (managed is null)
            {
                _unmanagedValue = null;
                return;
            }

            const int MaxWtf8BytesPerChar = 4;

            // >= for null terminator
            // Use the cast to long to avoid the checked operation
            if ((long)MaxWtf8BytesPerChar * managed.Length >= buffer.Length)
            {
                // Calculate accurate byte count when the provided stack-allocated buffer is not sufficient
                var exactByteCount = checked(GetByteCount(managed) + 1); // + 1 for null terminator
                if (exactByteCount > buffer.Length)
                {
                    buffer = new Span<byte>((byte*)NativeMemory.Alloc((nuint)exactByteCount), exactByteCount);
                    _allocated = true;
                }
            }

            // Unsafe.AsPointer is safe since buffer must be pinned
            _unmanagedValue = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(buffer));

            _count = GetBytes(managed, buffer);
        }

        public byte* ToUnmanaged() => _unmanagedValue;

        public void Free()
        {
            if (_allocated)
                NativeMemory.Free(_unmanagedValue);
        }
    }

    internal static void CheckName(string name)
    {
        ArgumentNullException.ThrowIfNull(name);
        if (name.Contains('\0'))
            throw new ArgumentException("This embedding name must not contain NUL.", nameof(name));
    }

    internal static int GetByteCount(ReadOnlySpan<char> text)
    {
        var count = 0;
        for (var i = 0; i < text.Length; i++)
        {
            var c = text[i];
            if (char.IsHighSurrogate(c) && i + 1 < text.Length && char.IsLowSurrogate(text[i + 1]))
            {
                i++;
                count += 4;
                continue;
            }

            if (c < 128)
            {
                count++;
            }
            else if (c < 2048)
            {
                count += 2;
            }
            else
            {
                count += 3;
            }
        }

        return count;
    }

    internal static int GetBytes(ReadOnlySpan<char> text, Span<byte> result)
    {
        var count = 0;
        for (var i = 0; i < text.Length; i++)
        {
            var c = text[i];
            if (char.IsHighSurrogate(c) && i + 1 < text.Length && char.IsLowSurrogate(text[i + 1]))
            {
                var scalar = char.ConvertToUtf32(c, text[++i]);
                result[count++] = (byte)(0xf0 | scalar >> 18);
                result[count++] = (byte)(0x80 | scalar >> 12 & 63);
                result[count++] = (byte)(0x80 | scalar >> 6 & 63);
                result[count++] = (byte)(0x80 | scalar & 63);
                continue;
            }

            if (c < 128)
            {
                result[count++] = (byte)c;
            }
            else if (c < 2048)
            {
                result[count++] = (byte)(0xc0 | c >> 6);
                result[count++] = (byte)(0x80 | c & 63);
            }
            else
            {
                result[count++] = (byte)(0xe0 | c >> 12);
                result[count++] = (byte)(0x80 | c >> 6 & 63);
                result[count++] = (byte)(0x80 | c & 63);
            }
        }

        result[count] = 0;
        return count;
    }

    public static string Decode(byte* pointer, ulong length)
    {
        if (length > int.MaxValue)
            throw new OverflowException("JavaScript string exceeds managed string capacity.");

        var bytes = new ReadOnlySpan<byte>(pointer, (int)length);
        var stringLength = GetCharCount(bytes);
        return string.Create(stringLength, bytes, static (destination, source) =>
        {
            var destinationIndex = 0;
            for (var i = 0; i < source.Length;)
            {
                int first = source[i++];
                if (first < 128)
                {
                    destination[destinationIndex++] = (char)first;
                }
                else if ((first & 0xe0) == 0xc0)
                {
                    destination[destinationIndex++] = (char)((first & 31) << 6 | source[i++] & 63);
                }
                else if ((first & 0xf0) == 0xe0)
                {
                    int second = source[i++];
                    destination[destinationIndex++] = (char)((first & 15) << 12 | (second & 63) << 6 | source[i++] & 63);
                }
                else
                {
                    int second = source[i++], third = source[i++], fourth = source[i++];
                    var codepoint = (first & 7) << 18 | (second & 63) << 12 | (third & 63) << 6 | fourth & 63;
                    if (codepoint <= 0xFFFFu)
                    {
                        destination[destinationIndex++] = (char)codepoint;
                    }
                    else
                    {
                        var highSurrogateCodePoint = (char)((codepoint + ((0xD800u - 0x40u) << 10)) >> 10);
                        var lowSurrogateCodePoint = (char)((codepoint & 0x3FFu) + 0xDC00u);

                        destination[destinationIndex++] = highSurrogateCodePoint;
                        destination[destinationIndex++] = lowSurrogateCodePoint;
                    }
                }
            }
        });
    }

    private static int GetCharCount(ReadOnlySpan<byte> value)
    {
        var count = 0;
        for (var i = 0; i < value.Length;)
        {
            int first = value[i++];
            if (first < 128)
            {
                count++;
            }
            else if ((first & 0xe0) == 0xc0)
            {
                i += 1;
                count++;
            }
            else if ((first & 0xf0) == 0xe0)
            {
                i += 2;
                count++;
            }
            else
            {
                int second = value[i++], third = value[i++], fourth = value[i++];
                var codepoint = (first & 7) << 18 | (second & 63) << 12 | (third & 63) << 6 | fourth & 63;
                if (codepoint <= 0xFFFFu)
                    count += 1;
                else
                    count += 2;
            }
        }

        return count;
    }
}