namespace DotObj;

internal sealed record InitializationAction(long Id, long EntityId, string Phase,
    long Priority, long Ordinal, long? SourceFileId, long Line, long Column);

internal sealed partial class ObjectFile
{
    private readonly List<InitializationAction> initializationActions = [];
    public IReadOnlyList<InitializationAction> InitializationActions => initializationActions;

    private void ReadInitializationActions()
    {
        using var query = database.Prepare("SELECT id,entity_id,phase,priority,ordinal,source_file_id,line,column FROM initialization_actions ORDER BY ordinal");
        var targets = new HashSet<long>();
        var ordinals = new HashSet<long>();
        while (query.Read())
        {
            var action = new InitializationAction(query.Integer(0), query.Integer(1), query.Text(2),
                query.Integer(3), query.Integer(4), query.OptionalInteger(5), query.Integer(6), query.Integer(7));
            if (action.Id <= 0 || action.Phase != "startup" || action.Priority is < 0 or > 65535 ||
                action.Ordinal < 0 || !ordinals.Add(action.Ordinal) || action.Line < 0 || action.Column < 0 || !targets.Add(action.EntityId))
                throw new InvalidDataException("Invalid startup action identity, phase, priority, order or location");
            if (!Entities.TryGetValue(action.EntityId, out Entity? entity) || !Functions.ContainsKey(entity.Id) ||
                entity.Definition is not ("strong" or "linkonce" or "weak") || Type(entity.ReturnTypeId) != "void" ||
                Parameters.GetValueOrDefault(entity.Id, []).Count != 0 || Routines[entity.RoutineTypeId].Variadic)
                throw new InvalidDataException("Startup action must name a defined nonvariadic void(void) function");
            initializationActions.Add(action);
        }
    }
}
