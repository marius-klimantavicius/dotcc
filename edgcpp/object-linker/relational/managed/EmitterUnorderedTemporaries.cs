namespace DotObj.Relational;

internal sealed partial class Emitter
{
    private sealed partial class FunctionWriter
    {
        private readonly Dictionary<long,(long Lifetime,int Start)> unorderedTemporaryGroups=[];
        private readonly Dictionary<long,int> unorderedTemporaryCompletionOrder=[];
        private int temporaryCompletionSequence;
        private int nonlinearTemporaryEmissionDepth;

        private void PrepareUnorderedTemporaryGroups(long functionId)
        {
            foreach(var list in unit.Destructions)
            {
                if(unit.Lifetimes[list.Key].Function!=functionId)continue;
                int start=-1;
                for(int ordinal=0;ordinal<list.Value.Count;++ordinal)
                {
                    long id=list.Value[ordinal];
                    if(!unit.Initializations[id].Unordered){start=-1;continue;}
                    if(start<0)start=ordinal;
                    unorderedTemporaryGroups.Add(id,(list.Key,start));
                }
            }
        }

        private bool CanEmitUnorderedTemporary(Initialization initialization)=>
            expressionTemporaryStorage.ContainsKey(initialization.Id) && unorderedTemporaryGroups.TryGetValue(initialization.Id,out var group) &&
            unit.Lifetimes[group.Lifetime].Kind=="expression-temporary" &&
            !initialization.Conditional && initialization.DestinationInitialization is null &&
            initialization.Kind is "constructor" or "class-result";

        private void RecordUnorderedTemporaryCompletion(Initialization initialization)
        {
            if(initialization.Unordered)
                if(!unorderedTemporaryCompletionOrder.TryAdd(initialization.Id,checked(++temporaryCompletionSequence)))
                    throw new InvalidDataException("Unordered temporary has multiple emitted constructions: initialization "+initialization.Id);
        }

        private IReadOnlyList<long> CompletedTemporaryDestructionOrder(IReadOnlyList<long> actions)
        {
            var ordered=actions.ToArray();
            for(int index=0;index<ordered.Length;)
            {
                if(!unit.Initializations[ordered[index]].Unordered){++index;continue;}
                if(!unorderedTemporaryGroups.TryGetValue(ordered[index],out var group))
                    throw new InvalidDataException("Recorded unordered cleanup group missing for initialization "+ordered[index]);
                int end=index;
                while(end<ordered.Length && unorderedTemporaryGroups.TryGetValue(ordered[end],out var next) && next==group)
                {
                    if(!unorderedTemporaryCompletionOrder.ContainsKey(ordered[end]))
                        throw new InvalidDataException("Unordered temporary cleanup requires a linear emitted construction for initialization "+ordered[end]);
                    ++end;
                }
                var run=ordered[index..end].OrderByDescending(id=>unorderedTemporaryCompletionOrder[id]).ToArray();
                run.CopyTo(ordered,index);
                index=end;
            }
            return ordered;
        }
    }
}
