using System.Globalization;

namespace Edg.Semantic;

public sealed partial class CProjectEmitter
{
    // Concrete helpers deliberately avoid generic pointer type arguments and
    // closures. Their arguments snapshot C operands once, in a valid C order.
    private readonly Dictionary<string, string> expressionHelperNames = new(StringComparer.Ordinal);
    private readonly List<string> expressionHelpers = new();
    private readonly HashSet<ulong> activeProjectExpressions = new();
    private readonly HashSet<ulong> activeProjectAddresses = new();
    private SemanticNode? expressionPromotionInt;
    private IEnumerable<string> ExpressionHelpers => expressionHelpers;

    private static InvalidDataException ExError(SemanticNode node, string message) =>
        new($"C project expression node {node.Id} ({node.Kind}/{node.Form}): {message}");

    private SemanticNode ExType(SemanticNode node) => Need(node, "type");
    private string ExValueType(SemanticNode type) => Canonical(type).Form == "tk_void" ? "int" : TypeName(type);
    private static string ExCast(string type, string value) => $"unchecked(({type})({value}))";
    private SemanticNode[] ExOperands(SemanticNode node, int count = -1)
    {
        var operands = graph.Linked(node.ReferenceId("operands")).ToArray();
        if (count >= 0 && operands.Length != count) throw ExError(node, $"expected {count} operands, found {operands.Length}");
        return operands;
    }

    private string ExHelper(string result, IReadOnlyList<string> parameterTypes, string body)
    {
        var parameters = string.Join(", ", parameterTypes.Select((type, index) => type + " p" + index));
        var key = result + "(" + parameters + ")" + body;
        if (expressionHelperNames.TryGetValue(key, out var previous)) return previous;
        var name = "__ExprHelper_" + expressionHelperNames.Count.ToString(CultureInfo.InvariantCulture);
        expressionHelperNames.Add(key, name);
        expressionHelpers.Add("private static " + result + " " + name + "(" + parameters + ") { unchecked { " + body + " } }");
        return name;
    }

    private string Expr(SemanticNode expression)
    {
        if (expression.Kind != "expr-node") throw ExError(expression, "expected an expression");
        if (!activeProjectExpressions.Add(expression.Id)) throw ExError(expression, "cyclic expression tree");
        try
        {
            var value = ExRaw(expression);
            if (expression.Text("valueCategory") == "lvalue" &&
                (expression.Flag("volatileFetch") || ExQualified(ExType(expression), "volatile") || ExQualified(ExType(expression), "atomic")))
            {
                // Atomic load/store share one lock, giving sequential consistency
                // even when the original operation requests a weaker ordering.
                if (ExBitfield(expression) is { } field)
                    return ExBitfieldRead(expression, field);
                var type = TypeName(ExType(expression));
                var atomic = ExQualified(ExType(expression), "atomic");
                if (atomic) ExAtomicGate();
                var body = atomic ? "lock (__AtomicGate) { return *p0; }" :
                    "global::System.Threading.Thread.MemoryBarrier(); var value = *p0; global::System.Threading.Thread.MemoryBarrier(); return value;";
                return ExHelper(type, [type + "*"], body) + "(" + Address(expression) + ")";
            }
            return value;
        }
        finally { activeProjectExpressions.Remove(expression.Id); }
    }

    private string ExRaw(SemanticNode expression)
    {
        switch (expression.Form)
        {
            case "constant": return Constant(Need(expression, "constant"));
            case "variable": return VariableValue(Need(expression, "symbol"));
            case "routine":
                var routine = Need(expression, "symbol");
                if (!routine.Flag("defined")) externalRoutines.Add(routine.Id);
                return "((" + TypeName(ExType(expression)) + ")(&" + Name(routine) + "))";
            case "sizeof":
                var sized = expression.Reference("operandType") ?? ExType(Need(expression, "operand"));
                sized = Canonical(sized);
                if (sized.Flag("variableLength")) throw ExError(expression, "variable length sizeof needs its runtime bound");
                return ExCast(TypeName(ExType(expression)), Num(sized, "size").ToString(CultureInfo.InvariantCulture) + "UL");
            case "builtin_operation": return ExBuiltinOperation(expression);
            case "operation": return ExOperation(expression);
            default: throw ExError(expression, "unimplemented C expression form");
        }
    }

    private string Condition(SemanticNode expression)
    {
        var type = Canonical(ExType(expression));
        if (type.Form is not ("tk_integer" or "tk_float" or "tk_pointer"))
            throw ExError(expression, "condition must have scalar type");
        var op = expression.Text("operator");
        if (expression.Form == "operation" && op is "eok_eq" or "eok_ne" or "eok_lt" or "eok_le" or "eok_gt" or "eok_ge" or "eok_land" or "eok_lor" or "eok_not")
        {
            if (!activeProjectExpressions.Add(expression.Id)) throw ExError(expression, "cyclic condition tree");
            try
            {
                var operands = ExOperands(expression, op == "eok_not" ? 1 : 2);
                if (op == "eok_not") return "!(" + Condition(operands[0]) + ")";
                if (op is "eok_land" or "eok_lor") return "(" + Condition(operands[0]) + (op == "eok_land" ? " && " : " || ") + Condition(operands[1]) + ")";
                return ExBinary(expression, operands, ExBinaryTokens[op], booleanResult: true);
            }
            finally { activeProjectExpressions.Remove(expression.Id); }
        }
        return "(" + Expr(expression) + (type.Form == "tk_pointer" ? " != null)" : " != 0)");
    }

    // Discard assignment is legal for every nonvoid C# expression, unlike C#
    // expression statements. Void C expressions use int-valued adapters below.
    private string StatementExpr(SemanticNode expression)
    {
        if (expression.Form == "operation" && Canonical(ExType(expression)).Form == "tk_void")
        {
            if (expression.Text("operator") == "eok_call") return ExCall(expression, ExOperands(expression), voidAsValue: false);
            if (expression.Text("operator") == "eok_cast") return StatementExpr(ExOperands(expression, 1)[0]);
        }
        return "_ = " + Expr(expression);
    }

    private string ConvertValue(string text, SemanticNode fromType, SemanticNode targetType)
    {
        var from = Canonical(fromType);
        var target = Canonical(targetType);
        if (target.Form == "tk_void")
            return ExHelper("int", [ExValueType(fromType)], "return 0;") + "(" + text + ")";
        if (target.Form == "tk_integer" && target.Flag("bool"))
            return ExCast(TypeName(targetType), "(" + text + (from.Form == "tk_pointer" ? " != null" : " != 0") + ") ? 1 : 0");
        var targetName = TypeName(targetType);
        if (targetName == TypeName(fromType)) return text;
        if (target.Form == "tk_pointer" && from.Form == "tk_float")
            throw ExError(targetType, "floating-point to pointer conversion is not a C scalar conversion");
        return ExCast(targetName, text);
    }

    private string Address(SemanticNode expression)
    {
        if (!activeProjectAddresses.Add(expression.Id)) throw ExError(expression, "cyclic lvalue address tree");
        try { return ExAddress(expression); }
        finally { activeProjectAddresses.Remove(expression.Id); }
    }

    private string ExAddress(SemanticNode expression)
    {
        if (expression.Form == "variable") return VariableAddress(Need(expression, "symbol"));
        if (expression.Form == "routine") return ExRaw(expression);
        if (expression.Form == "constant")
        {
            var constant = Need(expression, "constant");
            if (Canonical(ExType(constant)).Form == "tk_array" || constant.Form == "ck_string")
                return Constant(constant);
        }
        if (expression.Form == "operation")
        {
            var operands = ExOperands(expression);
            var op = expression.Text("operator");
            if (op is "eok_indirect" or "eok_ref_indirect") return Expr(operands[0]);
            if (op is "eok_parens") return Address(operands[0]);
            if (op == "eok_lvalue_adjust") return "((" + TypeName(ExType(expression)) + "*)" + Address(operands[0]) + ")";
            if (op is "eok_dot_field" or "eok_points_to_field")
            {
                var field = Need(operands[1], "symbol");
                if (field.Flag("bitfield")) throw ExError(expression, "C forbids taking a bitfield address");
                var receiver = op == "eok_dot_field" ? Address(operands[0]) : Expr(operands[0]);
                // Byte offsets also cover zero-length/flexible array members.
                return "((" + TypeName(ExType(field)) + "*)((byte*)(" + receiver + ") + " + Num(field, "offset") + "))";
            }
            if (op == "eok_subscript") return ExPointerAdd(expression, operands[0], operands[1], "+");
        }
        if (ExBitfield(expression) != null) throw ExError(expression, "C forbids taking a bitfield address");
        return "(&(" + ExRaw(expression) + "))";
    }

    private string ExOperation(SemanticNode expression)
    {
        var op = expression.Text("operator") ?? throw ExError(expression, "missing operator");
        var args = ExOperands(expression);
        var expected = op switch
        {
            "eok_call" => -1,
            "eok_va_start" => 2,
            "eok_question" => 3,
            "eok_cast" or "eok_array_to_pointer" or "eok_address_of" or "eok_reference_to" or
            "eok_indirect" or "eok_ref_indirect" or "eok_parens" or "eok_lvalue_adjust" or "eok_not" or
            "eok_negate" or "eok_unary_plus" or "eok_complement" or "eok_pre_incr" or "eok_pre_decr" or
            "eok_post_incr" or "eok_post_decr" or "eok_va_start_single_operand" or "eok_va_end" or "eok_va_arg" => 1,
            _ => 2
        };
        if (expected >= 0 && args.Length != expected) throw ExError(expression, $"operator {op} expects {expected} operands, found {args.Length}");
        switch (op)
        {
            case "eok_call": return ExCall(expression, args);
            case "eok_cast": return ConvertValue(Expr(args[0]), ExType(args[0]), ExType(expression));
            case "eok_array_to_pointer": return ExCast(TypeName(ExType(expression)), Address(args[0]));
            case "eok_address_of": case "eok_reference_to": return Address(args[0]);
            case "eok_indirect": case "eok_ref_indirect":
                return ExFunctionPointer(ExType(args[0])) ? Expr(args[0]) : "(*(" + Expr(args[0]) + "))";
            case "eok_dot_field": case "eok_points_to_field":
                var field = Need(args[1], "symbol");
                var baseValue = op == "eok_dot_field" ? ExRaw(args[0]) : Expr(args[0]);
                return "(" + baseValue + (op == "eok_dot_field" ? ")." : ")->") + Name(field);
            case "eok_subscript": return "(*(" + ExPointerAdd(expression, args[0], args[1], "+") + "))";
            case "eok_padd": return ExPointerAdd(expression, args[0], args[1], "+");
            case "eok_psubtract": return ExPointerAdd(expression, args[0], args[1], "-");
            case "eok_pdiff":
                return ExCast(TypeName(ExType(expression)), "(" + Expr(args[0]) + " - " + Expr(args[1]) + ")");
            case "eok_parens": return "(" + Expr(args[0]) + ")";
            case "eok_lvalue_adjust": return "(*(" + TypeName(ExType(expression)) + "*)" + Address(args[0]) + ")";
            case "eok_not": return "(" + Condition(args[0]) + " ? 0 : 1)";
            case "eok_land": case "eok_lor":
                return "((" + Condition(args[0]) + (op == "eok_land" ? " && " : " || ") + Condition(args[1]) + ") ? 1 : 0)";
            case "eok_question":
                if (expression.Flag("gnuTwoOperandConditional")) throw ExError(expression, "GNU omitted-middle conditional needs an evaluation temporary");
                return "(" + Condition(args[0]) + " ? " + ConvertValue(Expr(args[1]), ExType(args[1]), ExType(expression)) +
                    " : " + ConvertValue(Expr(args[2]), ExType(args[2]), ExType(expression)) + ")";
            case "eok_comma":
                return ExHelper(ExValueType(ExType(args[1])), [ExValueType(ExType(args[0])), ExValueType(ExType(args[1]))], "return p1;") +
                    "(" + Expr(args[0]) + ", " + Expr(args[1]) + ")";
            case "eok_negate": case "eok_unary_plus": case "eok_complement":
                var operand = Expr(args[0]);
                var physical = ExPromotedType(ExType(args[0]));
                operand = ExCast(physical, operand);
                var unary = op == "eok_negate" ? (physical is "uint" or "ulong" ? "(" + physical + ")0 - " : "-") : op == "eok_complement" ? "~" : "+";
                return ExCast(TypeName(ExType(expression)), unary + operand);
            case "eok_pre_incr": case "eok_pre_decr": case "eok_post_incr": case "eok_post_decr":
                return ExMutation(expression, args[0], null, op.EndsWith("incr", StringComparison.Ordinal) ? "+" : "-", op.Contains("post", StringComparison.Ordinal));
            case "eok_assign": case "eok_bassign": return ExMutation(expression, args[0], args[1], "=");
            case "eok_add_assign": case "eok_padd_assign": return ExMutation(expression, args[0], args[1], "+");
            case "eok_subtract_assign": case "eok_psubtract_assign": return ExMutation(expression, args[0], args[1], "-");
            case "eok_multiply_assign": return ExMutation(expression, args[0], args[1], "*");
            case "eok_divide_assign": return ExMutation(expression, args[0], args[1], "/");
            case "eok_remainder_assign": return ExMutation(expression, args[0], args[1], "%");
            case "eok_and_assign": return ExMutation(expression, args[0], args[1], "&");
            case "eok_or_assign": return ExMutation(expression, args[0], args[1], "|");
            case "eok_xor_assign": return ExMutation(expression, args[0], args[1], "^");
            case "eok_shiftl_assign": return ExMutation(expression, args[0], args[1], "<<");
            case "eok_shiftr_assign": return ExMutation(expression, args[0], args[1], ">>");
            case "eok_va_start": case "eok_va_start_single_operand":
                return "__VaStart((__VaList*)(" + ExVaAddress(args[0]) + "), __argData, __args.Length)";
            case "eok_va_end": return "__VaEnd((__VaList*)(" + ExVaAddress(args[0]) + "))";
            case "eok_va_copy": return "__VaCopy((__VaList*)(" + ExVaAddress(args[0]) + "), (__VaList*)(" + ExVaAddress(args[1]) + "))";
            case "eok_va_arg":
                var vaType = Canonical(ExType(expression));
                var take = "__VaTake<" + (vaType.Form == "tk_pointer" ? "ulong" : TypeName(vaType)) + ">((__VaList*)(" + ExVaAddress(args[0]) + "))";
                return vaType.Form == "tk_pointer" ? ExCast(TypeName(vaType), take) : take;
        }
        if (ExBinaryTokens.TryGetValue(op, out var token))
            return ExBinary(expression, args, token);
        throw ExError(expression, "unsupported operator " + op);
    }

    private string ExBinary(SemanticNode expression, SemanticNode[] args, string token, bool booleanResult = false)
    {
        var left = Expr(args[0]); var right = Expr(args[1]);
        var comparison = token is "==" or "!=" or "<" or "<=" or ">" or ">=";
        var pointers = Canonical(ExType(args[0])).Form == "tk_pointer" || Canonical(ExType(args[1])).Form == "tk_pointer";
        string? common = null;
        if (pointers)
        {
            if (!comparison) throw ExError(expression, "unexpected noncomparison pointer arithmetic operator");
            left = ExCast("nuint", left); right = ExCast("nuint", right);
        }
        else
        {
            common = token is "<<" or ">>" ? ExPromotedType(ExType(args[0])) : ExArithmeticType(ExType(args[0]), ExType(args[1]));
            if (TypeName(ExType(args[0])) != common) left = ExCast(common, left);
            var rightType = token is "<<" or ">>" ? "int" : common;
            if (TypeName(ExType(args[1])) != rightType) right = ExCast(rightType, right);
        }
        var calculation = "(" + left + " " + token + " " + right + ")";
        if (comparison) return booleanResult ? calculation : "(" + calculation + " ? 1 : 0)";
        return TypeName(ExType(expression)) == common ? calculation : ExCast(TypeName(ExType(expression)), calculation);
    }

    private static readonly Dictionary<string, string> ExBinaryTokens = new(StringComparer.Ordinal)
    {
        ["eok_add"] = "+", ["eok_subtract"] = "-", ["eok_multiply"] = "*", ["eok_divide"] = "/", ["eok_remainder"] = "%",
        ["eok_and"] = "&", ["eok_or"] = "|", ["eok_xor"] = "^", ["eok_shiftl"] = "<<", ["eok_shiftr"] = ">>",
        ["eok_eq"] = "==", ["eok_ne"] = "!=", ["eok_lt"] = "<", ["eok_le"] = "<=", ["eok_gt"] = ">", ["eok_ge"] = ">="
    };

    private string ExPromotedType(SemanticNode type)
    {
        type = Canonical(type);
        if (type.Form == "tk_integer" && Num(type, "size") < 4) return "int";
        return TypeName(type);
    }

    private string ExArithmeticType(SemanticNode left, SemanticNode right)
    {
        left = Canonical(left); right = Canonical(right);
        if (left.Form == "tk_float" || right.Form == "tk_float")
        {
            // The type adapter selects the actual exported representation,
            // including the software x87 extended value used for long double.
            var floatLeftSize = left.Form == "tk_float" ? Num(left, "size") : 0;
            var floatRightSize = right.Form == "tk_float" ? Num(right, "size") : 0;
            return TypeName(floatLeftSize >= floatRightSize ? left : right);
        }
        if (left.Form != "tk_integer" || right.Form != "tk_integer") throw ExError(left, "expected arithmetic operand types");
        var leftSize = Math.Max(4UL, Num(left, "size")); var rightSize = Math.Max(4UL, Num(right, "size"));
        var leftUnsigned = Num(left, "size") >= 4 && !left.Flag("signed");
        var rightUnsigned = Num(right, "size") >= 4 && !right.Flag("signed");
        var unsigned = leftUnsigned && leftSize >= rightSize || rightUnsigned && rightSize >= leftSize;
        return Math.Max(leftSize, rightSize) switch
        {
            4 => unsigned ? "uint" : "int", 8 => unsigned ? "ulong" : "long",
            16 => unsigned ? "global::System.UInt128" : "global::System.Int128",
            _ => throw ExError(left, "unsupported arithmetic width")
        };
    }

    private string ExPointerAdd(SemanticNode owner, SemanticNode first, SemanticNode second, string token)
    {
        if (Canonical(ExType(first)).Form != "tk_pointer") (first, second) = (second, first);
        var pointer = Canonical(ExType(first));
        if (pointer.Form != "tk_pointer" || Canonical(ExType(second)).Form != "tk_integer") throw ExError(owner, "pointer arithmetic operand types");
        var element = Canonical(Need(pointer, "elementType"));
        var value = Expr(first);
        if (element.Form == "tk_routine") throw ExError(owner, "arithmetic on a function pointer");
        if (element.Form == "tk_void") value = "((byte*)(" + value + "))";
        var result = "(" + value + " " + token + " " + Expr(second) + ")";
        return element.Form == "tk_void" ? ExCast(TypeName(pointer), result) : result;
    }

    private bool ExFunctionPointer(SemanticNode type)
    {
        type = Canonical(type);
        return type.Form == "tk_pointer" && Canonical(Need(type, "elementType")).Form == "tk_routine";
    }

    private bool ExQualified(SemanticNode type, string qualifier)
    {
        var seen = new HashSet<ulong>();
        while (type.Form == "tk_typeref")
        {
            if (!seen.Add(type.Id)) throw ExError(type, "cyclic qualified type");
            if (type.Flag(qualifier)) return true;
            type = Need(type, "underlyingType");
        }
        return false;
    }

    private SemanticNode? ExBitfield(SemanticNode expression)
    {
        if (expression.Form != "operation" || expression.Text("operator") is not ("eok_dot_field" or "eok_points_to_field")) return null;
        var field = Need(ExOperands(expression, 2)[1], "symbol");
        return field.Flag("bitfield") ? field : null;
    }

    private (SemanticNode Type, string Address) ExReceiver(SemanticNode expression)
    {
        var receiver = ExOperands(expression, 2)[0];
        var type = Canonical(ExType(receiver));
        return expression.Text("operator") == "eok_points_to_field" ? (Need(type, "elementType"), Expr(receiver)) : (type, Address(receiver));
    }

    private string ExBitfieldRead(SemanticNode expression, SemanticNode field)
    {
        var receiver = ExReceiver(expression);
        return ExHelper(TypeName(ExType(expression)), [TypeName(receiver.Type) + "*"],
            "global::System.Threading.Thread.MemoryBarrier(); var value = p0->" + Name(field) + "; global::System.Threading.Thread.MemoryBarrier(); return value;") +
            "(" + receiver.Address + ")";
    }

    private string ExMutation(SemanticNode owner, SemanticNode left, SemanticNode? right, string token, bool post = false)
    {
        var leftType = ExType(left); var leftName = TypeName(leftType);
        var rightName = right == null ? "int" : ExValueType(ExType(right));
        var rightValue = right == null ? "1" : Expr(right);
        var volatileAccess = ExQualified(leftType, "volatile") || left.Flag("volatileFetch");
        var atomic = ExQualified(leftType, "atomic");
        var field = ExBitfield(left);
        if (token == "=" && field == null && !volatileAccess && !atomic)
            return "(" + ExRaw(left) + " = " + ConvertValue(rightValue, ExType(right!), leftType) + ")";
        if (field == null && !volatileAccess && !atomic)
        {
            var canonical = Canonical(leftType);
            var ordinaryPointer = canonical.Form == "tk_pointer" && Canonical(Need(canonical, "elementType")).Form is not ("tk_void" or "tk_routine");
            if (right == null && (canonical.Form is "tk_integer" or "tk_float" && !canonical.Flag("bool") || ordinaryPointer))
            {
                var increment = token == "+" ? "++" : "--";
                return "unchecked(" + (post ? ExRaw(left) + increment : increment + ExRaw(left)) + ")";
            }
            if (right != null && (leftName == rightName && leftName is "int" or "uint" or "long" or "ulong" or "float" or "double" ||
                ordinaryPointer && token is "+" or "-"))
                return "unchecked(" + ExRaw(left) + " " + token + "= " + (token is "<<" or ">>" ? ExCast("int", rightValue) : rightValue) + ")";
        }
        string pointerType, pointer, lvalue;
        if (field != null)
        {
            var receiver = ExReceiver(left);
            pointerType = TypeName(receiver.Type) + "*"; pointer = receiver.Address; lvalue = "p1->" + Name(field);
        }
        else { pointerType = leftName + "*"; pointer = Address(left); lvalue = "*p1"; }
        string calculation;
        if (token == "=") calculation = ConvertValue("p0", ExType(right!), leftType);
        else if (Canonical(leftType).Form == "tk_pointer") calculation = "(old " + token + " p0)";
        else
        {
            // Integer promotions depend on a bitfield's value range, not merely
            // its declared unsigned storage type (unsigned : 3 promotes to int).
            var promotedLeft = leftType;
            if (field != null && Num(field, "bitSize") < 32)
                promotedLeft = expressionPromotionInt ??= graph.OfKind("type").First(n => n.Form == "tk_integer" && n.Flag("signed") && Num(n, "size") == 4 && !n.Flag("enum"));
            var common = token is "<<" or ">>" || right == null ? ExPromotedType(promotedLeft) : ExArithmeticType(promotedLeft, ExType(right));
            calculation = "(" + ExCast(common, "old") + " " + token + " " + ExCast(token is "<<" or ">>" ? "int" : common, "p0") + ")";
            calculation = ExCast(leftName, Canonical(leftType).Flag("bool") ? "(" + calculation + " != 0) ? 1 : 0" : calculation);
        }
        var normalize = "";
        if (field != null && Num(field, "bitSize") < 64)
        {
            var bits = checked((int)Num(field, "bitSize"));
            normalize = "value = " + ExCast(leftName, Canonical(ExType(field)).Flag("signed") ?
                "((long)((ulong)value << " + (64 - bits) + ") >> " + (64 - bits) + ")" :
                "((ulong)value & 0x" + ((1UL << bits) - 1).ToString("x", CultureInfo.InvariantCulture) + "UL)") + "; ";
        }
        var body = (volatileAccess ? "global::System.Threading.Thread.MemoryBarrier(); " : "") +
            (token != "=" ? leftName + " old = " + lvalue + "; " : "") + leftName + " value = " + calculation + "; " +
            normalize + lvalue + " = value; " + (volatileAccess ? "global::System.Threading.Thread.MemoryBarrier(); " : "") +
            "return " + (post ? "old" : "value") + ";";
        if (atomic) { ExAtomicGate(); body = "lock (__AtomicGate) { " + body + " }"; }
        return ExHelper(leftName, [rightName, pointerType], body) + "(" + rightValue + ", " + pointer + ")";
    }

    private string ExVaAddress(SemanticNode expression) => Canonical(ExType(expression)).Form == "tk_pointer" ? Expr(expression) : Address(expression);

    private string ExCall(SemanticNode expression, SemanticNode[] operands, bool voidAsValue = true)
    {
        if (operands.Length == 0) throw ExError(expression, "call has no callee");
        var callee = operands[0]; var arguments = operands.Skip(1).ToArray();
        var routine = graph.DirectCallTarget(expression);
        if (routine != null && ExIntrinsicCall(expression, routine.Name ?? "", arguments) is { } intrinsic) return intrinsic;
        if (routine != null && !routine.Flag("defined")) externalRoutines.Add(routine.Id);
        var callable = Canonical(ExType(callee));
        if (callable.Form == "tk_pointer") callable = Canonical(Need(callable, "elementType"));
        if (callable.Form != "tk_routine") throw ExError(expression, "call target is not a routine");
        var signature = Need(callable, "signature");
        var fixedTypes = graph.Linked(signature.ReferenceId("parameters")).Select(ExType).ToArray();
        var variadic = signature.Flag("variadic");
        if (arguments.Length < fixedTypes.Length || !variadic && arguments.Length != fixedTypes.Length)
            throw ExError(expression, "call argument count does not match signature");
        var values = arguments.Take(fixedTypes.Length).Select((arg, index) => ConvertValue(Expr(arg), ExType(arg), fixedTypes[index])).ToList();
        if (variadic)
        {
            var pack = arguments.Skip(fixedTypes.Length).Select(arg => Canonical(ExType(arg)).Form == "tk_pointer" ?
                "__CArg.Pointer((void*)(" + Expr(arg) + "))" : "__CArg.From<" + TypeName(ExType(arg)) + ">(" + Expr(arg) + ")");
            values.Add("[" + string.Join(", ", pack) + "]");
        }
        // An extra pair around a bare identifier makes C# parse `(callback)(x)`
        // as a cast. Expr already groups compound callees by precedence.
        var target = routine == null ? Expr(callee) : Name(routine);
        if (!voidAsValue || Canonical(ExType(expression)).Form != "tk_void") return target + "(" + string.Join(", ", values) + ")";
        // Void calls have an int-valued adapter so they can be sequenced by
        // comma and void conditional expressions without a capturing lambda.
        var parameterTypes = fixedTypes.Select(TypeName).ToList();
        if (variadic) parameterTypes.Add("global::System.ReadOnlySpan<__CArg>");
        var offset = routine == null ? 1 : 0;
        var invokeTarget = routine == null ? "p0" : target;
        if (routine == null) { parameterTypes.Insert(0, TypeName(ExType(callee))); values.Insert(0, Expr(callee)); }
        var call = invokeTarget + "(" + string.Join(", ", Enumerable.Range(offset, parameterTypes.Count - offset).Select(index => "p" + index)) + "); return 0;";
        return ExHelper("int", parameterTypes, call) + "(" + string.Join(", ", values) + ")";
    }

    private string ExBuiltinOperation(SemanticNode expression)
    {
        var operands = ExOperands(expression);
        switch (expression.Text("builtinOperator"))
        {
            case "__builtin_offsetof":
                if (operands.Length != 2 || operands[0].Form != "type_operand") throw ExError(expression, "offsetof operand shape");
                return ExCast(TypeName(ExType(expression)), ExOffset(operands[1]));
            case "__builtin_types_compatible_p":
                return Canonical(Need(operands[0], "operandType")).Id == Canonical(Need(operands[1], "operandType")).Id ? "1" : "0";
            case "__builtin_addressof": return Address(operands[0]);
            case "__INTADDR__": return ExCast(TypeName(ExType(expression)), Expr(operands[0]));
            default: throw ExError(expression, "unsupported builtin operation " + expression.Text("builtinOperator"));
        }
    }

    private string ExOffset(SemanticNode expression)
    {
        if (expression.Form == "field") return Num(Need(expression, "symbol"), "offset") + "UL";
        if (expression.Form == "constant") return Expr(expression);
        if (expression.Form != "operation") throw ExError(expression, "offsetof member designator");
        var args = ExOperands(expression);
        switch (expression.Text("operator"))
        {
            case "eok_indirect": return "0UL";
            case "eok_dot_field": case "eok_points_to_field":
                return "(" + ExOffset(args[0]) + " + " + Num(Need(args[1], "symbol"), "offset") + "UL)";
            case "eok_array_to_pointer": case "eok_cast": case "eok_parens": return ExOffset(args[0]);
            case "eok_subscript":
                return "(" + ExOffset(args[0]) + " + unchecked((ulong)(" + Expr(args[1]) + ")) * " + Num(Canonical(ExType(expression)), "size") + "UL)";
            default: throw ExError(expression, "offsetof member path operator");
        }
    }

    private void ExAtomicGate()
    {
        if (expressionHelperNames.TryAdd("atomic-gate", "__AtomicGate"))
            expressionHelpers.Add("private static readonly object __AtomicGate = new();");
    }

    private string? ExIntrinsicCall(SemanticNode expression, string name, SemanticNode[] args)
    {
        if (name is "__builtin_expect" or "__builtin_expect_with_probability")
            return ExHelper(TypeName(ExType(expression)), args.Select(arg => ExValueType(ExType(arg))).ToArray(), "return p0;") + "(" + string.Join(", ", args.Select(Expr)) + ")";
        if (name.StartsWith("__atomic_load", StringComparison.Ordinal) || name.StartsWith("__atomic_store", StringComparison.Ordinal))
        {
            var store = name.StartsWith("__atomic_store", StringComparison.Ordinal);
            var suffix = name[(name.LastIndexOf('_') + 1)..];
            var valueType = suffix switch { "1" => "byte", "2" => "ushort", "4" => "uint", "8" => "ulong", _ =>
                store ? TypeName(ExType(args[1])) : TypeName(ExType(expression)) };
            ExAtomicGate();
            var body = store ? "*(" + valueType + "*)p0 = " + ExCast(valueType, "p1") + "; return 0;" :
                "return " + ExCast(TypeName(ExType(expression)), "*(" + valueType + "*)p0") + ";";
            return ExHelper(store ? "int" : TypeName(ExType(expression)), args.Select(arg => ExValueType(ExType(arg))).ToArray(), "lock (__AtomicGate) { " + body + " }") +
                "(" + string.Join(", ", args.Select(Expr)) + ")";
        }
        if (name is "__builtin_add_overflow" or "__builtin_sub_overflow" or "__builtin_mul_overflow")
        {
            var destination = Canonical(Need(Canonical(ExType(args[2])), "elementType"));
            var bits = checked((int)Num(destination, "size") * 8);
            var signed = destination.Flag("signed");
            var minimum = signed ? "-(global::System.Numerics.BigInteger.One << " + (bits - 1) + ")" : "global::System.Numerics.BigInteger.Zero";
            var maximum = "((global::System.Numerics.BigInteger.One << " + (signed ? bits - 1 : bits) + ") - 1)";
            var token = name == "__builtin_add_overflow" ? "+" : name == "__builtin_sub_overflow" ? "-" : "*";
            var body = "var value = (global::System.Numerics.BigInteger)p0 " + token + " (global::System.Numerics.BigInteger)p1; " +
                "bool overflow = value < " + minimum + " || value > " + maximum + "; " +
                "*p2 = unchecked((" + TypeName(destination) + ")(ulong)(value & ((global::System.Numerics.BigInteger.One << " + bits + ") - 1))); " +
                "return " + ExCast(TypeName(ExType(expression)), "overflow ? 1 : 0") + ";";
            return ExHelper(TypeName(ExType(expression)), args.Select(arg => TypeName(ExType(arg))).ToArray(), body) + "(" + string.Join(", ", args.Select(Expr)) + ")";
        }
        if (name is "__builtin_bswap16" or "__builtin_bswap32" or "__builtin_bswap64")
        {
            var physical = name.EndsWith("16", StringComparison.Ordinal) ? "ushort" : name.EndsWith("32", StringComparison.Ordinal) ? "uint" : "ulong";
            return ExCast(TypeName(ExType(expression)), "global::System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(" + ExCast(physical, Expr(args[0])) + ")");
        }
        if (name is "__builtin_clz" or "__builtin_clzl" or "__builtin_clzll")
            return ExCast(TypeName(ExType(expression)), "global::System.Numerics.BitOperations.LeadingZeroCount(" + ExCast(Num(Canonical(ExType(args[0])), "size") <= 4 ? "uint" : "ulong", Expr(args[0])) + ")");
        if (name is "__builtin_ctz" or "__builtin_ctzl" or "__builtin_ctzll")
            return ExCast(TypeName(ExType(expression)), "global::System.Numerics.BitOperations.TrailingZeroCount(" + ExCast(Num(Canonical(ExType(args[0])), "size") <= 4 ? "uint" : "ulong", Expr(args[0])) + ")");
        if (name is "__builtin_popcount" or "__builtin_popcountl" or "__builtin_popcountll")
            return ExCast(TypeName(ExType(expression)), "global::System.Numerics.BitOperations.PopCount(" + ExCast(Num(Canonical(ExType(args[0])), "size") <= 4 ? "uint" : "ulong", Expr(args[0])) + ")");
        if (name is "__builtin_abs" or "__builtin_labs" or "__builtin_llabs")
            return ExHelper(TypeName(ExType(expression)), [TypeName(ExType(args[0]))], "return p0 < 0 ? -p0 : p0;") + "(" + Expr(args[0]) + ")";
        if (name is "__builtin_inf" or "__builtin_inff" or "__builtin_huge_val" or "__builtin_huge_valf")
            return TypeName(ExType(expression)) + ".PositiveInfinity";
        if (name is "__builtin_ceil" or "__builtin_floor" or "__builtin_fabs" or "__builtin_sqrt")
        {
            var method = name switch { "__builtin_ceil" => "Ceiling", "__builtin_floor" => "Floor", "__builtin_fabs" => "Abs", _ => "Sqrt" };
            return ExCast(TypeName(ExType(expression)), "global::System.Math." + method + "(" + Expr(args[0]) + ")");
        }
        return null;
    }
}
