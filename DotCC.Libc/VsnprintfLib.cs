#nullable enable

namespace DotCC.Libc;

public static unsafe partial class Libc
{
    /// <summary>Format a borrowed cursor, returning the untruncated byte count.
    /// A zero size never dereferences the destination, including a null pointer.</summary>
    public static int vsnprintf(byte* destination, ulong size, byte* format, scoped VaList arguments)
    {
        int capacity = size == 0 ? 0 : (int)global::System.Math.Min(size - 1, int.MaxValue);
        return new SprintfBuilder(destination, format, capacity, terminate: size != 0)
            .Arguments(arguments).Done();
    }
}

public unsafe ref partial struct PrintfBuilder
{
    internal PrintfBuilder Arguments(scoped Libc.VaList arguments)
    {
        while (true)
        {
            var spec = ConsumeUntilSpec(arguments, true, out int consumed);
            for (int i = 0; i < consumed; i++) arguments.Next();
            if (spec.Conv == 0) return this;
            bool wide = spec.Length is (byte)'l' or (byte)'j' or (byte)'z' or (byte)'t';
            var value = arguments.Next();
            switch (spec.Conv)
            {
                case (byte)'d': case (byte)'i':
                {
                    long number = wide ? (long)value : (int)value;
                    if (spec.Length == (byte)'h') number = spec.DoubleLength ? (sbyte)number : (short)number;
                    if (spec.Precision >= 0) spec.Zero = false;
                    Emit(ApplyWidth(PadInt(number, spec, global::System.Globalization.CultureInfo.InvariantCulture), spec));
                    break;
                }
                case (byte)'u': case (byte)'x': case (byte)'X': case (byte)'o':
                {
                    ulong number = wide ? (ulong)value : (uint)value;
                    if (spec.Length == (byte)'h') number = spec.DoubleLength ? (byte)number : (ushort)number;
                    string digits = spec.Conv switch
                    {
                        (byte)'x' => number.ToString("x", global::System.Globalization.CultureInfo.InvariantCulture),
                        (byte)'X' => number.ToString("X", global::System.Globalization.CultureInfo.InvariantCulture),
                        (byte)'o' => FormatOctal(number),
                        _ => number.ToString(global::System.Globalization.CultureInfo.InvariantCulture),
                    };
                    if (spec.Precision == 0 && number == 0) digits = "";
                    if (spec.Precision > digits.Length) digits = digits.PadLeft(spec.Precision, '0');
                    if (spec.Precision >= 0) spec.Zero = false;
                    string prefix = spec.Alt && number != 0 && spec.Conv == (byte)'x' ? "0x"
                        : spec.Alt && number != 0 && spec.Conv == (byte)'X' ? "0X" : "";
                    if (spec.Alt && spec.Conv == (byte)'o' && (digits.Length == 0 || digits[0] != '0')) digits = "0" + digits;
                    if (spec.Zero && !spec.Left && prefix.Length != 0 && spec.Width > prefix.Length + digits.Length)
                    { digits = digits.PadLeft(spec.Width - prefix.Length, '0'); spec.Zero = false; }
                    Emit(ApplyWidth(prefix + digits, spec));
                    break;
                }
                case (byte)'c':
                    if (spec.Length != 0) throw new global::System.NotSupportedException("vsnprintf wide characters are not supported");
                    EmitRaw(ApplyWidth(((char)(byte)(int)value).ToString(), spec));
                    break;
                case (byte)'s':
                {
                    if (spec.Length != 0) throw new global::System.NotSupportedException("vsnprintf wide strings are not supported");
                    byte* text = (byte*)(void*)value;
                    string bytes;
                    if (text == null) bytes = "(null)";
                    else
                    {
                        int length = 0;
                        while ((spec.Precision < 0 || length < spec.Precision) && text[length] != 0) length++;
                        bytes = global::System.Text.Encoding.Latin1.GetString(text, length);
                    }
                    EmitRaw(ApplyWidth(bytes, spec));
                    break;
                }
                case (byte)'n':
                {
                    void* target = (void*)value;
                    if (wide) *(long*)target = _count;
                    else if (spec.Length == (byte)'h' && spec.DoubleLength) *(sbyte*)target = (sbyte)_count;
                    else if (spec.Length == (byte)'h') *(short*)target = (short)_count;
                    else *(int*)target = _count;
                    break;
                }
                case (byte)'p':
                    _pendingSpec = spec; _hasPendingSpec = true;
                    this = Arg((void*)value);
                    break;
                case (byte)'a': case (byte)'A': case (byte)'e': case (byte)'E':
                case (byte)'f': case (byte)'F': case (byte)'g': case (byte)'G':
                    if (spec.Length == (byte)'L') throw new global::System.NotSupportedException("vsnprintf long double is not supported by VaArg storage");
                    _pendingSpec = spec; _hasPendingSpec = true;
                    this = Arg((double)value);
                    break;
                default:
                    throw new global::System.NotSupportedException("unsupported vsnprintf conversion: " + (char)spec.Conv);
            }
        }
    }

    private void EmitRaw(string text)
    {
        _w.Write(text);
        _count += text.Length;
    }
}
