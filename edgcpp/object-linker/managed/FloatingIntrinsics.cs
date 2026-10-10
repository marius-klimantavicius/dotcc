namespace DotObj;

// GNU classification builtins are polymorphic semantic operations. Their
// frontend ellipsis declaration does not request C default argument promotions.
// Keep exact builtin identity in the intrinsic operator, including f/l forms.
internal static class FloatingIntrinsics
{
    public static bool IsClassification(string operation) => operation is
        "__builtin_signbit" or "__builtin_signbitf" or "__builtin_signbitl" or
        "__builtin_isnan" or "__builtin_isnanf" or "__builtin_isnanl" or
        "__builtin_isinf" or "__builtin_isinff" or "__builtin_isinfl" or "__builtin_isfinite";

    public static bool AcceptsOperand(string operation, string type) => operation switch
    {
        "__builtin_signbitf" or "__builtin_isnanf" or "__builtin_isinff" => type == "f32",
        "__builtin_signbitl" or "__builtin_isnanl" or "__builtin_isinfl" => type == "f80",
        "__builtin_signbit" or "__builtin_isnan" or "__builtin_isinf" or "__builtin_isfinite" => ScalarTypes.IsFloating(type),
        _ => false,
    };
}
