// Generated from schema/roslynobj.sql by managed/tools/generate_bindings.py. Do not edit.
#nullable enable
using System.Numerics;
namespace RoslynObj;

public sealed class KindDispositionDetails : SemanticDetail
{
    internal KindDispositionDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public SemanticKind Kind => Object.Kind(Data.Long("kind_id"));
    public string State => Data.OptionalText("state")!;
    public SemanticKind? CanonicalKind => Object.Link(Data.OptionalLong("canonical_kind_id"), value => Object.Kind(value));
}

public sealed class EntityDetails : SemanticDetail
{
    internal EntityDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public SemanticKind Kind => Object.Kind(Data.Long("kind_id"));
    public IReadOnlyList<SemanticReferenceDetails> SemanticReferenceRows => Object.List("SELECT * FROM semantic_reference WHERE owner_id=$owner ORDER BY role_id,ordinal", Data.Long("id"), r => new SemanticReferenceDetails(Object, r));
    public IReadOnlyList<OwnerAttributeDetails> OwnerAttributeRows => Object.List("SELECT * FROM owner_attribute WHERE owner_id=$owner ORDER BY ordinal", Data.Long("id"), r => new OwnerAttributeDetails(Object, r));
    public IReadOnlyList<SourceDescriptorChildDetails> SourceDescriptorChildRows => Object.List("SELECT * FROM source_descriptor_child WHERE owner_id=$owner ORDER BY ordinal", Data.Long("id"), r => new SourceDescriptorChildDetails(Object, r));
    public IReadOnlyList<DeclarationElementPositionDetails> DeclarationElementPositionRows => Object.List("SELECT * FROM declaration_element_position WHERE owner_id=$owner ORDER BY ordinal", Data.Long("id"), r => new DeclarationElementPositionDetails(Object, r));
    public IReadOnlyList<OwnerMsAttributeDetails> OwnerMsAttributeRows => Object.List("SELECT * FROM owner_ms_attribute WHERE owner_id=$owner ORDER BY ordinal", Data.Long("id"), r => new OwnerMsAttributeDetails(Object, r));
}

public sealed class ObjectHeaderDetails : SemanticDetail
{
    internal ObjectHeaderDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public long FormatMajor => Data.Long("format_major");
    public long FormatMinor => Data.Long("format_minor");
    public string ArtifactId => Data.OptionalText("artifact_id")!;
    public string Producer => Object.Text(Data.OptionalLong("producer_id"))!;
    public string Revision => Object.Text(Data.OptionalLong("revision_id"))!;
    public string EdgVersion => Object.Text(Data.OptionalLong("edg_version_id"))!;
    public SemanticKind CaptureProfile => Object.Kind(Data.Long("capture_profile_id"));
    public TargetProfileDetails TargetProfile => Object.Link(Data.OptionalLong("target_profile_id"), value => Object.Detail("target_profile", value, r => new TargetProfileDetails(Object, r)))!;
    public SourceFile? MainSourceFile => Object.Link(Data.OptionalLong("main_source_file_id"), value => Object.File(value));
    public TranslationUnit TranslationUnit => Object.Link(Data.OptionalLong("translation_unit_id"), value => Object.Unit(value))!;
    public bool Complete => Data.Bool("complete");
}

public sealed class TargetProfileDetails : SemanticDetail
{
    internal TargetProfileDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public SemanticKind Abi => Object.Kind(Data.Long("abi_id"));
    public long AddressableUnitBits => Data.Long("addressable_unit_bits");
    public long LittleEndian => Data.Long("little_endian");
    public long PointerBits => Data.Long("pointer_bits");
    public BigInteger PointerAlignment => RequiredNumber(Data.OptionalText("pointer_alignment"));
}

public sealed class TranslationUnitDetails : SemanticDetail
{
    internal TranslationUnitDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public Scope GlobalScope => Object.Link(Data.OptionalLong("global_scope_id"), value => Object.Scope(value))!;
    public SourceFile? PrimaryInput => Object.Link(Data.OptionalLong("primary_input_id"), value => Object.File(value));
    public string LanguageMode => Object.Text(Data.OptionalLong("language_mode_id"))!;
    public FunctionSymbol? EntryPoint => Object.Link(Data.OptionalLong("entry_point_id"), value => Object.Function(value));
    public IReadOnlyList<TranslationUnitRootDetails> TranslationUnitRootRows => Object.List("SELECT * FROM translation_unit_root WHERE owner_id=$owner ORDER BY ordinal", Data.Long("id"), r => new TranslationUnitRootDetails(Object, r));
    public IReadOnlyList<FileScopeDynamicInitRoutineDetails> FileScopeDynamicInitRoutineRows => Object.List("SELECT * FROM file_scope_dynamic_init_routine WHERE owner_id=$owner ORDER BY ordinal", Data.Long("id"), r => new FileScopeDynamicInitRoutineDetails(Object, r));
    public IReadOnlyList<TranslationUnitModuleImportDetails> TranslationUnitModuleImportRows => Object.List("SELECT * FROM translation_unit_module_import WHERE owner_id=$owner ORDER BY ordinal", Data.Long("id"), r => new TranslationUnitModuleImportDetails(Object, r));
}

public sealed class TranslationUnitRootDetails : SemanticDetail
{
    internal TranslationUnitRootDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public TranslationUnit Owner => Object.Link(Data.OptionalLong("owner_id"), value => Object.Unit(value))!;
    public SemanticKind Kind => Object.Kind(Data.Long("kind_id"));
    public long Ordinal => Data.Long("ordinal");
    public SemanticEntity Entity => Object.Link(Data.OptionalLong("entity_id"), value => Object.Entity(value))!;
}

public sealed class FileScopeDynamicInitRoutineDetails : SemanticDetail
{
    internal FileScopeDynamicInitRoutineDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public TranslationUnit Owner => Object.Link(Data.OptionalLong("owner_id"), value => Object.Unit(value))!;
    public long Ordinal => Data.Long("ordinal");
    public FunctionSymbol Routine => Object.Link(Data.OptionalLong("routine_id"), value => Object.Function(value))!;
    public bool IsThreadLocal => Data.Bool("is_thread_local");
}

public sealed class OrphanScopeDetails : SemanticDetail
{
    internal OrphanScopeDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public TranslationUnit TranslationUnit => Object.Link(Data.OptionalLong("translation_unit_id"), value => Object.Unit(value))!;
    public FunctionSymbol? Routine => Object.Link(Data.OptionalLong("routine_id"), value => Object.Function(value));
    public BigInteger ScopeNumber => RequiredNumber(Data.OptionalText("scope_number"));
    public IReadOnlyList<OrphanScopeMemberDetails> OrphanScopeMemberRows => Object.List("SELECT * FROM orphan_scope_member WHERE owner_id=$owner ORDER BY ordinal", Data.Long("id"), r => new OrphanScopeMemberDetails(Object, r));
}

public sealed class OrphanScopeMemberDetails : SemanticDetail
{
    internal OrphanScopeMemberDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public OrphanScopeDetails Owner => Object.Link(Data.OptionalLong("owner_id"), value => Object.Detail("orphan_scope", value, r => new OrphanScopeDetails(Object, r)))!;
    public SemanticKind Kind => Object.Kind(Data.Long("kind_id"));
    public long Ordinal => Data.Long("ordinal");
    public SemanticEntity Entity => Object.Link(Data.OptionalLong("entity_id"), value => Object.Entity(value))!;
}

public sealed class BuildCapabilityDetails : SemanticDetail
{
    internal BuildCapabilityDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public string Name => Object.Text(Data.OptionalLong("name_id"))!;
    public SemanticKind State => Object.Kind(Data.Long("state_id"));
    public string? Value => Object.Text(Data.OptionalLong("value_id"));
    public bool Required => Data.Bool("required");
    public bool? Enabled => Data.OptionalBool("enabled");
}

public sealed class InvocationArgumentDetails : SemanticDetail
{
    internal InvocationArgumentDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public long Ordinal => Data.Long("ordinal");
    public string Text => Object.Text(Data.OptionalLong("text_id"))!;
}

public sealed class FactAvailabilityDetails : SemanticDetail
{
    internal FactAvailabilityDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public SemanticEntity Owner => Object.Link(Data.OptionalLong("owner_id"), value => Object.Entity(value))!;
    public SemanticKind Fact => Object.Kind(Data.Long("fact_id"));
    public SemanticKind State => Object.Kind(Data.Long("state_id"));
    public string? Reason => Object.Text(Data.OptionalLong("reason_id"));
}

public sealed class ScopeDetails : SemanticDetail
{
    internal ScopeDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public SemanticKind Kind => Object.Kind(Data.Long("kind_id"));
    public Scope? Parent => Object.Link(Data.OptionalLong("parent_id"), value => Object.Scope(value));
    public SemanticEntity? AssociatedEntity => Object.Link(Data.OptionalLong("associated_entity_id"), value => Object.Entity(value));
    public Lifetime? Lifetime => Object.Link(Data.OptionalLong("lifetime_id"), value => Object.Lifetime(value));
    public Lifetime? FunctionStaticLifetime => Object.Link(Data.OptionalLong("function_static_lifetime_id"), value => Object.Lifetime(value));
    public bool IsConstexprRoutine => Data.Bool("is_constexpr_routine");
    public bool IsStmtExprBlock => Data.Bool("is_stmt_expr_block");
    public bool IsPlaceholderScope => Data.Bool("is_placeholder_scope");
    public VariableSymbol? ThisParameter => Object.Link(Data.OptionalLong("this_parameter_id"), value => Object.Variable(value));
    public VariableSymbol? NrvoVariable => Object.Link(Data.OptionalLong("nrvo_variable_id"), value => Object.Variable(value));
    public IReadOnlyList<ScopeMemberDetails> ScopeMemberRows => Object.List("SELECT * FROM scope_member WHERE owner_id=$owner ORDER BY ordinal", Data.Long("id"), r => new ScopeMemberDetails(Object, r));
    public IReadOnlyList<ScopeInitializationDetails> ScopeInitializationRows => Object.List("SELECT * FROM scope_initialization WHERE owner_id=$owner ORDER BY ordinal", Data.Long("id"), r => new ScopeInitializationDetails(Object, r));
    public IReadOnlyList<ScopeSourceEntryDetails> ScopeSourceEntryRows => Object.List("SELECT * FROM scope_source_entry WHERE owner_id=$owner ORDER BY ordinal", Data.Long("id"), r => new ScopeSourceEntryDetails(Object, r));
    public IReadOnlyList<ScopeSourceSublistDetails> ScopeSourceSublistRows => Object.List("SELECT * FROM scope_source_sublist WHERE owner_id=$owner ORDER BY ordinal", Data.Long("id"), r => new ScopeSourceSublistDetails(Object, r));
    public IReadOnlyList<ScopePragmaDetails> ScopePragmaRows => Object.List("SELECT * FROM scope_pragma WHERE owner_id=$owner ORDER BY ordinal", Data.Long("id"), r => new ScopePragmaDetails(Object, r));
    public IReadOnlyList<ScopeIfExistsDetails> ScopeIfExistsRows => Object.List("SELECT * FROM scope_if_exists WHERE owner_id=$owner ORDER BY ordinal", Data.Long("id"), r => new ScopeIfExistsDetails(Object, r));
    public IReadOnlyList<LocalExpressionReferenceDetails> LocalExpressionReferenceRows => Object.List("SELECT * FROM local_expression_reference WHERE owner_id=$owner ORDER BY ordinal", Data.Long("id"), r => new LocalExpressionReferenceDetails(Object, r));
    public IReadOnlyList<ScopeLocalStaticInitializationDetails> ScopeLocalStaticInitializationRows => Object.List("SELECT * FROM scope_local_static_initialization WHERE owner_id=$owner ORDER BY ordinal", Data.Long("id"), r => new ScopeLocalStaticInitializationDetails(Object, r));
    public IReadOnlyList<ScopeConstructorInitializerDetails> ScopeConstructorInitializerRows => Object.List("SELECT * FROM scope_constructor_initializer WHERE owner_id=$owner ORDER BY ordinal", Data.Long("id"), r => new ScopeConstructorInitializerDetails(Object, r));
    public IReadOnlyList<ScopeChildDetails> ScopeChildRows => Object.List("SELECT * FROM scope_child WHERE owner_id=$owner ORDER BY ordinal", Data.Long("id"), r => new ScopeChildDetails(Object, r));
}

public sealed class ScopeMemberDetails : SemanticDetail
{
    internal ScopeMemberDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public Scope Owner => Object.Link(Data.OptionalLong("owner_id"), value => Object.Scope(value))!;
    public long Ordinal => Data.Long("ordinal");
    public SemanticEntity Member => Object.Link(Data.OptionalLong("member_id"), value => Object.Entity(value))!;
}

public sealed class ScopeInitializationDetails : SemanticDetail
{
    internal ScopeInitializationDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public Scope Owner => Object.Link(Data.OptionalLong("owner_id"), value => Object.Scope(value))!;
    public long Ordinal => Data.Long("ordinal");
    public Initialization Initialization => Object.Link(Data.OptionalLong("initialization_id"), value => Object.Initialization(value))!;
}

public sealed class SymbolDetails : SemanticDetail
{
    internal SymbolDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public SemanticKind Kind => Object.Kind(Data.Long("kind_id"));
    public CppType? Type => Object.Link(Data.OptionalLong("type_id"), value => Object.Type(value));
    public SemanticKind? Storage => Object.Link(Data.OptionalLong("storage_id"), value => Object.Kind(value));
    public bool CompilerGenerated => Data.Bool("compiler_generated");
}

public sealed class TypeSymbolDetails : SemanticDetail
{
    internal TypeSymbolDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public CppType Type => Object.Link(Data.OptionalLong("type_id"), value => Object.Type(value))!;
}

public sealed class DeclarationDetails : SemanticDetail
{
    internal DeclarationDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public SemanticKind Kind => Object.Kind(Data.Long("kind_id"));
    public SemanticEntity Symbol => Object.Link(Data.OptionalLong("symbol_id"), value => Object.Entity(value))!;
    public Scope? LexicalScope => Object.Link(Data.OptionalLong("lexical_scope_id"), value => Object.Scope(value));
    public SourceInfo? SourceOrigin => Object.Link(Data.OptionalLong("source_origin_id"), value => Object.Source(value));
    public bool CompilerGenerated => Data.Bool("compiler_generated");
}

public sealed class NamespaceSymbolDetails : SemanticDetail
{
    internal NamespaceSymbolDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public NamespaceSymbol? AliasTarget => Object.Link(Data.OptionalLong("alias_target_id"), value => Object.Namespace(value));
    public bool IsInline => Data.Bool("is_inline");
    public bool IsAnonymous => Data.Bool("is_anonymous");
    public Scope? Scope => Object.Link(Data.OptionalLong("scope_id"), value => Object.Scope(value));
    public CppType? ProxyClass => Object.Link(Data.OptionalLong("proxy_class_id"), value => Object.Type(value));
    public bool HasInternalLinkage => Data.Bool("has_internal_linkage");
    public bool NamedInStrongUsing => Data.Bool("named_in_strong_using");
    public bool IsStd => Data.Bool("is_std");
    public bool? HasGnuAbiTagAttribute => Data.OptionalBool("has_gnu_abi_tag_attribute");
    public bool IsNamespaceAlias => Data.Bool("is_namespace_alias");
}

public sealed class RoutineSymbolDetails : SemanticDetail
{
    internal RoutineSymbolDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public CppType RoutineType => Object.Link(Data.OptionalLong("routine_type_id"), value => Object.Type(value))!;
    public CppType? ContainingType => Object.Link(Data.OptionalLong("containing_type_id"), value => Object.Type(value));
    public SemanticNode? BodyRoot => Object.Link(Data.OptionalLong("body_root_id"), value => Object.Node(value));
    public Scope? BodyScope => Object.Link(Data.OptionalLong("body_scope_id"), value => Object.Scope(value));
    public SemanticKind BodyState => Object.Kind(Data.Long("body_state_id"));
    public SemanticKind? SpecialFunction => Object.Link(Data.OptionalLong("special_function_id"), value => Object.Kind(value));
    public SemanticKind? BuiltinKind => Object.Link(Data.OptionalLong("builtin_kind_id"), value => Object.Kind(value));
    public TemplateDescriptor? Template => Object.Link(Data.OptionalLong("template_id"), value => Object.Template(value));
    public TemplateInstantiation? Instance => Object.Link(Data.OptionalLong("instance_id"), value => Object.Instance(value));
    public FunctionSymbol? PrimaryEntry => Object.Link(Data.OptionalLong("primary_entry_id"), value => Object.Function(value));
    public bool IsVirtual => Data.Bool("is_virtual");
    public bool IsPure => Data.Bool("is_pure");
    public bool IsFinal => Data.Bool("is_final");
    public bool IsInline => Data.Bool("is_inline");
    public bool IsConstexpr => Data.Bool("is_constexpr");
    public bool IsConsteval => Data.Bool("is_consteval");
    public bool IsDeleted => Data.Bool("is_deleted");
    public bool IsDefaulted => Data.Bool("is_defaulted");
    public bool IsTlsInitializationRoutine => Data.Bool("is_tls_initialization_routine");
    public bool IsTlsInitializationAlias => Data.Bool("is_tls_initialization_alias");
    public bool IsDefined => Data.Bool("is_defined");
    public bool? HasNoEffect => Data.OptionalBool("has_no_effect");
    public bool? IsIneligible => Data.OptionalBool("is_ineligible");
    public bool AddressTaken => Data.Bool("address_taken");
    public bool OverridesBaseMember => Data.Bool("overrides_base_member");
    public bool IsOverride => Data.Bool("is_override");
    public bool? IsAbstract => Data.OptionalBool("is_abstract");
    public bool? IsSealed => Data.OptionalBool("is_sealed");
    public bool? IsNewMember => Data.OptionalBool("is_new_member");
    public bool? IsInterfaceSlot => Data.OptionalBool("is_interface_slot");
    public bool? DefinitionCannotBeGenerated => Data.OptionalBool("definition_cannot_be_generated");
    public bool CovariantReturnVirtualOverride => Data.Bool("covariant_return_virtual_override");
    public bool IsDeclaredConstexpr => Data.Bool("is_declared_constexpr");
    public bool IsConstexprIntrinsic => Data.Bool("is_constexpr_intrinsic");
    public bool IsCalled => Data.Bool("is_called");
    public bool IsExplicitConstructor => Data.Bool("is_explicit_constructor");
    public bool IsExplicitConversionFunction => Data.Bool("is_explicit_conversion_function");
    public bool IsTrivialDefaultConstructor => Data.Bool("is_trivial_default_constructor");
    public bool IsTrivialCopyFunction => Data.Bool("is_trivial_copy_function");
    public bool IsTrivialDestructor => Data.Bool("is_trivial_destructor");
    public bool IsInitializerListCtor => Data.Bool("is_initializer_list_ctor");
    public bool IsDelegatingCtor => Data.Bool("is_delegating_ctor");
    public bool IsInheritingCtor => Data.Bool("is_inheriting_ctor");
    public bool InheritsVirtually => Data.Bool("inherits_virtually");
    public bool IsDeductionGuideFromInheritingCtor => Data.Bool("is_deduction_guide_from_inheriting_ctor");
    public bool? AssignmentToThisDone => Data.OptionalBool("assignment_to_this_done");
    public bool IsTemplateFunction => Data.Bool("is_template_function");
    public bool IsSpecialized => Data.Bool("is_specialized");
    public bool SpecializedWithOldSyntax => Data.Bool("specialized_with_old_syntax");
    public bool IsPrototypeInstantiation => Data.Bool("is_prototype_instantiation");
    public bool NeverThrows => Data.Bool("never_throws");
    public bool IsInClassSpecialization => Data.Bool("is_in_class_specialization");
    public bool ExplicitInstantiation => Data.Bool("explicit_instantiation");
    public bool ClassExplicitlyInstantiated => Data.Bool("class_explicitly_instantiated");
    public bool ExplicitDoNotInstantiate => Data.Bool("explicit_do_not_instantiate");
    public bool HasNodiscardAttribute => Data.Bool("has_nodiscard_attribute");
    public bool? NeverInline => Data.OptionalBool("never_inline");
    public bool? IsNaked => Data.OptionalBool("is_naked");
    public bool? DeclaredOnlyAsFriend => Data.OptionalBool("declared_only_as_friend");
    public bool? ExplicitExternInline => Data.OptionalBool("explicit_extern_inline");
    public bool? IsDirectLinkageSpecifierOnNondefDecl => Data.OptionalBool("is_direct_linkage_specifier_on_nondef_decl");
    public bool? IsInitializationRoutine => Data.OptionalBool("is_initialization_routine");
    public bool? IsFinalizationRoutine => Data.OptionalBool("is_finalization_routine");
    public bool? IsWeak => Data.OptionalBool("is_weak");
    public bool? IsWeakref => Data.OptionalBool("is_weakref");
    public bool? IsGnuAlias => Data.OptionalBool("is_gnu_alias");
    public bool? IsIfunc => Data.OptionalBool("is_ifunc");
    public bool? HasGnuUsedAttribute => Data.OptionalBool("has_gnu_used_attribute");
    public bool? HasGnuAbiTagAttribute => Data.OptionalBool("has_gnu_abi_tag_attribute");
    public bool? InGnuAbiTagNamespace => Data.OptionalBool("in_gnu_abi_tag_namespace");
    public bool? ImplicitAbiTagsAdded => Data.OptionalBool("implicit_abi_tags_added");
    public bool? AllocatesMemory => Data.OptionalBool("allocates_memory");
    public bool? NoInstrumentFunction => Data.OptionalBool("no_instrument_function");
    public bool? NoCheckMemoryUsage => Data.OptionalBool("no_check_memory_usage");
    public bool? AlwaysInline => Data.OptionalBool("always_inline");
    public bool? GnuC89Inline => Data.OptionalBool("gnu_c89_inline");
    public bool? ImplicitAlias => Data.OptionalBool("implicit_alias");
    public bool? HasInternalLinkageAttribute => Data.OptionalBool("has_internal_linkage_attribute");
    public bool? CanBeInstantiated => Data.OptionalBool("can_be_instantiated");
    public bool? DoNotInstantiate => Data.OptionalBool("do_not_instantiate");
    public bool? InstanceRequired => Data.OptionalBool("instance_required");
    public bool ContainsTryBlock => Data.Bool("contains_try_block");
    public bool ContainsLocalClassType => Data.Bool("contains_local_class_type");
    public bool SupersededExternal => Data.Bool("superseded_external");
    public bool IsDefinedInFriendDecl => Data.Bool("is_defined_in_friend_decl");
    public bool IsDefinedOutsideOfParent => Data.Bool("is_defined_outside_of_parent");
    public bool? IsInlinable => Data.OptionalBool("is_inlinable");
    public bool? DefinitionNeeded => Data.OptionalBool("definition_needed");
    public bool ExplTemplateArgListUsed => Data.Bool("expl_template_arg_list_used");
    public bool? DefinitionHasDirectLinkageSpecifier => Data.OptionalBool("definition_has_direct_linkage_specifier");
    public bool DefinitionForInliningOnly => Data.Bool("definition_for_inlining_only");
    public bool? InlineInstanceRequired => Data.OptionalBool("inline_instance_required");
    public bool SuppressInlineBody => Data.Bool("suppress_inline_body");
    public bool NeedOutOfLineCopy => Data.Bool("need_out_of_line_copy");
    public bool ContainsStatementExpression => Data.Bool("contains_statement_expression");
    public bool? InlineInClassDefinition => Data.OptionalBool("inline_in_class_definition");
    public bool? IsUseComdat => Data.OptionalBool("is_use_comdat");
    public bool? IsAliasEntry => Data.OptionalBool("is_alias_entry");
    public bool? StaticsHaveBeenPromoted => Data.OptionalBool("statics_have_been_promoted");
    public bool IsLambdaBody => Data.Bool("is_lambda_body");
    public bool DeclaredUsingTypeWithoutLinkage => Data.Bool("declared_using_type_without_linkage");
    public bool ContainsLocalStaticVariable => Data.Bool("contains_local_static_variable");
    public bool? IsEmbedsSourceEntries => Data.OptionalBool("is_embeds_source_entries");
    public bool IsRawLiteralOperator => Data.Bool("is_raw_literal_operator");
    public bool? HasCtorPriority => Data.OptionalBool("has_ctor_priority");
    public bool? HasDtorPriority => Data.OptionalBool("has_dtor_priority");
    public bool HasDeducibleReturnType => Data.Bool("has_deducible_return_type");
    public bool HasDeducedReturnType => Data.Bool("has_deduced_return_type");
    public bool ContainsGenericLambda => Data.Bool("contains_generic_lambda");
    public bool IsCoroutine => Data.Bool("is_coroutine");
    public bool FriendDefinedInInstantiation => Data.Bool("friend_defined_in_instantiation");
    public bool HasPassObjectSizeAttr => Data.Bool("has_pass_object_size_attr");
    public bool FromInjectedTokens => Data.Bool("from_injected_tokens");
    public bool? IsPureFunction => Data.OptionalBool("is_pure_function");
    public SemanticKind? DeclaredStorageClass => Object.Link(Data.OptionalLong("declared_storage_class_id"), value => Object.Kind(value));
    public CppType? DeclaredType => Object.Link(Data.OptionalLong("declared_type_id"), value => Object.Type(value));
    public SemanticKind? OperatorName => Object.Link(Data.OptionalLong("operator_name_id"), value => Object.Kind(value));
    public FunctionSymbol? LambdaCallOperator => Object.Link(Data.OptionalLong("lambda_call_operator_id"), value => Object.Function(value));
    public TemplateDescriptor? DeductionGuideTemplate => Object.Link(Data.OptionalLong("deduction_guide_template_id"), value => Object.Template(value));
    public BigInteger? VirtualFunctionNumber => Number(Data.OptionalText("virtual_function_number"));
    public long? ConstexprIntrinsic => Data.OptionalLong("constexpr_intrinsic");
    public RequiresClause? TrailingRequiresClause => Object.Link(Data.OptionalLong("trailing_requires_clause_id"), value => Object.RequiresClause(value));
    public FunctionSymbol? InheritedRoutine => Object.Link(Data.OptionalLong("inherited_routine_id"), value => Object.Function(value));
    public UsingDeclaration? GeneratingUsing => Object.Link(Data.OptionalLong("generating_using_id"), value => Object.Using(value));
    public FunctionSymbol? OverridingFunction => Object.Link(Data.OptionalLong("overriding_function_id"), value => Object.Function(value));
    public FunctionSymbol? OverriddenFunction => Object.Link(Data.OptionalLong("overridden_function_id"), value => Object.Function(value));
    public BigInteger? ThisAdjustment => Number(Data.OptionalText("this_adjustment"));
    public BigInteger? VcallIndex => Number(Data.OptionalText("vcall_index"));
    public BigInteger? ReturnAdjustment => Number(Data.OptionalText("return_adjustment"));
    public BigInteger? VbaseIndex => Number(Data.OptionalText("vbase_index"));
    public SemanticKind? CtorDtorKind => Object.Link(Data.OptionalLong("ctor_dtor_kind_id"), value => Object.Kind(value));
    public BigInteger? InitializationPriority => Number(Data.OptionalText("initialization_priority"));
    public SemanticKind? FpContract => Object.Link(Data.OptionalLong("fp_contract_id"), value => Object.Kind(value));
    public SemanticKind? FenvAccess => Object.Link(Data.OptionalLong("fenv_access_id"), value => Object.Kind(value));
    public SemanticKind? CxLimitedRange => Object.Link(Data.OptionalLong("cx_limited_range_id"), value => Object.Kind(value));
    public SemanticKind? FxFullPrecision => Object.Link(Data.OptionalLong("fx_full_precision_id"), value => Object.Kind(value));
    public SemanticKind? FxFractOverflow => Object.Link(Data.OptionalLong("fx_fract_overflow_id"), value => Object.Kind(value));
    public SemanticKind? FxAccumOverflow => Object.Link(Data.OptionalLong("fx_accum_overflow_id"), value => Object.Kind(value));
    public long? UpcAccessMethod => Data.OptionalLong("upc_access_method");
    public SemanticKind? ElfVisibility => Object.Link(Data.OptionalLong("elf_visibility_id"), value => Object.Kind(value));
    public SemanticKind? SurroundingNameLinkage => Object.Link(Data.OptionalLong("surrounding_name_linkage_id"), value => Object.Kind(value));
    public bool? DefinitionCNameLinkageSpecified => Data.OptionalBool("definition_c_name_linkage_specified");
    public GnuRoutineSupplement? GnuSupplement => Object.Link(Data.OptionalLong("gnu_supplement_id"), value => Object.GnuRoutineSupplement(value));
    public IReadOnlyList<RoutineParameterDetails> RoutineParameterRows => Object.List("SELECT * FROM routine_parameter WHERE owner_id=$owner ORDER BY role_id,ordinal", Data.Long("id"), r => new RoutineParameterDetails(Object, r));
    public IReadOnlyList<OverrideRelationDetails> OverrideRelationRows => Object.List("SELECT * FROM override_relation WHERE owner_id=$owner ORDER BY ordinal", Data.Long("id"), r => new OverrideRelationDetails(Object, r));
    public IReadOnlyList<RoutineSelectedOverrideDetails> RoutineSelectedOverrideRows => Object.List("SELECT * FROM routine_selected_override WHERE owner_id=$owner ORDER BY ordinal", Data.Long("id"), r => new RoutineSelectedOverrideDetails(Object, r));
    public IReadOnlyList<RoutineBefriendingClassDetails> RoutineBefriendingClassRows => Object.List("SELECT * FROM routine_befriending_class WHERE owner_id=$owner ORDER BY ordinal", Data.Long("id"), r => new RoutineBefriendingClassDetails(Object, r));
}

public sealed class RoutineParameterDetails : SemanticDetail
{
    internal RoutineParameterDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public FunctionSymbol Owner => Object.Link(Data.OptionalLong("owner_id"), value => Object.Function(value))!;
    public long Ordinal => Data.Long("ordinal");
    public VariableSymbol Parameter => Object.Link(Data.OptionalLong("parameter_id"), value => Object.Variable(value))!;
    public SemanticKind Role => Object.Kind(Data.Long("role_id"));
    public bool IsSourceParameter => Data.Bool("is_source_parameter");
}

public sealed class VariableSymbolDetails : SemanticDetail
{
    internal VariableSymbolDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public CppType Type => Object.Link(Data.OptionalLong("type_id"), value => Object.Type(value))!;
    public SemanticKind InitializationKind => Object.Kind(Data.Long("initialization_kind_id"));
    public Initialization? Initializer => Object.Link(Data.OptionalLong("initializer_id"), value => Object.Initialization(value));
    public Constant? ConstantInitializer => Object.Link(Data.OptionalLong("constant_initializer_id"), value => Object.Constant(value));
    public SemanticKind? StorageDuration => Object.Link(Data.OptionalLong("storage_duration_id"), value => Object.Kind(value));
    public bool IsThreadLocal => Data.Bool("is_thread_local");
    public bool IsInline => Data.Bool("is_inline");
    public bool IsParameter => Data.Bool("is_parameter");
    public VariableSymbol? BindingContainer => Object.Link(Data.OptionalLong("binding_container_id"), value => Object.Variable(value));
    public TemplateInstantiation? Instance => Object.Link(Data.OptionalLong("instance_id"), value => Object.Instance(value));
    public FunctionSymbol? CleanupRoutine => Object.Link(Data.OptionalLong("cleanup_routine_id"), value => Object.Function(value));
    public bool IsStructuredBinding => Data.Bool("is_structured_binding");
    public bool IsStructuredBindingContainer => Data.Bool("is_structured_binding_container");
    public ExpressionOperation? BoundExpression => Object.Link(Data.OptionalLong("bound_expression_id"), value => Object.Expression(value));
    public bool StandardThreadLocalDeclared => Data.Bool("standard_thread_local_declared");
    public bool StandardThreadLocalEffective => Data.Bool("standard_thread_local_effective");
    public bool ExtensionThreadLocal => Data.Bool("extension_thread_local");
    public string? AssemblyName => Object.Text(Data.OptionalLong("assembly_name_id"));
    public long? NamedRegisterNumber => Data.OptionalLong("named_register_number");
    public SemanticKind? RegisterKind => Object.Link(Data.OptionalLong("register_kind_id"), value => Object.Kind(value));
    public FunctionSymbol? DynamicInitializationRoutine => Object.Link(Data.OptionalLong("dynamic_initialization_routine_id"), value => Object.Function(value));
    public FunctionSymbol? ThreadInitializationRoutine => Object.Link(Data.OptionalLong("thread_initialization_routine_id"), value => Object.Function(value));
    public FunctionSymbol? ThreadWrapperRoutine => Object.Link(Data.OptionalLong("thread_wrapper_routine_id"), value => Object.Function(value));
    public bool InitializationRewrittenAsAssignment => Data.Bool("initialization_rewritten_as_assignment");
    public bool LoweringGenerated => Data.Bool("lowering_generated");
    public bool PromotedLocalStatic => Data.Bool("promoted_local_static");
    public bool PromotedLocalStaticInitialization => Data.Bool("promoted_local_static_initialization");
    public bool ExtendsLifetime => Data.Bool("extends_lifetime");
    public bool IsUninitialized => Data.Bool("is_uninitialized");
    public bool HasFlexibleArrayInitializer => Data.Bool("has_flexible_array_initializer");
    public bool IsImmutable => Data.Bool("is_immutable");
    public bool IsConstexpr => Data.Bool("is_constexpr");
    public bool IsConstinit => Data.Bool("is_constinit");
    public bool IsPack => Data.Bool("is_pack");
    public bool IsPackElement => Data.Bool("is_pack_element");
    public ParameterType? AssociatedParameterType => Object.Link(Data.OptionalLong("associated_parameter_type_id"), value => Object.ParameterType(value));
    public BigInteger? ExplicitAlignment => Number(Data.OptionalText("explicit_alignment"));
    public long? InitializationPriority => Data.OptionalLong("initialization_priority");
    public string? Section => Object.Text(Data.OptionalLong("section_id"));
    public string? ComdatGroup => Object.Text(Data.OptionalLong("comdat_group_id"));
    public VariableSymbol? AliasedVariable => Object.Link(Data.OptionalLong("aliased_variable_id"), value => Object.Variable(value));
    public VariableSymbol? VlaElementCountVariable => Object.Link(Data.OptionalLong("vla_element_count_variable_id"), value => Object.Variable(value));
    public VariableSymbol? EffectiveClassTypeinfoVariable => Object.Link(Data.OptionalLong("effective_class_typeinfo_variable_id"), value => Object.Variable(value));
    public bool IsUsed => Data.Bool("is_used");
    public bool AddressTaken => Data.Bool("address_taken");
    public bool ReferencedNonLocally => Data.Bool("referenced_non_locally");
    public bool ModifiedWithinTryBlock => Data.Bool("modified_within_try_block");
    public bool IsTemplateVariable => Data.Bool("is_template_variable");
    public bool IsPrototypeInstantiation => Data.Bool("is_prototype_instantiation");
    public bool IsNonreal => Data.Bool("is_nonreal");
    public bool IsSpecialized => Data.Bool("is_specialized");
    public bool SpecializedWithOldSyntax => Data.Bool("specialized_with_old_syntax");
    public bool ExplicitInstantiation => Data.Bool("explicit_instantiation");
    public bool ClassExplicitlyInstantiated => Data.Bool("class_explicitly_instantiated");
    public bool ExplicitDoNotInstantiate => Data.Bool("explicit_do_not_instantiate");
    public bool ParameterValueChanged => Data.Bool("parameter_value_changed");
    public bool ParameterUsedMoreThanOnce => Data.Bool("parameter_used_more_than_once");
    public bool IsHandlerParameter => Data.Bool("is_handler_parameter");
    public bool IsThisParameter => Data.Bool("is_this_parameter");
    public bool IsAnonymousParentObject => Data.Bool("is_anonymous_parent_object");
    public bool IsMemberConstant => Data.Bool("is_member_constant");
    public bool SuppressInlineDefinition => Data.Bool("suppress_inline_definition");
    public bool SupersededExternal => Data.Bool("superseded_external");
    public bool HasVariablyModifiedType => Data.Bool("has_variably_modified_type");
    public bool IsVla => Data.Bool("is_vla");
    public bool IsCompoundLiteral => Data.Bool("is_compound_literal");
    public bool HasExplicitInitializer => Data.Bool("has_explicit_initializer");
    public bool HasParenthesizedInitializer => Data.Bool("has_parenthesized_initializer");
    public bool HasDirectBracedInitializer => Data.Bool("has_direct_braced_initializer");
    public bool DeclaredWithAuto => Data.Bool("declared_with_auto");
    public bool DeclaredWithDecltypeAuto => Data.Bool("declared_with_decltype_auto");
    public bool DeclaredWithClassTemplatePlaceholder => Data.Bool("declared_with_class_template_placeholder");
    public bool DeclaredUsingTypeWithoutLinkage => Data.Bool("declared_using_type_without_linkage");
    public bool IsEnhancedForIterator => Data.Bool("is_enhanced_for_iterator");
    public bool InitializerInClass => Data.Bool("initializer_in_class");
    public bool ConstantValuedRecorded => Data.Bool("constant_valued_recorded");
    public bool IsTemplateParameterObject => Data.Bool("is_template_parameter_object");
    public bool IsInClassSpecialization => Data.Bool("is_in_class_specialization");
    public bool IsOptionalVtable => Data.Bool("is_optional_vtable");
    public bool VtableDefined => Data.Bool("vtable_defined");
    public bool IsWeak => Data.Bool("is_weak");
    public bool IsWeakref => Data.Bool("is_weakref");
    public bool IsGnuAlias => Data.Bool("is_gnu_alias");
    public bool HasGnuUsedAttribute => Data.Bool("has_gnu_used_attribute");
    public bool HasGnuAbiTagAttribute => Data.Bool("has_gnu_abi_tag_attribute");
    public bool IsNotCommon => Data.Bool("is_not_common");
    public bool IsCommon => Data.Bool("is_common");
    public bool HasInternalLinkageAttribute => Data.Bool("has_internal_linkage_attribute");
    public SemanticKind? ElfVisibilityKind => Object.Link(Data.OptionalLong("elf_visibility_kind_id"), value => Object.Kind(value));
    public SemanticKind? DeclaredStorageClass => Object.Link(Data.OptionalLong("declared_storage_class_id"), value => Object.Kind(value));
    public CppType? DeclaredType => Object.Link(Data.OptionalLong("declared_type_id"), value => Object.Type(value));
    public bool EmbedsSourceEntries => Data.Bool("embeds_source_entries");
    public bool? CanBeInstantiated => Data.OptionalBool("can_be_instantiated");
    public bool? DoNotInstantiate => Data.OptionalBool("do_not_instantiate");
    public bool? InstanceRequired => Data.OptionalBool("instance_required");
    public bool? ParameterUsedAsLvalue => Data.OptionalBool("parameter_used_as_lvalue");
    public bool? InlineInstanceRequired => Data.OptionalBool("inline_instance_required");
    public bool? IsTempForUnmodifiedInlinedParameter => Data.OptionalBool("is_temp_for_unmodified_inlined_parameter");
    public bool? IsTempForConstructorThisInlinedParameter => Data.OptionalBool("is_temp_for_constructor_this_inlined_parameter");
    public IReadOnlyList<VariableBindingDetails> VariableBindingRows => Object.List("SELECT * FROM variable_binding WHERE owner_id=$owner ORDER BY ordinal", Data.Long("id"), r => new VariableBindingDetails(Object, r));
    public IReadOnlyList<VariableInitializerEntityDetails> VariableInitializerEntityRows => Object.List("SELECT * FROM variable_initializer_entity WHERE owner_id=$owner ORDER BY ordinal", Data.Long("id"), r => new VariableInitializerEntityDetails(Object, r));
}

public sealed class VariableBindingDetails : SemanticDetail
{
    internal VariableBindingDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public VariableSymbol Owner => Object.Link(Data.OptionalLong("owner_id"), value => Object.Variable(value))!;
    public long Ordinal => Data.Long("ordinal");
    public VariableSymbol Variable => Object.Link(Data.OptionalLong("variable_id"), value => Object.Variable(value))!;
}

public sealed class FieldSymbolDetails : SemanticDetail
{
    internal FieldSymbolDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public CppType RecordType => Object.Link(Data.OptionalLong("record_type_id"), value => Object.Type(value))!;
    public CppType Type => Object.Link(Data.OptionalLong("type_id"), value => Object.Type(value))!;
    public Initialization? DefaultInitialization => Object.Link(Data.OptionalLong("default_initialization_id"), value => Object.Initialization(value));
    public bool IsMutable => Data.Bool("is_mutable");
    public bool IsBitField => Data.Bool("is_bit_field");
    public bool IsGeneratedBase => Data.Bool("is_generated_base");
    public bool? IsGeneratedVptr => Data.OptionalBool("is_generated_vptr");
    public Constant? BitSizeConstant => Object.Link(Data.OptionalLong("bit_size_constant_id"), value => Object.Constant(value));
    public ExpressionOperation? BitWidthExpression => Object.Link(Data.OptionalLong("bit_width_expression_id"), value => Object.Expression(value));
    public BigInteger? DeclaredBitSize => Number(Data.OptionalText("declared_bit_size"));
    public bool IsAnonymousParentObject => Data.Bool("is_anonymous_parent_object");
    public bool IsInitCapture => Data.Bool("is_init_capture");
    public bool IsCapturedThis => Data.Bool("is_captured_this");
    public bool IsCapturedPackElement => Data.Bool("is_captured_pack_element");
    public bool HasInitializer => Data.Bool("has_initializer");
    public bool InitIsCtorDependent => Data.Bool("init_is_ctor_dependent");
    public bool HasDirectBracedInitializer => Data.Bool("has_direct_braced_initializer");
    public bool HasNonconstantInitializer => Data.Bool("has_nonconstant_initializer");
    public bool HasNoUniqueAddressAttribute => Data.Bool("has_no_unique_address_attribute");
    public bool IsOptimizedEmptyClass => Data.Bool("is_optimized_empty_class");
    public bool BitSizeConstantExprInLocalExprNodeRef => Data.Bool("bit_size_constant_expr_in_local_expr_node_ref");
    public bool? IsPacked => Data.OptionalBool("is_packed");
    public bool? VlaTreatedAsZeroLengthArray => Data.OptionalBool("vla_treated_as_zero_length_array");
    public bool? ClassSubobjectWithTailPadding => Data.OptionalBool("class_subobject_with_tail_padding");
    public bool? BitFieldIsSigned => Data.OptionalBool("bit_field_is_signed");
    public BigInteger? ExplicitAlignment => Number(Data.OptionalText("explicit_alignment"));
    public IReadOnlyList<FieldInitializerEntityDetails> FieldInitializerEntityRows => Object.List("SELECT * FROM field_initializer_entity WHERE owner_id=$owner ORDER BY ordinal", Data.Long("id"), r => new FieldInitializerEntityDetails(Object, r));
}

public sealed class FieldPropertyDetails : SemanticDetail
{
    internal FieldPropertyDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public string? GetName => Object.Text(Data.OptionalLong("get_name_id"));
    public string? PutName => Object.Text(Data.OptionalLong("put_name_id"));
}

public sealed class LabelSymbolDetails : SemanticDetail
{
    internal LabelSymbolDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public StatementOperation? Statement => Object.Link(Data.OptionalLong("statement_id"), value => Object.Statement(value));
    public Lifetime? Lifetime => Object.Link(Data.OptionalLong("lifetime_id"), value => Object.Lifetime(value));
    public bool IsBreak => Data.Bool("is_break");
    public bool IsContinue => Data.Bool("is_continue");
    public bool? IsLeave => Data.OptionalBool("is_leave");
    public bool ReachableByFallThrough => Data.Bool("reachable_by_fall_through");
    public bool IsSwitchBreak => Data.Bool("is_switch_break");
    public bool? AddressTaken => Data.OptionalBool("address_taken");
    public bool? LocallyDeclared => Data.OptionalBool("locally_declared");
    public bool IsLikely => Data.Bool("is_likely");
    public bool IsUnlikely => Data.Bool("is_unlikely");
    public BigInteger? MicrosoftTryDepth => Number(Data.OptionalText("microsoft_try_depth"));
}

public sealed class EnumSymbolDetails : SemanticDetail
{
    internal EnumSymbolDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public CppType? UnderlyingType => Object.Link(Data.OptionalLong("underlying_type_id"), value => Object.Type(value));
    public bool IsScoped => Data.Bool("is_scoped");
    public CppType? ExplicitUnderlyingType => Object.Link(Data.OptionalLong("explicit_underlying_type_id"), value => Object.Type(value));
    public bool HasExplicitBase => Data.Bool("has_explicit_base");
    public bool UnderlyingTypeShouldUseUnsigned => Data.Bool("underlying_type_should_use_unsigned");
    public bool IsPacked => Data.Bool("is_packed");
    public bool OriginallyUnnamed => Data.Bool("originally_unnamed");
    public bool IsTemplateEnum => Data.Bool("is_template_enum");
    public bool IsPrototypeInstantiation => Data.Bool("is_prototype_instantiation");
    public bool IsNonreal => Data.Bool("is_nonreal");
    public bool IsSpecialized => Data.Bool("is_specialized");
    public bool IsMsInstantiatedNonrealEnum => Data.Bool("is_ms_instantiated_nonreal_enum");
    public SemanticKind? ElfVisibility => Object.Link(Data.OptionalLong("elf_visibility_id"), value => Object.Kind(value));
    public bool EnumeratorListSeen => Data.Bool("enumerator_list_seen");
    public bool EnumeratorListComplete => Data.Bool("enumerator_list_complete");
    public bool HasNodiscard => Data.Bool("has_nodiscard");
    public SourcePosition? BasePosition => Object.Link(Data.OptionalLong("base_position_id"), value => Object.Position(value));
    public TemplateDescriptor? Template => Object.Link(Data.OptionalLong("template_id"), value => Object.Template(value));
    public string? UuidString => Object.Text(Data.OptionalLong("uuid_string_id"));
    public VariableSymbol? UuidVariable => Object.Link(Data.OptionalLong("uuid_variable_id"), value => Object.Variable(value));
    public IReadOnlyList<EnumeratorDetails> EnumeratorRows => Object.List("SELECT * FROM enumerator WHERE owner_id=$owner ORDER BY ordinal", Data.Long("id"), r => new EnumeratorDetails(Object, r));
}

public sealed class EnumeratorDetails : SemanticDetail
{
    internal EnumeratorDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public EnumSymbol Owner => Object.Link(Data.OptionalLong("owner_id"), value => Object.Enum(value))!;
    public long Ordinal => Data.Long("ordinal");
    public Symbol Symbol => Object.Link(Data.OptionalLong("symbol_id"), value => Object.Symbol(value))!;
    public Constant Constant => Object.Link(Data.OptionalLong("constant_id"), value => Object.Constant(value))!;
}

public sealed class AliasSymbolDetails : SemanticDetail
{
    internal AliasSymbolDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public CppType? UnderlyingType => Object.Link(Data.OptionalLong("underlying_type_id"), value => Object.Type(value));
}

public sealed class UsingDeclarationDetails : SemanticDetail
{
    internal UsingDeclarationDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public SemanticEntity? Target => Object.Link(Data.OptionalLong("target_id"), value => Object.Entity(value));
    public Scope? LexicalScope => Object.Link(Data.OptionalLong("lexical_scope_id"), value => Object.Scope(value));
    public SourceInfo? SourceOrigin => Object.Link(Data.OptionalLong("source_origin_id"), value => Object.Source(value));
    public bool IsDirective => Data.Bool("is_directive");
    public bool IsClassMember => Data.Bool("is_class_member");
    public bool IsInheritingConstructor => Data.Bool("is_inheriting_constructor");
    public bool IsHidden => Data.Bool("is_hidden");
    public bool CompilerGenerated => Data.Bool("compiler_generated");
    public bool IsInlineNamespace => Data.Bool("is_inline_namespace");
    public bool IsStrong => Data.Bool("is_strong");
    public bool IsPackExpansion => Data.Bool("is_pack_expansion");
    public bool IsRepresentative => Data.Bool("is_representative");
    public bool IsUsingEnum => Data.Bool("is_using_enum");
    public bool IsEnumerator => Data.Bool("is_enumerator");
    public SemanticKind? AdjustedAccess => Object.Link(Data.OptionalLong("adjusted_access_id"), value => Object.Kind(value));
    public CppType? QualifierType => Object.Link(Data.OptionalLong("qualifier_type_id"), value => Object.Type(value));
    public NamespaceSymbol? QualifierNamespace => Object.Link(Data.OptionalLong("qualifier_namespace_id"), value => Object.Namespace(value));
    public bool? IsGlobalQualified => Data.OptionalBool("is_global_qualified");
    public UsingDeclaration? NextInSet => Object.Link(Data.OptionalLong("next_in_set_id"), value => Object.Using(value));
}

public sealed class StaticAssertionDetails : SemanticDetail
{
    internal StaticAssertionDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public Constant Condition => Object.Link(Data.OptionalLong("condition_id"), value => Object.Constant(value))!;
    public Constant? Message => Object.Link(Data.OptionalLong("message_id"), value => Object.Constant(value));
    public SourceInfo? SourceOrigin => Object.Link(Data.OptionalLong("source_origin_id"), value => Object.Source(value));
}

public sealed class TypeDetails : SemanticDetail
{
    internal TypeDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public SemanticKind Kind => Object.Kind(Data.Long("kind_id"));
    public Symbol? Symbol => Object.Link(Data.OptionalLong("symbol_id"), value => Object.Symbol(value));
    public BigInteger Size => RequiredNumber(Data.OptionalText("size"));
    public BigInteger Alignment => RequiredNumber(Data.OptionalText("alignment"));
    public BigInteger? DescriptorSize => Number(Data.OptionalText("descriptor_size"));
    public BigInteger? DescriptorAlignment => Number(Data.OptionalText("descriptor_alignment"));
    public bool Incomplete => Data.Bool("incomplete");
    public bool IsDependent => Data.Bool("is_dependent");
    public bool IsAtomic => Data.Bool("is_atomic");
    public bool? DescriptorIncomplete => Data.OptionalBool("descriptor_incomplete");
    public bool UsedInExceptionOrRtti => Data.Bool("used_in_exception_or_rtti");
    public bool DeclaredInFunctionPrototype => Data.Bool("declared_in_function_prototype");
    public bool IsTagRedefinition => Data.Bool("is_tag_redefinition");
    public bool AlignmentSetExplicitly => Data.Bool("alignment_set_explicitly");
    public bool MayAlias => Data.Bool("may_alias");
    public bool HasMicrosoftW64Specifier => Data.Bool("has_microsoft_w64_specifier");
    public bool IsMicrosoftIntrinsic => Data.Bool("is_microsoft_intrinsic");
    public bool AutonomousPrimaryTagDeclaration => Data.Bool("autonomous_primary_tag_declaration");
    public bool IsBuiltinVaList => Data.Bool("is_builtin_va_list");
    public bool IsBuiltinVaListFromCstdarg => Data.Bool("is_builtin_va_list_from_cstdarg");
    public bool HasGnuAbiTagAttribute => Data.Bool("has_gnu_abi_tag_attribute");
    public bool InGnuAbiTagNamespace => Data.Bool("in_gnu_abi_tag_namespace");
    public bool UseCfrontTransitionalNestedTypeNameMangling => Data.Bool("use_cfront_transitional_nested_type_name_mangling");
    public VariableSymbol? TypeinfoVariable => Object.Link(Data.OptionalLong("typeinfo_variable_id"), value => Object.Variable(value));
    public SemanticKind SizeKind => Object.Kind(Data.Long("size_kind_id"));
    public IReadOnlyList<DirectBaseDetails> DirectBaseRows => Object.List("SELECT * FROM direct_base WHERE owner_id=$owner ORDER BY ordinal", Data.Long("id"), r => new DirectBaseDetails(Object, r));
}

public sealed class ArithmeticTypeDetails : SemanticDetail
{
    internal ArithmeticTypeDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public SemanticKind ArithmeticKind => Object.Kind(Data.Long("arithmetic_kind_id"));
    public SemanticKind? EdgIntegerKind => Object.Link(Data.OptionalLong("edg_integer_kind_id"), value => Object.Kind(value));
    public SemanticKind? EdgFloatKind => Object.Link(Data.OptionalLong("edg_float_kind_id"), value => Object.Kind(value));
    public SemanticKind? EdgFixedPrecision => Object.Link(Data.OptionalLong("edg_fixed_precision_id"), value => Object.Kind(value));
    public long? Bits => Data.OptionalLong("bits");
    public bool? Signed => Data.OptionalBool("signed");
    public long? Precision => Data.OptionalLong("precision");
    public long? Radix => Data.OptionalLong("radix");
    public long? Scale => Data.OptionalLong("scale");
    public bool IsBool => Data.Bool("is_bool");
    public bool IsWchar => Data.Bool("is_wchar");
    public bool IsChar8 => Data.Bool("is_char8");
    public bool IsChar16 => Data.Bool("is_char16");
    public bool IsChar32 => Data.Bool("is_char32");
    public bool IsMicrosoftSizedInteger => Data.Bool("is_microsoft_sized_integer");
    public bool ExplicitlySigned => Data.Bool("explicitly_signed");
    public CppType? AffiliatedEnumType => Object.Link(Data.OptionalLong("affiliated_enum_type_id"), value => Object.Type(value));
    public bool IsFractType => Data.Bool("is_fract_type");
    public bool IsSaturating => Data.Bool("is_saturating");
}

public sealed class QualifiedTypeDetails : SemanticDetail
{
    internal QualifiedTypeDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public CppType UnderlyingType => Object.Link(Data.OptionalLong("underlying_type_id"), value => Object.Type(value))!;
    public bool IsConst => Data.Bool("is_const");
    public bool IsVolatile => Data.Bool("is_volatile");
    public bool IsRestrict => Data.Bool("is_restrict");
    public long? AddressSpace => Data.OptionalLong("address_space");
    public bool IsShared => Data.Bool("is_shared");
    public BigInteger? SharedBlockSize => Number(Data.OptionalText("shared_block_size"));
    public bool IsAtomic => Data.Bool("is_atomic");
    public bool IsNullable => Data.Bool("is_nullable");
    public bool IsNonnull => Data.Bool("is_nonnull");
    public bool IsUnaligned => Data.Bool("is_unaligned");
    public bool IsNear => Data.Bool("is_near");
    public bool IsFar => Data.Bool("is_far");
    public bool IsUpcStrict => Data.Bool("is_upc_strict");
    public bool IsUpcRelaxed => Data.Bool("is_upc_relaxed");
    public bool IsNullUnspecified => Data.Bool("is_null_unspecified");
}

public sealed class PointerTypeDetails : SemanticDetail
{
    internal PointerTypeDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public CppType PointeeType => Object.Link(Data.OptionalLong("pointee_type_id"), value => Object.Type(value))!;
    public bool IsReference => Data.Bool("is_reference");
    public bool IsRvalueReference => Data.Bool("is_rvalue_reference");
    public bool IsHandle => Data.Bool("is_handle");
    public bool IsTrackingReference => Data.Bool("is_tracking_reference");
    public bool IsInterior => Data.Bool("is_interior");
    public bool IsPinned => Data.Bool("is_pinned");
    public bool IsPtr32 => Data.Bool("is_ptr32");
    public bool IsPtr64 => Data.Bool("is_ptr64");
    public bool IsSptr => Data.Bool("is_sptr");
    public bool IsUptr => Data.Bool("is_uptr");
    public VariableSymbol? BasedVariable => Object.Link(Data.OptionalLong("based_variable_id"), value => Object.Variable(value));
}

public sealed class MemberPointerTypeDetails : SemanticDetail
{
    internal MemberPointerTypeDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public CppType DeclaringType => Object.Link(Data.OptionalLong("declaring_type_id"), value => Object.Type(value))!;
    public CppType MemberType => Object.Link(Data.OptionalLong("member_type_id"), value => Object.Type(value))!;
    public CppType? OriginalDeclaringType => Object.Link(Data.OptionalLong("original_declaring_type_id"), value => Object.Type(value));
    public bool IsPtr32 => Data.Bool("is_ptr32");
    public bool IsPtr64 => Data.Bool("is_ptr64");
}

public sealed class ArrayTypeDetails : SemanticDetail
{
    internal ArrayTypeDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public CppType ElementType => Object.Link(Data.OptionalLong("element_type_id"), value => Object.Type(value))!;
    public SemanticKind BoundKind => Object.Kind(Data.Long("bound_kind_id"));
    public BigInteger? Bound => Number(Data.OptionalText("bound"));
    public ExpressionOperation? BoundExpression => Object.Link(Data.OptionalLong("bound_expression_id"), value => Object.Expression(value));
    public VlaDimension? VlaDimension => Object.Link(Data.OptionalLong("vla_dimension_id"), value => Object.VlaDimension(value));
    public bool BoundIsZero => Data.Bool("bound_is_zero");
    public bool IsTemplateDependent => Data.Bool("is_template_dependent");
    public bool IsVariable => Data.Bool("is_variable");
    public bool IsVla => Data.Bool("is_vla");
    public bool HasVlaDimension => Data.Bool("has_vla_dimension");
    public bool IsStatic => Data.Bool("is_static");
    public bool IsThreadsDimension => Data.Bool("is_threads_dimension");
    public Constant? BoundConstant => Object.Link(Data.OptionalLong("bound_constant_id"), value => Object.Constant(value));
    public Constant? DependentBoundConstant => Object.Link(Data.OptionalLong("dependent_bound_constant_id"), value => Object.Constant(value));
    public bool IsConst => Data.Bool("is_const");
    public bool IsVolatile => Data.Bool("is_volatile");
    public bool IsRestrict => Data.Bool("is_restrict");
    public bool IsAtomic => Data.Bool("is_atomic");
    public bool IsNullable => Data.Bool("is_nullable");
    public bool IsNonnull => Data.Bool("is_nonnull");
    public bool IsUnaligned => Data.Bool("is_unaligned");
    public bool IsNear => Data.Bool("is_near");
    public bool IsFar => Data.Bool("is_far");
    public bool IsShared => Data.Bool("is_shared");
    public bool IsUpcStrict => Data.Bool("is_upc_strict");
    public bool IsUpcRelaxed => Data.Bool("is_upc_relaxed");
    public ExpressionOperation? DependentBoundExpression => Object.Link(Data.OptionalLong("dependent_bound_expression_id"), value => Object.Expression(value));
    public long? AddressSpace => Data.OptionalLong("address_space");
    public bool IsNullUnspecified => Data.Bool("is_null_unspecified");
}

public sealed class RoutineTypeDetails : SemanticDetail
{
    internal RoutineTypeDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public CppType ResultType => Object.Link(Data.OptionalLong("result_type_id"), value => Object.Type(value))!;
    public CppType? ThisType => Object.Link(Data.OptionalLong("this_type_id"), value => Object.Type(value));
    public SemanticKind? CallingConvention => Object.Link(Data.OptionalLong("calling_convention_id"), value => Object.Kind(value));
    public SemanticKind? LanguageLinkage => Object.Link(Data.OptionalLong("language_linkage_id"), value => Object.Kind(value));
    public ExceptionSpecification? ExceptionSpecification => Object.Link(Data.OptionalLong("exception_specification_id"), value => Object.ExceptionSpecification(value));
    public bool IsVariadic => Data.Bool("is_variadic");
    public bool IsConst => Data.Bool("is_const");
    public bool IsVolatile => Data.Bool("is_volatile");
    public SemanticKind? RefQualifier => Object.Link(Data.OptionalLong("ref_qualifier_id"), value => Object.Kind(value));
    public bool HasPrototype => Data.Bool("has_prototype");
    public bool OldStyleParamsScanned => Data.Bool("old_style_params_scanned");
    public bool HasTrailingReturnType => Data.Bool("has_trailing_return_type");
    public bool LintArgsusedFlag => Data.Bool("lint_argsused_flag");
    public bool ValueReturnedByCctor => Data.Bool("value_returned_by_cctor");
    public bool? ValueReturnedAsParameter => Data.OptionalBool("value_returned_as_parameter");
    public bool? ReturnValueParameterFollowsThis => Data.OptionalBool("return_value_parameter_follows_this");
    public bool AssocRoutineIsCtor => Data.Bool("assoc_routine_is_ctor");
    public bool AssocRoutineIsDtor => Data.Bool("assoc_routine_is_dtor");
    public bool AssocRoutineIsLambdaBody => Data.Bool("assoc_routine_is_lambda_body");
    public bool RoutineNameLinkageIsExplicit => Data.Bool("routine_name_linkage_is_explicit");
    public bool DoesNotReturn => Data.Bool("does_not_return");
    public bool HasEnableIfAttribute => Data.Bool("has_enable_if_attribute");
    public bool? ResultShouldBeUsed => Data.OptionalBool("result_should_be_used");
    public bool? IsGnuConst => Data.OptionalBool("is_gnu_const");
    public bool IsVariadicInstance => Data.Bool("is_variadic_instance");
    public bool? ExplicitCallingConvention => Data.OptionalBool("explicit_calling_convention");
    public bool HadBeenImplicitlyConst => Data.Bool("had_been_implicitly_const");
    public bool IsConditionallyExplicit => Data.Bool("is_conditionally_explicit");
    public bool HasThisParam => Data.Bool("has_this_param");
    public FunctionSymbol? AssociatedRoutine => Object.Link(Data.OptionalLong("associated_routine_id"), value => Object.Function(value));
    public Scope? PrototypeScope => Object.Link(Data.OptionalLong("prototype_scope_id"), value => Object.Scope(value));
    public long LintVarargsCount => Data.Long("lint_varargs_count");
    public SemanticKind? ArgumentPragma => Object.Link(Data.OptionalLong("argument_pragma_id"), value => Object.Kind(value));
    public long? FormatArgument => Data.OptionalLong("format_argument");
    public long? FirstFormatSubstitutionArgument => Data.OptionalLong("first_format_substitution_argument");
    public long? SentinelPosition => Data.OptionalLong("sentinel_position");
    public bool IsRestrict => Data.Bool("is_restrict");
    public bool IsAtomic => Data.Bool("is_atomic");
    public bool IsNullable => Data.Bool("is_nullable");
    public bool IsNonnull => Data.Bool("is_nonnull");
    public bool IsNullUnspecified => Data.Bool("is_null_unspecified");
    public bool IsUnaligned => Data.Bool("is_unaligned");
    public bool IsNear => Data.Bool("is_near");
    public bool IsFar => Data.Bool("is_far");
    public bool IsShared => Data.Bool("is_shared");
    public bool IsUpcStrict => Data.Bool("is_upc_strict");
    public bool IsUpcRelaxed => Data.Bool("is_upc_relaxed");
    public long? AddressSpace => Data.OptionalLong("address_space");
    public bool IsThisConst => Data.Bool("is_this_const");
    public bool IsThisVolatile => Data.Bool("is_this_volatile");
    public bool IsThisRestrict => Data.Bool("is_this_restrict");
    public bool IsThisAtomic => Data.Bool("is_this_atomic");
    public bool IsThisNullable => Data.Bool("is_this_nullable");
    public bool IsThisNonnull => Data.Bool("is_this_nonnull");
    public bool IsThisNullUnspecified => Data.Bool("is_this_null_unspecified");
    public bool IsThisUnaligned => Data.Bool("is_this_unaligned");
    public bool IsThisNear => Data.Bool("is_this_near");
    public bool IsThisFar => Data.Bool("is_this_far");
    public bool IsThisShared => Data.Bool("is_this_shared");
    public bool IsThisUpcStrict => Data.Bool("is_this_upc_strict");
    public bool IsThisUpcRelaxed => Data.Bool("is_this_upc_relaxed");
    public long? ThisAddressSpace => Data.OptionalLong("this_address_space");
    public IReadOnlyList<RoutineTypeParameterDetails> RoutineTypeParameterRows => Object.List("SELECT * FROM routine_type_parameter WHERE owner_id=$owner ORDER BY role_id,ordinal", Data.Long("id"), r => new RoutineTypeParameterDetails(Object, r));
}

public sealed class ParameterTypeDetails : SemanticDetail
{
    internal ParameterTypeDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public CppType Type => Object.Link(Data.OptionalLong("type_id"), value => Object.Type(value))!;
    public CppType? DeclaredType => Object.Link(Data.OptionalLong("declared_type_id"), value => Object.Type(value));
    public string? Name => Object.Text(Data.OptionalLong("name_id"));
    public long ParameterNumber => Data.Long("parameter_number");
    public ExpressionOperation? DefaultExpression => Object.Link(Data.OptionalLong("default_expression_id"), value => Object.Expression(value));
    public bool HasNameConflict => Data.Bool("has_name_conflict");
    public bool PassedViaCopyConstructor => Data.Bool("passed_via_copy_constructor");
    public bool HasDefaultArgument => Data.Bool("has_default_argument");
    public bool DefaultArgumentInClass => Data.Bool("default_argument_in_class");
    public bool HasUnevaluatedTemplateDefault => Data.Bool("has_unevaluated_template_default");
    public bool TypeInvolvesDeducedTemplateParameter => Data.Bool("type_involves_deduced_template_parameter");
    public bool TypeInvolvesTemplateParameter => Data.Bool("type_involves_template_parameter");
    public bool IsParameterPack => Data.Bool("is_parameter_pack");
    public bool IsPackElement => Data.Bool("is_pack_element");
    public bool WasNontrailingPack => Data.Bool("was_nontrailing_pack");
    public bool IsAutoParameter => Data.Bool("is_auto_parameter");
    public bool DuplicateName => Data.Bool("duplicate_name");
    public bool MoveConstructorOrAssignmentParameter => Data.Bool("move_constructor_or_assignment_parameter");
    public bool CopyOrMoveConstructorParameter => Data.Bool("copy_or_move_constructor_parameter");
    public bool IsRequiresExpressionParameter => Data.Bool("is_requires_expression_parameter");
    public bool IsExplicitThis => Data.Bool("is_explicit_this");
    public bool IsTransparent => Data.Bool("is_transparent");
    public bool IsNonnull => Data.Bool("is_nonnull");
    public bool IsCliParameterArray => Data.Bool("is_cli_parameter_array");
    public bool IsConst => Data.Bool("is_const");
    public bool IsVolatile => Data.Bool("is_volatile");
    public bool IsRestrict => Data.Bool("is_restrict");
    public bool IsAtomic => Data.Bool("is_atomic");
    public bool IsNullable => Data.Bool("is_nullable");
    public bool IsNonnullQualified => Data.Bool("is_nonnull_qualified");
    public bool IsNullUnspecified => Data.Bool("is_null_unspecified");
    public bool IsUnaligned => Data.Bool("is_unaligned");
    public bool IsNear => Data.Bool("is_near");
    public bool IsFar => Data.Bool("is_far");
    public bool IsUpcShared => Data.Bool("is_upc_shared");
    public bool IsUpcStrict => Data.Bool("is_upc_strict");
    public bool IsUpcRelaxed => Data.Bool("is_upc_relaxed");
    public long? NamedAddressSpace => Data.OptionalLong("named_address_space");
    public IReadOnlyList<ParameterDefaultEntityDetails> ParameterDefaultEntityRows => Object.List("SELECT * FROM parameter_default_entity WHERE owner_id=$owner ORDER BY ordinal", Data.Long("id"), r => new ParameterDefaultEntityDetails(Object, r));
}

public sealed class ParameterDefaultEntityDetails : SemanticDetail
{
    internal ParameterDefaultEntityDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public ParameterType Owner => Object.Link(Data.OptionalLong("owner_id"), value => Object.ParameterType(value))!;
    public long Ordinal => Data.Long("ordinal");
    public SemanticEntity Entity => Object.Link(Data.OptionalLong("entity_id"), value => Object.Entity(value))!;
}

public sealed class RoutineTypeParameterDetails : SemanticDetail
{
    internal RoutineTypeParameterDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public RoutineTypeDetails Owner => Object.Link(Data.OptionalLong("owner_id"), value => Object.Detail("routine_type", value, r => new RoutineTypeDetails(Object, r)))!;
    public long Ordinal => Data.Long("ordinal");
    public ParameterType ParameterType => Object.Link(Data.OptionalLong("parameter_type_id"), value => Object.ParameterType(value))!;
    public SemanticKind Role => Object.Kind(Data.Long("role_id"));
    public BaseSubobject? BaseSubobject => Object.Link(Data.OptionalLong("base_subobject_id"), value => Object.Subobject(value));
}

public sealed class RecordTypeDetails : SemanticDetail
{
    internal RecordTypeDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public Scope? Scope => Object.Link(Data.OptionalLong("scope_id"), value => Object.Scope(value));
    public SemanticKind DefinitionState => Object.Kind(Data.Long("definition_state_id"));
    public TemplateInstantiation? Instance => Object.Link(Data.OptionalLong("instance_id"), value => Object.Instance(value));
    public CppType? SubobjectPartner => Object.Link(Data.OptionalLong("subobject_partner_id"), value => Object.Type(value));
    public bool IsLambda => Data.Bool("is_lambda");
    public bool IsEmpty => Data.Bool("is_empty");
    public bool IsAbstract => Data.Bool("is_abstract");
    public bool IsFinal => Data.Bool("is_final");
    public bool AnyConstMember => Data.Bool("any_const_member");
    public bool AnyVolatileMember => Data.Bool("any_volatile_member");
    public bool AnyMutableMember => Data.Bool("any_mutable_member");
    public bool AnyVirtualBaseClasses => Data.Bool("any_virtual_base_classes");
    public bool AnyVirtualFunctions => Data.Bool("any_virtual_functions");
    public bool AnyPureVirtualFunctions => Data.Bool("any_pure_virtual_functions");
    public bool AnyVirtualFunctionsIncludingBases => Data.Bool("any_virtual_functions_including_bases");
    public bool NestedClassDefinedOutsideParent => Data.Bool("nested_class_defined_outside_parent");
    public bool OriginallyUnnamed => Data.Bool("originally_unnamed");
    public bool IsNonstandardAnonymousUnion => Data.Bool("is_nonstandard_anonymous_union");
    public bool IsTemplateClass => Data.Bool("is_template_class");
    public bool IsNonrealClass => Data.Bool("is_nonreal_class");
    public bool IsMsInstantiatedNonrealClass => Data.Bool("is_ms_instantiated_nonreal_class");
    public bool IsPrototypeInstantiation => Data.Bool("is_prototype_instantiation");
    public bool IsSpecialized => Data.Bool("is_specialized");
    public bool SpecializedWithOldSyntax => Data.Bool("specialized_with_old_syntax");
    public bool IsInClassSpecialization => Data.Bool("is_in_class_specialization");
    public bool ExplicitlyInstantiated => Data.Bool("explicitly_instantiated");
    public bool DoNotInstantiate => Data.Bool("do_not_instantiate");
    public bool IsProxyClass => Data.Bool("is_proxy_class");
    public bool NoProperData => Data.Bool("no_proper_data");
    public bool HasZeroInitComponent => Data.Bool("has_zero_init_component");
    public bool HasPointerComponent => Data.Bool("has_pointer_component");
    public bool ContainsFlexibleArrayMember => Data.Bool("contains_flexible_array_member");
    public bool IsTransparent => Data.Bool("is_transparent");
    public bool IsPacked => Data.Bool("is_packed");
    public bool HasInternalLinkageAttribute => Data.Bool("has_internal_linkage_attribute");
    public bool HasOperatorAmpersand => Data.Bool("has_operator_ampersand");
    public bool CopyAssignmentDeclarationSuppressed => Data.Bool("copy_assignment_declaration_suppressed");
    public bool CopyConstructorDeclarationSuppressed => Data.Bool("copy_constructor_declaration_suppressed");
    public bool DefaultConstructorDeclarationSuppressed => Data.Bool("default_constructor_declaration_suppressed");
    public bool DestructorDeclarationSuppressed => Data.Bool("destructor_declaration_suppressed");
    public BigInteger? MaximumMemberAlignment => Number(Data.OptionalText("maximum_member_alignment"));
    public bool IsInterface => Data.Bool("is_interface");
    public bool IsInterfaceLike => Data.Bool("is_interface_like");
    public bool IsSealed => Data.Bool("is_sealed");
    public bool? CompilerGenerated => Data.OptionalBool("compiler_generated");
    public bool? HasSubobjectType => Data.OptionalBool("has_subobject_type");
    public bool IsGenericLambdaClosureClass => Data.Bool("is_generic_lambda_closure_class");
    public bool HasLambdaConversionFunction => Data.Bool("has_lambda_conversion_function");
    public bool IsInitializerList => Data.Bool("is_initializer_list");
    public bool HasInitializerListCtor => Data.Bool("has_initializer_list_ctor");
    public bool HasAnonymousUnionMember => Data.Bool("has_anonymous_union_member");
    public bool DefinedInVariableInitializer => Data.Bool("defined_in_variable_initializer");
    public bool DefinedInFieldInitializer => Data.Bool("defined_in_field_initializer");
    public bool NamedInInlineTemplateDirective => Data.Bool("named_in_inline_template_directive");
    public bool IsVaListTag => Data.Bool("is_va_list_tag");
    public bool DefinedInParentClass => Data.Bool("defined_in_parent_class");
    public bool HasNodiscardAttribute => Data.Bool("has_nodiscard_attribute");
    public bool HasFieldInitializer => Data.Bool("has_field_initializer");
    public bool? ContainsError => Data.OptionalBool("contains_error");
    public bool? ContainsLocalType => Data.OptionalBool("contains_local_type");
    public bool? ContainsUnnamedNamespaceType => Data.OptionalBool("contains_unnamed_namespace_type");
    public bool? DoesNotContainParentlessLambdaInDefaultArgument => Data.OptionalBool("does_not_contain_parentless_lambda_in_default_argument");
    public bool? DoesNotContainDeprecatedOrUnavailableType => Data.OptionalBool("does_not_contain_deprecated_or_unavailable_type");
    public long? InheritanceKindIsExplicit => Data.OptionalLong("inheritance_kind_is_explicit");
    public bool? HasCoclassAttribute => Data.OptionalBool("has_coclass_attribute");
    public bool? HasExplicitlyAlignedSubobject => Data.OptionalBool("has_explicitly_aligned_subobject");
    public bool? DefinitionNeeded => Data.OptionalBool("definition_needed");
    public BaseSubobject? PrimaryBaseSubobject => Object.Link(Data.OptionalLong("primary_base_subobject_id"), value => Object.Subobject(value));
    public BaseSubobject? VirtualFunctionInfoBaseSubobject => Object.Link(Data.OptionalLong("virtual_function_info_base_subobject_id"), value => Object.Subobject(value));
    public BigInteger? VirtualFunctionInfoOffset => Number(Data.OptionalText("virtual_function_info_offset"));
    public BigInteger? HighestVirtualFunctionNumber => Number(Data.OptionalText("highest_virtual_function_number"));
    public BigInteger? NextNegativeVirtualTableIndex => Number(Data.OptionalText("next_negative_virtual_table_index"));
    public BigInteger? FirstVcallOffsetIndex => Number(Data.OptionalText("first_vcall_offset_index"));
    public VariableSymbol? VirtualFunctionTableVariable => Object.Link(Data.OptionalLong("virtual_function_table_variable_id"), value => Object.Variable(value));
    public VariableSymbol? VirtualTableTableVariable => Object.Link(Data.OptionalLong("virtual_table_table_variable_id"), value => Object.Variable(value));
    public VariableSymbol? UuidVariable => Object.Link(Data.OptionalLong("uuid_variable_id"), value => Object.Variable(value));
    public string? UuidString => Object.Text(Data.OptionalLong("uuid_string_id"));
    public SemanticKind? OriginalTagKind => Object.Link(Data.OptionalLong("original_tag_kind_id"), value => Object.Kind(value));
    public SemanticKind? InheritanceKind => Object.Link(Data.OptionalLong("inheritance_kind_id"), value => Object.Kind(value));
    public SemanticKind? ElfVisibility => Object.Link(Data.OptionalLong("elf_visibility_id"), value => Object.Kind(value));
    public SemanticKind? AnonymousUnionKind => Object.Link(Data.OptionalLong("anonymous_union_kind_id"), value => Object.Kind(value));
    public FieldSymbol? AnonymousUnionField => Object.Link(Data.OptionalLong("anonymous_union_field_id"), value => Object.Field(value));
    public FunctionSymbol? AllocationRoutine => Object.Link(Data.OptionalLong("allocation_routine_id"), value => Object.Function(value));
    public FunctionSymbol? DeallocationRoutine => Object.Link(Data.OptionalLong("deallocation_routine_id"), value => Object.Function(value));
    public long MinimumTemplateArguments => Data.Long("minimum_template_arguments");
    public SemanticEntity? LambdaParentEntity => Object.Link(Data.OptionalLong("lambda_parent_entity_id"), value => Object.Entity(value));
    public CppType? ProxyOfType => Object.Link(Data.OptionalLong("proxy_of_type_id"), value => Object.Type(value));
    public bool? IsNear => Data.OptionalBool("is_near");
    public bool? IsFar => Data.OptionalBool("is_far");
    public IReadOnlyList<RecordBasePostorderDetails> RecordBasePostorderRows => Object.List("SELECT * FROM record_base_postorder WHERE owner_id=$owner ORDER BY ordinal", Data.Long("id"), r => new RecordBasePostorderDetails(Object, r));
    public IReadOnlyList<RecordBasePreorderDetails> RecordBasePreorderRows => Object.List("SELECT * FROM record_base_preorder WHERE owner_id=$owner ORDER BY ordinal", Data.Long("id"), r => new RecordBasePreorderDetails(Object, r));
    public IReadOnlyList<RecordBefriendingClassDetails> RecordBefriendingClassRows => Object.List("SELECT * FROM record_befriending_class WHERE owner_id=$owner ORDER BY ordinal", Data.Long("id"), r => new RecordBefriendingClassDetails(Object, r));
    public IReadOnlyList<RecordFriendDetails> RecordFriendRows => Object.List("SELECT * FROM record_friend WHERE owner_id=$owner ORDER BY ordinal", Data.Long("id"), r => new RecordFriendDetails(Object, r));
    public IReadOnlyList<RecordPromotedTypeDetails> RecordPromotedTypeRows => Object.List("SELECT * FROM record_promoted_type WHERE owner_id=$owner ORDER BY ordinal", Data.Long("id"), r => new RecordPromotedTypeDetails(Object, r));
    public IReadOnlyList<RecordEventInterfaceDetails> RecordEventInterfaceRows => Object.List("SELECT * FROM record_event_interface WHERE owner_id=$owner ORDER BY ordinal", Data.Long("id"), r => new RecordEventInterfaceDetails(Object, r));
    public IReadOnlyList<RecordVcallOffsetDetails> RecordVcallOffsetRows => Object.List("SELECT * FROM record_vcall_offset WHERE owner_id=$owner ORDER BY ordinal", Data.Long("id"), r => new RecordVcallOffsetDetails(Object, r));
}

public sealed class TypeReferenceDetails : SemanticDetail
{
    internal TypeReferenceDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public CppType UnderlyingType => Object.Link(Data.OptionalLong("underlying_type_id"), value => Object.Type(value))!;
    public SemanticKind ReferenceKind => Object.Kind(Data.Long("reference_kind_id"));
    public ExpressionOperation? DecltypeExpression => Object.Link(Data.OptionalLong("decltype_expression_id"), value => Object.Expression(value));
    public TemplateInstantiation? Instance => Object.Link(Data.OptionalLong("instance_id"), value => Object.Instance(value));
    public bool IsPredeclared => Data.Bool("is_predeclared");
    public bool HasVariablyModifiedType => Data.Bool("has_variably_modified_type");
    public bool IsLoweredVariablyModifiedType => Data.Bool("is_lowered_variably_modified_type");
    public bool DecltypeExpressionNotParenthesized => Data.Bool("decltype_expression_not_parenthesized");
    public bool IsDependentTypeOperator => Data.Bool("is_dependent_type_operator");
    public bool IsNonreal => Data.Bool("is_nonreal");
    public bool IsDependent => Data.Bool("is_dependent");
    public bool IsPrototypeInstantiation => Data.Bool("is_prototype_instantiation");
    public bool IsLoweredComplexType => Data.Bool("is_lowered_complex_type");
    public bool EmbedsSourceEntries => Data.Bool("embeds_source_entries");
    public bool AddedToRecordName => Data.Bool("added_to_record_name");
    public bool HasTypenamePrefix => Data.Bool("has_typename_prefix");
    public bool IsGlobalQualifiedName => Data.Bool("is_global_qualified_name");
    public bool IsIntrinsicMember => Data.Bool("is_intrinsic_member");
    public bool ExplicitMemoryAttributeMadeImplicit => Data.Bool("explicit_memory_attribute_made_implicit");
    public NameQualifier? NameQualifier => Object.Link(Data.OptionalLong("name_qualifier_id"), value => Object.NameQualifier(value));
    public CppType? ProxyClass => Object.Link(Data.OptionalLong("proxy_class_id"), value => Object.Type(value));
    public CppType? OperatorTypeArgument => Object.Link(Data.OptionalLong("operator_type_argument_id"), value => Object.Type(value));
    public long? MinimumTemplateArguments => Data.OptionalLong("minimum_template_arguments");
    public SourceRange? TypeIdRange => Object.Link(Data.OptionalLong("type_id_range_id"), value => Object.Range(value));
    public IReadOnlyList<TyperefOriginalTemplateArgumentDetails> TyperefOriginalTemplateArgumentRows => Object.List("SELECT * FROM typeref_original_template_argument WHERE owner_id=$owner ORDER BY ordinal", Data.Long("id"), r => new TyperefOriginalTemplateArgumentDetails(Object, r));
}

public sealed class VectorTypeDetails : SemanticDetail
{
    internal VectorTypeDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public CppType ElementType => Object.Link(Data.OptionalLong("element_type_id"), value => Object.Type(value))!;
    public SemanticKind VectorKind => Object.Kind(Data.Long("vector_kind_id"));
    public Constant? SizeConstant => Object.Link(Data.OptionalLong("size_constant_id"), value => Object.Constant(value));
    public bool IsBoolean => Data.Bool("is_boolean");
    public long? TupleElements => Data.OptionalLong("tuple_elements");
    public long? LengthMultiplier => Data.OptionalLong("length_multiplier");
    public SemanticKind? EdgVectorKind => Object.Link(Data.OptionalLong("edg_vector_kind_id"), value => Object.Kind(value));
}

public sealed class DependentTypeDetails : SemanticDetail
{
    internal DependentTypeDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public SemanticKind? ParameterKind => Object.Link(Data.OptionalLong("parameter_kind_id"), value => Object.Kind(value));
    public long? ParameterNumber => Data.OptionalLong("parameter_number");
    public long? ParameterDepth => Data.OptionalLong("parameter_depth");
    public CppType? ParentType => Object.Link(Data.OptionalLong("parent_type_id"), value => Object.Type(value));
    public ExpressionOperation? Constraint => Object.Link(Data.OptionalLong("constraint_id"), value => Object.Expression(value));
    public Constant? BitWidthConstant => Object.Link(Data.OptionalLong("bit_width_constant_id"), value => Object.Constant(value));
    public bool IsPack => Data.Bool("is_pack");
    public bool IsAutoParameter => Data.Bool("is_auto_parameter");
    public bool IsDecltypeAuto => Data.Bool("is_decltype_auto");
    public bool OriginallyClassTemplateParameter => Data.Bool("originally_class_template_parameter");
    public bool IsUnsignedBitPreciseInteger => Data.Bool("is_unsigned_bit_precise_integer");
    public CppType? ProxyClass => Object.Link(Data.OptionalLong("proxy_class_id"), value => Object.Type(value));
}

public sealed class ManagedTypeDetails : SemanticDetail
{
    internal ManagedTypeDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public SemanticKind ManagedKind => Object.Kind(Data.Long("managed_kind_id"));
    public CppType? UnderlyingType => Object.Link(Data.OptionalLong("underlying_type_id"), value => Object.Type(value));
    public long? Rank => Data.OptionalLong("rank");
    public string? AssemblyName => Object.Text(Data.OptionalLong("assembly_name_id"));
}

public sealed class RecordLayoutDetails : SemanticDetail
{
    internal RecordLayoutDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public CppType Type => Object.Link(Data.OptionalLong("type_id"), value => Object.Type(value))!;
    public BigInteger? NonvirtualSize => Number(Data.OptionalText("nonvirtual_size"));
    public BigInteger? NonvirtualAlignment => Number(Data.OptionalText("nonvirtual_alignment"));
    public bool IsComplete => Data.Bool("is_complete");
    public IReadOnlyList<FieldLayoutDetails> FieldLayoutRows => Object.List("SELECT * FROM field_layout WHERE owner_id=$owner ORDER BY ordinal", Data.Long("id"), r => new FieldLayoutDetails(Object, r));
}

public sealed class FieldLayoutDetails : SemanticDetail
{
    internal FieldLayoutDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public RecordLayout Owner => Object.Link(Data.OptionalLong("owner_id"), value => Object.Layout(value))!;
    public long Ordinal => Data.Long("ordinal");
    public FieldSymbol Field => Object.Link(Data.OptionalLong("field_id"), value => Object.Field(value))!;
    public BigInteger? Offset => Number(Data.OptionalText("offset"));
    public BigInteger? BitOffset => Number(Data.OptionalText("bit_offset"));
    public BigInteger? BitWidth => Number(Data.OptionalText("bit_width"));
    public BigInteger? ContainerOffset => Number(Data.OptionalText("container_offset"));
}

public sealed class DirectBaseDetails : SemanticDetail
{
    internal DirectBaseDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public CppType Owner => Object.Link(Data.OptionalLong("owner_id"), value => Object.Type(value))!;
    public long Ordinal => Data.Long("ordinal");
    public CppType BaseType => Object.Link(Data.OptionalLong("base_type_id"), value => Object.Type(value))!;
    public SemanticKind? Access => Object.Link(Data.OptionalLong("access_id"), value => Object.Kind(value));
    public bool IsVirtual => Data.Bool("is_virtual");
    public BaseSubobject? Subobject => Object.Link(Data.OptionalLong("subobject_id"), value => Object.Subobject(value));
}

public sealed class BaseSubobjectDetails : SemanticDetail
{
    internal BaseSubobjectDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public CppType MostDerivedType => Object.Link(Data.OptionalLong("most_derived_type_id"), value => Object.Type(value))!;
    public CppType BaseType => Object.Link(Data.OptionalLong("base_type_id"), value => Object.Type(value))!;
    public BigInteger? Offset => Number(Data.OptionalText("offset"));
    public bool IsVirtual => Data.Bool("is_virtual");
    public bool IsDirect => Data.Bool("is_direct");
    public bool Ambiguous => Data.Bool("ambiguous");
    public bool SharesVirtualFunctionInfo => Data.Bool("shares_virtual_function_info");
    public bool IgnoreDuringDependentLookup => Data.Bool("ignore_during_dependent_lookup");
    public bool IsOptimizedEmptyBase => Data.Bool("is_optimized_empty_base");
    public bool IsPackExpansion => Data.Bool("is_pack_expansion");
    public bool HasPublicDerivation => Data.Bool("has_public_derivation");
    public bool? CompleteSubobject => Data.OptionalBool("complete_subobject");
    public CppType? OriginalType => Object.Link(Data.OptionalLong("original_type_id"), value => Object.Type(value));
    public long DirectBaseNumber => Data.Long("direct_base_number");
    public BaseSubobject? PrimaryBaseSubobject => Object.Link(Data.OptionalLong("primary_base_subobject_id"), value => Object.Subobject(value));
    public BaseSubobject? DataSectionBaseSubobject => Object.Link(Data.OptionalLong("data_section_base_subobject_id"), value => Object.Subobject(value));
    public BigInteger? VirtualBasePointerOffset => Number(Data.OptionalText("virtual_base_pointer_offset"));
    public BaseSubobject? VirtualBasePointerBaseSubobject => Object.Link(Data.OptionalLong("virtual_base_pointer_base_subobject_id"), value => Object.Subobject(value));
    public VariableSymbol? VirtualFunctionTableVariable => Object.Link(Data.OptionalLong("virtual_function_table_variable_id"), value => Object.Variable(value));
    public BigInteger? VirtualFunctionTableIndex => Number(Data.OptionalText("virtual_function_table_index"));
    public BigInteger? ConstructionTableIndex => Number(Data.OptionalText("construction_table_index"));
    public BigInteger? ConstructionSubarrayIndex => Number(Data.OptionalText("construction_subarray_index"));
    public BigInteger? VirtualBaseOffsetIndex => Number(Data.OptionalText("virtual_base_offset_index"));
    public IReadOnlyList<RoutineTypeParameterDetails> RoutineTypeParameterRows => Object.List("SELECT * FROM routine_type_parameter WHERE base_subobject_id=$owner ORDER BY role_id,ordinal", Data.Long("id"), r => new RoutineTypeParameterDetails(Object, r));
    public IReadOnlyList<BaseDerivationDetails> BaseDerivationRows => Object.List("SELECT * FROM base_derivation WHERE base_subobject_id=$owner ORDER BY ordinal,id", Data.Long("id"), r => new BaseDerivationDetails(Object, r), "base_derivation");
    public IReadOnlyList<SubobjectPathDetails> SubobjectPathRows => Object.List("SELECT * FROM subobject_path WHERE base_subobject_id=$owner ORDER BY id", Data.Long("id"), r => new SubobjectPathDetails(Object, r), "subobject_path");
    public IReadOnlyList<ConstructorInitializerDetails> ConstructorInitializerRows => Object.List("SELECT * FROM constructor_initializer WHERE base_subobject_id=$owner ORDER BY id", Data.Long("id"), r => new ConstructorInitializerDetails(Object, r), "constructor_initializer");
    public IReadOnlyList<RecordVcallOffsetDetails> RecordVcallOffsetRows => Object.List("SELECT * FROM record_vcall_offset WHERE base_subobject_id=$owner ORDER BY ordinal", Data.Long("id"), r => new RecordVcallOffsetDetails(Object, r));
}

public sealed class BaseDerivationDetails : SemanticDetail
{
    internal BaseDerivationDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public BaseSubobject BaseSubobject => Object.Link(Data.OptionalLong("base_subobject_id"), value => Object.Subobject(value))!;
    public long Ordinal => Data.Long("ordinal");
    public bool IsDirect => Data.Bool("is_direct");
    public bool IsPreferred => Data.Bool("is_preferred");
    public SemanticKind? Access => Object.Link(Data.OptionalLong("access_id"), value => Object.Kind(value));
    public long PathTailOrdinal => Data.Long("path_tail_ordinal");
    public IReadOnlyList<BaseDerivationStepDetails> BaseDerivationStepRows => Object.List("SELECT * FROM base_derivation_step WHERE owner_id=$owner ORDER BY ordinal", Data.Long("id"), r => new BaseDerivationStepDetails(Object, r));
}

public sealed class BaseDerivationStepDetails : SemanticDetail
{
    internal BaseDerivationStepDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public BaseDerivation Owner => Object.Link(Data.OptionalLong("owner_id"), value => Object.Derivation(value))!;
    public long Ordinal => Data.Long("ordinal");
    public BaseSubobject Subobject => Object.Link(Data.OptionalLong("subobject_id"), value => Object.Subobject(value))!;
}

public sealed class OverrideRelationDetails : SemanticDetail
{
    internal OverrideRelationDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public FunctionSymbol Owner => Object.Link(Data.OptionalLong("owner_id"), value => Object.Function(value))!;
    public long Ordinal => Data.Long("ordinal");
    public FunctionSymbol Overridden => Object.Link(Data.OptionalLong("overridden_id"), value => Object.Function(value))!;
    public BaseSubobject? Subobject => Object.Link(Data.OptionalLong("subobject_id"), value => Object.Subobject(value));
    public BaseSubobject? OverridingSubobject => Object.Link(Data.OptionalLong("overriding_subobject_id"), value => Object.Subobject(value));
    public BaseSubobject? ReturnAdjustmentSubobject => Object.Link(Data.OptionalLong("return_adjustment_subobject_id"), value => Object.Subobject(value));
    public long? BaseOrdinal => Data.OptionalLong("base_ordinal");
}

public sealed class SemanticNodeDetails : SemanticDetail
{
    internal SemanticNodeDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public SemanticKind Kind => Object.Kind(Data.Long("kind_id"));
    public SourceInfo? SourceOrigin => Object.Link(Data.OptionalLong("source_origin_id"), value => Object.Source(value));
    public bool CompilerGenerated => Data.Bool("compiler_generated");
    public SemanticKind RepresentationRole => Object.Kind(Data.Long("representation_role_id"));
    public IReadOnlyList<StructuralChildDetails> StructuralChildRows => Object.List("SELECT * FROM structural_child WHERE owner_id=$owner ORDER BY role_id,ordinal", Data.Long("id"), r => new StructuralChildDetails(Object, r));
}

public sealed class StructuralChildDetails : SemanticDetail
{
    internal StructuralChildDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public SemanticNode Owner => Object.Link(Data.OptionalLong("owner_id"), value => Object.Node(value))!;
    public SemanticKind Role => Object.Kind(Data.Long("role_id"));
    public long Ordinal => Data.Long("ordinal");
    public SemanticNode Child => Object.Link(Data.OptionalLong("child_id"), value => Object.Node(value))!;
}

public sealed class SemanticReferenceDetails : SemanticDetail
{
    internal SemanticReferenceDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public SemanticEntity Owner => Object.Link(Data.OptionalLong("owner_id"), value => Object.Entity(value))!;
    public SemanticKind Role => Object.Kind(Data.Long("role_id"));
    public long Ordinal => Data.Long("ordinal");
    public SemanticEntity Target => Object.Link(Data.OptionalLong("target_id"), value => Object.Entity(value))!;
}

public sealed class ExpressionDetails : SemanticDetail
{
    internal ExpressionDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public CppType Type => Object.Link(Data.OptionalLong("type_id"), value => Object.Type(value))!;
    public SemanticKind ValueCategory => Object.Kind(Data.Long("value_category_id"));
    public CppType? OriginalLvalueType => Object.Link(Data.OptionalLong("original_lvalue_type_id"), value => Object.Type(value));
    public bool ResultUnused => Data.Bool("result_unused");
    public bool InitializationGuard => Data.Bool("initialization_guard");
    public bool GeneratedDefaultArgument => Data.Bool("generated_default_argument");
    public bool VolatileFetch => Data.Bool("volatile_fetch");
    public bool IsPackExpansion => Data.Bool("is_pack_expansion");
    public bool IsParenthesized => Data.Bool("is_parenthesized");
    public bool IsStaticCast => Data.Bool("is_static_cast");
    public bool IsFunctionalCast => Data.Bool("is_functional_cast");
    public bool IsBraceCast => Data.Bool("is_brace_cast");
    public bool IsSafeCast => Data.Bool("is_safe_cast");
    public bool IsCliTypeid => Data.Bool("is_cli_typeid");
    public bool IsCliParamArrayElement => Data.Bool("is_cli_param_array_element");
    public bool IsObjectlessMemberReference => Data.Bool("is_objectless_member_reference");
    public bool IsGnuExtension => Data.Bool("is_gnu_extension");
    public bool TypeDefinitionNeeded => Data.Bool("type_definition_needed");
    public bool? BooleanNotNormalized => Data.OptionalBool("boolean_not_normalized");
    public bool IsTypeConstraint => Data.Bool("is_type_constraint");
    public bool WasLvalueTempInitializer => Data.Bool("was_lvalue_temp_initializer");
    public IReadOnlyList<ExpressionOperandDetails> ExpressionOperandRows => Object.List("SELECT * FROM expression_operand WHERE owner_id=$owner ORDER BY role_id,ordinal", Data.Long("id"), r => new ExpressionOperandDetails(Object, r));
    public IReadOnlyList<ExpressionTemplateArgumentDetails> ExpressionTemplateArgumentRows => Object.List("SELECT * FROM expression_template_argument WHERE owner_id=$owner ORDER BY ordinal", Data.Long("id"), r => new ExpressionTemplateArgumentDetails(Object, r));
    public IReadOnlyList<ExpressionVlaDimensionDetails> ExpressionVlaDimensionRows => Object.List("SELECT * FROM expression_vla_dimension WHERE owner_id=$owner ORDER BY ordinal", Data.Long("id"), r => new ExpressionVlaDimensionDetails(Object, r));
}

public sealed class StatementDetails : SemanticDetail
{
    internal StatementDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public SemanticKind Kind => Object.Kind(Data.Long("kind_id"));
    public ExpressionOperation? Expression => Object.Link(Data.OptionalLong("expression_id"), value => Object.Expression(value));
    public StatementOperation? Parent => Object.Link(Data.OptionalLong("parent_id"), value => Object.Statement(value));
    public bool IsGuard => Data.Bool("is_guard");
    public bool? LoweringGenerated => Data.OptionalBool("lowering_generated");
    public bool HasAssociatedPragma => Data.Bool("has_associated_pragma");
    public bool? IsLoweringBoilerplate => Data.OptionalBool("is_lowering_boilerplate");
    public bool IsFallthroughStatement => Data.Bool("is_fallthrough_statement");
    public bool IsLikely => Data.Bool("is_likely");
    public bool IsUnlikely => Data.Bool("is_unlikely");
    public SourceEntry? SourceSequenceEntry => Object.Link(Data.OptionalLong("source_sequence_entry_id"), value => Object.SourceEntry(value));
}

public sealed class ExpressionOperandDetails : SemanticDetail
{
    internal ExpressionOperandDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public ExpressionOperation Owner => Object.Link(Data.OptionalLong("owner_id"), value => Object.Expression(value))!;
    public SemanticKind Role => Object.Kind(Data.Long("role_id"));
    public long Ordinal => Data.Long("ordinal");
    public ExpressionOperation Expression => Object.Link(Data.OptionalLong("expression_id"), value => Object.Expression(value))!;
}

public sealed class OperatorExpressionDetails : SemanticDetail
{
    internal OperatorExpressionDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public SemanticKind OperatorKind => Object.Kind(Data.Long("operator_kind_id"));
    public SemanticKind? OperationTypeKind => Object.Link(Data.OptionalLong("operation_type_kind_id"), value => Object.Kind(value));
    public bool EvalLeftToRight => Data.Bool("eval_left_to_right");
    public bool EvalRightToLeft => Data.Bool("eval_right_to_left");
    public bool PointerOperandIsSecond => Data.Bool("pointer_operand_is_second");
    public bool IsReinterpretLikeCast => Data.Bool("is_reinterpret_like_cast");
    public bool RequiresRuntimeCastCheck => Data.Bool("requires_runtime_cast_check");
    public bool ReturnsLvalue => Data.Bool("returns_lvalue");
    public bool IsReinterpretCast => Data.Bool("is_reinterpret_cast");
    public bool IsReferenceCast => Data.Bool("is_reference_cast");
    public bool IsRvalueReferenceCast => Data.Bool("is_rvalue_reference_cast");
    public bool IsConstCast => Data.Bool("is_const_cast");
    public bool IsTrackingReferenceCast => Data.Bool("is_tracking_reference_cast");
    public bool IsGnuTwoOperandConditional => Data.Bool("is_gnu_two_operand_conditional");
    public bool CallUsesOperatorSyntax => Data.Bool("call_uses_operator_syntax");
    public bool ImplicitMemberNaming => Data.Bool("implicit_member_naming");
    public bool ImplicitExplicitCastStep => Data.Bool("implicit_explicit_cast_step");
    public bool IsConversionCall => Data.Bool("is_conversion_call");
    public bool ArgumentDependentLookupSuppressed => Data.Bool("argument_dependent_lookup_suppressed");
    public bool QualifiedFunctionName => Data.Bool("qualified_function_name");
    public bool RetainedVirtualCall => Data.Bool("retained_virtual_call");
    public SemanticKind? RewrittenPropertyKind => Object.Link(Data.OptionalLong("rewritten_property_kind_id"), value => Object.Kind(value));
}

public sealed class CallExpressionDetails : SemanticDetail
{
    internal CallExpressionDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public ExpressionOperation Callee => Object.Link(Data.OptionalLong("callee_id"), value => Object.Expression(value))!;
    public FunctionSymbol? TargetRoutine => Object.Link(Data.OptionalLong("target_routine_id"), value => Object.Function(value));
    public FunctionSymbol? SourceTargetRoutine => Object.Link(Data.OptionalLong("source_target_routine_id"), value => Object.Function(value));
    public ExpressionOperation? Receiver => Object.Link(Data.OptionalLong("receiver_id"), value => Object.Expression(value));
    public CppType? FunctionType => Object.Link(Data.OptionalLong("function_type_id"), value => Object.Type(value));
    public SemanticKind CallKind => Object.Kind(Data.Long("call_kind_id"));
    public SemanticKind? SourceDispatchKind => Object.Link(Data.OptionalLong("source_dispatch_kind_id"), value => Object.Kind(value));
    public ExpressionOperation? ReturnSlot => Object.Link(Data.OptionalLong("return_slot_id"), value => Object.Expression(value));
    public IReadOnlyList<CallArgumentDetails> CallArgumentRows => Object.List("SELECT * FROM call_argument WHERE owner_id=$owner ORDER BY role_id,ordinal", Data.Long("id"), r => new CallArgumentDetails(Object, r));
}

public sealed class CallArgumentDetails : SemanticDetail
{
    internal CallArgumentDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public CallExpressionDetails Owner => Object.Link(Data.OptionalLong("owner_id"), value => Object.Detail("call_expression", value, r => new CallExpressionDetails(Object, r)))!;
    public long Ordinal => Data.Long("ordinal");
    public ExpressionOperation Expression => Object.Link(Data.OptionalLong("expression_id"), value => Object.Expression(value))!;
    public ParameterType? ParameterType => Object.Link(Data.OptionalLong("parameter_type_id"), value => Object.ParameterType(value));
    public SemanticKind? Role => Object.Link(Data.OptionalLong("role_id"), value => Object.Kind(value));
}

public sealed class ReferenceExpressionDetails : SemanticDetail
{
    internal ReferenceExpressionDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public SemanticEntity? Target => Object.Link(Data.OptionalLong("target_id"), value => Object.Entity(value));
    public SemanticKind ReferenceKind => Object.Kind(Data.Long("reference_kind_id"));
    public NameReference? NameReference => Object.Link(Data.OptionalLong("name_reference_id"), value => Object.NameReference(value));
}

public sealed class NameReferenceDetails : SemanticDetail
{
    internal NameReferenceDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public SemanticKind? Kind => Object.Link(Data.OptionalLong("kind_id"), value => Object.Kind(value));
    public FieldSymbol? Property => Object.Link(Data.OptionalLong("property_id"), value => Object.Field(value));
    public CppType? DestructorType => Object.Link(Data.OptionalLong("destructor_type_id"), value => Object.Type(value));
    public NameQualifier? Qualifier => Object.Link(Data.OptionalLong("qualifier_id"), value => Object.NameQualifier(value));
    public BigInteger? NumTemplateArguments => Number(Data.OptionalText("num_template_arguments"));
    public bool IsGlobalQualifiedName => Data.Bool("is_global_qualified_name");
    public bool IsTemplateId => Data.Bool("is_template_id");
    public bool IsSuperQualified => Data.Bool("is_super_qualified");
    public bool IsDecltypeQualified => Data.Bool("is_decltype_qualified");
    public bool FromPrototypeInstantiation => Data.Bool("from_prototype_instantiation");
    public bool UsedInPrimaryDeclarator => Data.Bool("used_in_primary_declarator");
    public IReadOnlyList<OriginalTemplateArgumentDetails> OriginalTemplateArgumentRows => Object.List("SELECT * FROM original_template_argument WHERE owner_id=$owner ORDER BY ordinal", Data.Long("id"), r => new OriginalTemplateArgumentDetails(Object, r));
}

public sealed class NameQualifierDetails : SemanticDetail
{
    internal NameQualifierDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public NameQualifier? PreviousQualifier => Object.Link(Data.OptionalLong("previous_qualifier_id"), value => Object.NameQualifier(value));
    public SemanticEntity? Target => Object.Link(Data.OptionalLong("target_id"), value => Object.Entity(value));
    public string? Name => Object.Text(Data.OptionalLong("name_id"));
    public bool IsClass => Data.Bool("is_class");
}

public sealed class OriginalTemplateArgumentDetails : SemanticDetail
{
    internal OriginalTemplateArgumentDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public NameReference Owner => Object.Link(Data.OptionalLong("owner_id"), value => Object.NameReference(value))!;
    public long Ordinal => Data.Long("ordinal");
    public TemplateArgument Argument => Object.Link(Data.OptionalLong("argument_id"), value => Object.Argument(value))!;
}

public sealed class ConstantExpressionDetails : SemanticDetail
{
    internal ConstantExpressionDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public Constant Constant => Object.Link(Data.OptionalLong("constant_id"), value => Object.Constant(value))!;
    public NameReference? NameReference => Object.Link(Data.OptionalLong("name_reference_id"), value => Object.NameReference(value));
}

public sealed class BlockDescriptorDetails : SemanticDetail
{
    internal BlockDescriptorDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public Scope? Scope => Object.Link(Data.OptionalLong("scope_id"), value => Object.Scope(value));
    public Lifetime? Lifetime => Object.Link(Data.OptionalLong("lifetime_id"), value => Object.Lifetime(value));
    public SourcePosition? FinalPosition => Object.Link(Data.OptionalLong("final_position_id"), value => Object.Position(value));
    public bool EndReachable => Data.Bool("end_reachable");
    public bool IsStatementExpression => Data.Bool("is_statement_expression");
    public bool ImplicitScopeNotAllowed => Data.Bool("implicit_scope_not_allowed");
    public SemanticKind? UpcAccessMethod => Object.Link(Data.OptionalLong("upc_access_method_id"), value => Object.Kind(value));
}

public sealed class BlockStatementDetails : SemanticDetail
{
    internal BlockStatementDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public BlockDescriptor? Descriptor => Object.Link(Data.OptionalLong("descriptor_id"), value => Object.BlockDescriptor(value));
    public IReadOnlyList<BlockItemDetails> BlockItemRows => Object.List("SELECT * FROM block_item WHERE owner_id=$owner ORDER BY ordinal", Data.Long("id"), r => new BlockItemDetails(Object, r));
}

public sealed class DeclarationStatementDetails : SemanticDetail
{
    internal DeclarationStatementDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public bool HasStaticOrThreadVariable => Data.Bool("has_static_or_thread_variable");
    public IReadOnlyList<DeclarationStatementEntityDetails> DeclarationStatementEntityRows => Object.List("SELECT * FROM declaration_statement_entity WHERE owner_id=$owner ORDER BY ordinal", Data.Long("id"), r => new DeclarationStatementEntityDetails(Object, r));
}

public sealed class DeclarationStatementEntityDetails : SemanticDetail
{
    internal DeclarationStatementEntityDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public DeclarationStatementDetails Owner => Object.Link(Data.OptionalLong("owner_id"), value => Object.Detail("declaration_statement", value, r => new DeclarationStatementDetails(Object, r)))!;
    public long Ordinal => Data.Long("ordinal");
    public SemanticEntity Entity => Object.Link(Data.OptionalLong("entity_id"), value => Object.Entity(value))!;
}

public sealed class InitializationStatementDetails : SemanticDetail
{
    internal InitializationStatementDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public Initialization Initialization => Object.Link(Data.OptionalLong("initialization_id"), value => Object.Initialization(value))!;
}

public sealed class BlockItemDetails : SemanticDetail
{
    internal BlockItemDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public BlockStatementDetails Owner => Object.Link(Data.OptionalLong("owner_id"), value => Object.Detail("block_statement", value, r => new BlockStatementDetails(Object, r)))!;
    public long Ordinal => Data.Long("ordinal");
    public StatementOperation Statement => Object.Link(Data.OptionalLong("statement_id"), value => Object.Statement(value))!;
}

public sealed class IfStatementDetails : SemanticDetail
{
    internal IfStatementDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public StatementOperation? Then => Object.Link(Data.OptionalLong("then_id"), value => Object.Statement(value));
    public StatementOperation? Else => Object.Link(Data.OptionalLong("else_id"), value => Object.Statement(value));
    public ExpressionOperation? Condition => Object.Link(Data.OptionalLong("condition_id"), value => Object.Expression(value));
    public bool? SelectionKnown => Data.OptionalBool("selection_known");
    public bool? SelectedThen => Data.OptionalBool("selected_then");
}

public sealed class LoopStatementDetails : SemanticDetail
{
    internal LoopStatementDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public StatementOperation? Body => Object.Link(Data.OptionalLong("body_id"), value => Object.Statement(value));
    public StatementOperation? Initialization => Object.Link(Data.OptionalLong("initialization_id"), value => Object.Statement(value));
    public ExpressionOperation? Condition => Object.Link(Data.OptionalLong("condition_id"), value => Object.Expression(value));
    public ExpressionOperation? Increment => Object.Link(Data.OptionalLong("increment_id"), value => Object.Expression(value));
    public Scope? InitScope => Object.Link(Data.OptionalLong("init_scope_id"), value => Object.Scope(value));
    public ExpressionOperation? Affinity => Object.Link(Data.OptionalLong("affinity_id"), value => Object.Expression(value));
}

public sealed class RangeForDetails : SemanticDetail
{
    internal RangeForDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public StatementOperation? Body => Object.Link(Data.OptionalLong("body_id"), value => Object.Statement(value));
    public VariableSymbol? RangeVariable => Object.Link(Data.OptionalLong("range_variable_id"), value => Object.Variable(value));
    public VariableSymbol? BeginVariable => Object.Link(Data.OptionalLong("begin_variable_id"), value => Object.Variable(value));
    public VariableSymbol? EndVariable => Object.Link(Data.OptionalLong("end_variable_id"), value => Object.Variable(value));
    public VariableSymbol? IteratorVariable => Object.Link(Data.OptionalLong("iterator_variable_id"), value => Object.Variable(value));
    public ExpressionOperation? Comparison => Object.Link(Data.OptionalLong("comparison_id"), value => Object.Expression(value));
    public ExpressionOperation? Increment => Object.Link(Data.OptionalLong("increment_id"), value => Object.Expression(value));
    public StatementOperation? Initialization => Object.Link(Data.OptionalLong("initialization_id"), value => Object.Statement(value));
    public Scope? Scope => Object.Link(Data.OptionalLong("scope_id"), value => Object.Scope(value));
    public Scope? IteratorScope => Object.Link(Data.OptionalLong("iterator_scope_id"), value => Object.Scope(value));
    public bool UseAwait => Data.Bool("use_await");
}

public sealed class ForEachStatementDetails : SemanticDetail
{
    internal ForEachStatementDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public StatementOperation? Body => Object.Link(Data.OptionalLong("body_id"), value => Object.Statement(value));
    public ExpressionOperation? Collection => Object.Link(Data.OptionalLong("collection_id"), value => Object.Expression(value));
    public SemanticKind PatternKind => Object.Kind(Data.Long("pattern_kind_id"));
    public VariableSymbol? Iterator => Object.Link(Data.OptionalLong("iterator_id"), value => Object.Variable(value));
    public ExpressionOperation? Test => Object.Link(Data.OptionalLong("test_id"), value => Object.Expression(value));
    public ExpressionOperation? Increment => Object.Link(Data.OptionalLong("increment_id"), value => Object.Expression(value));
    public bool UsesPreviousDeclarationIterator => Data.Bool("uses_previous_declaration_iterator");
    public FieldSymbol? PreviousField => Object.Link(Data.OptionalLong("previous_field_id"), value => Object.Field(value));
    public ExpressionOperation? Assignment => Object.Link(Data.OptionalLong("assignment_id"), value => Object.Expression(value));
    public VariableSymbol? CollectionReference => Object.Link(Data.OptionalLong("collection_reference_id"), value => Object.Variable(value));
    public VariableSymbol? TemporaryVariable => Object.Link(Data.OptionalLong("temporary_variable_id"), value => Object.Variable(value));
    public VariableSymbol? EndVariable => Object.Link(Data.OptionalLong("end_variable_id"), value => Object.Variable(value));
    public Scope? Scope => Object.Link(Data.OptionalLong("scope_id"), value => Object.Scope(value));
    public Scope? IteratorScope => Object.Link(Data.OptionalLong("iterator_scope_id"), value => Object.Scope(value));
}

public sealed class SwitchStatementDetails : SemanticDetail
{
    internal SwitchStatementDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public StatementOperation? Body => Object.Link(Data.OptionalLong("body_id"), value => Object.Statement(value));
    public ExpressionOperation? Selector => Object.Link(Data.OptionalLong("selector_id"), value => Object.Expression(value));
    public SwitchDescriptor Descriptor => Object.Link(Data.OptionalLong("descriptor_id"), value => Object.SwitchDescriptor(value))!;
}

public sealed class SwitchDescriptorDetails : SemanticDetail
{
    internal SwitchDescriptorDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public SwitchCaseEntry? DefaultCase => Object.Link(Data.OptionalLong("default_case_id"), value => Object.SwitchCase(value));
    public bool HasSortedCases => Data.Bool("has_sorted_cases");
    public IReadOnlyList<SwitchDescriptorCaseDetails> SwitchDescriptorCaseRows => Object.List("SELECT * FROM switch_descriptor_case WHERE owner_id=$owner ORDER BY ordinal", Data.Long("id"), r => new SwitchDescriptorCaseDetails(Object, r));
    public IReadOnlyList<SwitchDescriptorSortedCaseDetails> SwitchDescriptorSortedCaseRows => Object.List("SELECT * FROM switch_descriptor_sorted_case WHERE owner_id=$owner ORDER BY ordinal", Data.Long("id"), r => new SwitchDescriptorSortedCaseDetails(Object, r));
}

public sealed class SwitchCaseEntryDetails : SemanticDetail
{
    internal SwitchCaseEntryDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public StatementOperation? Statement => Object.Link(Data.OptionalLong("statement_id"), value => Object.Statement(value));
    public Constant? LowConstant => Object.Link(Data.OptionalLong("low_constant_id"), value => Object.Constant(value));
    public Constant? HighConstant => Object.Link(Data.OptionalLong("high_constant_id"), value => Object.Constant(value));
    public bool ReachableByFallThrough => Data.Bool("reachable_by_fall_through");
    public SourceInfo? SourceOrigin => Object.Link(Data.OptionalLong("source_origin_id"), value => Object.Source(value));
}

public sealed class SwitchDescriptorCaseDetails : SemanticDetail
{
    internal SwitchDescriptorCaseDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public SwitchDescriptor Owner => Object.Link(Data.OptionalLong("owner_id"), value => Object.SwitchDescriptor(value))!;
    public long Ordinal => Data.Long("ordinal");
    public SwitchCaseEntry Case => Object.Link(Data.OptionalLong("case_id"), value => Object.SwitchCase(value))!;
}

public sealed class SwitchDescriptorSortedCaseDetails : SemanticDetail
{
    internal SwitchDescriptorSortedCaseDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public SwitchDescriptor Owner => Object.Link(Data.OptionalLong("owner_id"), value => Object.SwitchDescriptor(value))!;
    public long Ordinal => Data.Long("ordinal");
    public SwitchCaseEntry Case => Object.Link(Data.OptionalLong("case_id"), value => Object.SwitchCase(value))!;
}

public sealed class SwitchCaseStatementDetails : SemanticDetail
{
    internal SwitchCaseStatementDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public SwitchCaseEntry Entry => Object.Link(Data.OptionalLong("entry_id"), value => Object.SwitchCase(value))!;
    public StatementOperation SwitchStatement => Object.Link(Data.OptionalLong("switch_statement_id"), value => Object.Statement(value))!;
}

public sealed class JumpStatementDetails : SemanticDetail
{
    internal JumpStatementDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public SemanticKind Kind => Object.Kind(Data.Long("kind_id"));
    public ExpressionOperation? Value => Object.Link(Data.OptionalLong("value_id"), value => Object.Expression(value));
    public LabelSymbol? Label => Object.Link(Data.OptionalLong("label_id"), value => Object.Label(value));
    public Initialization? Initialization => Object.Link(Data.OptionalLong("initialization_id"), value => Object.Initialization(value));
    public Lifetime? CommonLifetime => Object.Link(Data.OptionalLong("common_lifetime_id"), value => Object.Lifetime(value));
}

public sealed class LabelStatementDetails : SemanticDetail
{
    internal LabelStatementDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public LabelSymbol Label => Object.Link(Data.OptionalLong("label_id"), value => Object.Label(value))!;
}

public sealed class TryDescriptorDetails : SemanticDetail
{
    internal TryDescriptorDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public StatementOperation Body => Object.Link(Data.OptionalLong("body_id"), value => Object.Statement(value))!;
    public bool IsFunctionTry => Data.Bool("is_function_try");
    public Lifetime? Lifetime => Object.Link(Data.OptionalLong("lifetime_id"), value => Object.Lifetime(value));
    public IReadOnlyList<TryHandlerDetails> TryHandlerRows => Object.List("SELECT * FROM try_handler WHERE owner_id=$owner ORDER BY ordinal", Data.Long("id"), r => new TryHandlerDetails(Object, r));
}

public sealed class TryStatementDetails : SemanticDetail
{
    internal TryStatementDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public TryDescriptor Descriptor => Object.Link(Data.OptionalLong("descriptor_id"), value => Object.TryDescriptor(value))!;
}

public sealed class TryHandlerDetails : SemanticDetail
{
    internal TryHandlerDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public TryDescriptor Owner => Object.Link(Data.OptionalLong("owner_id"), value => Object.TryDescriptor(value))!;
    public long Ordinal => Data.Long("ordinal");
    public ExceptionHandler Handler => Object.Link(Data.OptionalLong("handler_id"), value => Object.Handler(value))!;
}

public sealed class HandlerDetails : SemanticDetail
{
    internal HandlerDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public StatementOperation Body => Object.Link(Data.OptionalLong("body_id"), value => Object.Statement(value))!;
    public CppType? CaughtType => Object.Link(Data.OptionalLong("caught_type_id"), value => Object.Type(value));
    public VariableSymbol? Parameter => Object.Link(Data.OptionalLong("parameter_id"), value => Object.Variable(value));
    public Initialization? ParameterInitialization => Object.Link(Data.OptionalLong("parameter_initialization_id"), value => Object.Initialization(value));
    public VariableSymbol? Typeinfo => Object.Link(Data.OptionalLong("typeinfo_id"), value => Object.Variable(value));
}

public sealed class MicrosoftTryStatementDetails : SemanticDetail
{
    internal MicrosoftTryStatementDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public StatementOperation Body => Object.Link(Data.OptionalLong("body_id"), value => Object.Statement(value))!;
    public ExpressionOperation? Filter => Object.Link(Data.OptionalLong("filter_id"), value => Object.Expression(value));
    public StatementOperation? Handler => Object.Link(Data.OptionalLong("handler_id"), value => Object.Statement(value));
    public StatementOperation? Finally => Object.Link(Data.OptionalLong("finally_id"), value => Object.Statement(value));
    public SourcePosition? ExceptOrFinallyPosition => Object.Link(Data.OptionalLong("except_or_finally_position_id"), value => Object.Position(value));
    public bool IsFinally => Data.Bool("is_finally");
}

public sealed class ThrowExpressionDetails : SemanticDetail
{
    internal ThrowExpressionDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public ExpressionOperation? Operand => Object.Link(Data.OptionalLong("operand_id"), value => Object.Expression(value));
    public CppType? ThrownType => Object.Link(Data.OptionalLong("thrown_type_id"), value => Object.Type(value));
    public Initialization? Initialization => Object.Link(Data.OptionalLong("initialization_id"), value => Object.Initialization(value));
    public FunctionSymbol? Destructor => Object.Link(Data.OptionalLong("destructor_id"), value => Object.Function(value));
    public bool IsRethrow => Data.Bool("is_rethrow");
    public IReadOnlyList<ThrowAccessibleBaseDetails> ThrowAccessibleBaseRows => Object.List("SELECT * FROM throw_accessible_base WHERE owner_id=$owner ORDER BY ordinal", Data.Long("id"), r => new ThrowAccessibleBaseDetails(Object, r));
}

public sealed class ThrowAccessibleBaseDetails : SemanticDetail
{
    internal ThrowAccessibleBaseDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public ThrowExpressionDetails Owner => Object.Link(Data.OptionalLong("owner_id"), value => Object.Detail("throw_expression", value, r => new ThrowExpressionDetails(Object, r)))!;
    public long Ordinal => Data.Long("ordinal");
    public BaseSubobject Subobject => Object.Link(Data.OptionalLong("subobject_id"), value => Object.Subobject(value))!;
}

public sealed class TypeQueryExpressionDetails : SemanticDetail
{
    internal TypeQueryExpressionDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public SemanticKind QueryKind => Object.Kind(Data.Long("query_kind_id"));
    public CppType? OperandType => Object.Link(Data.OptionalLong("operand_type_id"), value => Object.Type(value));
    public ExpressionOperation? OperandExpression => Object.Link(Data.OptionalLong("operand_expression_id"), value => Object.Expression(value));
    public bool IsDynamic => Data.Bool("is_dynamic");
    public bool IsStandardAlignof => Data.Bool("is_standard_alignof");
}

public sealed class TemporaryExpressionDetails : SemanticDetail
{
    internal TemporaryExpressionDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public Initialization? Initialization => Object.Link(Data.OptionalLong("initialization_id"), value => Object.Initialization(value));
    public CppType? SourceType => Object.Link(Data.OptionalLong("source_type_id"), value => Object.Type(value));
}

public sealed class ConditionExpressionDetails : SemanticDetail
{
    internal ConditionExpressionDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public ExpressionOperation? Expression => Object.Link(Data.OptionalLong("expression_id"), value => Object.Expression(value));
    public StatementOperation? Initialization => Object.Link(Data.OptionalLong("initialization_id"), value => Object.Statement(value));
    public Initialization? DynamicInitialization => Object.Link(Data.OptionalLong("dynamic_initialization_id"), value => Object.Initialization(value));
    public Scope? Scope => Object.Link(Data.OptionalLong("scope_id"), value => Object.Scope(value));
    public VariableSymbol? Variable => Object.Link(Data.OptionalLong("variable_id"), value => Object.Variable(value));
}

public sealed class LifetimeExpressionDetails : SemanticDetail
{
    internal LifetimeExpressionDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public ExpressionOperation Expression => Object.Link(Data.OptionalLong("expression_id"), value => Object.Expression(value))!;
    public Lifetime? Lifetime => Object.Link(Data.OptionalLong("lifetime_id"), value => Object.Lifetime(value));
}

public sealed class StatementExpressionDetails : SemanticDetail
{
    internal StatementExpressionDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public StatementOperation Statement => Object.Link(Data.OptionalLong("statement_id"), value => Object.Statement(value))!;
}

public sealed class ReuseExpressionDetails : SemanticDetail
{
    internal ReuseExpressionDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public Initialization Initialization => Object.Link(Data.OptionalLong("initialization_id"), value => Object.Initialization(value))!;
}

public sealed class BuiltinExpressionDetails : SemanticDetail
{
    internal BuiltinExpressionDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public SemanticKind Kind => Object.Kind(Data.Long("kind_id"));
    public ExpressionOperation? SelectedExpression => Object.Link(Data.OptionalLong("selected_expression_id"), value => Object.Expression(value));
}

public sealed class ParameterReferenceExpressionDetails : SemanticDetail
{
    internal ParameterReferenceExpressionDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public long ParameterNumber => Data.Long("parameter_number");
    public long ParameterDepth => Data.Long("parameter_depth");
    public bool IsThis => Data.Bool("is_this");
}

public sealed class DependentExpressionDetails : SemanticDetail
{
    internal DependentExpressionDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public SemanticKind Kind => Object.Kind(Data.Long("kind_id"));
    public ExpressionOperation? WrappedExpression => Object.Link(Data.OptionalLong("wrapped_expression_id"), value => Object.Expression(value));
    public TemplateDescriptor? Template => Object.Link(Data.OptionalLong("template_id"), value => Object.Template(value));
    public Initialization? Initialization => Object.Link(Data.OptionalLong("initialization_id"), value => Object.Initialization(value));
    public bool? NoexceptRequired => Data.OptionalBool("noexcept_required");
    public ExpressionOperation? PackIndex => Object.Link(Data.OptionalLong("pack_index_id"), value => Object.Expression(value));
    public CppType? PackType => Object.Link(Data.OptionalLong("pack_type_id"), value => Object.Type(value));
    public TemplateDescriptor? PackTemplate => Object.Link(Data.OptionalLong("pack_template_id"), value => Object.Template(value));
    public SemanticKind? FoldOperator => Object.Link(Data.OptionalLong("fold_operator_id"), value => Object.Kind(value));
    public bool? FoldLeftAssociative => Data.OptionalBool("fold_left_associative");
    public TokenSequence? TokenSequence => Object.Link(Data.OptionalLong("token_sequence_id"), value => Object.TokenSequence(value));
    public bool? ReattemptDefaultArgument => Data.OptionalBool("reattempt_default_argument");
    public bool? ReattemptDefaultMemberInitializer => Data.OptionalBool("reattempt_default_member_initializer");
    public IReadOnlyList<RequirementParameterDetails> RequirementParameterRows => Object.List("SELECT * FROM requirement_parameter WHERE owner_id=$owner ORDER BY ordinal", Data.Long("id"), r => new RequirementParameterDetails(Object, r));
    public IReadOnlyList<DependentRequirementDetails> DependentRequirementRows => Object.List("SELECT * FROM dependent_requirement WHERE owner_id=$owner ORDER BY ordinal", Data.Long("id"), r => new DependentRequirementDetails(Object, r));
}

public sealed class RequirementParameterDetails : SemanticDetail
{
    internal RequirementParameterDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public DependentExpressionDetails Owner => Object.Link(Data.OptionalLong("owner_id"), value => Object.Detail("dependent_expression", value, r => new DependentExpressionDetails(Object, r)))!;
    public long Ordinal => Data.Long("ordinal");
    public ParameterType ParameterType => Object.Link(Data.OptionalLong("parameter_type_id"), value => Object.ParameterType(value))!;
}

public sealed class ExpressionTemplateArgumentDetails : SemanticDetail
{
    internal ExpressionTemplateArgumentDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public ExpressionOperation Owner => Object.Link(Data.OptionalLong("owner_id"), value => Object.Expression(value))!;
    public long Ordinal => Data.Long("ordinal");
    public TemplateArgument Argument => Object.Link(Data.OptionalLong("argument_id"), value => Object.Argument(value))!;
}

public sealed class TokenSequenceDetails : SemanticDetail
{
    internal TokenSequenceDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public IReadOnlyList<TokenSequenceTokenDetails> TokenSequenceTokenRows => Object.List("SELECT * FROM token_sequence_token WHERE owner_id=$owner ORDER BY ordinal", Data.Long("id"), r => new TokenSequenceTokenDetails(Object, r));
}

public sealed class TokenSequenceTokenDetails : SemanticDetail
{
    internal TokenSequenceTokenDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public TokenSequence Owner => Object.Link(Data.OptionalLong("owner_id"), value => Object.TokenSequence(value))!;
    public long Ordinal => Data.Long("ordinal");
    public SemanticKind Kind => Object.Kind(Data.Long("kind_id"));
    public string? Spelling => Object.Text(Data.OptionalLong("spelling_id"));
    public SourcePosition? SourcePosition => Object.Link(Data.OptionalLong("source_position_id"), value => Object.Position(value));
}

public sealed class DependentRequirementDetails : SemanticDetail
{
    internal DependentRequirementDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public DependentExpressionDetails Owner => Object.Link(Data.OptionalLong("owner_id"), value => Object.Detail("dependent_expression", value, r => new DependentExpressionDetails(Object, r)))!;
    public long Ordinal => Data.Long("ordinal");
    public ExpressionOperation Expression => Object.Link(Data.OptionalLong("expression_id"), value => Object.Expression(value))!;
}

public sealed class AllocationExpressionDetails : SemanticDetail
{
    internal AllocationExpressionDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public CppType? AllocatedType => Object.Link(Data.OptionalLong("allocated_type_id"), value => Object.Type(value));
    public FunctionSymbol? Allocator => Object.Link(Data.OptionalLong("allocator_id"), value => Object.Function(value));
    public FunctionSymbol? Deallocator => Object.Link(Data.OptionalLong("deallocator_id"), value => Object.Function(value));
    public Initialization? Initialization => Object.Link(Data.OptionalLong("initialization_id"), value => Object.Initialization(value));
    public ExpressionOperation? ArrayCount => Object.Link(Data.OptionalLong("array_count_id"), value => Object.Expression(value));
    public ExpressionOperation? Operand => Object.Link(Data.OptionalLong("operand_id"), value => Object.Expression(value));
    public bool IsDelete => Data.Bool("is_delete");
    public bool IsArray => Data.Bool("is_array");
    public bool IsGlobal => Data.Bool("is_global");
    public bool IsGcnew => Data.Bool("is_gcnew");
    public bool IsPlacement => Data.Bool("is_placement");
    public bool IsAligned => Data.Bool("is_aligned");
    public bool HasInitializer => Data.Bool("has_initializer");
    public bool BraceInitializer => Data.Bool("brace_initializer");
    public bool ParenAggregateInitializer => Data.Bool("paren_aggregate_initializer");
    public bool DeducibleType => Data.Bool("deducible_type");
    public bool ParenthesizedType => Data.Bool("parenthesized_type");
    public Initialization? ExceptionFreeing => Object.Link(Data.OptionalLong("exception_freeing_id"), value => Object.Initialization(value));
    public IReadOnlyList<AllocationArgumentDetails> AllocationArgumentRows => Object.List("SELECT * FROM allocation_argument WHERE owner_id=$owner ORDER BY ordinal", Data.Long("id"), r => new AllocationArgumentDetails(Object, r));
}

public sealed class AllocationArgumentDetails : SemanticDetail
{
    internal AllocationArgumentDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public AllocationExpressionDetails Owner => Object.Link(Data.OptionalLong("owner_id"), value => Object.Detail("allocation_expression", value, r => new AllocationExpressionDetails(Object, r)))!;
    public long Ordinal => Data.Long("ordinal");
    public ExpressionOperation Expression => Object.Link(Data.OptionalLong("expression_id"), value => Object.Expression(value))!;
}

public sealed class LambdaDefinitionDetails : SemanticDetail
{
    internal LambdaDefinitionDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public CppType ClosureType => Object.Link(Data.OptionalLong("closure_type_id"), value => Object.Type(value))!;
    public FunctionSymbol? CallOperator => Object.Link(Data.OptionalLong("call_operator_id"), value => Object.Function(value));
    public SourceInfo? SourceOrigin => Object.Link(Data.OptionalLong("source_origin_id"), value => Object.Source(value));
    public SourcePosition? CaptureEndPosition => Object.Link(Data.OptionalLong("capture_end_position_id"), value => Object.Position(value));
    public SourcePosition? MutablePosition => Object.Link(Data.OptionalLong("mutable_position_id"), value => Object.Position(value));
    public SourcePosition? ConstexprPosition => Object.Link(Data.OptionalLong("constexpr_position_id"), value => Object.Position(value));
    public bool IsGeneric => Data.Bool("is_generic");
    public bool IsMutable => Data.Bool("is_mutable");
    public bool ConstexprSpecified => Data.Bool("constexpr_specified");
    public bool ConstevalSpecified => Data.Bool("consteval_specified");
    public bool HasCaptureDefault => Data.Bool("has_capture_default");
    public bool DefaultIsReference => Data.Bool("default_is_reference");
    public bool ExplicitReturnType => Data.Bool("explicit_return_type");
    public bool HasParameterDeclaration => Data.Bool("has_parameter_declaration");
    public bool HasTemplateParameterList => Data.Bool("has_template_parameter_list");
    public IReadOnlyList<LambdaCaptureDetails> LambdaCaptureRows => Object.List("SELECT * FROM lambda_capture WHERE owner_id=$owner ORDER BY ordinal", Data.Long("id"), r => new LambdaCaptureDetails(Object, r));
}

public sealed class LambdaExpressionDetails : SemanticDetail
{
    internal LambdaExpressionDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public LambdaDefinition Lambda => Object.Link(Data.OptionalLong("lambda_id"), value => Object.LambdaDefinition(value))!;
    public Initialization? Initialization => Object.Link(Data.OptionalLong("initialization_id"), value => Object.Initialization(value));
}

public sealed class LambdaCaptureDetails : SemanticDetail
{
    internal LambdaCaptureDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public LambdaDefinition Owner => Object.Link(Data.OptionalLong("owner_id"), value => Object.LambdaDefinition(value))!;
    public long Ordinal => Data.Long("ordinal");
    public SemanticKind Kind => Object.Kind(Data.Long("kind_id"));
    public VariableSymbol? Variable => Object.Link(Data.OptionalLong("variable_id"), value => Object.Variable(value));
    public FieldSymbol? Field => Object.Link(Data.OptionalLong("field_id"), value => Object.Field(value));
    public Initialization? Initialization => Object.Link(Data.OptionalLong("initialization_id"), value => Object.Initialization(value));
    public FieldSymbol? SourceClosureField => Object.Link(Data.OptionalLong("source_closure_field_id"), value => Object.Field(value));
    public FieldSymbol? IndirectCaptureField => Object.Link(Data.OptionalLong("indirect_capture_field_id"), value => Object.Field(value));
    public SourceInfo? SourceOrigin => Object.Link(Data.OptionalLong("source_origin_id"), value => Object.Source(value));
    public bool IsPack => Data.Bool("is_pack");
    public bool IsInitCapture => Data.Bool("is_init_capture");
    public bool IsIndirectInitCapture => Data.Bool("is_indirect_init_capture");
    public bool IsParameterReference => Data.Bool("is_parameter_reference");
    public bool CaptureByReference => Data.Bool("capture_by_reference");
    public bool IsImplicit => Data.Bool("is_implicit");
    public bool IsPackElement => Data.Bool("is_pack_element");
    public bool DirectInitialization => Data.Bool("direct_initialization");
    public bool ParenthesizedInitialization => Data.Bool("parenthesized_initialization");
    public bool ConstCapture => Data.Bool("const_capture");
}

public sealed class CoroutineDetails : SemanticDetail
{
    internal CoroutineDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public FunctionSymbol Routine => Object.Link(Data.OptionalLong("routine_id"), value => Object.Function(value))!;
    public VariableSymbol? Promise => Object.Link(Data.OptionalLong("promise_id"), value => Object.Variable(value));
    public VariableSymbol? Handle => Object.Link(Data.OptionalLong("handle_id"), value => Object.Variable(value));
    public ExpressionOperation? InitialSuspend => Object.Link(Data.OptionalLong("initial_suspend_id"), value => Object.Expression(value));
    public ExpressionOperation? FinalSuspend => Object.Link(Data.OptionalLong("final_suspend_id"), value => Object.Expression(value));
    public ExpressionOperation? ReturnObject => Object.Link(Data.OptionalLong("return_object_id"), value => Object.Expression(value));
    public ExpressionOperation? Exception => Object.Link(Data.OptionalLong("exception_id"), value => Object.Expression(value));
    public CppType? TraitsType => Object.Link(Data.OptionalLong("traits_type_id"), value => Object.Type(value));
    public VariableSymbol? InitialAwaitResume => Object.Link(Data.OptionalLong("initial_await_resume_id"), value => Object.Variable(value));
    public VariableSymbol? ThisParameterCopy => Object.Link(Data.OptionalLong("this_parameter_copy_id"), value => Object.Variable(value));
    public LabelSymbol? FinalSuspendLabel => Object.Link(Data.OptionalLong("final_suspend_label_id"), value => Object.Label(value));
    public ExpressionOperation? AllocationFailureCall => Object.Link(Data.OptionalLong("allocation_failure_call_id"), value => Object.Expression(value));
    public FunctionSymbol? Allocator => Object.Link(Data.OptionalLong("allocator_id"), value => Object.Function(value));
    public FunctionSymbol? Deallocator => Object.Link(Data.OptionalLong("deallocator_id"), value => Object.Function(value));
    public SourcePosition? SourcePosition => Object.Link(Data.OptionalLong("source_position_id"), value => Object.Position(value));
    public bool ErrorDescriptor => Data.Bool("error_descriptor");
    public bool HasReturnVoid => Data.Bool("has_return_void");
    public bool BodyGenerated => Data.Bool("body_generated");
    public IReadOnlyList<CoroutineParameterCopyDetails> CoroutineParameterCopyRows => Object.List("SELECT * FROM coroutine_parameter_copy WHERE owner_id=$owner ORDER BY ordinal", Data.Long("id"), r => new CoroutineParameterCopyDetails(Object, r));
}

public sealed class CoroutineParameterCopyDetails : SemanticDetail
{
    internal CoroutineParameterCopyDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public CoroutineDetails Owner => Object.Link(Data.OptionalLong("owner_id"), value => Object.Detail("coroutine", value, r => new CoroutineDetails(Object, r)))!;
    public long Ordinal => Data.Long("ordinal");
    public VariableSymbol Copy => Object.Link(Data.OptionalLong("copy_id"), value => Object.Variable(value))!;
    public Initialization? Initialization => Object.Link(Data.OptionalLong("initialization_id"), value => Object.Initialization(value));
}

public sealed class CoroutineOperationDetails : SemanticDetail
{
    internal CoroutineOperationDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public SemanticKind Kind => Object.Kind(Data.Long("kind_id"));
    public ExpressionOperation? Operand => Object.Link(Data.OptionalLong("operand_id"), value => Object.Expression(value));
    public ExpressionOperation? Ready => Object.Link(Data.OptionalLong("ready_id"), value => Object.Expression(value));
    public ExpressionOperation? Suspend => Object.Link(Data.OptionalLong("suspend_id"), value => Object.Expression(value));
    public ExpressionOperation? Resume => Object.Link(Data.OptionalLong("resume_id"), value => Object.Expression(value));
}

public sealed class VlaDimensionDetails : SemanticDetail
{
    internal VlaDimensionDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public CppType? Type => Object.Link(Data.OptionalLong("type_id"), value => Object.Type(value));
    public ExpressionOperation? Expression => Object.Link(Data.OptionalLong("expression_id"), value => Object.Expression(value));
    public VlaDimension? OriginalDimension => Object.Link(Data.OptionalLong("original_dimension_id"), value => Object.VlaDimension(value));
    public VariableSymbol? DimensionVariable => Object.Link(Data.OptionalLong("dimension_variable_id"), value => Object.Variable(value));
    public bool InPrototypeScope => Data.Bool("in_prototype_scope");
    public bool HasSizeStatement => Data.Bool("has_size_statement");
    public SourcePosition? SourcePosition => Object.Link(Data.OptionalLong("source_position_id"), value => Object.Position(value));
}

public sealed class VlaOperationDetails : SemanticDetail
{
    internal VlaOperationDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public VariableSymbol? Variable => Object.Link(Data.OptionalLong("variable_id"), value => Object.Variable(value));
    public CppType? Type => Object.Link(Data.OptionalLong("type_id"), value => Object.Type(value));
    public VlaDimension? Dimension => Object.Link(Data.OptionalLong("dimension_id"), value => Object.VlaDimension(value));
    public bool IsTypedefDeclaration => Data.Bool("is_typedef_declaration");
}

public sealed class AsmEntryDetails : SemanticDetail
{
    internal AsmEntryDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public Constant AsmString => Object.Link(Data.OptionalLong("asm_string_id"), value => Object.Constant(value))!;
    public bool GnuAsmForm => Data.Bool("gnu_asm_form");
    public bool IsVolatile => Data.Bool("is_volatile");
    public bool HasVolatileKeyword => Data.Bool("has_volatile_keyword");
    public bool HasInlineKeyword => Data.Bool("has_inline_keyword");
    public bool IsGoto => Data.Bool("is_goto");
    public BigInteger? NumberOfConstraints => Number(Data.OptionalText("number_of_constraints"));
    public IReadOnlyList<AsmOperandDetails> AsmOperandRows => Object.List("SELECT * FROM asm_operand WHERE owner_id=$owner ORDER BY ordinal", Data.Long("id"), r => new AsmOperandDetails(Object, r));
    public IReadOnlyList<AsmOperandConstraintDetails> AsmOperandConstraintRows => Object.List("SELECT * FROM asm_operand_constraint WHERE owner_id=$owner ORDER BY ordinal", Data.Long("id"), r => new AsmOperandConstraintDetails(Object, r));
    public IReadOnlyList<AsmClobberDetails> AsmClobberRows => Object.List("SELECT * FROM asm_clobber WHERE owner_id=$owner ORDER BY ordinal", Data.Long("id"), r => new AsmClobberDetails(Object, r));
    public IReadOnlyList<AsmLabelDetails> AsmLabelRows => Object.List("SELECT * FROM asm_label WHERE owner_id=$owner ORDER BY ordinal", Data.Long("id"), r => new AsmLabelDetails(Object, r));
}

public sealed class AsmStatementDetails : SemanticDetail
{
    internal AsmStatementDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public AssemblyDefinition? Entry => Object.Link(Data.OptionalLong("entry_id"), value => Object.AssemblyDefinition(value));
    public string? Text => Object.Text(Data.OptionalLong("text_id"));
}

public sealed class AsmOperandDetails : SemanticDetail
{
    internal AsmOperandDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public AssemblyDefinition Owner => Object.Link(Data.OptionalLong("owner_id"), value => Object.AssemblyDefinition(value))!;
    public long Ordinal => Data.Long("ordinal");
    public SemanticKind Kind => Object.Kind(Data.Long("kind_id"));
    public string? Name => Object.Text(Data.OptionalLong("name_id"));
    public string? Constraint => Object.Text(Data.OptionalLong("constraint_id"));
    public SemanticKind? ModifierKind => Object.Link(Data.OptionalLong("modifier_kind_id"), value => Object.Kind(value));
    public ExpressionOperation Expression => Object.Link(Data.OptionalLong("expression_id"), value => Object.Expression(value))!;
    public SourcePosition? SourcePosition => Object.Link(Data.OptionalLong("source_position_id"), value => Object.Position(value));
}

public sealed class AsmOperandConstraintDetails : SemanticDetail
{
    internal AsmOperandConstraintDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public AssemblyDefinition Owner => Object.Link(Data.OptionalLong("owner_id"), value => Object.AssemblyDefinition(value))!;
    public long OperandOrdinal => Data.Long("operand_ordinal");
    public long Ordinal => Data.Long("ordinal");
    public SemanticKind Kind => Object.Kind(Data.Long("kind_id"));
    public string? ConditionCode => Object.Text(Data.OptionalLong("condition_code_id"));
}

public sealed class AsmClobberDetails : SemanticDetail
{
    internal AsmClobberDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public AssemblyDefinition Owner => Object.Link(Data.OptionalLong("owner_id"), value => Object.AssemblyDefinition(value))!;
    public long Ordinal => Data.Long("ordinal");
    public SemanticKind? RegisterKind => Object.Link(Data.OptionalLong("register_kind_id"), value => Object.Kind(value));
}

public sealed class AsmLabelDetails : SemanticDetail
{
    internal AsmLabelDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public AssemblyDefinition Owner => Object.Link(Data.OptionalLong("owner_id"), value => Object.AssemblyDefinition(value))!;
    public long Ordinal => Data.Long("ordinal");
    public LabelSymbol Label => Object.Link(Data.OptionalLong("label_id"), value => Object.Label(value))!;
}

public sealed class OwnerAttributeDetails : SemanticDetail
{
    internal OwnerAttributeDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public SemanticEntity Owner => Object.Link(Data.OptionalLong("owner_id"), value => Object.Entity(value))!;
    public long Ordinal => Data.Long("ordinal");
    public Attribute Attribute => Object.Link(Data.OptionalLong("attribute_id"), value => Object.Attribute(value))!;
}

public sealed class AttributeDetails : SemanticDetail
{
    internal AttributeDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public SemanticKind Kind => Object.Kind(Data.Long("kind_id"));
    public SemanticKind? Family => Object.Link(Data.OptionalLong("family_id"), value => Object.Kind(value));
    public SemanticKind? Location => Object.Link(Data.OptionalLong("location_id"), value => Object.Kind(value));
    public string? Name => Object.Text(Data.OptionalLong("name_id"));
    public string? NamespaceName => Object.Text(Data.OptionalLong("namespace_name_id"));
    public AttributeGroup? Group => Object.Link(Data.OptionalLong("group_id"), value => Object.AttributeGroup(value));
    public SourceInfo? SourceOrigin => Object.Link(Data.OptionalLong("source_origin_id"), value => Object.Source(value));
    public bool OnPrimaryDeclaration => Data.Bool("on_primary_declaration");
    public bool TransformsTypeSpecifier => Data.Bool("transforms_type_specifier");
    public bool IsPackExpansion => Data.Bool("is_pack_expansion");
    public bool IsStandardGcc => Data.Bool("is_standard_gcc");
    public bool IsImplicitAbiTag => Data.Bool("is_implicit_abi_tag");
    public bool NamespaceFromUsing => Data.Bool("namespace_from_using");
    public IReadOnlyList<AttributeArgumentLinkDetails> AttributeArgumentLinkRows => Object.List("SELECT * FROM attribute_argument_link WHERE owner_id=$owner ORDER BY ordinal", Data.Long("id"), r => new AttributeArgumentLinkDetails(Object, r));
}

public sealed class AttributeGroupDetails : SemanticDetail
{
    internal AttributeGroupDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public SourceInfo? SourceOrigin => Object.Link(Data.OptionalLong("source_origin_id"), value => Object.Source(value));
}

public sealed class AttributeArgumentLinkDetails : SemanticDetail
{
    internal AttributeArgumentLinkDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public Attribute Owner => Object.Link(Data.OptionalLong("owner_id"), value => Object.Attribute(value))!;
    public long Ordinal => Data.Long("ordinal");
    public AttributeArgument Argument => Object.Link(Data.OptionalLong("argument_id"), value => Object.AttributeArgument(value))!;
}

public sealed class AttributeArgumentDetails : SemanticDetail
{
    internal AttributeArgumentDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public SemanticKind Kind => Object.Kind(Data.Long("kind_id"));
    public CppType? Type => Object.Link(Data.OptionalLong("type_id"), value => Object.Type(value));
    public ExpressionOperation? Expression => Object.Link(Data.OptionalLong("expression_id"), value => Object.Expression(value));
    public Constant? Constant => Object.Link(Data.OptionalLong("constant_id"), value => Object.Constant(value));
    public string? TokenText => Object.Text(Data.OptionalLong("token_text_id"));
    public SemanticKind? TokenKind => Object.Link(Data.OptionalLong("token_kind_id"), value => Object.Kind(value));
    public bool IsPackExpansion => Data.Bool("is_pack_expansion");
    public SourceInfo? SourceOrigin => Object.Link(Data.OptionalLong("source_origin_id"), value => Object.Source(value));
    public bool IsLocalExpressionReference => Data.Bool("is_local_expression_reference");
    public ScopedExpression? ScopedExpression => Object.Link(Data.OptionalLong("scoped_expression_id"), value => Object.ScopedExpression(value));
}

public sealed class ConstantDetails : SemanticDetail
{
    internal ConstantDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public CppType? Type => Object.Link(Data.OptionalLong("type_id"), value => Object.Type(value));
    public CppType? OrigType => Object.Link(Data.OptionalLong("orig_type_id"), value => Object.Type(value));
    public ConstantValue Value => Object.Link(Data.OptionalLong("value_id"), value => Object.Value(value))!;
    public SourceInfo? SourceOrigin => Object.Link(Data.OptionalLong("source_origin_id"), value => Object.Source(value));
    public ExpressionOperation? BackingExpression => Object.Link(Data.OptionalLong("backing_expression_id"), value => Object.Expression(value));
    public NameReference? NameReference => Object.Link(Data.OptionalLong("name_reference_id"), value => Object.NameReference(value));
    public SemanticKind? CharacterKind => Object.Link(Data.OptionalLong("character_kind_id"), value => Object.Kind(value));
    public bool IsPackExpansion => Data.Bool("is_pack_expansion");
    public bool ImplicitCast => Data.Bool("implicit_cast");
    public bool ExplicitCastApplied => Data.Bool("explicit_cast_applied");
    public bool IsReinterpretCast => Data.Bool("is_reinterpret_cast");
    public bool IsReinterpretLikeCast => Data.Bool("is_reinterpret_like_cast");
    public bool IsSimpleZero => Data.Bool("is_simple_zero");
    public VariableSymbol? AssociatedStorageVariable => Object.Link(Data.OptionalLong("associated_storage_variable_id"), value => Object.Variable(value));
    public bool NonArithmetic => Data.Bool("non_arithmetic");
    public bool NullPointerConstantRuledOut => Data.Bool("null_pointer_constant_ruled_out");
    public bool NullptrKeyword => Data.Bool("nullptr_keyword");
    public bool ExplicitBracesOnAggregate => Data.Bool("explicit_braces_on_aggregate");
    public bool ExplicitParenthesesOnAggregate => Data.Bool("explicit_parentheses_on_aggregate");
    public bool FromUndefinedPreprocId => Data.Bool("from_undefined_preproc_id");
    public bool UsesDesignatedInitializers => Data.Bool("uses_designated_initializers");
    public bool IsNamedConstantDefinition => Data.Bool("is_named_constant_definition");
    public bool PartialAggrValue => Data.Bool("partial_aggr_value");
    public bool IsPartiallyInitialized => Data.Bool("is_partially_initialized");
    public bool ImplicitAggrElement => Data.Bool("implicit_aggr_element");
    public bool IsCompoundLiteral => Data.Bool("is_compound_literal");
    public bool IsResultOfConstexprCall => Data.Bool("is_result_of_constexpr_call");
    public bool IsGenericInitializer => Data.Bool("is_generic_initializer");
    public bool ConstantForBaseClass => Data.Bool("constant_for_base_class");
    public bool ConstantForBaseClassFromConstexprFolding => Data.Bool("constant_for_base_class_from_constexpr_folding");
    public bool FoldedStatementExpression => Data.Bool("folded_statement_expression");
    public bool FormedFromPromotedStorage => Data.Bool("formed_from_promoted_storage");
    public bool LocalExprRef => Data.Bool("local_expr_ref");
    public bool? NullKeyword => Data.OptionalBool("null_keyword");
    public bool? NativeNullptrKeyword => Data.OptionalBool("native_nullptr_keyword");
    public bool? PtrToMemConstantConstruct => Data.OptionalBool("ptr_to_mem_constant_construct");
    public bool? FlexibleArrayInitializer => Data.OptionalBool("flexible_array_initializer");
    public bool? InitializesEmptyObject => Data.OptionalBool("initializes_empty_object");
    public bool? IsImplicitInitialization => Data.OptionalBool("is_implicit_initialization");
    public SemanticKind? LiteralEncodingPrefix => Object.Link(Data.OptionalLong("literal_encoding_prefix_id"), value => Object.Kind(value));
    public bool? IsRawString => Data.OptionalBool("is_raw_string");
    public bool? IsStringLiteral => Data.OptionalBool("is_string_literal");
    public bool? IsFunctionNameLiteral => Data.OptionalBool("is_function_name_literal");
    public bool? ContainsFunctionNameToken => Data.OptionalBool("contains_function_name_token");
    public bool? IsEmbedExpansion => Data.OptionalBool("is_embed_expansion");
    public BigInteger? StringSequenceNumber => Number(Data.OptionalText("string_sequence_number"));
    public string? EmbedDirective => Object.Text(Data.OptionalLong("embed_directive_id"));
    public bool? HasDynamicInitComponent => Data.OptionalBool("has_dynamic_init_component");
    public bool? AddedConstForTemplateParam => Data.OptionalBool("added_const_for_template_param");
    public FieldSymbol? InitializedField => Object.Link(Data.OptionalLong("initialized_field_id"), value => Object.Field(value));
    public BaseSubobject? InitializedBaseSubobject => Object.Link(Data.OptionalLong("initialized_base_subobject_id"), value => Object.Subobject(value));
    public bool? TemplateLocalExprRef => Data.OptionalBool("template_local_expr_ref");
    public bool? VptrHasBeenLowered => Data.OptionalBool("vptr_has_been_lowered");
    public IReadOnlyList<ConstantComponentDetails> ConstantComponentRows => Object.List("SELECT * FROM constant_component WHERE owner_id=$owner ORDER BY role_id,ordinal", Data.Long("id"), r => new ConstantComponentDetails(Object, r));
}

public sealed class ConstantValueDetails : SemanticDetail
{
    internal ConstantValueDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public SemanticKind Kind => Object.Kind(Data.Long("kind_id"));
}

public sealed class ConstantComponentDetails : SemanticDetail
{
    internal ConstantComponentDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public Constant Owner => Object.Link(Data.OptionalLong("owner_id"), value => Object.Constant(value))!;
    public SemanticKind Role => Object.Kind(Data.Long("role_id"));
    public long Ordinal => Data.Long("ordinal");
    public Constant Constant => Object.Link(Data.OptionalLong("constant_id"), value => Object.Constant(value))!;
}

public sealed class IntegerValueDetails : SemanticDetail
{
    internal IntegerValueDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public BigInteger Value => RequiredNumber(Object.Text(Data.OptionalLong("value_id")));
}

public sealed class FloatingValueDetails : SemanticDetail
{
    internal FloatingValueDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public SemanticKind Class => Object.Kind(Data.Long("class_id"));
    public bool Negative => Data.Bool("negative");
    public long Radix => Data.Long("radix");
    public BigInteger? Significand => Number(Object.Text(Data.OptionalLong("significand_id")));
    public long? Exponent => Data.OptionalLong("exponent");
    public BigInteger? NanPayload => Number(Object.Text(Data.OptionalLong("nan_payload_id")));
    public bool? NanQuiet => Data.OptionalBool("nan_quiet");
    public SemanticKind? RepresentationKind => Object.Link(Data.OptionalLong("representation_kind_id"), value => Object.Kind(value));
}

public sealed class FixedPointValueDetails : SemanticDetail
{
    internal FixedPointValueDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public BigInteger ScaledInteger => RequiredNumber(Object.Text(Data.OptionalLong("scaled_integer_id")));
    public long Scale => Data.Long("scale");
}

public sealed class ComplexValueDetails : SemanticDetail
{
    internal ComplexValueDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public ConstantValue? RealValue => Object.Link(Data.OptionalLong("real_value_id"), value => Object.Value(value));
    public ConstantValue ImaginaryValue => Object.Link(Data.OptionalLong("imaginary_value_id"), value => Object.Value(value))!;
}

public sealed class StringValueDetails : SemanticDetail
{
    internal StringValueDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public long ElementBits => Data.Long("element_bits");
    public BigInteger Length => RequiredNumber(Data.OptionalText("length"));
    public SemanticKind Encoding => Object.Kind(Data.Long("encoding_id"));
    public SemanticKind UnitKind => Object.Kind(Data.Long("unit_kind_id"));
    public IReadOnlyList<StringUnitDetails> StringUnitRows => Object.List("SELECT * FROM string_unit WHERE owner_id=$owner ORDER BY ordinal", Data.Long("id"), r => new StringUnitDetails(Object, r));
}

public sealed class StringUnitDetails : SemanticDetail
{
    internal StringUnitDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public StringValueDetails Owner => Object.Link(Data.OptionalLong("owner_id"), value => Object.Detail("string_value", value, r => new StringValueDetails(Object, r)))!;
    public long Ordinal => Data.Long("ordinal");
    public long Unit => Data.Long("unit");
}

public sealed class AddressValueDetails : SemanticDetail
{
    internal AddressValueDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public SemanticKind BaseKind => Object.Kind(Data.Long("base_kind_id"));
    public SemanticEntity? BaseEntity => Object.Link(Data.OptionalLong("base_entity_id"), value => Object.Entity(value));
    public BigInteger Offset => RequiredNumber(Data.OptionalText("offset"));
    public bool OnePastEnd => Data.Bool("one_past_end");
    public SemanticEntity? SubobjectPath => Object.Link(Data.OptionalLong("subobject_path_id"), value => Object.Entity(value));
    public bool IsObjectReflection => Data.Bool("is_object_reflection");
    public long? ParameterNumber => Data.OptionalLong("parameter_number");
}

public sealed class StackOffsetValueDetails : SemanticDetail
{
    internal StackOffsetValueDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public VariableSymbol Variable => Object.Link(Data.OptionalLong("variable_id"), value => Object.Variable(value))!;
    public BigInteger Offset => RequiredNumber(Data.OptionalText("offset"));
}

public sealed class MemberPointerValueDetails : SemanticDetail
{
    internal MemberPointerValueDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public SemanticEntity? Member => Object.Link(Data.OptionalLong("member_id"), value => Object.Entity(value));
    public CppType? ContainingType => Object.Link(Data.OptionalLong("containing_type_id"), value => Object.Type(value));
    public BaseSubobject? Subobject => Object.Link(Data.OptionalLong("subobject_id"), value => Object.Subobject(value));
    public bool IsFunctionPtr => Data.Bool("is_function_ptr");
    public bool CastToBase => Data.Bool("cast_to_base");
    public bool IsNull => Data.Bool("is_null");
}

public sealed class LabelDifferenceValueDetails : SemanticDetail
{
    internal LabelDifferenceValueDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public LabelSymbol? LeftLabel => Object.Link(Data.OptionalLong("left_label_id"), value => Object.Label(value));
    public LabelSymbol? RightLabel => Object.Link(Data.OptionalLong("right_label_id"), value => Object.Label(value));
    public ConstantValue FromValue => Object.Link(Data.OptionalLong("from_value_id"), value => Object.Value(value))!;
    public CppType? FromType => Object.Link(Data.OptionalLong("from_type_id"), value => Object.Type(value));
    public ConstantValue ToValue => Object.Link(Data.OptionalLong("to_value_id"), value => Object.Value(value))!;
    public CppType? ToType => Object.Link(Data.OptionalLong("to_type_id"), value => Object.Type(value));
}

public sealed class AggregateValueDetails : SemanticDetail
{
    internal AggregateValueDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public CppType Type => Object.Link(Data.OptionalLong("type_id"), value => Object.Type(value))!;
    public IReadOnlyList<AggregateElementDetails> AggregateElementRows => Object.List("SELECT * FROM aggregate_element WHERE owner_id=$owner ORDER BY ordinal", Data.Long("id"), r => new AggregateElementDetails(Object, r));
}

public sealed class AggregateElementDetails : SemanticDetail
{
    internal AggregateElementDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public AggregateValueDetails Owner => Object.Link(Data.OptionalLong("owner_id"), value => Object.Detail("aggregate_value", value, r => new AggregateValueDetails(Object, r)))!;
    public long Ordinal => Data.Long("ordinal");
    public ConstantValue Value => Object.Link(Data.OptionalLong("value_id"), value => Object.Value(value))!;
    public CppType? Type => Object.Link(Data.OptionalLong("type_id"), value => Object.Type(value));
}

public sealed class RepeatValueDetails : SemanticDetail
{
    internal RepeatValueDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public ConstantValue Value => Object.Link(Data.OptionalLong("value_id"), value => Object.Value(value))!;
    public CppType? Type => Object.Link(Data.OptionalLong("type_id"), value => Object.Type(value));
    public BigInteger Count => RequiredNumber(Data.OptionalText("count"));
    public bool MultidimensionalTailNotRepeated => Data.Bool("multidimensional_tail_not_repeated");
}

public sealed class DesignatorValueDetails : SemanticDetail
{
    internal DesignatorValueDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public SemanticKind Kind => Object.Kind(Data.Long("kind_id"));
    public FieldSymbol? Field => Object.Link(Data.OptionalLong("field_id"), value => Object.Field(value));
    public string? FieldName => Object.Text(Data.OptionalLong("field_name_id"));
    public bool IsGeneric => Data.Bool("is_generic");
    public bool UsesDirectInitSyntax => Data.Bool("uses_direct_init_syntax");
    public BigInteger? IndexValue => Number(Data.OptionalText("index_value"));
    public IReadOnlyList<DesignatorSubscriptDetails> DesignatorSubscriptRows => Object.List("SELECT * FROM designator_subscript WHERE owner_id=$owner ORDER BY ordinal", Data.Long("id"), r => new DesignatorSubscriptDetails(Object, r));
}

public sealed class DesignatorSubscriptDetails : SemanticDetail
{
    internal DesignatorSubscriptDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public DesignatorValueDetails Owner => Object.Link(Data.OptionalLong("owner_id"), value => Object.Detail("designator_value", value, r => new DesignatorValueDetails(Object, r)))!;
    public long Ordinal => Data.Long("ordinal");
    public ConstantValue Value => Object.Link(Data.OptionalLong("value_id"), value => Object.Value(value))!;
    public CppType? Type => Object.Link(Data.OptionalLong("type_id"), value => Object.Type(value));
}

public sealed class DynamicInitializationValueDetails : SemanticDetail
{
    internal DynamicInitializationValueDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public Initialization Initialization => Object.Link(Data.OptionalLong("initialization_id"), value => Object.Initialization(value))!;
}

public sealed class DependentValueDetails : SemanticDetail
{
    internal DependentValueDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public SemanticKind Kind => Object.Kind(Data.Long("kind_id"));
    public long? ParameterNumber => Data.OptionalLong("parameter_number");
    public long? ParameterDepth => Data.OptionalLong("parameter_depth");
    public ExpressionOperation? Expression => Object.Link(Data.OptionalLong("expression_id"), value => Object.Expression(value));
    public CppType? Type => Object.Link(Data.OptionalLong("type_id"), value => Object.Type(value));
    public ConstantValue? ConstantValue => Object.Link(Data.OptionalLong("constant_value_id"), value => Object.Value(value));
    public CppType? ConstantType => Object.Link(Data.OptionalLong("constant_type_id"), value => Object.Type(value));
    public bool IsQualifiedName => Data.Bool("is_qualified_name");
    public bool HasAddressOf => Data.Bool("has_address_of");
    public bool IsPack => Data.Bool("is_pack");
    public bool HasGenericCast => Data.Bool("has_generic_cast");
    public CppType? ConversionType => Object.Link(Data.OptionalLong("conversion_type_id"), value => Object.Type(value));
    public SemanticKind? OpnameKind => Object.Link(Data.OptionalLong("opname_kind_id"), value => Object.Kind(value));
    public SemanticKind? SpecialKind => Object.Link(Data.OptionalLong("special_kind_id"), value => Object.Kind(value));
    public bool IsStandardAlignof => Data.Bool("is_standard_alignof");
    public bool DestructorUnqualified => Data.Bool("destructor_unqualified");
    public IReadOnlyList<DependentConstantElementDetails> DependentConstantElementRows => Object.List("SELECT * FROM dependent_constant_element WHERE owner_id=$owner ORDER BY ordinal", Data.Long("id"), r => new DependentConstantElementDetails(Object, r));
    public IReadOnlyList<DependentTemplateArgumentDetails> DependentTemplateArgumentRows => Object.List("SELECT * FROM dependent_template_argument WHERE owner_id=$owner ORDER BY ordinal", Data.Long("id"), r => new DependentTemplateArgumentDetails(Object, r));
}

public sealed class DependentConstantElementDetails : SemanticDetail
{
    internal DependentConstantElementDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public DependentValueDetails Owner => Object.Link(Data.OptionalLong("owner_id"), value => Object.Detail("dependent_value", value, r => new DependentValueDetails(Object, r)))!;
    public long Ordinal => Data.Long("ordinal");
    public ConstantValue Value => Object.Link(Data.OptionalLong("value_id"), value => Object.Value(value))!;
    public CppType? Type => Object.Link(Data.OptionalLong("type_id"), value => Object.Type(value));
}

public sealed class DependentTemplateArgumentDetails : SemanticDetail
{
    internal DependentTemplateArgumentDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public DependentValueDetails Owner => Object.Link(Data.OptionalLong("owner_id"), value => Object.Detail("dependent_value", value, r => new DependentValueDetails(Object, r)))!;
    public long Ordinal => Data.Long("ordinal");
    public TemplateArgument Argument => Object.Link(Data.OptionalLong("argument_id"), value => Object.Argument(value))!;
}

public sealed class ReflectionValueDetails : SemanticDetail
{
    internal ReflectionValueDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public SemanticKind Kind => Object.Kind(Data.Long("kind_id"));
    public SemanticEntity? Entity => Object.Link(Data.OptionalLong("entity_id"), value => Object.Entity(value));
    public BigInteger? LocalScopeNumber => Number(Data.OptionalText("local_scope_number"));
}

public sealed class SubobjectPathDetails : SemanticDetail
{
    internal SubobjectPathDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public SemanticKind Kind => Object.Kind(Data.Long("kind_id"));
    public FieldSymbol? Field => Object.Link(Data.OptionalLong("field_id"), value => Object.Field(value));
    public BaseSubobject? BaseSubobject => Object.Link(Data.OptionalLong("base_subobject_id"), value => Object.Subobject(value));
    public BigInteger? ElementOffset => Number(Data.OptionalText("element_offset"));
    public SubobjectPath? Next => Object.Link(Data.OptionalLong("next_id"), value => Object.SubobjectPath(value));
    public bool IsConverted => Data.Bool("is_converted");
}

public sealed class InitializationDetails : SemanticDetail
{
    internal InitializationDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public SemanticKind Kind => Object.Kind(Data.Long("kind_id"));
    public SemanticEntity? Target => Object.Link(Data.OptionalLong("target_id"), value => Object.Entity(value));
    public CppType? Type => Object.Link(Data.OptionalLong("type_id"), value => Object.Type(value));
    public LambdaDefinition? Lambda => Object.Link(Data.OptionalLong("lambda_id"), value => Object.LambdaDefinition(value));
    public ExpressionOperation? Expression => Object.Link(Data.OptionalLong("expression_id"), value => Object.Expression(value));
    public Constant? Constant => Object.Link(Data.OptionalLong("constant_id"), value => Object.Constant(value));
    public FunctionSymbol? Constructor => Object.Link(Data.OptionalLong("constructor_id"), value => Object.Function(value));
    public FunctionSymbol? Destructor => Object.Link(Data.OptionalLong("destructor_id"), value => Object.Function(value));
    public Lifetime? Lifetime => Object.Link(Data.OptionalLong("lifetime_id"), value => Object.Lifetime(value));
    public Lifetime? InitExpressionLifetime => Object.Link(Data.OptionalLong("init_expression_lifetime_id"), value => Object.Lifetime(value));
    public Initialization? Master => Object.Link(Data.OptionalLong("master_id"), value => Object.Initialization(value));
    public Initialization? NextDestruction => Object.Link(Data.OptionalLong("next_destruction_id"), value => Object.Initialization(value));
    public Lifetime? OverlappingLifetime => Object.Link(Data.OptionalLong("overlapping_lifetime_id"), value => Object.Lifetime(value));
    public ExpressionOperation? Source => Object.Link(Data.OptionalLong("source_id"), value => Object.Expression(value));
    public bool IsUnordered => Data.Bool("is_unordered");
    public bool IsConditional => Data.Bool("is_conditional");
    public bool IsOverlapping => Data.Bool("is_overlapping");
    public bool IsClassResultMaster => Data.Bool("is_class_result_master");
    public bool IsStaticTemporary => Data.Bool("is_static_temporary");
    public bool IsPartialAggregate => Data.Bool("is_partial_aggregate");
    public bool IsPartialInitialization => Data.Bool("is_partial_initialization");
    public bool FollowsExecutableStatement => Data.Bool("follows_executable_statement");
    public bool HasTemporaryLifetime => Data.Bool("has_temporary_lifetime");
    public bool IsConstructorInitialization => Data.Bool("is_constructor_initialization");
    public bool IsConditionalGuard => Data.Bool("is_conditional_guard");
    public bool IsVlaDeallocation => Data.Bool("is_vla_deallocation");
    public bool CreatesInitializerList => Data.Bool("creates_initializer_list");
    public bool IsInitializerListArray => Data.Bool("is_initializer_list_array");
    public bool IsReusedValue => Data.Bool("is_reused_value");
    public bool IsQuestionResult => Data.Bool("is_question_result");
    public bool IsCommaResult => Data.Bool("is_comma_result");
    public bool ConstructorCopyImpliedSource => Data.Bool("constructor_copy_implied_source");
    public bool ConstructorImplicitCopy => Data.Bool("constructor_implicit_copy");
    public bool ConstructorValueInitialization => Data.Bool("constructor_value_initialization");
    public bool ConstructorSequencedArguments => Data.Bool("constructor_sequenced_arguments");
    public bool ConstructorArrayCopy => Data.Bool("constructor_array_copy");
    public bool ConstantNonconstant => Data.Bool("constant_nonconstant");
    public bool FreesStorage => Data.Bool("frees_storage");
    public bool FreesArray => Data.Bool("frees_array");
    public bool IsExplicitCast => Data.Bool("is_explicit_cast");
    public bool IsBraced => Data.Bool("is_braced");
    public bool IsCompoundLiteral => Data.Bool("is_compound_literal");
    public bool IsFreeingOfExceptionObject => Data.Bool("is_freeing_of_exception_object");
    public IReadOnlyList<InitializationOperandDetails> InitializationOperandRows => Object.List("SELECT * FROM initialization_operand WHERE owner_id=$owner ORDER BY role_id,ordinal", Data.Long("id"), r => new InitializationOperandDetails(Object, r));
}

public sealed class InitializationOperandDetails : SemanticDetail
{
    internal InitializationOperandDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public Initialization Owner => Object.Link(Data.OptionalLong("owner_id"), value => Object.Initialization(value))!;
    public SemanticKind Role => Object.Kind(Data.Long("role_id"));
    public long Ordinal => Data.Long("ordinal");
    public ExpressionOperation? Expression => Object.Link(Data.OptionalLong("expression_id"), value => Object.Expression(value));
}

public sealed class LifetimeDetails : SemanticDetail
{
    internal LifetimeDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public SemanticKind Kind => Object.Kind(Data.Long("kind_id"));
    public Lifetime? Parent => Object.Link(Data.OptionalLong("parent_id"), value => Object.Lifetime(value));
    public SemanticEntity? BoundEntity => Object.Link(Data.OptionalLong("bound_entity_id"), value => Object.Entity(value));
    public Initialization? ParentCheckpoint => Object.Link(Data.OptionalLong("parent_checkpoint_id"), value => Object.Initialization(value));
    public bool HasImplicitChild => Data.Bool("has_implicit_child");
    public bool HasBlockAfterLabelChild => Data.Bool("has_block_after_label_child");
    public IReadOnlyList<LifetimeChildDetails> LifetimeChildRows => Object.List("SELECT * FROM lifetime_child WHERE owner_id=$owner ORDER BY ordinal", Data.Long("id"), r => new LifetimeChildDetails(Object, r));
    public IReadOnlyList<LifetimeCleanupDetails> LifetimeCleanupRows => Object.List("SELECT * FROM lifetime_cleanup WHERE owner_id=$owner ORDER BY ordinal", Data.Long("id"), r => new LifetimeCleanupDetails(Object, r));
}

public sealed class LifetimeChildDetails : SemanticDetail
{
    internal LifetimeChildDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public Lifetime Owner => Object.Link(Data.OptionalLong("owner_id"), value => Object.Lifetime(value))!;
    public long Ordinal => Data.Long("ordinal");
    public Lifetime Child => Object.Link(Data.OptionalLong("child_id"), value => Object.Lifetime(value))!;
}

public sealed class LifetimeCleanupDetails : SemanticDetail
{
    internal LifetimeCleanupDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public Lifetime Owner => Object.Link(Data.OptionalLong("owner_id"), value => Object.Lifetime(value))!;
    public long Ordinal => Data.Long("ordinal");
    public Initialization Initialization => Object.Link(Data.OptionalLong("initialization_id"), value => Object.Initialization(value))!;
}

public sealed class ExceptionSpecificationDetails : SemanticDetail
{
    internal ExceptionSpecificationDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public SemanticKind Kind => Object.Kind(Data.Long("kind_id"));
    public Constant? NoexceptArgument => Object.Link(Data.OptionalLong("noexcept_argument_id"), value => Object.Constant(value));
    public FunctionSymbol? CopyFromPrototypeRoutine => Object.Link(Data.OptionalLong("copy_from_prototype_routine_id"), value => Object.Function(value));
    public SourceInfo? SourceOrigin => Object.Link(Data.OptionalLong("source_origin_id"), value => Object.Source(value));
    public bool ArgumentCached => Data.Bool("argument_cached");
    public bool FromAttribute => Data.Bool("from_attribute");
    public bool CompilerGenerated => Data.Bool("compiler_generated");
    public bool ThrowAny => Data.Bool("throw_any");
    public bool? NoexceptEffective => Data.OptionalBool("noexcept_effective");
    public bool IsDependent => Data.Bool("is_dependent");
    public bool IsIndeterminate => Data.Bool("is_indeterminate");
    public bool CopyFromPrototype => Data.Bool("copy_from_prototype");
    public IReadOnlyList<ExceptionSpecificationTypeDetails> ExceptionSpecificationTypeRows => Object.List("SELECT * FROM exception_specification_type WHERE owner_id=$owner ORDER BY ordinal", Data.Long("id"), r => new ExceptionSpecificationTypeDetails(Object, r));
}

public sealed class ExceptionSpecificationTypeDetails : SemanticDetail
{
    internal ExceptionSpecificationTypeDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public ExceptionSpecification Owner => Object.Link(Data.OptionalLong("owner_id"), value => Object.ExceptionSpecification(value))!;
    public long Ordinal => Data.Long("ordinal");
    public CppType Type => Object.Link(Data.OptionalLong("type_id"), value => Object.Type(value))!;
    public bool IsRedundant => Data.Bool("is_redundant");
    public bool IsPackExpansion => Data.Bool("is_pack_expansion");
    public SourcePosition? SourcePosition => Object.Link(Data.OptionalLong("source_position_id"), value => Object.Position(value));
}

public sealed class FunctionEhDetails : SemanticDetail
{
    internal FunctionEhDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public FunctionSymbol Routine => Object.Link(Data.OptionalLong("routine_id"), value => Object.Function(value))!;
    public ExpressionOperation? Prologue => Object.Link(Data.OptionalLong("prologue_id"), value => Object.Expression(value));
    public VariableSymbol? RegionTable => Object.Link(Data.OptionalLong("region_table_id"), value => Object.Variable(value));
    public VariableSymbol? ArrayTable => Object.Link(Data.OptionalLong("array_table_id"), value => Object.Variable(value));
    public BigInteger? InitialState => Number(Data.OptionalText("initial_state"));
    public BigInteger? NullState => Number(Data.OptionalText("null_state"));
    public IReadOnlyList<CleanupRegionDetails> CleanupRegionRows => Object.List("SELECT * FROM cleanup_region WHERE function_eh_id=$owner ORDER BY ordinal,id", Data.Long("id"), r => new CleanupRegionDetails(Object, r), "cleanup_region");
    public IReadOnlyList<CleanupArrayDetails> CleanupArrayRows => Object.List("SELECT * FROM cleanup_array WHERE function_eh_id=$owner ORDER BY ordinal,id", Data.Long("id"), r => new CleanupArrayDetails(Object, r), "cleanup_array");
    public IReadOnlyList<LoweredEhOperationDetails> LoweredEhOperationRows => Object.List("SELECT * FROM lowered_eh_operation WHERE function_eh_id=$owner ORDER BY id", Data.Long("id"), r => new LoweredEhOperationDetails(Object, r), "lowered_eh_operation");
}

public sealed class EhTableProjectionDetails : SemanticDetail
{
    internal EhTableProjectionDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public VariableSymbol Owner => Object.Link(Data.OptionalLong("owner_id"), value => Object.Variable(value))!;
    public SemanticKind Kind => Object.Kind(Data.Long("kind_id"));
    public FunctionEhDetails FunctionEh => Object.Link(Data.OptionalLong("function_eh_id"), value => Object.Detail("function_eh", value, r => new FunctionEhDetails(Object, r)))!;
}

public sealed class CleanupRegionDetails : SemanticDetail
{
    internal CleanupRegionDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public FunctionEhDetails FunctionEh => Object.Link(Data.OptionalLong("function_eh_id"), value => Object.Detail("function_eh", value, r => new FunctionEhDetails(Object, r)))!;
    public long Ordinal => Data.Long("ordinal");
    public CleanupRegion? NextRegion => Object.Link(Data.OptionalLong("next_region_id"), value => Object.Region(value));
    public CleanupCondition? Condition => Object.Link(Data.OptionalLong("condition_id"), value => Object.Condition(value));
    public bool IsSupplement => Data.Bool("is_supplement");
    public CleanupRegion? SupplementOwner => Object.Link(Data.OptionalLong("supplement_owner_id"), value => Object.Region(value));
    public IReadOnlyList<CleanupActionDetails> CleanupActionRows => Object.List("SELECT * FROM cleanup_action WHERE region_id=$owner ORDER BY ordinal,id", Data.Long("id"), r => new CleanupActionDetails(Object, r), "cleanup_action");
    public IReadOnlyList<LoweredEhOperationDetails> LoweredEhOperationRows => Object.List("SELECT * FROM lowered_eh_operation WHERE region_id=$owner ORDER BY id", Data.Long("id"), r => new LoweredEhOperationDetails(Object, r), "lowered_eh_operation");
    public IReadOnlyList<CleanupStateOperationDetails> CleanupStateOperationRows => Object.List("SELECT * FROM cleanup_state_operation WHERE region_id=$owner ORDER BY id", Data.Long("id"), r => new CleanupStateOperationDetails(Object, r), "cleanup_state_operation");
}

public sealed class CleanupActionDetails : SemanticDetail
{
    internal CleanupActionDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public CleanupRegion Region => Object.Link(Data.OptionalLong("region_id"), value => Object.Region(value))!;
    public long Ordinal => Data.Long("ordinal");
    public SemanticKind Kind => Object.Kind(Data.Long("kind_id"));
    public FunctionSymbol? Routine => Object.Link(Data.OptionalLong("routine_id"), value => Object.Function(value));
    public CleanupStorageLocation? Storage => Object.Link(Data.OptionalLong("storage_id"), value => Object.Storage(value));
    public CleanupArray? Array => Object.Link(Data.OptionalLong("array_id"), value => Object.Array(value));
    public CleanupAbiDetail? AbiDetail => Object.Link(Data.OptionalLong("abi_detail_id"), value => Object.AbiDetail(value));
    public bool IsArray => Data.Bool("is_array");
    public bool IsIndirect => Data.Bool("is_indirect");
    public bool IsConditional => Data.Bool("is_conditional");
    public bool IsBaseSubobject => Data.Bool("is_base_subobject");
    public bool IsVla => Data.Bool("is_vla");
    public bool IsDeallocation => Data.Bool("is_deallocation");
    public bool IsLocalStaticGuard => Data.Bool("is_local_static_guard");
}

public sealed class CleanupStorageLocationDetails : SemanticDetail
{
    internal CleanupStorageLocationDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public SemanticKind Kind => Object.Kind(Data.Long("kind_id"));
    public VariableSymbol? Variable => Object.Link(Data.OptionalLong("variable_id"), value => Object.Variable(value));
    public SemanticEntity? BaseEntity => Object.Link(Data.OptionalLong("base_entity_id"), value => Object.Entity(value));
    public BigInteger Offset => RequiredNumber(Data.OptionalText("offset"));
    public bool IsIndirect => Data.Bool("is_indirect");
    public bool UsesThisParameterOffset => Data.Bool("uses_this_parameter_offset");
}

public sealed class CleanupArrayDetails : SemanticDetail
{
    internal CleanupArrayDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public FunctionEhDetails FunctionEh => Object.Link(Data.OptionalLong("function_eh_id"), value => Object.Detail("function_eh", value, r => new FunctionEhDetails(Object, r)))!;
    public long Ordinal => Data.Long("ordinal");
    public Constant HandleConstant => Object.Link(Data.OptionalLong("handle_constant_id"), value => Object.Constant(value))!;
    public BigInteger ElementSize => RequiredNumber(Data.OptionalText("element_size"));
    public BigInteger ElementCount => RequiredNumber(Data.OptionalText("element_count"));
    public bool IsNonarraySize => Data.Bool("is_nonarray_size");
}

public sealed class CleanupConditionDetails : SemanticDetail
{
    internal CleanupConditionDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public CleanupStorageLocation Storage => Object.Link(Data.OptionalLong("storage_id"), value => Object.Storage(value))!;
    public SemanticKind Kind => Object.Kind(Data.Long("kind_id"));
    public BigInteger? ComparisonValue => Number(Data.OptionalText("comparison_value"));
}

public sealed class CleanupAbiDetailDetails : SemanticDetail
{
    internal CleanupAbiDetailDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public CleanupStorageLocation? VttLocation => Object.Link(Data.OptionalLong("vtt_location_id"), value => Object.Storage(value));
    public bool? IsDelegating => Data.OptionalBool("is_delegating");
    public CleanupStorageLocation? VlaCountLocation => Object.Link(Data.OptionalLong("vla_count_location_id"), value => Object.Storage(value));
}

public sealed class LoweredEhOperationDetails : SemanticDetail
{
    internal LoweredEhOperationDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public SemanticKind Kind => Object.Kind(Data.Long("kind_id"));
    public FunctionEhDetails? FunctionEh => Object.Link(Data.OptionalLong("function_eh_id"), value => Object.Detail("function_eh", value, r => new FunctionEhDetails(Object, r)));
    public CleanupRegion? Region => Object.Link(Data.OptionalLong("region_id"), value => Object.Region(value));
    public BigInteger? StateNumber => Number(Data.OptionalText("state_number"));
    public FunctionSymbol? Routine => Object.Link(Data.OptionalLong("routine_id"), value => Object.Function(value));
    public ExceptionHandler? Handler => Object.Link(Data.OptionalLong("handler_id"), value => Object.Handler(value));
    public TryDescriptor? TryBlock => Object.Link(Data.OptionalLong("try_block_id"), value => Object.TryDescriptor(value));
    public ExpressionOperation? TryExpression => Object.Link(Data.OptionalLong("try_expression_id"), value => Object.Expression(value));
    public ExpressionOperation? CatchExpression => Object.Link(Data.OptionalLong("catch_expression_id"), value => Object.Expression(value));
    public Initialization? Initialization => Object.Link(Data.OptionalLong("initialization_id"), value => Object.Initialization(value));
}

public sealed class CleanupStateOperationDetails : SemanticDetail
{
    internal CleanupStateOperationDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public CleanupRegion? Region => Object.Link(Data.OptionalLong("region_id"), value => Object.Region(value));
    public BigInteger? StateNumber => Number(Data.OptionalText("state_number"));
    public bool IsUnreachable => Data.Bool("is_unreachable");
}

public sealed class TemplateDefinitionDetails : SemanticDetail
{
    internal TemplateDefinitionDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public SemanticKind Kind => Object.Kind(Data.Long("kind_id"));
    public SemanticEntity? Pattern => Object.Link(Data.OptionalLong("pattern_id"), value => Object.Entity(value));
    public ExpressionOperation? Constraint => Object.Link(Data.OptionalLong("constraint_id"), value => Object.Expression(value));
    public TemplateDescriptor? CanonicalTemplate => Object.Link(Data.OptionalLong("canonical_template_id"), value => Object.Template(value));
    public TemplateDescriptor? DefinitionTemplate => Object.Link(Data.OptionalLong("definition_template_id"), value => Object.Template(value));
    public TemplateDescriptor? PrototypeTemplate => Object.Link(Data.OptionalLong("prototype_template_id"), value => Object.Template(value));
    public bool IsExported => Data.Bool("is_exported");
    public bool IgnoreExport => Data.Bool("ignore_export");
    public bool IsPack => Data.Bool("is_pack");
    public bool IsFriendTemplate => Data.Bool("is_friend_template");
    public long? ParameterNumber => Data.OptionalLong("parameter_number");
    public long? ParameterDepth => Data.OptionalLong("parameter_depth");
    public string? DefinitionText => Object.Text(Data.OptionalLong("definition_text_id"));
    public SourcePosition? ExportPosition => Object.Link(Data.OptionalLong("export_position_id"), value => Object.Position(value));
    public SourceRange? DefinitionRange => Object.Link(Data.OptionalLong("definition_range_id"), value => Object.Range(value));
    public Symbol? Symbol => Object.Link(Data.OptionalLong("symbol_id"), value => Object.Symbol(value));
    public IReadOnlyList<RoutineSymbolDetails> RoutineSymbolRows => Object.List("SELECT * FROM routine_symbol WHERE template_id=$owner ORDER BY id", Data.Long("id"), r => new RoutineSymbolDetails(Object, r), "routine_symbol");
    public IReadOnlyList<EnumSymbolDetails> EnumSymbolRows => Object.List("SELECT * FROM enum_symbol WHERE template_id=$owner ORDER BY id", Data.Long("id"), r => new EnumSymbolDetails(Object, r), "enum_symbol");
    public IReadOnlyList<DependentExpressionDetails> DependentExpressionRows => Object.List("SELECT * FROM dependent_expression WHERE template_id=$owner ORDER BY id", Data.Long("id"), r => new DependentExpressionDetails(Object, r), "dependent_expression");
    public IReadOnlyList<TemplateHeaderDetails> TemplateHeaderRows => Object.List("SELECT * FROM template_header WHERE template_id=$owner ORDER BY id", Data.Long("id"), r => new TemplateHeaderDetails(Object, r), "template_header");
    public IReadOnlyList<TemplateParameterDetails> TemplateParameterRows => Object.List("SELECT * FROM template_parameter WHERE template_id=$owner ORDER BY depth,ordinal,id", Data.Long("id"), r => new TemplateParameterDetails(Object, r), "template_parameter");
    public IReadOnlyList<TemplateArgumentDetails> TemplateArgumentRows => Object.List("SELECT * FROM template_argument WHERE template_id=$owner ORDER BY id", Data.Long("id"), r => new TemplateArgumentDetails(Object, r), "template_argument");
    public IReadOnlyList<TemplateInstanceDetails> TemplateInstanceRows => Object.List("SELECT * FROM template_instance WHERE template_id=$owner ORDER BY id", Data.Long("id"), r => new TemplateInstanceDetails(Object, r), "template_instance");
}

public sealed class TemplateHeaderDetails : SemanticDetail
{
    internal TemplateHeaderDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public TemplateDescriptor? Template => Object.Link(Data.OptionalLong("template_id"), value => Object.Template(value));
    public TemplateHeader? Parent => Object.Link(Data.OptionalLong("parent_id"), value => Object.Header(value));
    public Scope? Scope => Object.Link(Data.OptionalLong("scope_id"), value => Object.Scope(value));
    public SourceInfo? SourceOrigin => Object.Link(Data.OptionalLong("source_origin_id"), value => Object.Source(value));
    public bool IsGeneric => Data.Bool("is_generic");
    public RequiresClause? RequiresClause => Object.Link(Data.OptionalLong("requires_clause_id"), value => Object.RequiresClause(value));
    public IReadOnlyList<TemplateParameterDetails> TemplateParameterRows => Object.List("SELECT * FROM template_parameter WHERE header_id=$owner ORDER BY depth,ordinal,id", Data.Long("id"), r => new TemplateParameterDetails(Object, r), "template_parameter");
    public IReadOnlyList<TemplateGenericConstraintClauseDetails> TemplateGenericConstraintClauseRows => Object.List("SELECT * FROM template_generic_constraint_clause WHERE header_id=$owner ORDER BY ordinal,id", Data.Long("id"), r => new TemplateGenericConstraintClauseDetails(Object, r), "template_generic_constraint_clause");
}

public sealed class TemplateParameterDetails : SemanticDetail
{
    internal TemplateParameterDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public TemplateDescriptor? Template => Object.Link(Data.OptionalLong("template_id"), value => Object.Template(value));
    public TemplateHeader? Header => Object.Link(Data.OptionalLong("header_id"), value => Object.Header(value));
    public long Ordinal => Data.Long("ordinal");
    public long Depth => Data.Long("depth");
    public SemanticKind Kind => Object.Kind(Data.Long("kind_id"));
    public bool IsPack => Data.Bool("is_pack");
    public bool IsAbbreviated => Data.Bool("is_abbreviated");
    public CppType? PlaceholderType => Object.Link(Data.OptionalLong("placeholder_type_id"), value => Object.Type(value));
    public Constant? PlaceholderConstant => Object.Link(Data.OptionalLong("placeholder_constant_id"), value => Object.Constant(value));
    public TemplateDescriptor? PlaceholderTemplate => Object.Link(Data.OptionalLong("placeholder_template_id"), value => Object.Template(value));
    public TemplateArgument? DefaultArgument => Object.Link(Data.OptionalLong("default_argument_id"), value => Object.Argument(value));
    public ExpressionOperation? Constraint => Object.Link(Data.OptionalLong("constraint_id"), value => Object.Expression(value));
}

public sealed class TemplateGenericConstraintClauseDetails : SemanticDetail
{
    internal TemplateGenericConstraintClauseDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public TemplateHeader Header => Object.Link(Data.OptionalLong("header_id"), value => Object.Header(value))!;
    public long Ordinal => Data.Long("ordinal");
    public CppType ParameterType => Object.Link(Data.OptionalLong("parameter_type_id"), value => Object.Type(value))!;
    public SourcePosition? TypePosition => Object.Link(Data.OptionalLong("type_position_id"), value => Object.Position(value));
    public SourcePosition? WherePosition => Object.Link(Data.OptionalLong("where_position_id"), value => Object.Position(value));
    public SourcePosition? ColonPosition => Object.Link(Data.OptionalLong("colon_position_id"), value => Object.Position(value));
    public IReadOnlyList<TemplateGenericConstraintDetails> TemplateGenericConstraintRows => Object.List("SELECT * FROM template_generic_constraint WHERE owner_id=$owner ORDER BY ordinal", Data.Long("id"), r => new TemplateGenericConstraintDetails(Object, r));
}

public sealed class TemplateGenericConstraintDetails : SemanticDetail
{
    internal TemplateGenericConstraintDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public GenericConstraintClause Owner => Object.Link(Data.OptionalLong("owner_id"), value => Object.GenericClause(value))!;
    public long Ordinal => Data.Long("ordinal");
    public SemanticKind Kind => Object.Kind(Data.Long("kind_id"));
    public CppType? Type => Object.Link(Data.OptionalLong("type_id"), value => Object.Type(value));
    public bool IsImplicit => Data.Bool("is_implicit");
    public SourcePosition? SourcePosition => Object.Link(Data.OptionalLong("source_position_id"), value => Object.Position(value));
}

public sealed class TemplateArgumentDetails : SemanticDetail
{
    internal TemplateArgumentDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public SemanticKind Kind => Object.Kind(Data.Long("kind_id"));
    public CppType? Type => Object.Link(Data.OptionalLong("type_id"), value => Object.Type(value));
    public Constant? Constant => Object.Link(Data.OptionalLong("constant_id"), value => Object.Constant(value));
    public TemplateDescriptor? Template => Object.Link(Data.OptionalLong("template_id"), value => Object.Template(value));
    public bool IsPack => Data.Bool("is_pack");
    public bool IsExplicit => Data.Bool("is_explicit");
    public bool IsPackElement => Data.Bool("is_pack_element");
    public bool IsEllipsis => Data.Bool("is_ellipsis");
    public bool IsIntegerPack => Data.Bool("is_integer_pack");
    public bool IsInjectedName => Data.Bool("is_injected_name");
    public bool IsProvisional => Data.Bool("is_provisional");
    public bool IsError => Data.Bool("is_error");
    public bool IsUnknownArrayBound => Data.Bool("is_unknown_array_bound");
    public IReadOnlyList<TemplateArgumentPackElementDetails> TemplateArgumentPackElementRows => Object.List("SELECT * FROM template_argument_pack_element WHERE owner_id=$owner ORDER BY ordinal", Data.Long("id"), r => new TemplateArgumentPackElementDetails(Object, r));
}

public sealed class TemplateArgumentPackElementDetails : SemanticDetail
{
    internal TemplateArgumentPackElementDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public TemplateArgument Owner => Object.Link(Data.OptionalLong("owner_id"), value => Object.Argument(value))!;
    public long Ordinal => Data.Long("ordinal");
    public TemplateArgument Argument => Object.Link(Data.OptionalLong("argument_id"), value => Object.Argument(value))!;
}

public sealed class TemplateInstanceDetails : SemanticDetail
{
    internal TemplateInstanceDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public SemanticEntity Result => Object.Link(Data.OptionalLong("result_id"), value => Object.Entity(value))!;
    public TemplateDescriptor? Template => Object.Link(Data.OptionalLong("template_id"), value => Object.Template(value));
    public TemplateSpecialization? SelectedSpecialization => Object.Link(Data.OptionalLong("selected_specialization_id"), value => Object.Specialization(value));
    public SemanticKind State => Object.Kind(Data.Long("state_id"));
    public SourcePosition? SourcePosition => Object.Link(Data.OptionalLong("source_position_id"), value => Object.Position(value));
    public bool IsDependent => Data.Bool("is_dependent");
    public bool IsPrototype => Data.Bool("is_prototype");
    public bool IsSuppressed => Data.Bool("is_suppressed");
    public bool IsExplicit => Data.Bool("is_explicit");
    public bool IsSpecialized => Data.Bool("is_specialized");
    public IReadOnlyList<TemplateInstanceArgumentDetails> TemplateInstanceArgumentRows => Object.List("SELECT * FROM template_instance_argument WHERE owner_id=$owner ORDER BY role_id,ordinal", Data.Long("id"), r => new TemplateInstanceArgumentDetails(Object, r));
}

public sealed class TemplateInstanceArgumentDetails : SemanticDetail
{
    internal TemplateInstanceArgumentDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public TemplateInstantiation Owner => Object.Link(Data.OptionalLong("owner_id"), value => Object.Instance(value))!;
    public SemanticKind Role => Object.Kind(Data.Long("role_id"));
    public long Ordinal => Data.Long("ordinal");
    public TemplateArgument Argument => Object.Link(Data.OptionalLong("argument_id"), value => Object.Argument(value))!;
}

public sealed class TemplateSpecializationDetails : SemanticDetail
{
    internal TemplateSpecializationDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public TemplateDescriptor PrimaryTemplate => Object.Link(Data.OptionalLong("primary_template_id"), value => Object.Template(value))!;
    public SemanticEntity Entity => Object.Link(Data.OptionalLong("entity_id"), value => Object.Entity(value))!;
    public SemanticKind Kind => Object.Kind(Data.Long("kind_id"));
}

public sealed class SourceFileDetails : SemanticDetail
{
    internal SourceFileDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public string Path => Object.Text(Data.OptionalLong("path_id"))!;
    public SemanticKind OriginKind => Object.Kind(Data.Long("origin_kind_id"));
}

public sealed class IncludeOccurrenceDetails : SemanticDetail
{
    internal IncludeOccurrenceDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public SourceFile File => Object.Link(Data.OptionalLong("file_id"), value => Object.File(value))!;
    public IncludeOccurrence? Parent => Object.Link(Data.OptionalLong("parent_id"), value => Object.Include(value));
    public long Ordinal => Data.Long("ordinal");
    public string? DisplayName => Object.Text(Data.OptionalLong("display_name_id"));
    public string? NameAsWritten => Object.Text(Data.OptionalLong("name_as_written_id"));
    public BigInteger? FirstSequence => Number(Data.OptionalText("first_sequence"));
    public BigInteger? LastSequence => Number(Data.OptionalText("last_sequence"));
    public BigInteger? FirstLine => Number(Data.OptionalText("first_line"));
    public CppModule? Module => Object.Link(Data.OptionalLong("module_id"), value => Object.Module(value));
    public bool IsSystem => Data.Bool("is_system");
    public bool UsesAngleBrackets => Data.Bool("uses_angle_brackets");
    public bool IsIncludeFile => Data.Bool("is_include_file");
    public bool IsPreinclude => Data.Bool("is_preinclude");
    public bool IsMacrosOnly => Data.Bool("is_macros_only");
    public bool IsTopLevel => Data.Bool("is_top_level");
    public bool IsTopLevelFromPch => Data.Bool("is_top_level_from_pch");
    public bool IsImplicitInclude => Data.Bool("is_implicit_include");
}

public sealed class LineDirectiveDetails : SemanticDetail
{
    internal LineDirectiveDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public string PresumedName => Object.Text(Data.OptionalLong("presumed_name_id"))!;
    public BigInteger FirstSequence => RequiredNumber(Data.OptionalText("first_sequence"));
    public BigInteger LastSequence => RequiredNumber(Data.OptionalText("last_sequence"));
    public BigInteger FirstLine => RequiredNumber(Data.OptionalText("first_line"));
    public IncludeOccurrence? Include => Object.Link(Data.OptionalLong("include_id"), value => Object.Include(value));
    public LineDirective? Parent => Object.Link(Data.OptionalLong("parent_id"), value => Object.LineDirective(value));
}

public sealed class SourceDescriptorChildDetails : SemanticDetail
{
    internal SourceDescriptorChildDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public SemanticEntity Owner => Object.Link(Data.OptionalLong("owner_id"), value => Object.Entity(value))!;
    public long Ordinal => Data.Long("ordinal");
    public SemanticEntity Child => Object.Link(Data.OptionalLong("child_id"), value => Object.Entity(value))!;
}

public sealed class SourceMappingSegmentDetails : SemanticDetail
{
    internal SourceMappingSegmentDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public BigInteger FirstSequence => RequiredNumber(Data.OptionalText("first_sequence"));
    public BigInteger LastSequence => RequiredNumber(Data.OptionalText("last_sequence"));
    public SourceFile File => Object.Link(Data.OptionalLong("file_id"), value => Object.File(value))!;
    public IncludeOccurrence? Include => Object.Link(Data.OptionalLong("include_id"), value => Object.Include(value));
    public LineDirective? LineDirective => Object.Link(Data.OptionalLong("line_directive_id"), value => Object.LineDirective(value));
    public BigInteger LogicalLine => RequiredNumber(Data.OptionalText("logical_line"));
    public BigInteger? PhysicalLine => Number(Data.OptionalText("physical_line"));
    public string? PresumedName => Object.Text(Data.OptionalLong("presumed_name_id"));
    public bool EndsAtEndOfSource => Data.Bool("ends_at_end_of_source");
}

public sealed class SourcePositionDetails : SemanticDetail
{
    internal SourcePositionDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public BigInteger Sequence => RequiredNumber(Data.OptionalText("sequence"));
    public long Column => Data.Long("column");
    public BigInteger? OriginalSequence => Number(Data.OptionalText("original_sequence"));
    public long? OriginalColumn => Data.OptionalLong("original_column");
    public MacroInvocation? MacroInvocation => Object.Link(Data.OptionalLong("macro_invocation_id"), value => Object.MacroInvocation(value));
    public SourceFile? File => Object.Link(Data.OptionalLong("file_id"), value => Object.File(value));
    public string? PresumedName => Object.Text(Data.OptionalLong("presumed_name_id"));
    public BigInteger? LogicalLine => Number(Data.OptionalText("logical_line"));
    public BigInteger? PhysicalLine => Number(Data.OptionalText("physical_line"));
    public IncludeOccurrence? Include => Object.Link(Data.OptionalLong("include_id"), value => Object.Include(value));
    public SourceMappingSegment? MappingSegment => Object.Link(Data.OptionalLong("mapping_segment_id"), value => Object.Mapping(value));
    public SourcePosition? SpellingPosition => Object.Link(Data.OptionalLong("spelling_position_id"), value => Object.Position(value));
    public bool AtEndOfSource => Data.Bool("at_end_of_source");
    public SemanticKind OriginKind => Object.Kind(Data.Long("origin_kind_id"));
}

public sealed class SourceRangeDetails : SemanticDetail
{
    internal SourceRangeDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public SourcePosition Start => Object.Link(Data.OptionalLong("start_id"), value => Object.Position(value))!;
    public SourcePosition End => Object.Link(Data.OptionalLong("end_id"), value => Object.Position(value))!;
    public SemanticKind EndpointKind => Object.Kind(Data.Long("endpoint_kind_id"));
}

public sealed class SourceOriginDetails : SemanticDetail
{
    internal SourceOriginDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public SourcePosition? Position => Object.Link(Data.OptionalLong("position_id"), value => Object.Position(value));
    public SourceRange? Range => Object.Link(Data.OptionalLong("range_id"), value => Object.Range(value));
    public SemanticKind Kind => Object.Kind(Data.Long("kind_id"));
}

public sealed class MacroDefinitionDetails : SemanticDetail
{
    internal MacroDefinitionDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public string Text => Object.Text(Data.OptionalLong("text_id"))!;
    public SourceRange? ReplacementRange => Object.Link(Data.OptionalLong("replacement_range_id"), value => Object.Range(value));
    public SemanticKind Kind => Object.Kind(Data.Long("kind_id"));
    public bool IsFunctionLike => Data.Bool("is_function_like");
    public bool IsPredefined => Data.Bool("is_predefined");
    public bool IsUndef => Data.Bool("is_undef");
}

public sealed class MacroInvocationDetails : SemanticDetail
{
    internal MacroInvocationDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public MacroDefinition Definition => Object.Link(Data.OptionalLong("definition_id"), value => Object.MacroDefinition(value))!;
    public MacroInvocation? Parent => Object.Link(Data.OptionalLong("parent_id"), value => Object.MacroInvocation(value));
    public SourcePosition? Position => Object.Link(Data.OptionalLong("position_id"), value => Object.Position(value));
    public SourcePosition? EndPosition => Object.Link(Data.OptionalLong("end_position_id"), value => Object.Position(value));
    public string? ArgumentText => Object.Text(Data.OptionalLong("argument_text_id"));
    public long Ordinal => Data.Long("ordinal");
}

public sealed class ModuleDetails : SemanticDetail
{
    internal ModuleDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public SemanticKind Kind => Object.Kind(Data.Long("kind_id"));
    public string? ResolvedFile => Object.Text(Data.OptionalLong("resolved_file_id"));
    public SemanticKind FileKind => Object.Kind(Data.Long("file_kind_id"));
    public bool ContainsUnsupportedConstructs => Data.Bool("contains_unsupported_constructs");
    public string? Name => Object.Text(Data.OptionalLong("name_id"));
    public string? ResolvedHeader => Object.Text(Data.OptionalLong("resolved_header_id"));
    public CppModule? Unit => Object.Link(Data.OptionalLong("unit_id"), value => Object.Module(value));
    public bool UsesAngleBrackets => Data.Bool("uses_angle_brackets");
    public bool SuppressMacroExport => Data.Bool("suppress_macro_export");
    public bool IsInternalPartition => Data.Bool("is_internal_partition");
}

public sealed class ModuleImportDetails : SemanticDetail
{
    internal ModuleImportDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public CppModule Module => Object.Link(Data.OptionalLong("module_id"), value => Object.Module(value))!;
    public SourcePosition? SourcePosition => Object.Link(Data.OptionalLong("source_position_id"), value => Object.Position(value));
    public SourcePosition? ModuleNamePosition => Object.Link(Data.OptionalLong("module_name_position_id"), value => Object.Position(value));
    public bool ImplementationUnitImportingSelf => Data.Bool("implementation_unit_importing_self");
}

public sealed class TranslationUnitModuleImportDetails : SemanticDetail
{
    internal TranslationUnitModuleImportDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public TranslationUnit Owner => Object.Link(Data.OptionalLong("owner_id"), value => Object.Unit(value))!;
    public long Ordinal => Data.Long("ordinal");
    public ModuleImport Import => Object.Link(Data.OptionalLong("import_id"), value => Object.ModuleImport(value))!;
}

public sealed class SourceEntryDetails : SemanticDetail
{
    internal SourceEntryDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public SemanticKind TagKind => Object.Kind(Data.Long("tag_kind_id"));
    public SemanticEntity? Target => Object.Link(Data.OptionalLong("target_id"), value => Object.Entity(value));
    public Declaration? Declaration => Object.Link(Data.OptionalLong("declaration_id"), value => Object.Declaration(value));
}

public sealed class ScopeSourceEntryDetails : SemanticDetail
{
    internal ScopeSourceEntryDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public Scope Owner => Object.Link(Data.OptionalLong("owner_id"), value => Object.Scope(value))!;
    public long Ordinal => Data.Long("ordinal");
    public SourceEntry Entry => Object.Link(Data.OptionalLong("entry_id"), value => Object.SourceEntry(value))!;
}

public sealed class SourceSublistDetails : SemanticDetail
{
    internal SourceSublistDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public IReadOnlyList<SourceSublistEntryDetails> SourceSublistEntryRows => Object.List("SELECT * FROM source_sublist_entry WHERE owner_id=$owner ORDER BY ordinal", Data.Long("id"), r => new SourceSublistEntryDetails(Object, r));
}

public sealed class SourceSublistEntryDetails : SemanticDetail
{
    internal SourceSublistEntryDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public SourceSublist Owner => Object.Link(Data.OptionalLong("owner_id"), value => Object.SourceSublist(value))!;
    public long Ordinal => Data.Long("ordinal");
    public SourceEntry Entry => Object.Link(Data.OptionalLong("entry_id"), value => Object.SourceEntry(value))!;
}

public sealed class ScopeSourceSublistDetails : SemanticDetail
{
    internal ScopeSourceSublistDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public Scope Owner => Object.Link(Data.OptionalLong("owner_id"), value => Object.Scope(value))!;
    public long Ordinal => Data.Long("ordinal");
    public SourceSublist Sublist => Object.Link(Data.OptionalLong("sublist_id"), value => Object.SourceSublist(value))!;
}

public sealed class SecondaryDeclarationDetails : SemanticDetail
{
    internal SecondaryDeclarationDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public SemanticKind EntityTagKind => Object.Kind(Data.Long("entity_tag_kind_id"));
    public CppType? DeclaredType => Object.Link(Data.OptionalLong("declared_type_id"), value => Object.Type(value));
    public NameReference? NameReference => Object.Link(Data.OptionalLong("name_reference_id"), value => Object.NameReference(value));
    public SemanticKind DeclaredStorageClass => Object.Kind(Data.Long("declared_storage_class_id"));
    public bool AutonomousTagDeclaration => Data.Bool("autonomous_tag_declaration");
    public bool EmbedsSourceEntries => Data.Bool("embeds_source_entries");
    public bool IsFriend => Data.Bool("is_friend");
    public bool DeclaredInFunctionPrototype => Data.Bool("declared_in_function_prototype");
    public bool SpecializedWithNewSyntax => Data.Bool("specialized_with_new_syntax");
    public bool? IsFirstDeclaration => Data.OptionalBool("is_first_declaration");
    public bool? IsPartialInstantiation => Data.OptionalBool("is_partial_instantiation");
    public bool? IsCompilerGeneratedForwardDeclaration => Data.OptionalBool("is_compiler_generated_forward_declaration");
    public bool? IsOriginallyNonautonomousDefinition => Data.OptionalBool("is_originally_nonautonomous_definition");
    public bool IsGnuExtension => Data.Bool("is_gnu_extension");
    public bool IsAfterFirstDeclarator => Data.Bool("is_after_first_declarator");
    public bool HasExplicitStorageClass => Data.Bool("has_explicit_storage_class");
    public bool IsAlias => Data.Bool("is_alias");
    public bool? IsEventInterface => Data.OptionalBool("is_event_interface");
}

public sealed class DeclarationPositionsDetails : SemanticDetail
{
    internal DeclarationPositionsDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public SemanticEntity Owner => Object.Link(Data.OptionalLong("owner_id"), value => Object.Entity(value))!;
    public SourceRange? IdentifierRange => Object.Link(Data.OptionalLong("identifier_range_id"), value => Object.Range(value));
    public SourceRange? SpecifiersRange => Object.Link(Data.OptionalLong("specifiers_range_id"), value => Object.Range(value));
    public SourceRange? DeclaratorRange => Object.Link(Data.OptionalLong("declarator_range_id"), value => Object.Range(value));
    public SourceRange? EnumValueRange => Object.Link(Data.OptionalLong("enum_value_range_id"), value => Object.Range(value));
    public SourceRange? NamespaceDefinitionRange => Object.Link(Data.OptionalLong("namespace_definition_range_id"), value => Object.Range(value));
}

public sealed class DeclarationElementPositionDetails : SemanticDetail
{
    internal DeclarationElementPositionDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public SemanticEntity Owner => Object.Link(Data.OptionalLong("owner_id"), value => Object.Entity(value))!;
    public long Ordinal => Data.Long("ordinal");
    public SemanticKind Kind => Object.Kind(Data.Long("kind_id"));
    public SourcePosition? Position => Object.Link(Data.OptionalLong("position_id"), value => Object.Position(value));
}

public sealed class SourceEndConstructDetails : SemanticDetail
{
    internal SourceEndConstructDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public SemanticKind TargetTagKind => Object.Kind(Data.Long("target_tag_kind_id"));
    public SemanticEntity? Target => Object.Link(Data.OptionalLong("target_id"), value => Object.Entity(value));
    public SourcePosition? Position => Object.Link(Data.OptionalLong("position_id"), value => Object.Position(value));
}

public sealed class InstantiationDirectiveDetails : SemanticDetail
{
    internal InstantiationDirectiveDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public SemanticKind TargetTagKind => Object.Kind(Data.Long("target_tag_kind_id"));
    public SemanticEntity? Target => Object.Link(Data.OptionalLong("target_id"), value => Object.Entity(value));
    public CppType? DeclaredType => Object.Link(Data.OptionalLong("declared_type_id"), value => Object.Type(value));
    public SourceInfo? SourceOrigin => Object.Link(Data.OptionalLong("source_origin_id"), value => Object.Source(value));
    public bool DoNotInstantiate => Data.Bool("do_not_instantiate");
}

public sealed class LinkageSpecificationDetails : SemanticDetail
{
    internal LinkageSpecificationDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public Constant NameString => Object.Link(Data.OptionalLong("name_string_id"), value => Object.Constant(value))!;
    public SemanticKind NameLinkage => Object.Kind(Data.Long("name_linkage_id"));
    public SourcePosition? Position => Object.Link(Data.OptionalLong("position_id"), value => Object.Position(value));
    public SourcePosition? EndPosition => Object.Link(Data.OptionalLong("end_position_id"), value => Object.Position(value));
}

public sealed class MicrosoftIfExistsDetails : SemanticDetail
{
    internal MicrosoftIfExistsDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public SemanticKind TargetTagKind => Object.Kind(Data.Long("target_tag_kind_id"));
    public SemanticEntity? Target => Object.Link(Data.OptionalLong("target_id"), value => Object.Entity(value));
    public NameReference? NameReference => Object.Link(Data.OptionalLong("name_reference_id"), value => Object.NameReference(value));
    public SourcePosition? Position => Object.Link(Data.OptionalLong("position_id"), value => Object.Position(value));
    public bool? IsIfExists => Data.OptionalBool("is_if_exists");
    public bool IsThis => Data.Bool("is_this");
}

public sealed class RecordDeclarationDetails : SemanticDetail
{
    internal RecordDeclarationDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public RecordType RecordType => Object.Link(Data.OptionalLong("record_type_id"), value => Object.RecordType(value))!;
    public long Ordinal => Data.Long("ordinal");
    public SemanticKind TagKind => Object.Kind(Data.Long("tag_kind_id"));
    public SemanticEntity? Target => Object.Link(Data.OptionalLong("target_id"), value => Object.Entity(value));
    public Declaration? Declaration => Object.Link(Data.OptionalLong("declaration_id"), value => Object.Declaration(value));
}

public sealed class PragmaDetails : SemanticDetail
{
    internal PragmaDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public SemanticKind Kind => Object.Kind(Data.Long("kind_id"));
    public SemanticKind TargetTagKind => Object.Kind(Data.Long("target_tag_kind_id"));
    public SemanticEntity? Target => Object.Link(Data.OptionalLong("target_id"), value => Object.Entity(value));
    public SourceInfo? SourceOrigin => Object.Link(Data.OptionalLong("source_origin_id"), value => Object.Source(value));
    public SourceEntry? SourceEntry => Object.Link(Data.OptionalLong("source_entry_id"), value => Object.SourceEntry(value));
    public string? Text => Object.Text(Data.OptionalLong("text_id"));
    public bool IgnoreInBackEnd => Data.Bool("ignore_in_back_end");
    public bool IsMicrosoftOperator => Data.Bool("is_microsoft_operator");
}

public sealed class ScopePragmaDetails : SemanticDetail
{
    internal ScopePragmaDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public Scope Owner => Object.Link(Data.OptionalLong("owner_id"), value => Object.Scope(value))!;
    public long Ordinal => Data.Long("ordinal");
    public PragmaDirective Pragma => Object.Link(Data.OptionalLong("pragma_id"), value => Object.Pragma(value))!;
}

public sealed class ScopeIfExistsDetails : SemanticDetail
{
    internal ScopeIfExistsDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public Scope Owner => Object.Link(Data.OptionalLong("owner_id"), value => Object.Scope(value))!;
    public long Ordinal => Data.Long("ordinal");
    public MicrosoftIfExists IfExists => Object.Link(Data.OptionalLong("if_exists_id"), value => Object.IfExists(value))!;
}

public sealed class PragmaStdcDetails : SemanticDetail
{
    internal PragmaStdcDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public SemanticKind Kind => Object.Kind(Data.Long("kind_id"));
    public SemanticKind Value => Object.Kind(Data.Long("value_id"));
}

public sealed class PragmaUpcDetails : SemanticDetail
{
    internal PragmaUpcDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public SemanticKind Kind => Object.Kind(Data.Long("kind_id"));
    public SemanticKind? AccessMethod => Object.Link(Data.OptionalLong("access_method_id"), value => Object.Kind(value));
    public SemanticKind? CoherenceOperation => Object.Link(Data.OptionalLong("coherence_operation_id"), value => Object.Kind(value));
}

public sealed class PragmaIdentDetails : SemanticDetail
{
    internal PragmaIdentDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public Constant StringConstant => Object.Link(Data.OptionalLong("string_constant_id"), value => Object.Constant(value))!;
}

public sealed class PragmaCommentDetails : SemanticDetail
{
    internal PragmaCommentDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public SemanticKind Kind => Object.Kind(Data.Long("kind_id"));
    public Constant? StringConstant => Object.Link(Data.OptionalLong("string_constant_id"), value => Object.Constant(value));
}

public sealed class PragmaConformDetails : SemanticDetail
{
    internal PragmaConformDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public SemanticKind Kind => Object.Kind(Data.Long("kind_id"));
    public bool IsOn => Data.Bool("is_on");
    public bool IsOff => Data.Bool("is_off");
    public bool IsShow => Data.Bool("is_show");
    public bool IsPush => Data.Bool("is_push");
    public bool IsPop => Data.Bool("is_pop");
    public string? Identifier => Object.Text(Data.OptionalLong("identifier_id"));
}

public sealed class PragmaIncludeAliasDetails : SemanticDetail
{
    internal PragmaIncludeAliasDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public string? LongFileName => Object.Text(Data.OptionalLong("long_file_name_id"));
    public string? ShortFileName => Object.Text(Data.OptionalLong("short_file_name_id"));
}

public sealed class PragmaGccDetails : SemanticDetail
{
    internal PragmaGccDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public SemanticKind Kind => Object.Kind(Data.Long("kind_id"));
    public SemanticKind? Visibility => Object.Link(Data.OptionalLong("visibility_id"), value => Object.Kind(value));
}

public sealed class TyperefOriginalTemplateArgumentDetails : SemanticDetail
{
    internal TyperefOriginalTemplateArgumentDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public TypeReferenceDetails Owner => Object.Link(Data.OptionalLong("owner_id"), value => Object.Detail("type_reference", value, r => new TypeReferenceDetails(Object, r)))!;
    public long Ordinal => Data.Long("ordinal");
    public TemplateArgument Argument => Object.Link(Data.OptionalLong("argument_id"), value => Object.Argument(value))!;
}

public sealed class MsAttributeDetails : SemanticDetail
{
    internal MsAttributeDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public SemanticKind Kind => Object.Kind(Data.Long("kind_id"));
    public string? Name => Object.Text(Data.OptionalLong("name_id"));
    public string? Text => Object.Text(Data.OptionalLong("text_id"));
    public SemanticKind TargetTagKind => Object.Kind(Data.Long("target_tag_kind_id"));
    public SemanticEntity? Target => Object.Link(Data.OptionalLong("target_id"), value => Object.Entity(value));
    public SourcePosition? SourcePosition => Object.Link(Data.OptionalLong("source_position_id"), value => Object.Position(value));
    public SourceEntry? SourceEntry => Object.Link(Data.OptionalLong("source_entry_id"), value => Object.SourceEntry(value));
    public MicrosoftAttribute? NextInBlock => Object.Link(Data.OptionalLong("next_in_block_id"), value => Object.MicrosoftAttribute(value));
    public IReadOnlyList<MsAttributeArgumentLinkDetails> MsAttributeArgumentLinkRows => Object.List("SELECT * FROM ms_attribute_argument_link WHERE owner_id=$owner ORDER BY ordinal", Data.Long("id"), r => new MsAttributeArgumentLinkDetails(Object, r));
    public IReadOnlyList<MsAttributeTargetDetails> MsAttributeTargetRows => Object.List("SELECT * FROM ms_attribute_target WHERE owner_id=$owner ORDER BY ordinal", Data.Long("id"), r => new MsAttributeTargetDetails(Object, r));
}

public sealed class MsAttributeArgumentDetails : SemanticDetail
{
    internal MsAttributeArgumentDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public SemanticKind Kind => Object.Kind(Data.Long("kind_id"));
    public string? ParameterName => Object.Text(Data.OptionalLong("parameter_name_id"));
    public BigInteger? IntegerValue => Number(Data.OptionalText("integer_value"));
    public bool? BooleanValue => Data.OptionalBool("boolean_value");
    public long? EnumerationOrdinal => Data.OptionalLong("enumeration_ordinal");
    public Constant? StringConstant => Object.Link(Data.OptionalLong("string_constant_id"), value => Object.Constant(value));
    public string? OtherText => Object.Text(Data.OptionalLong("other_text_id"));
    public string? UuidText => Object.Text(Data.OptionalLong("uuid_text_id"));
}

public sealed class OwnerMsAttributeDetails : SemanticDetail
{
    internal OwnerMsAttributeDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public SemanticEntity Owner => Object.Link(Data.OptionalLong("owner_id"), value => Object.Entity(value))!;
    public long Ordinal => Data.Long("ordinal");
    public MicrosoftAttribute Attribute => Object.Link(Data.OptionalLong("attribute_id"), value => Object.MicrosoftAttribute(value))!;
}

public sealed class MsAttributeArgumentLinkDetails : SemanticDetail
{
    internal MsAttributeArgumentLinkDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public MicrosoftAttribute Owner => Object.Link(Data.OptionalLong("owner_id"), value => Object.MicrosoftAttribute(value))!;
    public long Ordinal => Data.Long("ordinal");
    public MicrosoftAttributeArgument Argument => Object.Link(Data.OptionalLong("argument_id"), value => Object.MicrosoftAttributeArgument(value))!;
}

public sealed class MsAttributeTargetDetails : SemanticDetail
{
    internal MsAttributeTargetDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public MicrosoftAttribute Owner => Object.Link(Data.OptionalLong("owner_id"), value => Object.MicrosoftAttribute(value))!;
    public long Ordinal => Data.Long("ordinal");
    public SemanticKind Kind => Object.Kind(Data.Long("kind_id"));
}

public sealed class ScopedExpressionDetails : SemanticDetail
{
    internal ScopedExpressionDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public ExpressionOperation Expression => Object.Link(Data.OptionalLong("expression_id"), value => Object.Expression(value))!;
}

public sealed class LocalExpressionReferenceDetails : SemanticDetail
{
    internal LocalExpressionReferenceDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public Scope Owner => Object.Link(Data.OptionalLong("owner_id"), value => Object.Scope(value))!;
    public long Ordinal => Data.Long("ordinal");
    public SemanticKind Kind => Object.Kind(Data.Long("kind_id"));
    public SemanticEntity Referrer => Object.Link(Data.OptionalLong("referrer_id"), value => Object.Entity(value))!;
    public ExpressionOperation Expression => Object.Link(Data.OptionalLong("expression_id"), value => Object.Expression(value))!;
}

public sealed class TemplateSymbolDetails : SemanticDetail
{
    internal TemplateSymbolDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public TemplateDescriptor Descriptor => Object.Link(Data.OptionalLong("descriptor_id"), value => Object.Template(value))!;
}

public sealed class LocalStaticInitializationDetails : SemanticDetail
{
    internal LocalStaticInitializationDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public VariableSymbol Variable => Object.Link(Data.OptionalLong("variable_id"), value => Object.Variable(value))!;
    public SemanticKind InitializationKind => Object.Kind(Data.Long("initialization_kind_id"));
    public Initialization? Initialization => Object.Link(Data.OptionalLong("initialization_id"), value => Object.Initialization(value));
    public Constant? Constant => Object.Link(Data.OptionalLong("constant_id"), value => Object.Constant(value));
    public Lifetime? Lifetime => Object.Link(Data.OptionalLong("lifetime_id"), value => Object.Lifetime(value));
}

public sealed class ScopeLocalStaticInitializationDetails : SemanticDetail
{
    internal ScopeLocalStaticInitializationDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public Scope Owner => Object.Link(Data.OptionalLong("owner_id"), value => Object.Scope(value))!;
    public long Ordinal => Data.Long("ordinal");
    public LocalStaticInitialization Initialization => Object.Link(Data.OptionalLong("initialization_id"), value => Object.LocalStaticInitialization(value))!;
}

public sealed class ConstructorInitializerDetails : SemanticDetail
{
    internal ConstructorInitializerDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public SemanticKind Kind => Object.Kind(Data.Long("kind_id"));
    public FieldSymbol? Field => Object.Link(Data.OptionalLong("field_id"), value => Object.Field(value));
    public BaseSubobject? BaseSubobject => Object.Link(Data.OptionalLong("base_subobject_id"), value => Object.Subobject(value));
    public Initialization? Initialization => Object.Link(Data.OptionalLong("initialization_id"), value => Object.Initialization(value));
    public ExpressionOperation? SourceExpression => Object.Link(Data.OptionalLong("source_expression_id"), value => Object.Expression(value));
    public bool CompilerGenerated => Data.Bool("compiler_generated");
    public bool IsPackExpansion => Data.Bool("is_pack_expansion");
    public bool IsBraced => Data.Bool("is_braced");
    public bool UsesFieldInitializer => Data.Bool("uses_field_initializer");
    public CppType? OriginalType => Object.Link(Data.OptionalLong("original_type_id"), value => Object.Type(value));
    public SourceRange? SourceRange => Object.Link(Data.OptionalLong("source_range_id"), value => Object.Range(value));
}

public sealed class ScopeConstructorInitializerDetails : SemanticDetail
{
    internal ScopeConstructorInitializerDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public Scope Owner => Object.Link(Data.OptionalLong("owner_id"), value => Object.Scope(value))!;
    public long Ordinal => Data.Long("ordinal");
    public ConstructorInitializer Initializer => Object.Link(Data.OptionalLong("initializer_id"), value => Object.ConstructorInitializer(value))!;
}

public sealed class DataMemberSpecDetails : SemanticDetail
{
    internal DataMemberSpecDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public CppType Type => Object.Link(Data.OptionalLong("type_id"), value => Object.Type(value))!;
    public string? Name => Object.Text(Data.OptionalLong("name_id"));
    public BigInteger RequestedAlignment => RequiredNumber(Data.OptionalText("requested_alignment"));
    public BigInteger BitWidth => RequiredNumber(Data.OptionalText("bit_width"));
    public bool NoUniqueAddress => Data.Bool("no_unique_address");
}

public sealed class RequiresClauseDetails : SemanticDetail
{
    internal RequiresClauseDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public ExpressionOperation ConstraintExpression => Object.Link(Data.OptionalLong("constraint_expression_id"), value => Object.Expression(value))!;
    public SourcePosition? RequiresPosition => Object.Link(Data.OptionalLong("requires_position_id"), value => Object.Position(value));
}

public sealed class GnuRoutineSupplementDetails : SemanticDetail
{
    internal GnuRoutineSupplementDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public string? Section => Object.Text(Data.OptionalLong("section_id"));
    public string? AssemblyName => Object.Text(Data.OptionalLong("assembly_name_id"));
    public FunctionSymbol? AliasedRoutine => Object.Link(Data.OptionalLong("aliased_routine_id"), value => Object.Function(value));
    public FunctionSymbol? InlinePartner => Object.Link(Data.OptionalLong("inline_partner_id"), value => Object.Function(value));
    public VariableSymbol? ResolverVariable => Object.Link(Data.OptionalLong("resolver_variable_id"), value => Object.Variable(value));
    public BigInteger? ConstructorPriority => Number(Data.OptionalText("constructor_priority"));
    public BigInteger? DestructorPriority => Number(Data.OptionalText("destructor_priority"));
    public bool? IsRepresentative => Data.OptionalBool("is_representative");
    public bool? IsTargetSpecificVersion => Data.OptionalBool("is_target_specific_version");
    public bool? ResolverRequired => Data.OptionalBool("resolver_required");
    public FunctionSymbol? Representative => Object.Link(Data.OptionalLong("representative_id"), value => Object.Function(value));
    public IReadOnlyList<RoutineMultiversionDetails> RoutineMultiversionRows => Object.List("SELECT * FROM routine_multiversion WHERE owner_id=$owner ORDER BY ordinal", Data.Long("id"), r => new RoutineMultiversionDetails(Object, r));
}

public sealed class RoutineSelectedOverrideDetails : SemanticDetail
{
    internal RoutineSelectedOverrideDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public FunctionSymbol Owner => Object.Link(Data.OptionalLong("owner_id"), value => Object.Function(value))!;
    public long Ordinal => Data.Long("ordinal");
    public SemanticEntity Entity => Object.Link(Data.OptionalLong("entity_id"), value => Object.Entity(value))!;
}

public sealed class RoutineBefriendingClassDetails : SemanticDetail
{
    internal RoutineBefriendingClassDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public FunctionSymbol Owner => Object.Link(Data.OptionalLong("owner_id"), value => Object.Function(value))!;
    public long Ordinal => Data.Long("ordinal");
    public CppType Type => Object.Link(Data.OptionalLong("type_id"), value => Object.Type(value))!;
}

public sealed class RoutineMultiversionDetails : SemanticDetail
{
    internal RoutineMultiversionDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public GnuRoutineSupplement Owner => Object.Link(Data.OptionalLong("owner_id"), value => Object.GnuRoutineSupplement(value))!;
    public long Ordinal => Data.Long("ordinal");
    public FunctionSymbol Routine => Object.Link(Data.OptionalLong("routine_id"), value => Object.Function(value))!;
}

public sealed class RecordBasePostorderDetails : SemanticDetail
{
    internal RecordBasePostorderDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public RecordType Owner => Object.Link(Data.OptionalLong("owner_id"), value => Object.RecordType(value))!;
    public long Ordinal => Data.Long("ordinal");
    public BaseSubobject Target => Object.Link(Data.OptionalLong("target_id"), value => Object.Subobject(value))!;
}

public sealed class RecordBasePreorderDetails : SemanticDetail
{
    internal RecordBasePreorderDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public RecordType Owner => Object.Link(Data.OptionalLong("owner_id"), value => Object.RecordType(value))!;
    public long Ordinal => Data.Long("ordinal");
    public BaseSubobject Target => Object.Link(Data.OptionalLong("target_id"), value => Object.Subobject(value))!;
}

public sealed class RecordBefriendingClassDetails : SemanticDetail
{
    internal RecordBefriendingClassDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public RecordType Owner => Object.Link(Data.OptionalLong("owner_id"), value => Object.RecordType(value))!;
    public long Ordinal => Data.Long("ordinal");
    public CppType Target => Object.Link(Data.OptionalLong("target_id"), value => Object.Type(value))!;
}

public sealed class RecordFriendDetails : SemanticDetail
{
    internal RecordFriendDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public RecordType Owner => Object.Link(Data.OptionalLong("owner_id"), value => Object.RecordType(value))!;
    public long Ordinal => Data.Long("ordinal");
    public SemanticEntity Target => Object.Link(Data.OptionalLong("target_id"), value => Object.Entity(value))!;
}

public sealed class RecordPromotedTypeDetails : SemanticDetail
{
    internal RecordPromotedTypeDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public RecordType Owner => Object.Link(Data.OptionalLong("owner_id"), value => Object.RecordType(value))!;
    public long Ordinal => Data.Long("ordinal");
    public CppType Target => Object.Link(Data.OptionalLong("target_id"), value => Object.Type(value))!;
}

public sealed class FieldInitializerEntityDetails : SemanticDetail
{
    internal FieldInitializerEntityDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public FieldSymbol Owner => Object.Link(Data.OptionalLong("owner_id"), value => Object.Field(value))!;
    public long Ordinal => Data.Long("ordinal");
    public SemanticEntity Entity => Object.Link(Data.OptionalLong("entity_id"), value => Object.Entity(value))!;
}

public sealed class OwnerDeclModifierDetails : SemanticDetail
{
    internal OwnerDeclModifierDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public SemanticEntity Owner => Object.Link(Data.OptionalLong("owner_id"), value => Object.Entity(value))!;
    public SemanticKind Kind => Object.Kind(Data.Long("kind_id"));
}

public sealed class RoutineTargetArchitectureDetails : SemanticDetail
{
    internal RoutineTargetArchitectureDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public GnuRoutineSupplement Owner => Object.Link(Data.OptionalLong("owner_id"), value => Object.GnuRoutineSupplement(value))!;
    public SemanticKind Kind => Object.Kind(Data.Long("kind_id"));
}

public sealed class RecordEventInterfaceDetails : SemanticDetail
{
    internal RecordEventInterfaceDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public RecordType Owner => Object.Link(Data.OptionalLong("owner_id"), value => Object.RecordType(value))!;
    public long Ordinal => Data.Long("ordinal");
    public CppType Target => Object.Link(Data.OptionalLong("target_id"), value => Object.Type(value))!;
    public SourceInfo? SourceOrigin => Object.Link(Data.OptionalLong("source_origin_id"), value => Object.Source(value));
}

public sealed class RecordVcallOffsetDetails : SemanticDetail
{
    internal RecordVcallOffsetDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public RecordType Owner => Object.Link(Data.OptionalLong("owner_id"), value => Object.RecordType(value))!;
    public long Ordinal => Data.Long("ordinal");
    public FunctionSymbol? Routine => Object.Link(Data.OptionalLong("routine_id"), value => Object.Function(value));
    public BaseSubobject? BaseSubobject => Object.Link(Data.OptionalLong("base_subobject_id"), value => Object.Subobject(value));
    public BigInteger VtableIndex => RequiredNumber(Data.OptionalText("vtable_index"));
    public bool IsPrimary => Data.Bool("is_primary");
}

public sealed class SourceCorrespondenceDetails : SemanticDetail
{
    internal SourceCorrespondenceDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public string? CurrentName => Object.Text(Data.OptionalLong("current_name_id"));
    public string? UnmangledNameOrMangledEncoding => Object.Text(Data.OptionalLong("unmangled_name_or_mangled_encoding_id"));
    public Scope? ParentScope => Object.Link(Data.OptionalLong("parent_scope_id"), value => Object.Scope(value));
    public FunctionSymbol? EnclosingRoutine => Object.Link(Data.OptionalLong("enclosing_routine_id"), value => Object.Function(value));
    public bool IsImportedFromModule => Data.Bool("is_imported_from_module");
    public SourceInfo? SourceOrigin => Object.Link(Data.OptionalLong("source_origin_id"), value => Object.Source(value));
    public SemanticKind Access => Object.Kind(Data.Long("access_id"));
    public bool Referenced => Data.Bool("referenced");
    public bool? Needed => Data.OptionalBool("needed");
    public SemanticKind Linkage => Object.Kind(Data.Long("linkage_id"));
    public bool IsLocalToFunction => Data.Bool("is_local_to_function");
    public bool IsClassMember => Data.Bool("is_class_member");
    public bool? NameHasBeenMangled => Data.OptionalBool("name_has_been_mangled");
    public bool? UnnamedEntityGivenFabricatedName => Data.OptionalBool("unnamed_entity_given_fabricated_name");
    public bool? IsDeclAfterFirstInCommaList => Data.OptionalBool("is_decl_after_first_in_comma_list");
    public bool CopiedFromSecondaryTransUnit => Data.Bool("copied_from_secondary_trans_unit");
    public bool SameNameAsExternalEntityInSecondaryTransUnit => Data.Bool("same_name_as_external_entity_in_secondary_trans_unit");
    public bool MemberOfUnknownBase => Data.Bool("member_of_unknown_base");
    public bool QualifiedUnknownBaseMember => Data.Bool("qualified_unknown_base_member");
    public bool? MemberOfUnknownSuper => Data.OptionalBool("member_of_unknown_super");
    public bool? MicrosoftIdentifierUsed => Data.OptionalBool("microsoft_identifier_used");
    public bool? MarkedAsGnuExtension => Data.OptionalBool("marked_as_gnu_extension");
    public bool IsDeprecatedOrUnavailable => Data.Bool("is_deprecated_or_unavailable");
    public bool Externalized => Data.Bool("externalized");
    public bool MaybeUnused => Data.Bool("maybe_unused");
    public SourceEntry? SourceSequenceEntry => Object.Link(Data.OptionalLong("source_sequence_entry_id"), value => Object.SourceEntry(value));
}

public sealed class EntitySourceCorrespondenceDetails : SemanticDetail
{
    internal EntitySourceCorrespondenceDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public SemanticEntity Owner => Object.Link(Data.OptionalLong("owner_id"), value => Object.Entity(value))!;
    public SourceCorrespondence SourceCorrespondence => Object.Link(Data.OptionalLong("source_correspondence_id"), value => Object.Correspondence(value))!;
}

public sealed class ScopeChildDetails : SemanticDetail
{
    internal ScopeChildDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public Scope Owner => Object.Link(Data.OptionalLong("owner_id"), value => Object.Scope(value))!;
    public long Ordinal => Data.Long("ordinal");
    public Scope Child => Object.Link(Data.OptionalLong("child_id"), value => Object.Scope(value))!;
}

public sealed class UpcThreadsValueDetails : SemanticDetail
{
    internal UpcThreadsValueDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public BigInteger Multiplier => RequiredNumber(Object.Text(Data.OptionalLong("multiplier_id")));
}

public sealed class ExpressionVlaDimensionDetails : SemanticDetail
{
    internal ExpressionVlaDimensionDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public ExpressionOperation Owner => Object.Link(Data.OptionalLong("owner_id"), value => Object.Expression(value))!;
    public long Ordinal => Data.Long("ordinal");
    public VlaDimension Dimension => Object.Link(Data.OptionalLong("dimension_id"), value => Object.VlaDimension(value))!;
}

public sealed class VariableInitializerEntityDetails : SemanticDetail
{
    internal VariableInitializerEntityDetails(RoslynObject owner, Row row) : base(owner, row) { }
    public VariableSymbol Owner => Object.Link(Data.OptionalLong("owner_id"), value => Object.Variable(value))!;
    public long Ordinal => Data.Long("ordinal");
    public SemanticEntity Entity => Object.Link(Data.OptionalLong("entity_id"), value => Object.Entity(value))!;
}

