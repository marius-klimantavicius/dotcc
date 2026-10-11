namespace DotObj.Relational;

internal sealed record FunctionDetails(string Kind,long? Receiver,string ExceptionPolicy,long? NrvoCandidate);
internal sealed record Lifetime(long Id,long? Function,string Kind,long? Parent,long? OwnerNode,long? ParentInitialization,long? SiblingOrdinal);
internal sealed record Initialization(long Id,long? Function,string Kind,long? Type,long? Variable,long? Constructor,long? Destructor,
    long? Lifetime,long? ExpressionLifetime,long? Value,bool Conditional,bool Unordered,bool OverlapsInner,bool PartialAggregate,
    bool FreeOnException,bool FollowsExecution,bool TemporaryLifetime,bool ConstructorInit,bool PartiallyInitialized,bool StaticStorage,
    bool ImpliedSource,bool SequencedArguments,bool ValueInitialization,long? OverlapLifetime,long? DestinationInitialization);
internal sealed record ConstructorInitializer(string Kind,long? Field,long? Type,bool Virtual,long Initialization,long? Base,long? CopySource);
internal sealed record Handler(long CatchNode,long? Parameter,long? Type,long? Initialization);
internal sealed record Throw(long Node,long? Type,long? Initialization,long? Destructor,bool Rethrow);

internal sealed partial class Unit
{
    public Dictionary<long,FunctionDetails> FunctionDetails {get;}=[];
    private bool hasCopySource,hasInitializationDestination;
    public Dictionary<long,Lifetime> Lifetimes=>Body.Lifetimes;
    public Dictionary<long,Initialization> Initializations=>Body.Initializations;
    public Dictionary<long,List<long>> InitializationArguments=>Body.InitializationArguments;
    public Dictionary<long,List<long>> Destructions=>Body.Destructions;
    public Dictionary<long,List<ConstructorInitializer>> ConstructorInitializers=>Body.ConstructorInitializers;
    public Dictionary<long,List<Handler>> Handlers=>Body.Handlers;
    public Dictionary<long,Throw> Throws=>Body.Throws;

    private void ReadFunctionDetails()
    {
        if(!HasLifecycles)
        {
            foreach(long function in Functions.Keys)FunctionDetails.Add(function,new("ordinary",null,"unwind",null));
            return;
        }
        using(var q=database.Prepare("SELECT symbol_id,kind,receiver_symbol_id,exception_policy,"+(HasReturnStorage?"nrvo_candidate_symbol_id":"NULL")+" FROM functions ORDER BY symbol_id"))
            while(q.Read())FunctionDetails.Add(q.Integer(0),new(q.Text(1),q.OptionalInteger(2),q.Text(3),q.OptionalInteger(4)));
        using(var q=database.Prepare("SELECT 1 FROM pragma_table_info('constructor_initializers') WHERE name='copy_source_symbol_id'"))
            hasCopySource=q.Read();
        using(var q=database.Prepare("SELECT 1 FROM pragma_table_info('initializations') WHERE name='destination_initialization_id'"))
            hasInitializationDestination=q.Read();
    }

    private void ReadLifecycles(long? function)
    {
        if(!HasLifecycles)return;
        using(var q=database.Prepare("SELECT id,function_id,kind,parent_id,owner_node_id,parent_initialization_id,sibling_ordinal FROM lifetimes WHERE "+FunctionFilter(function)+" ORDER BY id"))
            while(q.Read())Lifetimes.Add(q.Integer(0),new(q.Integer(0),q.OptionalInteger(1),q.Text(2),q.OptionalInteger(3),q.OptionalInteger(4),q.OptionalInteger(5),q.OptionalInteger(6)));
        using(var q=database.Prepare("SELECT id,function_id,kind,type_id,variable_symbol_id,constructor_symbol_id,destructor_symbol_id,lifetime_id,init_expr_lifetime_id,value_node_id,conditional,unordered,overlaps_inner,partial_aggregate,free_on_exception,follows_exec,temporary_lifetime,is_constructor_init,is_partially_initialized,static_storage,implied_source,sequenced_arguments,value_initialization,overlap_lifetime_id,"+(hasInitializationDestination?"destination_initialization_id":"NULL")+" FROM initializations WHERE "+FunctionFilter(function)+" ORDER BY id"))
            while(q.Read())Initializations.Add(q.Integer(0),new(q.Integer(0),q.OptionalInteger(1),q.Text(2),q.OptionalInteger(3),q.OptionalInteger(4),q.OptionalInteger(5),q.OptionalInteger(6),q.OptionalInteger(7),q.OptionalInteger(8),q.OptionalInteger(9),q.Integer(10)!=0,q.Integer(11)!=0,q.Integer(12)!=0,q.Integer(13)!=0,q.Integer(14)!=0,q.Integer(15)!=0,q.Integer(16)!=0,q.Integer(17)!=0,q.Integer(18)!=0,q.Integer(19)!=0,q.Integer(20)!=0,q.Integer(21)!=0,q.Integer(22)!=0,q.OptionalInteger(23),q.OptionalInteger(24)));
        using(var q=database.Prepare("SELECT a.initialization_id,a.node_id FROM initializations i JOIN initialization_arguments a ON a.initialization_id=i.id WHERE "+FunctionFilter(function,"i.")+" ORDER BY a.initialization_id,a.ordinal"))
            while(q.Read())Add(InitializationArguments,q.Integer(0),q.Integer(1));
        using(var q=database.Prepare("SELECT d.lifetime_id,d.initialization_id FROM lifetimes l JOIN lifetime_destructions d ON d.lifetime_id=l.id WHERE "+FunctionFilter(function,"l.")+" ORDER BY d.lifetime_id,d.ordinal"))
            while(q.Read())Add(Destructions,q.Integer(0),q.Integer(1));
        using(var q=database.Prepare("SELECT function_id,kind,field_id,target_type_id,is_virtual,initialization_id,"+(HasBaseSubobjects?"base_subobject_id":"NULL")+","+(hasCopySource?"copy_source_symbol_id":"NULL")+" FROM constructor_initializers WHERE "+FunctionFilter(function)+" ORDER BY function_id,ordinal"))
            while(q.Read())Add(ConstructorInitializers,q.Integer(0),new ConstructorInitializer(q.Text(1),q.OptionalInteger(2),q.OptionalInteger(3),q.Integer(4)!=0,q.Integer(5),q.OptionalInteger(6),q.OptionalInteger(7)));
        using(var q=database.Prepare("SELECT h.try_node_id,h.catch_node_id,h.parameter_symbol_id,h.parameter_type_id,h.initialization_id FROM nodes n JOIN try_handlers h ON h.try_node_id=n.id WHERE "+ProgramNodeFilter(function,"n.")+" ORDER BY h.try_node_id,h.ordinal"))
            while(q.Read())Add(Handlers,q.Integer(0),new Handler(q.Integer(1),q.OptionalInteger(2),q.OptionalInteger(3),q.OptionalInteger(4)));
        using(var q=database.Prepare("SELECT t.node_id,t.type_id,t.initialization_id,t.destructor_symbol_id,t.is_rethrow FROM nodes n JOIN throws t ON t.node_id=n.id WHERE "+ProgramNodeFilter(function,"n.")+" ORDER BY t.node_id"))
            while(q.Read())Throws.Add(q.Integer(0),new(q.Integer(0),q.OptionalInteger(1),q.OptionalInteger(2),q.OptionalInteger(3),q.Integer(4)!=0));
    }
}
