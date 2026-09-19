using System.Reflection;
using System.Reflection.Emit;

// Structural decoder only: branch destinations and stack typing are the CLR
// verifier's concern. It never reads outside a method or moves backwards.
internal static class IlDecoder
{
    private static readonly Dictionary<ushort, OpCode> Opcodes = typeof(OpCodes)
        .GetFields(BindingFlags.Public | BindingFlags.Static).Where(f => f.FieldType == typeof(OpCode))
        .Select(f => (OpCode)f.GetValue(null)!).ToDictionary(op => unchecked((ushort)op.Value));
    internal static (OpCode Opcode, int Operand, int Size) Read(byte[] bytes, ref int position)
    {
        int cursor = position;
        Require(bytes, cursor, 1);
        ushort code = bytes[cursor++];
        if (code == 0xfe) { Require(bytes, cursor, 1); code = (ushort)(0xfe00 | bytes[cursor++]); }
        if (!Opcodes.TryGetValue(code, out OpCode op)) throw new InvalidDataException("Unknown IL opcode " + code);
        int operand = cursor;
        int size = op.OperandType switch
        {
            OperandType.InlineNone => 0,
            OperandType.ShortInlineBrTarget or OperandType.ShortInlineI or OperandType.ShortInlineVar => 1,
            OperandType.InlineVar => 2,
            OperandType.InlineI8 or OperandType.InlineR => 8,
            OperandType.InlineBrTarget or OperandType.InlineField or OperandType.InlineI or
                OperandType.InlineMethod or OperandType.InlineSig or OperandType.InlineString or
                OperandType.InlineTok or OperandType.InlineType or OperandType.ShortInlineR => 4,
            OperandType.InlineSwitch => SwitchSize(bytes, cursor),
            _ => throw new InvalidDataException("Unsupported operand kind " + op.OperandType)
        };
        Require(bytes, cursor, size);
        position = cursor + size;
        return (op, operand, size);
    }
    private static int SwitchSize(byte[] bytes, int cursor)
    {
        Require(bytes, cursor, 4);
        int count = BitConverter.ToInt32(bytes, cursor);
        if (count < 0 || count > (bytes.Length - cursor - 4) / 4)
            throw new InvalidDataException("Invalid or truncated IL switch table");
        return 4 + count * 4;
    }
    private static void Require(byte[] bytes, int cursor, int length)
    {
        if (cursor < 0 || cursor > bytes.Length || length < 0 || length > bytes.Length - cursor)
            throw new InvalidDataException("Truncated IL operand or opcode");
    }
}
