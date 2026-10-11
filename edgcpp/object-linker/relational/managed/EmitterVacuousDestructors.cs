namespace DotObj.Relational;

internal sealed partial class Emitter
{
    private sealed partial class FunctionWriter
    {
        private string VacuousDestructorExpression(Node node)
        {
            Node operand=Child(node,"operand");
            // EDG selected a vacuous operation. The points-to form evaluates
            // the pointer itself, without loading an object or calling a dtor.
            return node.Operator=="points-to-vacuous-destructor-call"
                ?Expression(operand):DiscardedLocation(operand);
        }

        private string DiscardedLocation(Node node)
        {
            if(node.ValueCategory is not ("lvalue" or "xvalue"))return Expression(node);
            if(node.Kind=="variable")
            {
                long symbol=node.Symbol??throw unit.Missing(node,"selected variable missing");
                if(unit.IsArray(unit.Symbols[symbol].Type) || emitter.HasPinnedScalarStorage(unit,symbol) || emitter.HasPinnedRecordStorage(unit,symbol))return Name(symbol);
                if(unit.Symbols[symbol].Parent is null)
                    throw unit.Missing(node,"discarded global location requires pinned storage emission");
                return "(&"+Expression(node)+")";
            }
            if(node.Kind=="lifetime-scope")return DiscardedLocation(Child(node,"operand"));
            if(node.Kind=="operation")
            {
                Node first=unit.Children(node,"operand").FirstOrDefault()
                    ??throw unit.Missing(node,"discarded location operand missing");
                switch(node.Operator)
                {
                    case "indirect":case "ref-indirect":case "points-to-field":return Expression(first);
                    case "dot-field":case "lvalue-adjust":case "class-rvalue-adjust":return DiscardedLocation(first);
                    case "subscript":return "(&"+Expression(node)+")";
                    case "assign":case "pre-increment":case "pre-decrement":return Expression(node);
                }
                if(UsesCompoundHelper(node))return Expression(node);
            }
            throw unit.Missing(node,"discarded glvalue location emission is not implemented");
        }
    }
}
