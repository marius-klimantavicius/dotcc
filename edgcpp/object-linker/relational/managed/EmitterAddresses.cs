namespace DotObj.Relational;

internal sealed partial class Emitter
{
    private sealed partial class FunctionWriter
    {
        private bool IsVolatileLocation(Node node)=>
            node.OriginalLvalueType is long original && unit.IsVolatile(original) ||
            node.Type is long type && unit.IsVolatile(type);

        private string AddressExpression(Node node)
        {
            if(node.Type is long array && unit.IsArray(array))return ArrayAddress(node);
            if(node.Kind=="variable" && node.Symbol is long symbol)
            {
                if(emitter.HasPinnedRecordStorage(unit,symbol) || emitter.HasPinnedScalarStorage(unit,symbol) || emitter.HasProviderAddress(unit,symbol))return Name(symbol);
                if(unit.Symbols[symbol].Parent is null)
                    throw unit.Missing(node,"address-taken global requires pinned storage support");
                return "(&"+Name(symbol)+")";
            }
            if(node.Kind=="operation")
            {
                Node first=unit.Children(node,"operand").FirstOrDefault()
                    ??throw unit.Missing(node,"address designation operand missing");
                switch(node.Operator)
                {
                    case "indirect":case "ref-indirect":return Expression(first);
                    case "lvalue-adjust":return AddressExpression(first);
                    case "dot-field":case "points-to-field":
                        long field=node.Field??unit.Children(node,"operand").Skip(1).FirstOrDefault()?.Field
                            ??throw unit.Missing(node,"selected address field missing");
                        if(unit.Fields[field].BitWidth is not null)
                            throw unit.Missing(node,"bitfield property has no separately addressable storage");
                        string receiver=node.Operator=="points-to-field"?Expression(first):AddressExpression(first);
                        return "(&("+receiver+")->"+emitter.fieldNames[(unit,field)]+")";
                    case "subscript":
                        return "(&("+Expression(first)+")["+Expression(unit.Children(node,"operand").Skip(1).First())+"])";
                }
            }
            return "(&"+Expression(node)+")";
        }

        private void ExpandAddressLocation(Node node,int indent)
        {
            if(node.Kind=="variable")return;
            if(node.Kind=="operation")
            {
                Node first=unit.Children(node,"operand").FirstOrDefault()
                    ??throw unit.Missing(node,"address designation operand missing");
                switch(node.Operator)
                {
                    case "indirect":case "ref-indirect":case "points-to-field":
                        ExpandedExpression(first,indent);return;
                    case "dot-field":case "lvalue-adjust":
                        ExpandAddressLocation(first,indent);return;
                    case "subscript":
                        if(node.EvaluationOrder=="right-to-left")
                            throw unit.Missing(node,"right-to-left address designation expansion is not yet implemented");
                        ExpandedExpression(first,indent);
                        ExpandedExpression(unit.Children(node,"operand").Skip(1).First(),indent);return;
                }
            }
            ExpandedExpression(node,indent);
        }
    }
}
