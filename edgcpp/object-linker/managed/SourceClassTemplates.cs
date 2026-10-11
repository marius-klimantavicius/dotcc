using System.Globalization;
namespace DotObj;

internal sealed record SourceClassTemplateHeader(long? Primary,long FormalCount,long PatternCount,long? Constraint);
internal sealed record SourceClassSpecialization(long Selected,long ArgumentCount);
internal sealed partial class ObjectFile
{
    public Dictionary<long,SourceClassTemplateHeader> SourceClassTemplateHeaders { get; }=[];
    public Dictionary<long,SourceClassSpecialization> SourceClassSpecializations { get; }=[];
    public Dictionary<long,List<SourceTemplateFormal>> SourceClassTemplateFormals { get; }=[];
    public Dictionary<long,List<SourceTemplateArgument>> SourceClassTemplatePatterns { get; }=[];
    public Dictionary<long,List<SourceTemplateArgument>> SourceClassSpecializationArguments { get; }=[];

    private void ReadSourceClassTemplates()
    {
        using(var q=database.Prepare("SELECT source_record_id,primary_source_record_id,formal_count,pattern_count,constraint_expression_id,capture_proof FROM source_class_template_headers ORDER BY source_record_id"))
            while(q.Read())
            {
                long id=q.Integer(0);var value=new SourceClassTemplateHeader(q.OptionalInteger(1),q.Integer(2),q.Integer(3),q.OptionalInteger(4));
                if(!SourceRecords.TryGetValue(id,out var record) || !record.IsPrototypeInstantiation || !record.IsDependent ||
                    value.Primary==id || value.Primary is long primary && !SourceRecords.ContainsKey(primary) ||
                    value.FormalCount<0 || value.PatternCount<0 || value.Constraint is long expression && !SourceExpressions.ContainsKey(expression) ||
                    q.Text(5)!="original-class-template-header" || !SourceClassTemplateHeaders.TryAdd(id,value))
                    throw new InvalidDataException("Original class template header lost its prototype/formal owner");
            }
        using(var q=database.Prepare("SELECT source_record_id,selected_source_record_id,selected_argument_count,capture_proof FROM source_class_specializations ORDER BY source_record_id"))
            while(q.Read())
            {
                long id=q.Integer(0);var value=new SourceClassSpecialization(q.Integer(1),q.Integer(2));
                if(!SourceRecordInstantiations.TryGetValue(id,out var instance) || !instance.HasSelectedPartialSpecialization ||
                    instance.ResolutionKind!="class-template" || instance.PrototypeSourceRecordId!=value.Selected ||
                    !SourceClassTemplateHeaders.TryGetValue(value.Selected,out var selected) || selected.Primary is null || value.ArgumentCount<0 ||
                    q.Text(3)!="original-partial-class-substitution" || !SourceClassSpecializations.TryAdd(id,value))
                    throw new InvalidDataException("Partial class instance lost its selected original specialization");
            }
        ReadSourceConstraintFormals("source_class_template_formals","source_record_id",SourceClassTemplateHeaders.ContainsKey,SourceClassTemplateFormals);
        ReadSourceClassArguments("source_class_template_patterns",SourceClassTemplateHeaders.ContainsKey,SourceClassTemplatePatterns);
        ReadSourceClassArguments("source_class_specialization_arguments",SourceClassSpecializations.ContainsKey,SourceClassSpecializationArguments);
    }
    private void ReadSourceClassArguments(string table,Func<long,bool> ownerExists,Dictionary<long,List<SourceTemplateArgument>> target)
    {
        using var q=database.Prepare($"SELECT source_record_id,ordinal,kind,type_id,constant_id,template_id,is_pack FROM {table} ORDER BY source_record_id,ordinal");
        while(q.Read())
        {
            long owner=q.Integer(0);var argument=new SourceTemplateArgument(q.Integer(1),q.Text(2),q.OptionalInteger(3),q.OptionalInteger(4),q.OptionalInteger(5),q.Integer(6)==1);
            if(!ownerExists(owner) || q.Integer(6) is not(0 or 1) || !SourceConstraintPayloadValid(argument.Kind,argument.TypeId,argument.ConstantId,argument.TemplateId))
                throw new InvalidDataException("Class template arguments lost their original typed owner");
            if(!target.TryGetValue(owner,out var list))target[owner]=list=[];
            if(argument.Ordinal!=list.Count)throw new InvalidDataException("Class template argument order is not contiguous");list.Add(argument);
        }
    }
    private void VerifySourceClassTemplates()
    {
        foreach(var pair in SourceClassTemplateHeaders)
        {
            var value=pair.Value;var record=SourceRecords[pair.Key];
            var formals=SourceClassTemplateFormals.GetValueOrDefault(pair.Key,[]);var pattern=SourceClassTemplatePatterns.GetValueOrDefault(pair.Key,[]);
            if(formals.Count!=value.FormalCount || pattern.Count!=value.PatternCount || value.Primary is null && pattern.Count!=0)
                throw new InvalidDataException("Original class template header/pattern lists are incomplete");
            if(value.Primary is long primary)
            {
                if(!SourceClassTemplateHeaders.TryGetValue(primary,out var original) || original.Primary is not null ||
                    SourceRecords[primary].SourceName!=record.SourceName || SourceRecords[primary].RecordKind!=record.RecordKind)
                    throw new InvalidDataException("Selected class specialization lost its original primary header");
            }
        }
        foreach(var pair in SourceRecordInstantiations)
        {
            if(pair.Value.HasSelectedPartialSpecialization!=SourceClassSpecializations.ContainsKey(pair.Key))
                throw new InvalidDataException("Partial class instance is missing its two original argument mappings");
            if(!pair.Value.HasSelectedPartialSpecialization)continue;
            var selected=SourceClassSpecializations[pair.Key];var header=SourceClassTemplateHeaders[selected.Selected];
            var arguments=SourceClassSpecializationArguments.GetValueOrDefault(pair.Key,[]);
            if(arguments.Count!=selected.ArgumentCount)throw new InvalidDataException("Selected class argument mapping is incomplete");

        }
    }
}
