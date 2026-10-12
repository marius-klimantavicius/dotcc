namespace DotObj.Relational;

internal sealed partial class Emitter
{
    private sealed partial class FunctionWriter
    {
        private void RequireSupportedAbiBody(Node body,long functionId)
        {
            if(unit.Edges.GetValueOrDefault(body.Id,[]).Any(edge=>edge.Role is "destruction-condition" or "abi-prologue"))
                throw unit.Missing(body,"selected destructor ABI entry predicate/VTT prologue emission is not yet implemented");
            foreach(var action in unit.Initializations.Values.Where(action=>action.Function==functionId))
                if(action.DestructionCondition is not null || action.SelectedCleanupEntry is not null ||
                   action.SelectedCleanupCallableType is not null || unit.CleanupArguments.GetValueOrDefault(action.Id,[]).Count!=0)
                    throw unit.Missing(body,"selected destructor ABI cleanup invocation emission is not yet implemented: initialization "+action.Id);
        }
    }
}
