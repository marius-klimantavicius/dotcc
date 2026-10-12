namespace DotObj.Relational;

internal sealed record StartupGroup(long Function,long Ordinal,long Priority,bool ThreadLocal);

internal sealed partial class Unit
{
    public List<StartupGroup> StartupGroups {get;}=[];

    private void ReadStartupGroups()
    {
        using(var q=database.Prepare("SELECT 1 FROM sqlite_master WHERE type='table' AND name='startup_groups'"))
            if(!q.Read())return;
        using(var q=database.Prepare("SELECT function_symbol_id,ordinal,init_priority,is_thread_local FROM startup_groups ORDER BY ordinal"))
            while(q.Read())StartupGroups.Add(new(q.Integer(0),q.Integer(1),q.Integer(2),q.Integer(3)!=0));
    }
}
