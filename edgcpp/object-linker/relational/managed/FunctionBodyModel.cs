namespace DotObj.Relational;

// Only the active function owns these dictionaries. Dropping its lease also
// drops their backing arrays; a Unit does not retain its largest past body.
internal sealed class FunctionBody
{
    public Dictionary<long,Node> Nodes {get;}
    public Dictionary<long,List<Edge>> Edges {get;}
    public Dictionary<long,Lifetime> Lifetimes {get;}
    public Dictionary<long,Initialization> Initializations {get;}
    public Dictionary<long,List<long>> InitializationArguments {get;}
    public Dictionary<long,List<long>> Destructions {get;}
    public Dictionary<long,List<ConstructorInitializer>> ConstructorInitializers {get;}
    public Dictionary<long,List<Handler>> Handlers {get;}
    public Dictionary<long,Throw> Throws {get;}
    public Dictionary<long,ClassConversion> ClassConversions {get;}
    public Dictionary<long,FloatingValue> FloatingValues {get;}

    public FunctionBody(FunctionBody? globals=null)
    {
        Nodes=globals is null?[]:new(globals.Nodes);
        Edges=globals is null?[]:new(globals.Edges);
        Lifetimes=globals is null?[]:new(globals.Lifetimes);
        Initializations=globals is null?[]:new(globals.Initializations);
        InitializationArguments=globals is null?[]:new(globals.InitializationArguments);
        Destructions=globals is null?[]:new(globals.Destructions);
        ConstructorInitializers=globals is null?[]:new(globals.ConstructorInitializers);
        Handlers=globals is null?[]:new(globals.Handlers);
        Throws=globals is null?[]:new(globals.Throws);
        ClassConversions=globals is null?[]:new(globals.ClassConversions);
        FloatingValues=globals is null?[]:new(globals.FloatingValues);
    }

    public long RowCount=>Nodes.Count+Edges.Values.Sum(values=>(long)values.Count)+Lifetimes.Count+Initializations.Count+
        InitializationArguments.Values.Sum(values=>(long)values.Count)+Destructions.Values.Sum(values=>(long)values.Count)+
        ConstructorInitializers.Values.Sum(values=>(long)values.Count)+Handlers.Values.Sum(values=>(long)values.Count)+
        Throws.Count+ClassConversions.Count+FloatingValues.Count;
}

internal sealed class ReleaseScope(Action release) : IDisposable
{
    private Action? callback=release;
    public void Dispose()
    {
        var action=callback;callback=null;action?.Invoke();
    }
}

internal sealed partial class Unit
{
    private readonly FunctionBody globalBody=new();
    private FunctionBody? functionBody;
    private FunctionBody Body=>functionBody??globalBody;
    public long LoadedFunctionRows=>functionBody is null?0:functionBody.RowCount-globalBody.RowCount;
    public long LoadedFunctionNodes=>functionBody is null?0:functionBody.Nodes.Count-globalBody.Nodes.Count;
    public long GlobalExpressionRows=>globalBody.RowCount;
    public long GlobalExpressionNodes=>globalBody.Nodes.Count;

    public IDisposable LoadFunction(long function)
    {
        if(functionBody is not null)throw new InvalidOperationException("A function body is already loaded for "+Path);
        functionBody=new(globalBody);
        try
        {
            ReadNodes(function);
            ReadFloatingValues(function);
            ReadLifecycles(function);
            ReadClassConversions(function);
            return new ReleaseScope(()=>functionBody=null);
        }
        catch {functionBody=null;throw;}
    }

    // The interpolated values below are internal numeric SQLite identities.
    private static string FunctionFilter(long? function,string alias="")=>alias+"function_id"+
        (function is long id?" = "+id.ToString(System.Globalization.CultureInfo.InvariantCulture):" IS NULL");

    private void ReadNodes(long? function)
    {
        using(var q=database.Prepare("SELECT id,function_id,kind,type_id,execution_type_id,value_category,operator,constant_text,symbol_id,field_id,file_id,line,column,original_lvalue_type_id,"+(HasLifecycles?"initialization_id,lifetime_id,evaluation_order":"NULL,NULL,NULL")+","+(HasEnums?"enum_value_id":"NULL")+","+(HasFloatingValues?"floating_value_id":"NULL")+" FROM nodes WHERE "+FunctionFilter(function)+" ORDER BY id"))
            while(q.Read())Nodes.Add(q.Integer(0),new(q.Integer(0),q.OptionalInteger(1),q.Text(2),q.OptionalInteger(3),q.OptionalInteger(4),q.OptionalText(5),q.OptionalText(6),q.OptionalText(7),q.OptionalInteger(8),q.OptionalInteger(9),q.OptionalInteger(10),q.OptionalInteger(11),q.OptionalInteger(12),q.OptionalInteger(13),q.OptionalInteger(14),q.OptionalInteger(15),q.OptionalText(16),q.OptionalInteger(17),q.OptionalInteger(18)));
        using(var q=database.Prepare("SELECT e.parent_id,e.role,e.ordinal,e.child_id FROM nodes n JOIN edges e ON e.parent_id=n.id WHERE "+FunctionFilter(function,"n.")+" ORDER BY e.parent_id,e.role,e.ordinal"))
            while(q.Read())Add(Edges,q.Integer(0),new Edge(q.Text(1),q.Integer(2),q.Integer(3)));
    }
}
