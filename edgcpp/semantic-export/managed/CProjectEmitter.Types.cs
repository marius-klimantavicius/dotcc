using System.Globalization;
using System.Numerics;
using System.Text;

namespace Edg.Semantic;

public sealed partial class CProjectEmitter
{
    private bool UsesFloat80 { get; set; }
    private readonly Dictionary<ulong, string> aggregateConstantHelpers = new();

    private string TypeName(SemanticNode original)
    {
        var type = Canonical(original);
        if (typeNames.TryGetValue(type.Id, out var existing)) return existing;
        string result;
        switch (type.Form)
        {
            case "tk_void": result = "void"; break;
            case "tk_integer":
                result = (Num(type, "size"), type.Flag("signed")) switch
                {
                    (1, true) => "sbyte", (1, false) => "byte", (2, true) => "short", (2, false) => "ushort",
                    (4, true) => "int", (4, false) => "uint", (8, true) => "long", (8, false) => "ulong",
                    (16, true) => "Int128", (16, false) => "UInt128",
                    _ => throw new InvalidDataException($"node {type.Id}: unsupported integer width")
                };
                break;
            case "tk_float":
                result = (Num(type, "floatKind"), Num(type, "size")) switch
                {
                    (2, 4) => "float", (4, 8) => "double", (6, 16) => "global::Edg.Runtime.Float80",
                    _ => throw new InvalidDataException($"node {type.Id}: unsupported floating representation")
                };
                if (Num(type, "floatKind") == 6) UsesFloat80 = true;
                break;
            case "tk_pointer":
                if (type.Flag("reference")) Fail(type, "C++ references in C project");
                var element = Canonical(Need(type, "elementType"));
                result = element.Form == "tk_routine" ? TypeName(element) : TypeName(element) + "*";
                break;
            case "tk_routine":
                var sig = Need(type, "signature");
                if (sig.Flag("hasThis") || Num(sig, "callingConvention") != 0) Fail(type, "unsupported function calling convention");
                var parameters = graph.Linked(sig.ReferenceId("parameters")).Select(p => TypeName(Need(p, "type"))).ToList();
                if (sig.Flag("variadic")) parameters.Add("ReadOnlySpan<__CArg>");
                parameters.Add(TypeName(Need(type, "returnType")));
                result = "delegate* managed<" + string.Join(", ", parameters) + ">";
                break;
            case "tk_struct": case "tk_union":
                if (type.Name == "__va_list_tag") { if (Num(type, "size") != 24) Fail(type, "unsupported va_list layout"); result = "__VaList"; break; }
                result = Allocate(type, memberNames);
                typeNames[type.Id] = result; recordTypes.Add(type.Id);
                var fields = graph.Linked(type.ReferenceId("fields")).ToArray();
                var fieldNames = new HashSet<string>(StringComparer.Ordinal) { result.TrimStart('@') };
                foreach (var field in fields) Allocate(field, fieldNames);
                foreach (var field in fields) TypeName(Need(field, "type"));
                return result;
            case "tk_array":
                if (type.Flag("variableLength")) Fail(type, "variable-length array storage");
                result = "__Array_" + type.Id; typeNames[type.Id] = result; arrayTypes.Add(type.Id);
                TypeName(Need(type, "elementType"));
                return result;
            default: Fail(type, "unsupported project type"); return "";
        }
        typeNames[type.Id] = result;
        return result;
    }

    private void EmitArray(SemanticNode type)
    {
        var size = Num(type, "size");
        var element = Need(type, "elementType");
        // Incomplete arrays only participate in pointer expressions. A complete
        // array always carries its EDG size, including its element alignment.
        var physicalSize = Math.Max(size, Math.Max(1UL, Num(Canonical(element), "size")));
        if (physicalSize > int.MaxValue) Fail(type, "array exceeds CLR explicit-layout size");
        Line($"[StructLayout(LayoutKind.Explicit, Size = {physicalSize}, Pack = 1)]");
        Line("public struct " + TypeName(type)); Line("{"); indent++;
        Line("[FieldOffset(0)] public " + TypeName(element) + " First;");
        indent--; Line("}");
    }

    private void EmitRecord(SemanticNode type)
    {
        var size = Num(type, "size");
        if (size > int.MaxValue) Fail(type, "record exceeds CLR explicit-layout size");
        var recordName = TypeName(type);
        var fields = graph.Linked(type.ReferenceId("fields")).ToArray();
        Line($"[StructLayout(LayoutKind.Explicit, Size = {Math.Max(1UL, size)}, Pack = 1)]");
        Line("public struct " + recordName); Line("{"); indent++;
        var storageBytes = new SortedSet<ulong>();
        foreach (var field in fields)
        {
            if (field.Flag("bitfield"))
            {
                var width = Num(field, "bitSize");
                if (width > 64) Fail(field, "bitfield exceeds 64 bits");
                for (ulong b = 0; b < (Num(field, "bitOffset") + width + 7) / 8; b++) storageBytes.Add(Num(field, "offset") + b);
                continue;
            }
            var fieldType = Canonical(Need(field, "type"));
            // A flexible member has no storage; its address is computed from
            // the exported field offset by the expression emitter.
            if (fieldType.Form == "tk_array" && Num(fieldType, "size") == 0) continue;
            if (!type.Flag("incomplete") && Num(field, "offset") + Num(fieldType, "size") > size) Fail(field, "field exceeds enclosing record");
            Line($"[FieldOffset({Num(field, "offset")})] public {TypeName(fieldType)} {Name(field)};");
        }
        foreach (var offset in storageBytes)
        {
            if (offset >= size) Fail(type, "bitfield exceeds record storage");
            Line($"[FieldOffset({offset})] private byte __bits_{offset};");
        }
        foreach (var field in fields.Where(f => f.Flag("bitfield") && Num(f, "bitSize") != 0)) EmitBitfield(field);
        indent--; Line("}");
    }

    private void EmitBitfield(SemanticNode field)
    {
        var offset = Num(field, "offset"); var bit = (int)Num(field, "bitOffset"); var width = (int)Num(field, "bitSize");
        var type = Canonical(Need(field, "type"));
        if (type.Form != "tk_integer") Fail(field, "noninteger bitfield");
        var pieces = new List<string>();
        for (int i = 0; i < width;)
        {
            var absolute = bit + i; var inByte = absolute % 8; var take = Math.Min(width - i, 8 - inByte);
            var part = $"((ulong)((__bits_{offset + (ulong)(absolute / 8)} >> {inByte}) & {(1 << take) - 1}) << {i})";
            pieces.Add(part); i += take;
        }
        string value = "(" + string.Join(" | ", pieces) + ")";
        if (type.Flag("signed") && width < 64) value = $"((long)({value} << {64 - width}) >> {64 - width})";
        Line("public " + TypeName(type) + " " + Name(field)); Line("{"); indent++;
        Line("get => unchecked((" + TypeName(type) + ")" + value + ");");
        Line("set"); Line("{"); indent++;
        for (int i = 0; i < width;)
        {
            var absolute = bit + i; var inByte = absolute % 8; var take = Math.Min(width - i, 8 - inByte); var mask = ((1 << take) - 1) << inByte;
            var storage = "__bits_" + (offset + (ulong)(absolute / 8));
            Line($"{storage} = unchecked((byte)(((ulong){storage} & {255 ^ mask}UL) | (((ulong)value >> {i} & {(1 << take) - 1}UL) << {inByte})));");
            i += take;
        }
        indent--; Line("}"); indent--; Line("}");
    }

    private string Constant(SemanticNode constant)
    {
        switch (constant.Form)
        {
            case "ck_integer":
                var integerType = Canonical(Need(constant, "type"));
                var integer = constant.Text("integerValue") ?? throw new InvalidDataException($"node {constant.Id}: missing integer value");
                var literal = integer.StartsWith('-') ? integer + "L" : integer + "UL";
                return "unchecked((" + TypeName(integerType) + ")" + literal + ")";
            case "ck_float": return FloatingConstant(constant);
            case "ck_string":
                RegisterString(constant);
                return "((" + TypeName(Need(Canonical(Need(constant, "type")), "elementType")) + "*)__string_" + constant.Id + ")";
            case "ck_address":
                string address;
                if (constant.Reference("routine") is { } routine)
                {
                    if (!routine.Flag("defined")) externalRoutines.Add(routine.Id);
                    address = "(" + TypeName(Need(routine, "type")) + ")&" + Name(routine);
                }
                else if (constant.Reference("variable") is { } variable) address = VariableAddress(variable);
                else if (constant.Reference("constant") is { } pointee && pointee.Form == "ck_string") { RegisterString(pointee); address = "__string_" + pointee.Id; }
                else { Fail(constant, "unsupported address-constant base"); return ""; }
                long offset = constant.Raw.GetProperty("offset").GetInt64();
                var cast = TypeName(Need(constant, "type"));
                return offset == 0 ? "((" + cast + ")(" + address + "))" : "((" + cast + ")((byte*)(" + address + ") + (" + offset + "L)))";
            case "ck_dynamic_init":
                var init = Need(constant, "initialization");
                if (init.Reference("expression") is { } expression) return Expr(expression);
                if (init.Reference("constant") is { } value) return Constant(value);
                Fail(constant, "dynamic constant without expression or value"); return "";
            case "ck_aggregate":
                if (!aggregateConstantHelpers.TryGetValue(constant.Id, out var helper))
                {
                    helper = "__AggregateConstant_" + constant.Id;
                    aggregateConstantHelpers.Add(constant.Id, helper);
                    var aggregateType = Need(constant, "type");
                    int start = body.Length, previousIndent = indent;
                    indent = 0;
                    Line("private static " + TypeName(aggregateType) + " " + helper + "()");
                    Line("{"); indent++;
                    Line(TypeName(aggregateType) + " value = default;");
                    EmitConstantInitialization("(&value)", aggregateType, constant, false);
                    Line("return value;"); indent--; Line("}");
                    expressionHelpers.Add(body.ToString(start, body.Length - start));
                    body.Length = start; indent = previousIndent;
                }
                return helper + "()";
            default: Fail(constant, "constant requires unsupported value semantics"); return "";
        }
    }

    private void RegisterString(SemanticNode constant)
    {
        if (strings.ContainsKey(constant.Id)) return;
        var text = constant.Text("bytesLatin1") ?? throw new InvalidDataException($"node {constant.Id}: string bytes missing");
        if (text.Any(c => c > 255)) Fail(constant, "invalid Latin1 byte encoding");
        strings.Add(constant.Id, text.Select(c => (byte)c).ToArray());
    }

    private string FloatingConstant(SemanticNode constant)
    {
        var type = Canonical(Need(constant, "type")); var name = TypeName(type);
        var value = constant.Text("floatValue") ?? throw new InvalidDataException($"node {constant.Id}: floating value missing");
        if (constant.Flag("notANumber")) Fail(constant, "NaN constants require exported payload bits");
        if (name == "global::Edg.Runtime.Float80")
        {
            ushort sign = value.StartsWith('-') || constant.Flag("negativeInfinity") ? (ushort)0x8000 : (ushort)0;
            ulong mantissa; ushort exponent;
            if (constant.Flag("positiveInfinity") || constant.Flag("negativeInfinity")) { mantissa = 0x8000000000000000; exponent = 0x7fff; }
            else (mantissa, exponent) = Binary80(value);
            return $"new global::Edg.Runtime.Float80(0x{mantissa:x16}UL, 0x{(ushort)(exponent | sign):x4})";
        }
        if (constant.Flag("positiveInfinity")) return name + ".PositiveInfinity";
        if (constant.Flag("negativeInfinity")) return name + ".NegativeInfinity";
        if (name == "float")
        {
            var parsed = float.Parse(value, NumberStyles.Float, CultureInfo.InvariantCulture);
            var bits = BitConverter.SingleToUInt32Bits(parsed);
            return "BitConverter.UInt32BitsToSingle(0x" + bits.ToString("x8") + "U)";
        }
        var parsedDouble = double.Parse(value, NumberStyles.Float, CultureInfo.InvariantCulture);
        return "BitConverter.UInt64BitsToDouble(0x" + BitConverter.DoubleToUInt64Bits(parsedDouble).ToString("x16") + "UL)";
    }

    private static (ulong Mantissa, ushort Exponent) Binary80(string text)
    {
        text = text.TrimStart('+', '-');
        var parts = text.Split('e', 'E'); var exponent10 = parts.Length == 2 ? int.Parse(parts[1], CultureInfo.InvariantCulture) : 0;
        var point = parts[0].IndexOf('.'); if (point >= 0) exponent10 -= parts[0].Length - point - 1;
        BigInteger numerator = BigInteger.Parse(parts[0].Replace(".", ""), CultureInfo.InvariantCulture), denominator = BigInteger.One;
        if (numerator.IsZero) return (0, 0);
        if (exponent10 >= 0) numerator *= BigInteger.Pow(10, exponent10); else denominator = BigInteger.Pow(10, -exponent10);
        int exponent2 = (int)(numerator.GetBitLength() - denominator.GetBitLength());
        if (exponent2 >= 0 ? numerator < (denominator << exponent2) : (numerator << -exponent2) < denominator) exponent2--;
        int power = Math.Max(exponent2 - 63, -16445);
        var n = numerator; var d = denominator; if (power >= 0) d <<= power; else n <<= -power;
        var mantissa = BigInteger.DivRem(n, d, out var remainder);
        var halfway = (remainder * 2).CompareTo(d); if (halfway > 0 || (halfway == 0 && !mantissa.IsEven)) mantissa++;
        if (mantissa > ulong.MaxValue) { mantissa >>= 1; power++; }
        if (power > 16320) return (0x8000000000000000, 0x7fff);
        return ((ulong)mantissa, mantissa < (BigInteger.One << 63) ? (ushort)0 : checked((ushort)(power + 63 + 16383)));
    }

    private void EmitGlobals()
    {
        // Resolve all initializer values before emitting the static constructor:
        // this discovers string storage and ensures every address is allocated
        // before any initializer can refer to it.
        int start = body.Length;
        var initialized = new List<SemanticNode>();
        foreach (var variable in globals.Values.OrderBy(n => n.Id))
        {
            TypeName(Need(variable, "type"));
            SemanticNode? initializer = null;
            string? kind = variable.Text("initializationKind");
            if (kind == "initk_function_local")
            {
                if (!localStaticInitializers.TryGetValue(variable.Id, out var local)) Fail(variable, "local static initializer missing");
                else { initializer = local.Reference("initializer"); kind = local.Text("initializationKind"); }
            }
            else initializer = variable.Reference("initializer");
            if (kind == "initk_none") continue;
            if (initializer == null) Fail(variable, "global initializer missing");
            initialized.Add(variable);
            Line("private static void __InitializeGlobal_" + variable.Id + "()"); Line("{"); indent++;
            if (kind == "initk_static") EmitConstantInitialization(Name(variable), Need(variable, "type"), initializer!, true);
            else if (kind == "initk_dynamic") EmitInitialization(Name(variable), Need(variable, "type"), initializer!);
            else Fail(variable, "unsupported global initialization kind " + kind);
            indent--; Line("}");
        }
        string initializers = body.ToString(start, body.Length - start); body.Length = start;
        foreach (var variable in globals.Values.OrderBy(n => n.Id))
            Line("public static readonly " + TypeName(Need(variable, "type")) + "* " + Name(variable) + ";");
        foreach (var pair in strings.OrderBy(p => p.Key)) Line("private static readonly byte* __string_" + pair.Key + ";");
        Line("static SqliteCore()"); Line("{"); indent++;
        Line("if (IntPtr.Size != 8 || !BitConverter.IsLittleEndian) throw new PlatformNotSupportedException(\"The semantic package requires little-endian 64-bit pointers.\");");
        foreach (var variable in globals.Values.OrderBy(n => n.Id))
        {
            var type = Canonical(Need(variable, "type")); var size = Num(type, "size");
            if (size == 0) Fail(variable, "incomplete global storage");
            var alignment = Math.Max(Num(type, "alignment"), Num(variable, "alignment"));
            Line(Name(variable) + " = (" + TypeName(type) + "*)__AllocateGlobal(" + size + ", " + Math.Max(1UL, alignment) + ");");
        }
        foreach (var pair in strings.OrderBy(p => p.Key))
        {
            var constant = graph.Get(pair.Key)!; var alignment = Math.Max(1UL, Num(Canonical(Need(constant, "type")), "alignment"));
            Line($"__string_{pair.Key} = (byte*)__AllocateGlobal({Math.Max(1, pair.Value.Length)}, {alignment});");
            Line($"__InitializeString_{pair.Key}();");
        }
        foreach (var variable in initialized) Line("__InitializeGlobal_" + variable.Id + "();");
        indent--; Line("}");
        foreach (var pair in strings.OrderBy(p => p.Key))
        {
            Line("private static void __InitializeString_" + pair.Key + "()"); Line("{"); indent++;
            Line("ReadOnlySpan<byte> value = [" + string.Join(",", pair.Value) + "];");
            Line($"value.CopyTo(new Span<byte>(__string_{pair.Key}, value.Length));");
            indent--; Line("}");
        }
        body.Append(initializers);
    }

    private void EmitInitialization(string address, SemanticNode type, SemanticNode initializer)
    {
        switch (initializer.Form)
        {
            case "dik_none": break;
            case "dik_zero": Line("*(" + TypeName(type) + "*)(" + address + ") = default;"); break;
            case "dik_constant": case "dik_nonconstant_aggregate":
                EmitConstantInitialization(address, type, Need(initializer, "constant"), true); break;
            case "dik_expression":
                var expression = Need(initializer, "expression");
                Line("*(" + TypeName(type) + "*)(" + address + ") = " + ConvertValue(Expr(expression), Need(expression, "type"), type) + ";"); break;
            default: Fail(initializer, "unsupported C initialization"); break;
        }
    }

    private void EmitConstantInitialization(string address, SemanticNode original, SemanticNode constant, bool clear)
    {
        var type = Canonical(original);
        if (constant.Form == "ck_dynamic_init") { EmitInitialization(address, type, Need(constant, "initialization")); return; }
        if (constant.Form == "ck_string" && type.Form == "tk_array")
        {
            RegisterString(constant);
            var count = Math.Min(Num(type, "size"), (ulong)strings[constant.Id].Length);
            if (clear) Line("*(" + TypeName(type) + "*)(" + address + ") = default;");
            Line("new ReadOnlySpan<byte>(__string_" + constant.Id + ", " + count + ").CopyTo(new Span<byte>((void*)(" + address + "), " + count + "));");
            return;
        }
        if (constant.Form != "ck_aggregate")
        {
            Line("*(" + TypeName(type) + "*)(" + address + ") = " + ConvertValue(Constant(constant), Need(constant, "type"), type) + ";");
            return;
        }
        if (type.Form is not ("tk_array" or "tk_struct" or "tk_union")) { Fail(constant, "aggregate initializer with scalar destination"); return; }
        if (clear) Line("*(" + TypeName(type) + "*)(" + address + ") = default;");
        var fields = type.Form == "tk_array" ? [] : graph.Linked(type.ReferenceId("fields")).Where(f => !f.Flag("bitfield") || !string.IsNullOrEmpty(f.Name)).ToArray();
        ulong cursor = 0;
        foreach (var value in graph.Linked(constant.ReferenceId("elements")))
        {
            if (value.Form == "ck_designator")
            {
                if (value.Flag("generic")) Fail(value, "dependent initializer designator");
                if (value.Flag("fieldDesignator"))
                {
                    int fieldIndex = Array.FindIndex(fields, f => f.Id == value.ReferenceId("field"));
                    if (fieldIndex < 0) Fail(value, "initializer designator is not a destination member");
                    cursor = (ulong)fieldIndex;
                }
                else { if (type.Form != "tk_array") Fail(value, "array designator for non-array"); cursor = Num(value, "elementIndex"); }
                continue;
            }
            ulong repeat = value.Form == "ck_init_repeat" ? Num(value, "count") : 1;
            var item = value.Form == "ck_init_repeat" ? Need(value, "constant") : value;
            if (value.Flag("multidimensionalTailNotRepeated")) Fail(value, "GNU multidimensional range designator");
            for (ulong r = 0; r < repeat; r++, cursor++)
            {
                bool clearChild = item.Form != "ck_aggregate" || item.Flag("explicitBraces") || !item.Flag("designatedInitializers");
                if (type.Form == "tk_array")
                {
                    if (cursor >= Num(type, "count")) Fail(item, "array initializer outside exported bound");
                    var element = Need(type, "elementType");
                    EmitConstantInitialization("((" + TypeName(element) + "*)(" + address + ") + " + cursor + ")", element, item, clearChild);
                }
                else
                {
                    if (cursor >= (ulong)fields.Length || (type.Form == "tk_union" && cursor != 0 && !constant.Flag("designatedInitializers"))) Fail(item, "record initializer outside member list");
                    var field = fields[(int)cursor]; var fieldType = Need(field, "type");
                    if (field.Flag("bitfield"))
                        Line("((" + TypeName(type) + "*)(" + address + "))->" + Name(field) + " = " + ConvertValue(Constant(item), Need(item, "type"), fieldType) + ";");
                    else EmitConstantInitialization("((byte*)(" + address + ") + " + Num(field, "offset") + ")", fieldType, item, clearChild);
                }
            }
        }
    }
}
