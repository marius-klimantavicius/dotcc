namespace RoslynObj;

public sealed partial class RoslynObject
{
    internal static string EntityTable(string kind) => kind switch
    {
        "routine"=>"routine_symbol", "variable"=>"variable_symbol", "field"=>"field_symbol",
        "namespace"=>"namespace_symbol", "label"=>"label_symbol", "enumerator_symbol" or "symbol"=>"symbol",
        "enum" or "alias" or "type_symbol"=>"type_symbol",
        "expression" or "statement"=>"semantic_node", "template"=>"template_definition",
        "cleanup_storage"=>"cleanup_storage_location", "cleanup_condition"=>"cleanup_condition",
        "cleanup_abi"=>"cleanup_abi_detail", "using"=>"using_declaration",
        "generic_constraint_clause"=>"template_generic_constraint_clause",
        "source_mapping"=>"source_mapping_segment",
        "translation_unit" or "constant" or "constant_value" or "handler" or "template_header" or "lambda_definition"
            or "type" or "declaration" or "scope" or "initialization" or "lifetime" or "template_argument"
            or "template_instance" or "source_file" or "source_position" or "source_range" or "source_origin"
            or "macro_definition" or "macro_invocation" or "record_layout" or "cleanup_region" or "cleanup_action"
            or "cleanup_array" or "name" or "temporary" or "base_derivation" or "base_subobject"
            or "template_parameter" or "template_specialization" or "include_occurrence" or "source_attachment"
            or "attribute" or "attribute_group" or "subobject_path" or "name_reference" or "exception_specification"
            or "virtual_table" or "vla_dimension" or "coroutine" or "function_eh" or "static_assertion" or "template_symbol"
            or "name_qualifier" or "orphan_scope" or "token_sequence" or "parameter_type" or "asm_entry" or "module" or "module_import"
            or "line_directive" or "source_entry" or "source_sublist" or "source_end_construct" or "instantiation_directive"
            or "linkage_specification" or "microsoft_if_exists" or "record_declaration" or "pragma"
            or "ms_attribute" or "ms_attribute_argument" or "scoped_expression"=>kind,
        _=>throw new IncompatibleObjectException("A required entity kind is unsupported.")
    };
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
    internal TemplateDescriptor Template(long id) => Cached("template_definition",id,r=>new TemplateDescriptor(this,id,r));
    internal TemplateSymbol TemplateSymbol(long id) => Cached("template_symbol",id,r=>new TemplateSymbol(this,id,r));
    internal TemplateInstantiation Instance(long id) => Cached("template_instance",id,r=>new TemplateInstantiation(this,id,r));
    internal TemplateHeader Header(long id) => Cached("template_header",id,r=>new TemplateHeader(this,id,r));
    internal TemplateParameter TemplateParameter(long id) => Cached("template_parameter",id,r=>new TemplateParameter(this,id,r));
    internal GenericConstraintClause GenericClause(long id) => Cached("template_generic_constraint_clause",id,r=>new GenericConstraintClause(this,id,r));
    internal TemplateArgument Argument(long id) => Cached("template_argument",id,r=>new TemplateArgument(this,id,r));
    internal SourceInfo Source(long id) => Cached("source_origin",id,r=>new SourceInfo(this,id,r));
    internal SourcePosition Position(long id) => Cached("source_position",id,r=>new SourcePosition(this,id,r));
    internal SourceRange Range(long id) => Cached("source_range",id,r=>new SourceRange(this,id,r));
    internal SourceFile File(long id) => Cached("source_file",id,r=>new SourceFile(this,id,r));
    internal IncludeOccurrence Include(long id) => Cached("include_occurrence",id,r=>new IncludeOccurrence(this,id,r));
    internal LineDirective LineDirective(long id) => Cached("line_directive",id,r=>new LineDirective(this,id,r));
    internal SourceMappingSegment Mapping(long id) => Cached("source_mapping_segment",id,r=>new SourceMappingSegment(this,id,r));
    internal SourceEntry SourceEntry(long id) => Cached("source_entry",id,r=>new SourceEntry(this,id,r));
    internal SourceSublist SourceSublist(long id) => Cached("source_sublist",id,r=>new SourceSublist(this,id,r));
    internal RecordDeclaration RecordDeclaration(long id) => Cached("record_declaration",id,r=>new RecordDeclaration(this,id,r));
    internal SourceEndConstruct SourceEnd(long id) => Cached("source_end_construct",id,r=>new SourceEndConstruct(this,id,r));
    internal InstantiationDirective InstantiationDirective(long id) => Cached("instantiation_directive",id,r=>new InstantiationDirective(this,id,r));
    internal LinkageSpecification LinkageSpecification(long id) => Cached("linkage_specification",id,r=>new LinkageSpecification(this,id,r));
    internal MicrosoftIfExists IfExists(long id) => Cached("microsoft_if_exists",id,r=>new MicrosoftIfExists(this,id,r));
    internal PragmaDirective Pragma(long id) => Cached("pragma",id,r=>new PragmaDirective(this,id,r));
    internal MicrosoftAttribute MicrosoftAttribute(long id) => Cached("ms_attribute",id,r=>new MicrosoftAttribute(this,id,r));
    internal MicrosoftAttributeArgument MicrosoftAttributeArgument(long id) => Cached("ms_attribute_argument",id,r=>new MicrosoftAttributeArgument(this,id,r));
    internal ScopedExpression ScopedExpression(long id) => Cached("scoped_expression",id,r=>new ScopedExpression(this,id,r));
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
    internal TypeSymbol TypeSymbol(long id) => Cached<TypeSymbol>("type_symbol",id,r=>Store.Get("enum_symbol",id) is {} enumeration ? new EnumSymbol(this,id,enumeration)
        : Store.Get("alias_symbol",id) is {} alias ? new AliasSymbol(this,id,alias) : new TypeSymbol(this,id,r));
    internal EnumSymbol Enum(long id) => TypeSymbol(id) as EnumSymbol ?? throw new IncompatibleObjectException("The relationship does not identify an enum symbol.");
    internal AliasSymbol Alias(long id) => TypeSymbol(id) as AliasSymbol ?? throw new IncompatibleObjectException("The relationship does not identify an alias symbol.");
    internal ExceptionHandler Handler(long id) => Cached("handler",id,r=>new ExceptionHandler(this,id,r));
    internal ParameterType ParameterType(long id) => Cached("parameter_type",id,r=>new ParameterType(this,id,r));
    internal TokenSequence TokenSequence(long id) => Cached("token_sequence",id,r=>new TokenSequence(this,id,r));
    internal LambdaDefinition LambdaDefinition(long id) => Cached("lambda_definition",id,r=>new LambdaDefinition(this,id,r));
    internal AssemblyDefinition AssemblyDefinition(long id) => Cached("asm_entry",id,r=>new AssemblyDefinition(this,id,r));
    internal CppModule Module(long id) => Cached("module",id,r=>new CppModule(this,id,r));
    internal ModuleImport ModuleImport(long id) => Cached("module_import",id,r=>new ModuleImport(this,id,r));
    internal TemplateSpecialization Specialization(long id) => Cached("template_specialization",id,r=>new TemplateSpecialization(this,id,r));
    internal EnumeratorSymbol Enumerator(long id) => Cached("symbol",id,r=>new EnumeratorSymbol(this,id,r));

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
    internal RecordType RecordType(long id) => Type(id) as RecordType
        ?? throw new IncompatibleObjectException("The relationship does not identify a record type.");

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
        "stmk_range_based_for"=>new RangeForOperation(this,id,row),
        "stmk_for_each"=>new ForEachOperation(this,id,row),
        "stmk_microsoft_try"=>new MicrosoftTryOperation(this,id,row),
        "stmk_label"=>new LabelOperation(this,id,row),
        "stmk_coroutine"=>new CoroutineOperation(this,id,row),
        "stmk_set_vla_size" or "stmk_vla_decl"=>new VlaStatementOperation(this,id,row),
        "stmk_decl"=>new DeclarationOperation(this,id,row),
        "stmk_expr" or "stmk_empty" or "stmk_coroutine_return" or "stmk_switch_case" or "stmk_init" or "stmk_upc_notify" or "stmk_upc_wait" or "stmk_upc_barrier" or "stmk_upc_fence" or "stmk_stmt_expr_result"=>new StatementOperation(this,id,row),
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
        "enk_temp_init"=>new TemporaryOperation(this,id,row),
        "enk_condition"=>new ConditionOperation(this,id,row),
        "enk_object_lifetime"=>new LifetimeOperation(this,id,row),
        "enk_type_operand"=>new TypeOperandOperation(this,id,row),
        "enk_typeid" or "enk_sizeof" or "enk_runtime_sizeof" or "enk_alignof" or "enk_datasizeof"=>new TypeQueryOperation(this,id,row),
        "enk_statement"=>new StatementExpressionOperation(this,id,row),
        "enk_reuse_value"=>new ReuseOperation(this,id,row),
        "enk_vla_dealloc"=>new VlaExpressionOperation(this,id,row),
        "enk_builtin_operation" or "enk_c11_generic" or "enk_builtin_choose_expr"=>new BuiltinOperation(this,id,row),
        "enk_param_ref"=>new ParameterReferenceOperation(this,id,row),
        "enk_sizeof_pack" or "enk_braced_init_list" or "enk_fold" or "enk_initializer" or "enk_concept_id" or "enk_requires" or "enk_compound_req" or "enk_nested_req" or "enk_const_eval_deferred" or "enk_template_name" or "enk_token_sequence" or "enk_pack_index"=>new DependentOperation(this,id,row),
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
        "ck_label_difference"=>new LabelDifferenceValue(this,id,r),
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
            "handler"=>Handler(id),
            "generic_constraint_clause"=>GenericClause(id),
            "template_header"=>Header(id),
            "lambda_definition"=>LambdaDefinition(id),
            "asm_entry"=>AssemblyDefinition(id),
            "module"=>Module(id),
            "module_import"=>ModuleImport(id),
            "translation_unit"=>Unit(id),
            "namespace"=>Namespace(id),
            "routine"=>Function(id),
            "variable"=>Variable(id),
            "field"=>Field(id),
            "label"=>Label(id),
            "type"=>Type(id),
            "type_symbol"=>TypeSymbol(id),
            "declaration"=>Declaration(id),
            "scope"=>Scope(id),
            "expression"=>Expression(id),
            "statement"=>Statement(id),
            "initialization"=>Initialization(id),
            "lifetime"=>Lifetime(id),
            "template"=>Template(id),
            "template_symbol"=>TemplateSymbol(id),
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
            "enum"=>Enum(id),
            "enumerator_symbol"=>Enumerator(id),
            "alias"=>Alias(id),
            "using"=>new SemanticRecord<UsingDeclarationDetails>(this,id,Detail("using_declaration",id,r=>new UsingDeclarationDetails(this,r))),
            "temporary"=>new SemanticRecord<TemporaryDetails>(this,id,Detail("temporary",id,r=>new TemporaryDetails(this,r))),
            "base_derivation"=>Derivation(id),
            "base_subobject"=>Subobject(id),
            "template_parameter"=>TemplateParameter(id),
            "template_specialization"=>Specialization(id),
            "include_occurrence"=>Include(id),
            "line_directive"=>LineDirective(id),
            "source_mapping"=>Mapping(id),
            "source_entry"=>SourceEntry(id),
            "source_sublist"=>SourceSublist(id),
            "record_declaration"=>RecordDeclaration(id),
            "source_end_construct"=>SourceEnd(id),
            "instantiation_directive"=>InstantiationDirective(id),
            "linkage_specification"=>LinkageSpecification(id),
            "microsoft_if_exists"=>IfExists(id),
            "pragma"=>Pragma(id),
            "ms_attribute"=>MicrosoftAttribute(id),
            "ms_attribute_argument"=>MicrosoftAttributeArgument(id),
            "scoped_expression"=>ScopedExpression(id),
            "source_attachment"=>new SemanticRecord<SourceAttachmentDetails>(this,id,Detail("source_attachment",id,r=>new SourceAttachmentDetails(this,r))),
            "attribute"=>new SemanticRecord<AttributeDetails>(this,id,Detail("attribute",id,r=>new AttributeDetails(this,r))),
            "attribute_group"=>new SemanticRecord<AttributeGroupDetails>(this,id,Detail("attribute_group",id,r=>new AttributeGroupDetails(this,r))),
            "subobject_path"=>new SemanticRecord<SubobjectPathDetails>(this,id,Detail("subobject_path",id,r=>new SubobjectPathDetails(this,r))),
            "name_reference"=>new SemanticRecord<NameReferenceDetails>(this,id,Detail("name_reference",id,r=>new NameReferenceDetails(this,r))),
            "exception_specification"=>new SemanticRecord<ExceptionSpecificationDetails>(this,id,Detail("exception_specification",id,r=>new ExceptionSpecificationDetails(this,r))),
            "virtual_table"=>new SemanticRecord<VirtualTableDetails>(this,id,Detail("virtual_table",id,r=>new VirtualTableDetails(this,r))),
            "vla_dimension"=>new SemanticRecord<VlaDimensionDetails>(this,id,Detail("vla_dimension",id,r=>new VlaDimensionDetails(this,r))),
            "coroutine"=>new SemanticRecord<CoroutineDetails>(this,id,Detail("coroutine",id,r=>new CoroutineDetails(this,r))),
            "function_eh"=>new SemanticRecord<FunctionEhDetails>(this,id,Detail("function_eh",id,r=>new FunctionEhDetails(this,r))),
            "static_assertion"=>new SemanticRecord<StaticAssertionDetails>(this,id,Detail("static_assertion",id,r=>new StaticAssertionDetails(this,r))),
            "name_qualifier"=>new SemanticRecord<NameQualifierDetails>(this,id,Detail("name_qualifier",id,r=>new NameQualifierDetails(this,r))),
            "orphan_scope"=>new SemanticRecord<OrphanScopeDetails>(this,id,Detail("orphan_scope",id,r=>new OrphanScopeDetails(this,r))),
            "token_sequence"=>TokenSequence(id),
            "parameter_type"=>ParameterType(id),
            _=>throw new IncompatibleObjectException("A required entity kind is unsupported.")
        };
    }
}
