namespace DotObj.Relational;

internal sealed partial class Emitter
{
    private static bool IsZeroExtentArray(Unit unit,long id)
    {
        var type=unit.Original(id);
        return type.Kind=="array" && !type.Dependent && !type.Incomplete &&
            type.Count==0 && type.Size==0 && type.Alignment is >0 && type.Element is not null;
    }

    private void WriteZeroExtentArrayField(Unit unit,ProgramType owner,string ownerName,Field field)
    {
        var array=unit.Original(field.Type);
        if(field.Offset<0 || field.Offset>owner.Size || field.BitWidth is not null)
            throw new InvalidDataException("Recorded zero-extent field offset exceeds its owner: "+owner.Name+"."+field.Name);
        long element=array.Element!.Value;
        string type=Type(unit,element),name=fieldNames[(unit,field.Id)];
        string source=unit.Types[element].Name;
        if(source.Length==0)source=unit.Original(element).Name;
        if(source.Length==0)source=unit.Original(element).BuiltinName??type;
        Line(2,"// Source zero-extent field: "+source+" "+field.Name+"[0], byte offset "+field.Offset+"; no storage.");
        Line(2,"public "+type+"* "+name);Line(2,"{");Line(3,"get");Line(3,"{");
        Line(4,"fixed ("+ownerName+"* __receiver = &this)");
        Line(5,"return ("+type+"*)((byte*)__receiver + "+field.Offset+");");
        Line(3,"}");Line(2,"}");
    }

    private sealed partial class FunctionWriter
    {
        private string? ZeroExtentFieldAddress(Node node)
        {
            if(node.Kind!="operation")return null;
            if(node.Operator is "lvalue-adjust" or "lvalue-cast")return ZeroExtentFieldAddress(Child(node,"operand"));
            if(node.Operator is not ("dot-field" or "points-to-field"))return null;
            var operands=unit.Children(node,"operand").ToArray();
            long? selected=node.Field??operands.Skip(1).FirstOrDefault()?.Field;
            if(selected is not long id || !IsZeroExtentArray(unit,unit.Fields[id].Type))return null;
            if(node.Lifetime is long lifetime && unit.Destructions.GetValueOrDefault(lifetime,[]).Any(cleanup=>!allocationOwnedCleanups.Contains(cleanup)))
                throw unit.Missing(node,"zero-extent field address expression cleanup needs statement expansion");
            if(operands.Length==0)throw unit.Missing(node,"zero-extent field receiver missing");
            return "("+Expression(operands[0])+")"+(node.Operator=="dot-field"?".":"->")+emitter.fieldNames[(unit,id)];
        }
    }
}
