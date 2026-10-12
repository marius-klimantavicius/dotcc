namespace DotObj.Relational;

internal sealed partial class Emitter
{
    private readonly Dictionary<string,(Unit Unit,ProgramType Type,string Name)> enumerations=new(StringComparer.Ordinal);
    private readonly Dictionary<(Unit,long),string> enumValueNames=[];

    private void PrepareEnums()
    {
        foreach(var unit in program.Units)
            foreach(var type in ExecutableTypeRows(unit).Where(type=>type.Kind=="enum"))
            {
                if(!unit.Enums.TryGetValue(type.Id,out var details))
                    throw new InvalidDataException("Exporter defect: enum declaration missing for "+type.Name);
                string key=type.Linkage=="external"?"external:"+type.LinkName:unit.Path+":"+type.Id;
                if(!enumerations.TryGetValue(key,out var enumeration))
                {
                    enumeration=(unit,type,names.Allocate(type.Name,"enum_"+type.Id));
                    enumerations.Add(key,enumeration);
                }
                else if(!enumeration.Unit.Enums[enumeration.Type.Id].Definition && details.Definition)
                    enumerations[key]=(unit,type,enumeration.Name);
                typeNames.Add((unit,type.Id),enumeration.Name);
                var members=new Names();
                members.Reserve(enumeration.Name);
                foreach(var value in unit.EnumValues.Values.Where(value=>value.Type==type.Id).OrderBy(value=>value.Ordinal))
                    enumValueNames.Add((unit,value.Id),members.Allocate(value.Name,"value_"+value.Ordinal));
            }
    }

    private static string IntegerLiteral(string type,string value)=>type switch
    {
        "uint"=>value+"U","long"=>value+"L","ulong"=>value+"UL",
        "sbyte" or "byte" or "short" or "ushort"=>"(("+type+")"+value+")",_=>value
    };

    private void WriteEnums()
    {
        foreach(var enumeration in enumerations.Values)
        {
            var (unit,type,name)=enumeration;
            long underlying=type.Underlying??throw new InvalidDataException("Exporter defect: enum underlying type missing for "+type.Name);
            string storage=Type(unit,underlying);
            if(storage is not ("sbyte" or "byte" or "short" or "ushort" or "int" or "uint" or "long" or "ulong"))
                throw new InvalidDataException("C# enum storage is not supported: "+type.Name+" ("+storage+")");
            Line(1,"public enum "+name+" : "+storage);Line(1,"{");
            foreach(var value in unit.EnumValues.Values.Where(value=>value.Type==type.Id).OrderBy(value=>value.Ordinal))
                Line(2,enumValueNames[(unit,value.Id)]+" = "+IntegerLiteral(storage,value.Value??throw new InvalidDataException("Dependent enumerator runtime emission is not supported: "+type.Name+"."+value.Name))+",");
            Line(1,"}");
        }
    }

    private sealed partial class FunctionWriter
    {
        private string? EnumConstant(Node node)
        {
            long typeId=node.ExecutionType??node.Type??throw unit.Missing(node,"constant expression type missing");
            var type=unit.Original(typeId);
            if(node.EnumValue is long selected)
            {
                if(!unit.EnumValues.TryGetValue(selected,out var value))
                    throw unit.Missing(node,"exporter defect: selected enumerator missing");
                string reference=Type(value.Type)+"."+emitter.enumValueNames[(unit,selected)];
                return type.Id==unit.Original(value.Type).Id?reference:"unchecked(("+Type(typeId)+")"+reference+")";
            }
            if(type.Kind!="enum")return null;
            long underlying=type.Underlying??throw unit.Missing(node,"exporter defect: enum underlying type missing");
            string literal=IntegerLiteral(Type(underlying),node.Constant??throw unit.Missing(node,"enum constant payload missing"));
            return "unchecked(("+Type(typeId)+")"+literal+")";
        }
    }
}
