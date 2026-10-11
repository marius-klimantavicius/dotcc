namespace DotObj.Relational;

internal sealed partial class Emitter
{
    private bool needsNullptrNormalization;
    private readonly string? nullptrValueName;

    private void WriteNullptrHelper()
    {
        if(nullptrValueName is string name)
        {
            Line(1,"// The recorded value normalization preserves operand effects, then yields null.");
            Line(1,"private static void* "+name+"(void* value) => null;");
        }
    }

    private sealed partial class FunctionWriter
    {
        private string NormalizeValue(Node node,string value)
        {
            return node.ValueNormalization switch
            {
                null=>value,
                "nullptr-zero"=>(emitter.nullptrValueName??throw unit.Missing(node,"value normalization helper missing"))+"("+value+")",
                _=>throw unit.Missing(node,"unsupported recorded value normalization: "+node.ValueNormalization)
            };
        }
    }
}
