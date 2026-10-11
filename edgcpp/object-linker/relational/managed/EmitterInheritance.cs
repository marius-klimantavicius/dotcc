namespace DotObj.Relational;

internal sealed partial class Emitter
{
    private readonly Dictionary<(Unit,long),string> baseProjections=[];
    private readonly Dictionary<(Unit,long),string> classConversions=[];

    private void WriteInheritanceHelpers()
    {
        // A base's complete-object size can include padding occupied by another
        // subobject. Project its recorded address rather than embedding a CLR
        // field that could enlarge the containing record.
        foreach(var unit in program.Units)
        {
            foreach(var subobject in unit.BaseSubobjects.Values.Where(subobject=>executableTypes.Contains((unit,subobject.Owner))))
            {
                if(subobject.Virtual)continue;
                string owner=Type(unit,subobject.Owner),target=Type(unit,subobject.Type);
                string name=names.Allocate("__base_"+target+"_in_"+owner,"base_subobject");
                baseProjections.Add((unit,subobject.Id),name);
                Line(1,"private static "+target+"* "+name+"("+owner+"* value) => ("+target+"*)((byte*)value + "+subobject.Offset+");");
            }
        }
    }

    private void WriteClassConversionHelpers(Unit unit,long? function)
    {
        foreach(var conversion in unit.ClassConversions.Values.Where(conversion=>unit.Nodes[conversion.Node].Function==function &&
            (function is not null || executableGlobalNodes.Contains((unit,conversion.Node)))))
        {
            Node node=unit.Nodes[conversion.Node];
            if(!unit.BaseSubobjects.TryGetValue(conversion.Base,out var subobject))
                throw unit.Missing(node,"exporter defect: selected base subobject missing");
            if(subobject.Virtual || conversion.Adjustment is null)
                throw unit.Missing(node,"virtual base conversion emission is not yet implemented");
            long source,target;
            switch(conversion.Direction)
            {
                case "to-base":source=subobject.Owner;target=subobject.Type;break;
                case "to-derived":source=subobject.Type;target=subobject.Owner;break;
                default:throw unit.Missing(node,"unsupported recorded class conversion direction "+conversion.Direction);
            }
            string sourceType=Type(unit,source),targetType=Type(unit,target);
            string name=names.Allocate("__as_"+targetType+"_from_"+sourceType,"class_conversion");
            classConversions.Add((unit,node.Id),name);
            string address="("+targetType+"*)((byte*)value + ("+conversion.Adjustment+"))";
            // The helper evaluates a side-effectful source once, including
            // when EDG requests null-preserving pointer adjustment.
            Line(1,"private static "+targetType+"* "+name+"("+sourceType+"* value) => "+(conversion.PreservesNull?"value == null ? null : ":"")+address+";");
        }
    }

    private void ReleaseClassConversionNames(Unit unit,long function)
    {
        foreach(var conversion in unit.ClassConversions.Values.Where(conversion=>unit.Nodes[conversion.Node].Function==function))
            classConversions.Remove((unit,conversion.Node));
    }

    private sealed partial class FunctionWriter
    {
        private string ClassConversionExpression(Node node,Node operand)
        {
            if(!unit.ClassConversions.TryGetValue(node.Id,out var conversion))
                throw unit.Missing(node,"exporter defect: selected class conversion facts missing");
            if(conversion.SourceRepresentation=="object" && operand.ValueCategory=="prvalue" && !expandedValues.ContainsKey(operand.Id))
                throw unit.Missing(node,"class value conversion requires temporary materialization");
            string source=Expression(operand);
            source=conversion.SourceRepresentation switch
            {
                "pointer" or "reference"=>source,
                "object"=>"(&"+source+")",
                _=>throw unit.Missing(node,"unsupported class conversion source representation "+conversion.SourceRepresentation)
            };
            string result=emitter.classConversions[(unit,node.Id)]+"("+source+")";
            return conversion.ResultRepresentation switch
            {
                "pointer" or "reference"=>result,
                "object"=>"(*"+result+")",
                _=>throw unit.Missing(node,"unsupported class conversion result representation "+conversion.ResultRepresentation)
            };
        }
    }
}
