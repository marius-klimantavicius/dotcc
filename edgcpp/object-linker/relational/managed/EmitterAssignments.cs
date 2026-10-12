namespace DotObj.Relational;

internal sealed partial class Emitter
{
    private sealed partial class FunctionWriter
    {
        private bool IsDirectLocalAssignment(Node node)
        {
            return node.ValueCategory=="prvalue" && IsDiscardedLocalAssignment(node);
        }

        private bool IsDiscardedLocalAssignment(Node node)
        {
            if(node.Kind!="operation" || node.Operator!="assign")return false;
            var operands=unit.Children(node,"operand").ToArray();
            if(operands.Length!=2 || operands[0].Kind!="variable" || operands[0].Symbol is not long symbol ||
               !locals.ContainsKey(symbol) || unit.Symbols[symbol].Kind is not ("local" or "parameter"))return false;
            if(unit.IsVolatile(unit.Symbols[symbol].Type) || IsVolatileLocation(operands[0]))return false;
            return unit.Original(unit.Symbols[symbol].Type).Kind is
                "bool" or "i8" or "u8" or "i16" or "u16" or "i32" or "u32" or "i64" or "u64" or "i128" or "u128" or
                "f32" or "f64" or "f80" or "f128" or "enum" or "pointer" or "nullptr";
        }
    }
}
