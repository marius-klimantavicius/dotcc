namespace DotObj.Relational;

internal sealed partial class Emitter
{
    private sealed partial class FunctionWriter
    {
        private sealed class ContinueTarget(string? label)
        {
            public string? Label {get;}=label;
            public bool Used {get;set;}
        }
        private readonly Stack<ContinueTarget> continueTargets=[];

        private void WriteContinue(Node node,int indent)
        {
            if(!continueTargets.TryPeek(out var target))throw unit.Missing(node,"continue has no emitted loop target");
            if(target.Label is string label)
            {
                target.Used=true;
                Line(indent,"goto "+label+";");
            }
            else Line(indent,"continue;");
        }

        private void WriteLoopBody(Node loop,int indent)
        {
            continueTargets.Push(new(null));
            breakTargets.Push(null);
            try {Body(Child(loop,"body"),indent);}
            finally {breakTargets.Pop();continueTargets.Pop();}
        }

        private void WriteConditionInitialization(Node condition,int indent)
        {
            if(unit.Children(condition,"initializer").FirstOrDefault() is Node initializer)Statement(initializer,indent);
            if(condition.Initialization is not null)WriteDeclaration(condition,indent);
        }

        private void WriteIf(Node statement,int indent)
        {
            Node condition=Child(statement,"condition");
            void Selection(int level,Node value)
            {
                Line(level,"if ("+Truth(value,ExpandedExpression(value,level))+")");Body(Child(statement,"then"),level);
                if(unit.Children(statement,"else").FirstOrDefault() is Node alternative)
                {
                    Line(level,"else");Body(alternative,level);
                }
            }
            if(condition.Kind!="condition"){Selection(indent,condition);return;}
            Line(indent,"{");
            WriteLifetimeRegion(condition,indent+1,level=>
            {
                WriteConditionInitialization(condition,level);
                Selection(level,Child(condition,"value"));
            });
            Line(indent,"}");
        }

        private void WriteConstexprIf(Node statement,int indent)
        {
            string selection=statement.Constant??throw unit.Missing(statement,"exporter defect: constexpr-if selection missing");
            Line(indent,"// if constexpr ("+selection+")");
            Node? selected=unit.Children(statement,"selected-body").FirstOrDefault();
            Node? condition=unit.Children(statement,"condition").FirstOrDefault();
            void Selected(int level)
            {
                if(selected is null)Line(level,";");
                else Body(selected,level);
            }
            // EDG selected the branch. A condition wrapper still owns its
            // runtime initializer and cleanup; its predicate is metadata only.
            if(condition is null){Selected(indent);return;}
            if(condition.Kind!="condition")
                throw unit.Missing(condition,"constexpr-if initializer wrapper emission is not implemented");
            Line(indent,"{");
            WriteLifetimeRegion(condition,indent+1,level=>
            {
                WriteConditionInitialization(condition,level);
                Selected(level);
            });
            Line(indent,"}");
        }

        private void WriteWhile(Node statement,int indent)
        {
            Node condition=Child(statement,"condition");
            if(condition.Kind=="condition")WriteConditionLoop(statement,condition,null,indent);
            else
            {
                Line(indent,"while ("+Truth(condition)+")");WriteLoopBody(statement,indent);
            }
        }

        private void WriteFor(Node statement,int indent)
        {
            Node? condition=unit.Children(statement,"condition").FirstOrDefault();
            Node? initializer=unit.Children(statement,"initializer").FirstOrDefault();
            Node? step=unit.Children(statement,"step").FirstOrDefault();
            bool hasLifetime=statement.Lifetime is not null || unit.Lifetimes.Values.Any(lifetime=>lifetime.OwnerNode==statement.Id);
            if(condition?.Kind!="condition" && !hasLifetime)
            {
                Line(indent,"for ("+(initializer is null?"":Header(initializer))+"; "+(condition is null?"":Truth(condition))+"; "+(step is null?"":Header(step))+")");
                WriteLoopBody(statement,indent);return;
            }
            Line(indent,"{");
            WriteLifetimeRegion(statement,indent+1,level=>
            {
                if(initializer is not null)WriteForInitializer(initializer,level);
                if(condition?.Kind=="condition")WriteConditionLoop(statement,condition,step,level);
                else
                {
                    Line(level,"for (; "+(condition is null?"":Truth(condition))+"; "+(step is null?"":Header(step))+")");
                    WriteLoopBody(statement,level);
                }
            });
            Line(indent,"}");
        }

        private void WriteRangeFor(Node statement,int indent)
        {
            Line(indent,"{");
            WriteLifetimeRegion(statement,indent+1,level=>
            {
                if(unit.Children(statement,"initializer").FirstOrDefault() is Node initializer)WriteForInitializer(initializer,level);
                foreach(string role in new[]{"range","begin","end"})Statement(Child(statement,role),level);
                // EDG supplies the selected comparison/increment expressions.
                // The body owns its iterator lifetime, so continue first runs
                // that cleanup and then executes the ordinary for increment.
                Line(level,"for (; "+Truth(Child(statement,"condition"))+"; "+StatementExpression(Child(statement,"step"))+")");
                WriteLoopBody(statement,level);
            });
            Line(indent,"}");
        }

        private void WriteForInitializer(Node initializer,int indent)
        {
            if(initializer.Kind!="block"){Statement(initializer,indent);return;}
            if(initializer.Lifetime is not null || unit.Lifetimes.Values.Any(lifetime=>lifetime.OwnerNode==initializer.Id))
                throw unit.Missing(initializer,"for initializer block has separate lifetime ownership requiring scope emission");
            foreach(var child in unit.Children(initializer,"statement"))Statement(child,indent);
        }

        private void WriteConditionLoop(Node statement,Node condition,Node? step,int indent)
        {
            Line(indent,"while (true)");Line(indent,"{");
            WriteLifetimeRegion(condition,indent+1,level=>
            {
                WriteConditionInitialization(condition,level);
                Line(level,"if (!"+Truth(Child(condition,"value"))+") break;");
                var target=new ContinueTarget(step is null?null:"__continue_step_"+statement.Id);
                continueTargets.Push(target);
                breakTargets.Push(null);
                try {Body(Child(statement,"body"),level);}
                finally {breakTargets.Pop();continueTargets.Pop();}
                if(target.Used)Line(level,target.Label+":;");
                if(step is not null)Line(level,StatementExpression(step,ExpandedExpression(step,level))+";");
            });
            Line(indent,"}");
        }
    }
}
