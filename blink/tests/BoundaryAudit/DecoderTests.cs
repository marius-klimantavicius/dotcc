using System.Reflection.Emit;
static void Valid(byte[] bytes, OpCode opcode, int size)
{
    int position = 0;
    var result = IlDecoder.Read(bytes, ref position);
    if (result.Opcode != opcode || result.Size != size || position != bytes.Length)
        throw new Exception("Incorrect operand width: " + opcode);
}
static void Invalid(byte[] bytes, int position = 0)
{
    int before = position;
    try { IlDecoder.Read(bytes, ref position); }
    catch (InvalidDataException) { if (position != before) throw new Exception("Failed decoder advanced cursor"); return; }
    throw new Exception("Malformed IL accepted");
}
Valid([0x00], OpCodes.Nop, 0);
Valid([0x0e, 7], OpCodes.Ldarg_S, 1);
Valid([0xfe, 0x09, 7, 0], OpCodes.Ldarg, 2);
Valid([0x28, 1, 0, 0, 6], OpCodes.Call, 4);
Valid([0x22, 0, 0, 0, 0], OpCodes.Ldc_R4, 4);
Valid([0x23, 0, 0, 0, 0, 0, 0, 0, 0], OpCodes.Ldc_R8, 8);
Valid([0x21, 0, 0, 0, 0, 0, 0, 0, 0], OpCodes.Ldc_I8, 8);
Valid([0x45, 0, 0, 0, 0], OpCodes.Switch, 4);
Valid([0x45, 2, 0, 0, 0, 1, 0, 0, 0, 2, 0, 0, 0], OpCodes.Switch, 12);
Invalid([]); Invalid([0xfe]); Invalid([0x24]); Invalid([0x28, 0]);
Invalid([0x45, 1]); Invalid([0x45, 1, 0, 0, 0]);
Invalid([0x45, 255, 255, 255, 255]); Invalid([0x45, 255, 255, 255, 127]);
Invalid([0], -1); Invalid([0], 2);
Console.WriteLine("IL widths, malformed prefixes/switches and atomic cursor checks: PASS");
