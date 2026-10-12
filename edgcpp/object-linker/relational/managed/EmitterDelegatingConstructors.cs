namespace DotObj.Relational;

internal sealed partial class Emitter
{
    private sealed partial class FunctionWriter
    {
        private string DelegatingConstructorTarget(ConstructorInitializer member,long receiver)
        {
            long functionId=function?.Id??throw new InvalidDataException("Delegating constructor function scope missing");
            var initialization=unit.Initializations[member.Initialization];
            long destination=initialization.Type??member.Type??throw unit.Missing(unit.Nodes[unit.Functions[functionId]],
                "Delegating constructor destination type was not exported");
            var target=unit.Original(destination);
            if(target.Kind!="record" || target.Incomplete || target.Dependent ||
                initialization.Kind!="constructor" || initialization.Constructor is null)
                throw unit.Missing(unit.Nodes[unit.Functions[functionId]],
                    "Delegating constructor emission requires a supported complete record layout and selected constructor action");
            if(unit.BaseSubobjects.Values.Any(subobject=>subobject.Owner==target.Id && subobject.Virtual))
                throw unit.Missing(unit.Nodes[unit.Functions[functionId]],
                    "Delegating constructor virtual-base/VTT emission is not yet implemented");
            if(initialization.Destructor is not null || initialization.SelectedCleanupEntry is not null ||
                initialization.Lifetime is not null || initialization.ExpressionLifetime is not null ||
                initialization.DestructionCondition is not null || initialization.DestinationInitialization is not null)
                throw unit.Missing(unit.Nodes[unit.Functions[functionId]],
                    "Delegating constructor cleanup emission is not yet implemented");
            // The selected action constructs the same object, before this
            // constructor's body. Existing action emission supplies its exact
            // constructor and arguments without a separate temporary or copy.
            return "(*"+Name(receiver)+")";
        }
    }
}
