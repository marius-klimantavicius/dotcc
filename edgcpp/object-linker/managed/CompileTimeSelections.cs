namespace DotObj;

internal sealed record CompileTimeSelection(long NodeId, bool ConditionValue, bool HasElse);

internal sealed partial class ObjectFile
{
    public Dictionary<long, CompileTimeSelection> CompileTimeSelections { get; } = [];

    private void ReadCompileTimeSelections()
    {
        using var query = database.Prepare("SELECT x.node_id,x.condition_value,x.has_else,x.capture_proof,n.function_id,n.kind FROM compile_time_selections x LEFT JOIN nodes n ON n.id=x.node_id ORDER BY x.node_id");
        while (query.Read())
        {
            if (SourceLanguage != "c++" || query.Integer(1) is not (0 or 1) || query.Integer(2) is not (0 or 1) ||
                query.Text(3) != "observed-constexpr-selection" || query.OptionalInteger(4) is not long owner ||
                !Functions.ContainsKey(owner) || query.OptionalText(5) != "compile-time-if" ||
                !CompileTimeSelections.TryAdd(query.Integer(0), new(query.Integer(0), query.Integer(1) != 0, query.Integer(2) != 0)))
                throw new InvalidDataException("Compile-time selection lacks its original resolved C++ decision and function");
        }
    }
}
