namespace RoslynObj;

public sealed partial class RoslynObject
{
    internal TranslationUnit Unit(long id) => Cached("translation_unit",id,r=>new TranslationUnit(this,id,r));
    internal FunctionSymbol Function(long id) => Cached("routine_symbol",id,r=>new FunctionSymbol(this,id,r));
    internal VariableSymbol Variable(long id) => Cached("variable_symbol",id,r=>new VariableSymbol(this,id,r));
    internal FieldSymbol Field(long id) => Cached("field_symbol",id,r=>new FieldSymbol(this,id,r));
    internal NamespaceSymbol Namespace(long id) => Cached("namespace_symbol",id,r=>new NamespaceSymbol(this,id,r));
    internal LabelSymbol Label(long id) => Cached("label_symbol",id,r=>new LabelSymbol(this,id,r));
    internal Declaration Declaration(long id) => Cached("declaration",id,r=>new Declaration(this,id,r));
    internal Scope Scope(long id) => Cached("scope",id,r=>new Scope(this,id,r));
    internal Constant Constant(long id) => Cached("constant",id,r=>new Constant(this,id,r));
    internal Initialization Initialization(long id) => Cached("initialization",id,r=>new Initialization(this,id,r));
    internal Lifetime Lifetime(long id) => Cached("lifetime",id,r=>new Lifetime(this,id,r));
    internal TemplateSymbol Template(long id) => Cached("template_definition",id,r=>new TemplateSymbol(this,id,r));
    internal TemplateInstantiation Instance(long id) => Cached("template_instance",id,r=>new TemplateInstantiation(this,id,r));
    internal TemplateArgument Argument(long id) => Cached("template_argument",id,r=>new TemplateArgument(this,id,r));
    internal SourceInfo Source(long id) => Cached("source_origin",id,r=>new SourceInfo(this,id,r));
    internal SourcePosition Position(long id) => Cached("source_position",id,r=>new SourcePosition(this,id,r));
    internal SourceRange Range(long id) => Cached("source_range",id,r=>new SourceRange(this,id,r));
    internal SourceFile File(long id) => Cached("source_file",id,r=>new SourceFile(this,id,r));
    internal MacroDefinition MacroDefinition(long id) => Cached("macro_definition",id,r=>new MacroDefinition(this,id,r));
    internal MacroInvocation MacroInvocation(long id) => Cached("macro_invocation",id,r=>new MacroInvocation(this,id,r));
    internal CleanupRegion Region(long id) => Cached("cleanup_region",id,r=>new CleanupRegion(this,id,r));
    internal CleanupAction Action(long id) => Cached("cleanup_action",id,r=>new CleanupAction(this,id,r));
    internal CleanupStorageLocation Storage(long id) => Cached("cleanup_storage_location",id,r=>new CleanupStorageLocation(this,id,r));
    internal CleanupArray Array(long id) => Cached("cleanup_array",id,r=>new CleanupArray(this,id,r));
    internal CleanupCondition Condition(long id) => Cached("cleanup_condition",id,r=>new CleanupCondition(this,id,r));
    internal CleanupAbiDetail AbiDetail(long id) => Cached("cleanup_abi_detail",id,r=>new CleanupAbiDetail(this,id,r));
    internal BaseSubobject Subobject(long id) => Cached("base_subobject",id,r=>new BaseSubobject(this,id,r));
    internal BaseDerivation Derivation(long id) => Cached("base_derivation",id,r=>new BaseDerivation(this,id,r));
    internal RecordLayout Layout(long id) => Cached("record_layout",id,r=>new RecordLayout(this,id,r));

    internal Symbol Symbol(long id) => Entity(id) as Symbol
        ?? throw new IncompatibleObjectException("A symbol relationship has an incompatible entity kind.");

    internal CppType Type(long id) => Cached<CppType>("type",id,r=>Kind(r.Long("kind_id")).Name switch
    {
        "tk_integer" or "tk_enum" or "tk_float" or "tk_fixed_point" or "tk_imaginary" or "tk_complex" or "tk_mfp8" or "tk_float8e4m3" or "tk_float8e5m2" => new ArithmeticType(this,id,r),
        "tk_pointer"=>new PointerType(this,id,r), "tk_array"=>new ArrayType(this,id,r),
        "tk_routine"=>new RoutineType(this,id,r),
        "tk_class" or "tk_struct" or "tk_union"=>Store.Get("record_type",id)?.OptionalLong("instance_id") is {} instance
            && Store.Required("template_instance",instance).OptionalLong("template_id") is not null
                ? new ConstructedType(this,id,r) : new RecordType(this,id,r),
        "tk_typeref"=>Store.Get("qualified_type",id) is not null ? new QualifiedType(this,id,r) : new TypeReference(this,id,r),
        "tk_ptr_to_member"=>new MemberPointerType(this,id,r),
        "tk_vector" or "tk_scalable_vector" or "tk_scalable_vector_count" or "tk_riscv_vector"=>new VectorType(this,id,r),
        "tk_template_param"=>new DependentType(this,id,r),
        "tk_void" or "tk_nullptr" or "tk_reflection"=>new CppType(this,id,r),
        _=>throw new IncompatibleObjectException("The required type kind is unsupported.")
    });

    internal SemanticNode Node(long id) => Cached<SemanticNode>("semantic_node",id,r=>Kind(r.Long("kind_id")).Family switch
    {
        "statement"=>CreateStatement(id,r), "expression"=>CreateExpression(id,r),
        "dynamic_init"=>new InitializationOperation(this,id,r),
        _=>throw new IncompatibleObjectException("The required operation kind is unsupported.")
    });
    internal ExpressionOperation Expression(long id) => Node(id) as ExpressionOperation
        ?? throw new IncompatibleObjectException("The relationship does not identify an expression.");
    internal StatementOperation Statement(long id) => Node(id) as StatementOperation
        ?? throw new IncompatibleObjectException("The relationship does not identify a statement.");

    private StatementOperation CreateStatement(long id,Row row) => Kind(row.Long("kind_id")).Name switch
    {
        "stmk_block"=>new BlockOperation(this,id,row),
        "stmk_if" or "stmk_constexpr_if" or "stmk_if_consteval" or "stmk_if_not_consteval"=>new ConditionalOperation(this,id,row),
        "stmk_while" or "stmk_end_test_while" or "stmk_for" or "stmk_upc_forall"=>new LoopOperation(this,id,row),
        "stmk_goto" or "stmk_return" or "stmk_assigned_goto"=>new JumpOperation(this,id,row),
        "stmk_switch"=>new SwitchOperation(this,id,row), "stmk_try_block"=>new TryOperation(this,id,row),
        "stmk_asm" or "stmk_asm_func_body"=>new AssemblyOperation(this,id,row),
        "stmk_range_based_for"=>new StatementOperation<RangeForDetails>(this,id,row,"range_for",r=>new(this,r)),
        "stmk_for_each"=>new StatementOperation<ForEachStatementDetails>(this,id,row,"for_each_statement",r=>new(this,r)),
        "stmk_microsoft_try"=>new StatementOperation<MicrosoftTryStatementDetails>(this,id,row,"microsoft_try_statement",r=>new(this,r)),
        "stmk_label"=>new StatementOperation<LabelStatementDetails>(this,id,row,"label_statement",r=>new(this,r)),
        "stmk_coroutine"=>new StatementOperation<CoroutineDetails>(this,id,row,"coroutine",r=>new(this,r)),
        "stmk_set_vla_size" or "stmk_vla_decl"=>new StatementOperation<VlaOperationDetails>(this,id,row,"vla_operation",r=>new(this,r)),
        "stmk_expr" or "stmk_empty" or "stmk_coroutine_return" or "stmk_switch_case" or "stmk_init" or "stmk_decl" or "stmk_upc_notify" or "stmk_upc_wait" or "stmk_upc_barrier" or "stmk_upc_fence" or "stmk_stmt_expr_result"=>new StatementOperation(this,id,row),
        _=>throw new IncompatibleObjectException("The required statement kind is unsupported.")
    };

    private ExpressionOperation CreateExpression(long id,Row row) => Kind(row.Long("kind_id")).Name switch
    {
        "enk_operation"=>Store.Get("call_expression",id) is not null ? new CallOperation(this,id,row)
            : Store.Get("conversion_expression",id) is not null ? new ConversionOperation(this,id,row) : new OperatorOperation(this,id,row),
        "enk_constant"=>new ConstantOperation(this,id,row),
        "enk_variable" or "enk_routine" or "enk_field"=>new ReferenceOperation(this,id,row),
        "enk_throw"=>new ThrowOperation(this,id,row), "enk_await" or "enk_yield"=>new AwaitOperation(this,id,row),
        "enk_new_delete" or "enk_gcnew"=>new AllocationOperation(this,id,row), "enk_lambda"=>new LambdaOperation(this,id,row),
        "enk_lowered_eh_construct"=>Store.Get("cleanup_state_operation",id) is not null ? new CleanupStateOperation(this,id,row) : new LoweredEhOperation(this,id,row),
        "enk_temp_init"=>new ExpressionOperation<TemporaryExpressionDetails>(this,id,row,"temporary_expression",r=>new(this,r)),
        "enk_condition"=>new ExpressionOperation<ConditionExpressionDetails>(this,id,row,"condition_expression",r=>new(this,r)),
        "enk_object_lifetime"=>new ExpressionOperation<LifetimeExpressionDetails>(this,id,row,"lifetime_expression",r=>new(this,r)),
        "enk_typeid" or "enk_sizeof" or "enk_runtime_sizeof" or "enk_alignof" or "enk_datasizeof" or "enk_type_operand"=>new ExpressionOperation<TypeQueryExpressionDetails>(this,id,row,"type_query_expression",r=>new(this,r)),
        "enk_statement"=>new ExpressionOperation<StatementExpressionDetails>(this,id,row,"statement_expression",r=>new(this,r)),
        "enk_reuse_value"=>new ExpressionOperation<ReuseExpressionDetails>(this,id,row,"reuse_expression",r=>new(this,r)),
        "enk_vla_dealloc"=>new ExpressionOperation<VlaOperationDetails>(this,id,row,"vla_operation",r=>new(this,r)),
        "enk_builtin_operation" or "enk_c11_generic" or "enk_builtin_choose_expr"=>new ExpressionOperation<BuiltinExpressionDetails>(this,id,row,"builtin_expression",r=>new(this,r)),
        "enk_param_ref"=>new ExpressionOperation<ParameterReferenceExpressionDetails>(this,id,row,"parameter_reference_expression",r=>new(this,r)),
        "enk_sizeof_pack" or "enk_braced_init_list" or "enk_fold" or "enk_initializer" or "enk_concept_id" or "enk_requires" or "enk_compound_req" or "enk_nested_req" or "enk_const_eval_deferred" or "enk_template_name" or "enk_token_sequence" or "enk_pack_index"=>new ExpressionOperation<DependentExpressionDetails>(this,id,row,"dependent_expression",r=>new(this,r)),
        "enk_address_of_ellipsis" or "enk_result_of_overriding_function"=>new ExpressionOperation(this,id,row),
        _=>throw new IncompatibleObjectException("The required expression kind is unsupported.")
    };

    internal ConstantValue Value(long id) => Cached<ConstantValue>("constant_value",id,r=>Kind(r.Long("kind_id")).Name switch
    {
        "ck_integer"=>new IntegerValue(this,id,r), "ck_float" or "ck_imaginary"=>new FloatingValue(this,id,r),
        "ck_string"=>new StringValue(this,id,r), "ck_aggregate"=>new AggregateValue(this,id,r),
        "ck_fixed_point"=>new ConstantValue<FixedPointValueDetails>(this,id,r,"fixed_point_value",v=>new(this,v)),
        "ck_complex"=>new ConstantValue<ComplexValueDetails>(this,id,r,"complex_value",v=>new(this,v)),
        "ck_address"=>new ConstantValue<AddressValueDetails>(this,id,r,"address_value",v=>new(this,v)),
        "ck_stack_offset"=>new ConstantValue<StackOffsetValueDetails>(this,id,r,"stack_offset_value",v=>new(this,v)),
        "ck_ptr_to_member"=>new ConstantValue<MemberPointerValueDetails>(this,id,r,"member_pointer_value",v=>new(this,v)),
        "ck_label_difference"=>new ConstantValue<LabelDifferenceValueDetails>(this,id,r,"label_difference_value",v=>new(this,v)),
        "ck_init_repeat"=>new ConstantValue<RepeatValueDetails>(this,id,r,"repeat_value",v=>new(this,v)),
        "ck_designator"=>new ConstantValue<DesignatorValueDetails>(this,id,r,"designator_value",v=>new(this,v)),
        "ck_dynamic_init"=>new ConstantValue<DynamicInitializationValueDetails>(this,id,r,"dynamic_initialization_value",v=>new(this,v)),
        "ck_template_param"=>new ConstantValue<DependentValueDetails>(this,id,r,"dependent_value",v=>new(this,v)),
        "ck_reflection"=>new ConstantValue<ReflectionValueDetails>(this,id,r,"reflection_value",v=>new(this,v)),
        "ck_void" or "ck_upc_threads" or "ck_upc_mythread"=>new ConstantValue(this,id,r),
        _=>throw new IncompatibleObjectException("The required constant kind is unsupported.")
    });

    internal SemanticEntity Entity(long id)
    {
        var kind=Kind(Store.Required("entity",id).Long("kind_id")).Name;
        return kind switch
        {
            "constant"=>Constant(id),
            "constant_value"=>Value(id),
            "handler"=>new SemanticRecord<HandlerDetails>(this,id,Detail("handler",id,r=>new HandlerDetails(this,r))),
            "template_header"=>new SemanticRecord<TemplateHeaderDetails>(this,id,Detail("template_header",id,r=>new TemplateHeaderDetails(this,r))),
            "lambda_definition"=>new SemanticRecord<LambdaDefinitionDetails>(this,id,Detail("lambda_definition",id,r=>new LambdaDefinitionDetails(this,r))),
            "translation_unit"=>Unit(id),
            "namespace"=>Namespace(id),
            "routine"=>Function(id),
            "variable"=>Variable(id),
            "field"=>Field(id),
            "label"=>Label(id),
            "type"=>Type(id),
            "declaration"=>Declaration(id),
            "scope"=>Scope(id),
            "expression"=>Expression(id),
            "statement"=>Statement(id),
            "initialization"=>Initialization(id),
            "lifetime"=>Lifetime(id),
            "template"=>Template(id),
            "template_argument"=>Argument(id),
            "template_instance"=>Instance(id),
            "source_file"=>File(id),
            "source_position"=>Position(id),
            "source_range"=>Range(id),
            "source_origin"=>Source(id),
            "macro_definition"=>MacroDefinition(id),
            "macro_invocation"=>MacroInvocation(id),
            "record_layout"=>Layout(id),
            "cleanup_region"=>Region(id),
            "cleanup_action"=>Action(id),
            "cleanup_storage"=>Storage(id),
            "cleanup_array"=>Array(id),
            "cleanup_condition"=>Condition(id),
            "cleanup_abi"=>AbiDetail(id),
            "name"=>new SemanticRecord<NameDetails>(this,id,Detail("name",id,r=>new NameDetails(this,r))),
            "symbol"=>new Symbol(this,id),
            "enum"=>Cached("enum_symbol",id,r=>new EnumSymbol(this,id,r)),
            "alias"=>Cached("alias_symbol",id,r=>new AliasSymbol(this,id,r)),
            "using"=>new SemanticRecord<UsingDeclarationDetails>(this,id,Detail("using_declaration",id,r=>new UsingDeclarationDetails(this,r))),
            "temporary"=>new SemanticRecord<TemporaryDetails>(this,id,Detail("temporary",id,r=>new TemporaryDetails(this,r))),
            "base_derivation"=>Derivation(id),
            "base_subobject"=>Subobject(id),
            "template_parameter"=>new SemanticRecord<TemplateParameterDetails>(this,id,Detail("template_parameter",id,r=>new TemplateParameterDetails(this,r))),
            "template_specialization"=>new SemanticRecord<TemplateSpecializationDetails>(this,id,Detail("template_specialization",id,r=>new TemplateSpecializationDetails(this,r))),
            "include_occurrence"=>new SemanticRecord<IncludeOccurrenceDetails>(this,id,Detail("include_occurrence",id,r=>new IncludeOccurrenceDetails(this,r))),
            "source_mapping"=>new SemanticRecord<SourceMappingSegmentDetails>(this,id,Detail("source_mapping_segment",id,r=>new SourceMappingSegmentDetails(this,r))),
            "source_attachment"=>new SemanticRecord<SourceAttachmentDetails>(this,id,Detail("source_attachment",id,r=>new SourceAttachmentDetails(this,r))),
            "attribute"=>new SemanticRecord<AttributeDetails>(this,id,Detail("attribute",id,r=>new AttributeDetails(this,r))),
            "subobject_path"=>new SemanticRecord<SubobjectPathDetails>(this,id,Detail("subobject_path",id,r=>new SubobjectPathDetails(this,r))),
            "name_reference"=>new SemanticRecord<NameReferenceDetails>(this,id,Detail("name_reference",id,r=>new NameReferenceDetails(this,r))),
            "exception_specification"=>new SemanticRecord<ExceptionSpecificationDetails>(this,id,Detail("exception_specification",id,r=>new ExceptionSpecificationDetails(this,r))),
            "virtual_table"=>new SemanticRecord<VirtualTableDetails>(this,id,Detail("virtual_table",id,r=>new VirtualTableDetails(this,r))),
            "vla_dimension"=>new SemanticRecord<VlaDimensionDetails>(this,id,Detail("vla_dimension",id,r=>new VlaDimensionDetails(this,r))),
            "coroutine"=>new SemanticRecord<CoroutineDetails>(this,id,Detail("coroutine",id,r=>new CoroutineDetails(this,r))),
            "function_eh"=>new SemanticRecord<FunctionEhDetails>(this,id,Detail("function_eh",id,r=>new FunctionEhDetails(this,r))),
            _=>throw new IncompatibleObjectException("A required entity kind is unsupported.")
        };
    }
}
