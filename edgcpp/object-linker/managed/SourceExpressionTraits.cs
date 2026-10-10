namespace DotObj;

internal sealed partial class ObjectFile
{
    // Profile 43 binds these numeric opcodes to the embedded EDG IL enum.
    // Name, opcode, arity and type-only operand roles are checked together.
    private static (string Name, int Arity) SourceBuiltinTrait(long opcode) => opcode switch
    {
        1 => ("__has_assign", 1), // bok_has_assign
        2 => ("__has_copy", 1), // bok_has_copy
        3 => ("__has_nothrow_assign", 1), // bok_has_nothrow_assign
        4 => ("__has_nothrow_constructor", 1), // bok_has_nothrow_constructor
        5 => ("__has_nothrow_copy", 1), // bok_has_nothrow_copy
        6 => ("__has_trivial_assign", 1), // bok_has_trivial_assign
        7 => ("__has_trivial_constructor", 1), // bok_has_trivial_constructor
        8 => ("__has_trivial_copy", 1), // bok_has_trivial_copy
        9 => ("__has_trivial_destructor", 1), // bok_has_trivial_destructor
        10 => ("__has_user_destructor", 1), // bok_has_user_destructor
        11 => ("__has_virtual_destructor", 1), // bok_has_virtual_destructor
        12 => ("__is_abstract", 1), // bok_is_abstract
        13 => ("__is_base_of", 2), // bok_is_base_of
        14 => ("__is_class", 1), // bok_is_class
        15 => ("__is_convertible_to", 2), // bok_is_convertible_to
        16 => ("__is_empty", 1), // bok_is_empty
        17 => ("__is_enum", 1), // bok_is_enum
        18 => ("__is_pod", 1), // bok_is_pod
        19 => ("__is_polymorphic", 1), // bok_is_polymorphic
        20 => ("__is_union", 1), // bok_is_union
        21 => ("__builtin_types_compatible_p", 2), // bok_types_compatible
        23 => ("__is_trivial", 1), // bok_is_trivial
        24 => ("__is_standard_layout", 1), // bok_is_standard_layout
        25 => ("__is_trivially_copyable", 1), // bok_is_trivially_copyable
        26 => ("__is_literal_type", 1), // bok_is_literal_type
        27 => ("__has_trivial_move_constructor", 1), // bok_has_trivial_move_constructor
        28 => ("__has_trivial_move_assign", 1), // bok_has_trivial_move_assign
        29 => ("__has_nothrow_move_assign", 1), // bok_has_nothrow_move_assign
        30 => ("__is_constructible", -1), // bok_is_constructible
        31 => ("__is_nothrow_constructible", -1), // bok_is_nothrow_constructible
        32 => ("__has_finalizer", 1), // bok_has_finalizer
        33 => ("__is_delegate", 1), // bok_is_delegate
        34 => ("__is_interface_class", 1), // bok_is_interface_class
        35 => ("__is_ref_array", 1), // bok_is_ref_array
        36 => ("__is_ref_class", 1), // bok_is_ref_class
        37 => ("__is_sealed", 1), // bok_is_sealed
        38 => ("__is_simple_value_class", 1), // bok_is_simple_value_class
        39 => ("__is_value_class", 1), // bok_is_value_class
        40 => ("__is_final", 1), // bok_is_final
        41 => ("__is_trivially_constructible", -1), // bok_is_trivially_constructible
        42 => ("__is_destructible", 1), // bok_is_destructible
        43 => ("__is_nothrow_destructible", 1), // bok_is_nothrow_destructible
        44 => ("__is_trivially_destructible", 1), // bok_is_trivially_destructible
        45 => ("__is_nothrow_assignable", 2), // bok_is_nothrow_assignable
        46 => ("__is_trivially_assignable", 2), // bok_is_trivially_assignable
        49 => ("__is_valid_winrt_type", 1), // bok_is_valid_winrt_type
        50 => ("__is_win_class", 1), // bok_is_win_class
        51 => ("__is_win_interface", 1), // bok_is_win_interface
        54 => ("__is_assignable", 2), // bok_is_assignable
        55 => ("__is_trivially_copy_assignable", 1), // bok_is_trivially_copy_assignable
        56 => ("__is_assignable_no_precondition_check", 2), // bok_is_assignable_no_precondition_check
        58 => ("__has_unique_object_representations", 1), // bok_has_unique_object_representations
        59 => ("__is_aggregate", 1), // bok_is_aggregate
        60 => ("__reference_binds_to_temporary", 2), // bok_reference_binds_to_temporary
        61 => ("__is_same", 2), // bok_is_same
        62 => ("__is_same_as", 2), // bok_is_same_as
        63 => ("__is_function", 1), // bok_is_function
        66 => ("__is_layout_compatible", 2), // bok_is_layout_compatible
        67 => ("__is_pointer_interconvertible_base_of", 2), // bok_is_pointer_interconvertible_base_of
        73 => ("__is_array", 1), // bok_is_array
        74 => ("__array_rank", 1), // bok_array_rank
        76 => ("__is_arithmetic", 1), // bok_is_arithmetic
        77 => ("__is_complete_type", 1), // bok_is_complete_type
        78 => ("__is_compound", 1), // bok_is_compound
        79 => ("__is_const", 1), // bok_is_const
        80 => ("__is_floating_point", 1), // bok_is_floating_point
        81 => ("__is_fundamental", 1), // bok_is_fundamental
        82 => ("__is_integral", 1), // bok_is_integral
        83 => ("__is_lvalue_reference", 1), // bok_is_lvalue_reference
        84 => ("__is_member_function_pointer", 1), // bok_is_member_function_pointer
        85 => ("__is_member_object_pointer", 1), // bok_is_member_object_pointer
        86 => ("__is_member_pointer", 1), // bok_is_member_pointer
        87 => ("__is_object", 1), // bok_is_object
        88 => ("__is_pointer", 1), // bok_is_pointer
        89 => ("__is_reference", 1), // bok_is_reference
        90 => ("__is_rvalue_reference", 1), // bok_is_rvalue_reference
        91 => ("__is_scalar", 1), // bok_is_scalar
        92 => ("__is_signed", 1), // bok_is_signed
        93 => ("__is_unsigned", 1), // bok_is_unsigned
        94 => ("__is_void", 1), // bok_is_void
        95 => ("__is_volatile", 1), // bok_is_volatile
        96 => ("__is_bounded_array", 1), // bok_is_bounded_array
        97 => ("__is_unbounded_array", 1), // bok_is_unbounded_array
        98 => ("__is_referenceable", 1), // bok_is_referenceable
        99 => ("__is_nothrow_convertible", 2), // bok_is_nothrow_convertible
        100 => ("__reference_constructs_from_temporary", 2), // bok_reference_constructs_from_temporary
        101 => ("__reference_converts_from_temporary", 2), // bok_reference_converts_from_temporary
        102 => ("__is_convertible", 2), // bok_is_convertible
        103 => ("__is_trivially_equality_comparable", 1), // bok_is_trivially_equality_comparable
        104 => ("__is_scoped_enum", 1), // bok_is_scoped_enum
        105 => ("__is_trivially_relocatable", 1), // bok_is_trivially_relocatable
        106 => ("__is_invocable", -1), // bok_is_invocable
        107 => ("__is_nothrow_invocable", -1), // bok_is_nothrow_invocable
        108 => ("__is_bitwise_cloneable", 1), // bok_is_bitwise_cloneable
        109 => ("__builtin_is_virtual_base_of", 2), // bok_builtin_is_virtual_base_of
        110 => ("__builtin_is_implicit_lifetime", 1), // bok_builtin_is_implicit_lifetime
        111 => ("__builtin_lt_synthesizes_from_spaceship", 2), // bok_builtin_lt_synthesizes_from_spaceship
        112 => ("__builtin_gt_synthesizes_from_spaceship", 2), // bok_builtin_gt_synthesizes_from_spaceship
        113 => ("__builtin_le_synthesizes_from_spaceship", 2), // bok_builtin_le_synthesizes_from_spaceship
        114 => ("__builtin_ge_synthesizes_from_spaceship", 2), // bok_builtin_ge_synthesizes_from_spaceship
        115 => ("__builtin_is_structural", 1), // bok_builtin_is_structural
        _ => throw new InvalidDataException("Source builtin operation has no owned type-trait contract")
    };

    private bool SourceTypeContainsParameter(long id, int depth)
    {
        if (depth > 128) throw new InvalidDataException("Source type dependency cycle/depth");
        SourceType type = SourceTypeById(id);
        if (type.Kind == "dependent") return true;
        if (type.Kind == "record") return SourceRecordsByType[id].IsDependent;
        return type.UnderlyingTypeId is long underlying && SourceTypeContainsParameter(underlying, depth + 1) ||
            type.ArrayBoundKind == "dependent";
    }

    private void VerifySourceTypedExpression(SourceSemanticConstant value, SourceType result)
    {
        long[] children = SourceSemanticConstantOperands.GetValueOrDefault(value.Id, []);
        bool typeQuery = value.Kind is "sizeof" or "alignof" or "datasizeof";
        if ((value.Kind == "builtin-trait") != (value.BuiltinOperatorKind is not null) ||
            value.IsStandardAlignof && value.Kind != "alignof" ||
            value.OperandSourceTypeId is not null && value.Kind != "type-operand" && !typeQuery)
            throw new InvalidDataException("Source expression contains mismatched type-query metadata");
        if (value.Kind == "type-operand")
        {
            if (value.OperandSourceTypeId is not long type || !SourceTypes.ContainsKey(type) ||
                children.Length != 0 || value.OperatorName.Length != 0 || result.Kind != "void")
                throw new InvalidDataException("Original type operand requires its exact type payload and nonvalue expression type");
        }
        else if (value.Kind == "builtin-trait")
        {
            var contract = SourceBuiltinTrait(value.BuiltinOperatorKind!.Value);
            if (value.OperatorName != contract.Name || (contract.Arity < 0 ? children.Length < 1 : children.Length != contract.Arity) ||
                children.Any(child => SourceSemanticConstants[child].Kind != "type-operand") ||
                (contract.Name == "__array_rank" ? SourceIntegerShape(result).Signed || result.Kind == "bool"
                 : contract.Name == "__builtin_types_compatible_p" ? result.Kind != "int" : result.Kind != "bool"))
                throw new InvalidDataException("Original builtin trait opcode/name, result type or ordered type arguments differ");
        }
        else if (typeQuery)
        {
            if (value.OperatorName.Length != 0 || (value.OperandSourceTypeId is long type
                ? !SourceTypes.ContainsKey(type) || children.Length != 0
                : children.Length != 1 || SourceSemanticConstants[children[0]].Kind == "type-operand") ||
                SourceIntegerShape(result).Signed || result.Kind == "bool")
                throw new InvalidDataException("Original size/alignment query lost its type-versus-expression operand or unsigned result");
        }
        else if (children.Any(child => SourceSemanticConstants[child].Kind == "type-operand"))
            throw new InvalidDataException("A source type operand cannot be used as a computed constant value");
    }
}
