using System.Text;

namespace Managed.CPython;

/// <summary>Controls lone UTF-16 surrogates. Scalar UTF-8 always rejects them.</summary>
public enum PythonSurrogatePolicy { Reject, Preserve }

public static class PythonText
{
    private static readonly UTF8Encoding StrictUtf8 = new(false, true);

    // Length-bearing data: embedded NUL is deliberately preserved.
    public static byte[] EncodeUtf8(string value) => StrictUtf8.GetBytes(value);
    public static string DecodeUtf8(ReadOnlySpan<byte> value) => StrictUtf8.GetString(value);

    // CPython's Unicode storage is not wchar_t storage. These are ordinal values
    // for PyUnicode_FromKindAndData(PyUnicode_4BYTE_KIND), including surrogates
    // only when explicitly requested. No struct/storage-kind cast is involved.
    public static uint[] ToCodePoints(string value, PythonSurrogatePolicy policy = PythonSurrogatePolicy.Reject)
    {
        ArgumentNullException.ThrowIfNull(value);
        var result = new List<uint>(value.Length);
        for (int i = 0; i < value.Length; i++)
        {
            char current = value[i];
            if (char.IsHighSurrogate(current) && i + 1 < value.Length && char.IsLowSurrogate(value[i + 1]))
                result.Add((uint)char.ConvertToUtf32(current, value[++i]));
            else if (char.IsSurrogate(current) && policy == PythonSurrogatePolicy.Reject)
                throw new EncoderFallbackException($"Lone UTF-16 surrogate at code-unit index {i}.");
            else result.Add(current);
        }
        return result.ToArray();
    }

    public static string FromCodePoints(ReadOnlySpan<uint> value, PythonSurrogatePolicy policy = PythonSurrogatePolicy.Reject)
    {
        var result = new StringBuilder(value.Length);
        foreach (uint point in value)
        {
            if (point > 0x10ffff || (point is >= 0xd800 and <= 0xdfff && policy == PythonSurrogatePolicy.Reject))
                throw new DecoderFallbackException($"Invalid Unicode code point U+{point:X}.");
            if (point <= 0xffff) result.Append((char)point);
            else result.Append(char.ConvertFromUtf32((int)point));
        }
        return result.ToString();
    }

    internal static string RequireCString(string value, string name)
    {
        ArgumentNullException.ThrowIfNull(value, name);
        if (value.Contains('\0')) throw new ArgumentException("This name/path/source is passed to a NUL-terminated API.", name);
        _ = EncodeUtf8(value); // Validate before entering translated code.
        return value;
    }
}
