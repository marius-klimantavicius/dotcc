using RoslynObj;
using System.Numerics;
using System.Reflection;

internal static class ExecutionProjection
{
    internal static void Verify(TranslationUnit unit,string fixture,string profile,Action<bool,string> check)
    {
        var source=profile=="source_semantic";
        if(fixture is "execution_c.roslynobj" or "execution_cpp.roslynobj" or "retained.roslynobj" or "source_statements.roslynobj" or "native_microsoft.roslynobj" or "source_vla.roslynobj" or "template_edges.roslynobj" or "m0.roslynobj")
            SweepFamilies(unit.Entities.Where(e=>e is Operation or SwitchDescriptor or SwitchCaseEntry or VlaDimension or AssemblyDefinition or LambdaDefinition),check);
        if(fixture is "constant_occurrences.roslynobj" or "constant_aggregates.roslynobj" or "constant_embed.roslynobj" or "fixed_point.roslynobj" or "nonreal_float.roslynobj" or "gnu_nonreal_float.roslynobj" or "member_pointers.roslynobj" or "label_difference.roslynobj" or "upc_dynamic.roslynobj" or "upc_static.roslynobj")
            SweepFamilies(unit.Entities.Where(e=>e is Constant or ConstantValue or SubobjectPath),check);
        if(fixture is "execution_c.roslynobj" or "execution_cpp.roslynobj")
        {
            var nodes=unit.Entities.OfType<SemanticNode>().ToArray();
            SemanticProjection.Sweep(nodes.OfType<ExpressionOperation>().Select(o=>o.Details),"expression",check);
            SemanticProjection.Sweep(nodes.OfType<OperatorOperation>().Select(o=>o.Operator),"operator_expression",check);
            SemanticProjection.Sweep(nodes.OfType<StatementOperation>().Select(o=>o.Details),"statement",check);
            SemanticProjection.Sweep(unit.Entities.OfType<SwitchDescriptor>().Select(d=>d.Details),"switch_descriptor",check);
            SemanticProjection.Sweep(unit.Entities.OfType<SwitchCaseEntry>().Select(d=>d.Details),"switch_case_entry",check);
            SemanticProjection.Sweep(unit.Entities.OfType<VlaDimension>().Select(d=>d.Details),"vla_dimension",check);
            check(nodes.OfType<ConversionOperation>().Any(c=>c.Operand.Type==c.SourceType&&c.TargetType==c.Type&&c.OperatorKind.Name is "eok_cast" or "eok_lvalue_adjust"),"Actual cast operators must dispatch to ordinary conversion navigation over their retained operand and node types.");
            if(source)check(nodes.OfType<StatementOperation>().All(s=>s.Details.LoweringGenerated is null&&s.Details.IsLoweringBoilerplate is null)&&nodes.OfType<ExpressionOperation>().All(e=>e.Details.BooleanNotNormalized is null),"Source profiles must retain unknown lowering-only flag states when those fields are absent from the compiled IL.");
        }
        if(fixture=="execution_c.roslynobj")
        {
            FunctionBody Body(string name) => unit.Functions.Single(f=>f.Name==name).Body!;
            var sw=Body("execution_switch").DescendantOperations().OfType<SwitchOperation>().Single();
            check(sw.Cases.Select(c=>c.IsDefault?new BigInteger?():((IntegerValue)c.LowConstant!.Value).Value).SequenceEqual(new BigInteger?[]{9,1,5,null})&&sw.SortedCases.Select(c=>((IntegerValue)c.LowConstant!.Value).Value).SequenceEqual(new BigInteger[]{1,5,9}),"The actual switch descriptor must preserve source case order separately from recorded numeric order.");
            check(sw.DefaultCase==sw.Cases[3]&&sw.SortedCases.All(c=>sw.Cases.Contains(c))&&sw.Cases.All(c=>c.Statement is SwitchCaseOperation operation&&operation.Entry==c&&operation.EnclosingSwitch==sw),"Source/default/sorted lists and statement backpointers must share exact canonical switch-case identities.");
            check(((IntegerValue)sw.Cases[2].HighConstant!.Value).Value==7&&sw.Cases.Any(c=>c.ReachableByFallThrough),"A genuine GNU case range and recorded fallthrough reachability must remain typed exact payloads.");
            var only=Body("execution_default_only").DescendantOperations().OfType<SwitchOperation>().Single();
            check(!only.Descriptor.HasSortedCases&&only.SortedCases.Count==0&&only.DefaultCase==only.Cases.Single(),"A NULL sorted-list head in a default-only switch must preserve empty-list presence without claiming dependency.");
            var incomplete=(ArrayType)unit.Variables.Single(v=>v.Name=="execution_incomplete").Type;
            var zero=(ArrayType)unit.Variables.Single(v=>v.Name=="execution_zero").Type;
            check(incomplete.Bound is null&&incomplete.Size is null&&zero.Bound==0&&zero.Size==0,"An incomplete array must stay distinct from an explicitly zero-sized GNU array in ordinary bounds and size navigation.");
            if(source)
            {
                int Calls(string name) => Body(name).DescendantOperations().OfType<CallOperation>().Count(c=>c.TargetMethod?.Name=="execution_dimension");
                check(Calls("execution_sizeof_type")==1&&Calls("execution_vla_cast")==1&&Calls("execution_typedef_size")==1,"On-the-fly sizeof/cast dimensions must execute once while retained typedef dimensions are not reevaluated at later sizeof uses.");
                var query=Body("execution_sizeof_type").DescendantOperations().OfType<TypeQueryOperation>().Single();
                check(query.IsDynamic&&query.EvaluatedDimensions.Single().Expression is CallOperation&&query.VlaDimensions.Single()==query.EvaluatedDimensions.Single(),"A runtime source sizeof type must expose its actual evaluated dimension and dynamic flag.");
                var expression=Body("execution_sizeof_expression").DescendantOperations().OfType<TypeQueryOperation>().Single();
                check(expression.IsDynamic&&expression.Operand is not null&&Body("execution_sizeof_expression").DescendantOperations().OfType<OperatorOperation>().Count(o=>o.OperatorKind.Name=="eok_post_incr")==1,"Runtime source sizeof(*pointer++) must retain the actual evaluated operand and its one post-increment.");
                check(Calls("execution_unevaluated")==0&&Body("execution_unevaluated").DescendantOperations().OfType<TypeQueryOperation>().All(q=>!q.IsDynamic),"Ordinary sizeof/alignof operands must stay unevaluated and retain a negative dynamic flag.");
                check(Calls("execution_generic")==0&&Calls("execution_choose")==0,"Actual generic/choose selection must exclude the unselected dimension call from executable containment.");
            }
        }
        if(fixture=="execution_cpp.roslynobj"&&source)
        {
            check(unit.Entities.OfType<ExpressionOperation>().Any(e=>e.Details.IsTypeConstraint),"A real constrained template must retain the concept type-constraint expression flag.");
            check(unit.Entities.OfType<ConversionOperation>().Any(c=>c.OperatorKind.Name=="eok_dynamic_cast"&&c.Operand.Type is PointerType&&c.TargetType is PointerType),"A source dynamic cast must navigate actual pointer operand/result types without a fabricated conversion row.");
        }
        if(fixture=="retained.roslynobj"&&source)
            check(unit.Entities.OfType<TemporaryOperation>().Count(t=>t.SourceType is {Name:"ProtocolAwaiter"}&&t.Initialization is not null)==6,"Actual coroutine temporaries must retain their six source ProtocolAwaiter descriptors and initialization identities without consulting private original-type links.");
        if(fixture is "upc_dynamic.roslynobj" or "upc_static.roslynobj")
        {
            var dynamic=fixture=="upc_dynamic.roslynobj";var value=unit.Variables.Single(v=>v.Name=="upc_values").Type;
            var array=(ArrayType)((TypeReference)value).UnderlyingType;
            check(dynamic?value.Size is null&&value.EffectiveSize==8&&value.ThreadsSizeMultiplier==8&&array.Bound is null&&array.ThreadsMultiplier==2:value.Size==32&&array.Bound==8&&array.ThreadsMultiplier is null,"Dynamic UPC THREADS coefficients must stay symbolic while a fixed four-thread profile exposes exact concrete totals.");
            check(unit.Variables.Single(v=>v.Name=="upc_row_pointer").Type.Size==8,"A pointer to a THREADS-dependent array must retain its concrete target pointer size.");
            if(dynamic)check(unit.Entities.OfType<UpcThreadsValue>().Any(v=>v.Multiplier==3),"A genuine 3*THREADS constant must expose its exact symbolic multiplier rather than a concrete thread count.");
        }
        if(fixture=="vector_arm.roslynobj")
        {
            check(unit.Types.OfType<StorageFloatingType>().Any(t=>t.Kind.Name=="tk_mfp8"&&t.Size==1)&&unit.Types.OfType<ScalableVectorCountType>().Any(t=>t.Size is null),"Payloadless modal-float storage and scalable-count types must navigate valid ordinary facts without fabricated arithmetic/vector subtype rows.");
        }
        if(fixture=="constant_aggregates.roslynobj")
        {
            var constants=unit.Entities.OfType<Constant>().ToArray();
            check(constants.Any(c=>c.Details.UsesDesignatedInitializers)&&constants.Any(c=>c.Details.IsCompoundLiteral)&&constants.Any(c=>c.Details.FlexibleArrayInitializer==true)&&constants.Any(c=>c.Details.IsPartiallyInitialized),"Actual C designated/compound/flexible/partial aggregate occurrences must retain their independent captured flags.");
            var repeats=unit.Entities.OfType<ConstantValue<RepeatValueDetails>>().ToArray();
            check(repeats.Length==2&&repeats.Any(v=>v.Payload.Count==4&&v.Payload.Value is IntegerValue {Value:var n}&&n==13),"A genuine GNU range initializer must preserve its exact repeated value and repetition count.");
            if(source)check(unit.Entities.OfType<ConstantValue<DesignatorValueDetails>>().Any(v=>v.Payload.IndexValue==2)&&unit.Entities.OfType<ConstantValue<DesignatorValueDetails>>().Any(v=>v.Payload.Field?.Name=="tail"),"Actual source designators must retain separate selected field and numeric subscript arms.");
        }
        if(fixture=="constant_embed.roslynobj")
        {
            var constants=unit.Entities.OfType<Constant>().Where(c=>c.Details.IsEmbedExpansion==true).ToArray();
            check(constants.Length==2&&constants.All(c=>c.Value is StringValue {StorageUnitBits:8,ByteLength:var bytes}&&bytes==4)&&((StringValue)unit.Variables.Single(v=>v.Name=="constant_embed").ConstantInitializer!.Value).TargetBytes.SequenceEqual(new byte[]{69,68,71,10})&&((StringValue)unit.Variables.Single(v=>v.Name=="constant_embed_limited").ConstantInitializer!.Value).TargetBytes.SequenceEqual(new byte[]{7,69,68,11}),"Real embed occurrences must preserve their exact bytes, including limit/prefix/suffix results; optional directive text stays governed by its retention profile.");
        }
        if(fixture=="constant_occurrences.roslynobj")
        {
            var constants=unit.Entities.OfType<Constant>().ToArray();
            SemanticProjection.Sweep(constants.Select(c=>c.Details),"constant",check);
            SemanticProjection.Sweep(unit.Entities.OfType<SubobjectPath>().Select(p=>p.Details),"subobject_path",check);
            Constant Init(string name) => unit.Variables.Single(v=>v.Name==name).ConstantInitializer!;
            var ordinary=Init("constant_ordinary");var raw=Init("constant_raw");var utf8=Init("constant_utf8");
            check(ordinary.Value==raw.Value&&raw.Value==utf8.Value&&raw.Details.IsRawString==true&&ordinary.Details.LiteralEncodingPrefix!.Name=="ordinary"&&utf8.Details.LiteralEncodingPrefix!.Name=="utf8","String occurrences must preserve distinct raw/prefix facts while sharing identical exact target-byte values.");
            check(((StringValue)ordinary.Value) is {ByteLength:var length,StorageUnitBits:8}&&length==5&&((StringValue)Init("constant_embedded_null").Value).TargetBytes.SequenceEqual(new byte[]{97,0,98,0}),"Target strings must expose exact stored byte length, byte width and embedded NUL bytes without Unicode inference.");
            check(new[]{"constant_utf16","constant_utf32","constant_wide"}.Select(n=>Init(n).Details.LiteralEncodingPrefix!.Name).SequenceEqual(new[]{"utf16","utf32","wide"}),"Wide and Unicode source-prefix discriminants must remain occurrence facts independently from stored byte contents.");
            var aliases=unit.Types.OfType<TypeReference>().Where(t=>t.OriginalTemplateArguments.Count>0).ToArray();
            check(aliases.Length==3&&aliases.All(t=>t.OriginalTemplateArguments.Count==1&&t.Instantiation!.Arguments.Count==1&&t.Instantiation.SelectedPartialArguments.Count==0),"Real alias templates must preserve original and primary arguments without misclassifying originals as selected-partial arguments.");
            check(constants.All(c=>c.Details.IsLiteral is null),"Literal classification must remain unknown when no authoritative EDG producer records the property.");
            check(constants.Any(c=>c.Details.IsGenericInitializer)&&constants.Any(c=>c.Details.ExplicitParenthesesOnAggregate)&&constants.Any(c=>c.Details.ConstantForBaseClassFromConstexprFolding)&&constants.Any(c=>c.Details.IsResultOfConstexprCall)&&constants.Any(c=>c.Details.FoldedStatementExpression)&&constants.Any(c=>c.Details.HasDynamicInitComponent==true),"Actual generic/parenthesized/base-class/constexpr/folded/dynamic-component constant occurrence flags must remain independent retained facts.");
            check(source?constants.Any(c=>c.Value is ConstantValue<DynamicInitializationValueDetails> {Payload.Initialization:not null}):constants.Any(c=>c.Details.VptrHasBeenLowered==true),"Source dynamic aggregate values and lowered inserted-vptr facts must preserve their actual profile-specific semantic payloads.");
            if(source)
            {
                var address=(AddressValue)Init("constant_element_address").Value;
                check(address.BaseEntity is VariableSymbol {Name:"constant_aggregate"}&&address.SubobjectPath!.Steps.Any(p=>p.Field?.Name=="elements")&&address.SubobjectPath.Steps.Any(p=>p.ElementOffset==1),"A genuine address must retain its actual base variable, field path and signed element count without byte-offset reinterpretation.");
                check(((AddressValue)Init("constant_one_past_address").Value).OnePastEnd&&((AddressValue)Init("constant_base_address").Value).SubobjectPath!.Steps.Any(p=>p.BaseSubobject?.BaseType.Name=="ConstantBase"),"Actual one-past and base-subobject address arms must preserve their exact typed paths.");
            }
        }
    }

    private static void SweepFamilies(IEnumerable<SemanticEntity> entities,Action<bool,string> check)
    {
        var details=new List<SemanticDetail>();
        foreach(var entity in entities)
            foreach(var property in entity.GetType().GetProperties(BindingFlags.Instance|BindingFlags.Public)
                .Where(p=>typeof(SemanticDetail).IsAssignableFrom(p.PropertyType)).GroupBy(p=>p.PropertyType).Select(g=>g.First()))
                if(property.GetValue(entity) is SemanticDetail detail)
                {
                    details.Add(detail);
                    foreach(var rows in detail.GetType().GetProperties(BindingFlags.Instance|BindingFlags.Public))
                        if(rows.PropertyType.IsGenericType&&rows.PropertyType.GetGenericArguments().Length==1&&typeof(SemanticDetail).IsAssignableFrom(rows.PropertyType.GetGenericArguments()[0])&&rows.GetValue(detail) is IEnumerable<SemanticDetail> ordered)
                            details.AddRange(ordered);
                }
        foreach(var family in details.GroupBy(d=>d.GetType()))
        {
            var properties=family.Key.GetProperties(BindingFlags.Instance|BindingFlags.Public)
                .Where(p=>new[]{typeof(bool),typeof(long),typeof(BigInteger),typeof(string),typeof(SemanticKind)}.Contains(Nullable.GetUnderlyingType(p.PropertyType)??p.PropertyType)).ToArray();
            if(properties.Length==0)continue;
            foreach(var detail in family)foreach(var property in properties)_=property.GetValue(detail);
            check(true,$"The genuine {family.Key.Name} family scalar projection must navigate all its typed retained fields.");
            Console.WriteLine($"scalar:{family.Key.Name},{family.Count()} records,{string.Join(',',properties.Select(p=>p.Name))}");
        }
    }

}
