//type:fp
//options_all:--clang_v 160000 --c++20
//remark:[6.5] Clang compatibility: Type-transforming intrinsics (IL CHANGE)
// 5/4/23   [EDGcpfe/26193]
//
// Clang compatibility: Type-transforming intrinsics (IL CHANGE)
//
// In Clang modes, the front end now accepts a number of type-transforming
// intrinsics.
//
// The intrinsics initially being supported are __add_lvalue_reference,
// __add_pointer, __add_rvalue_reference, __decay, __make_signed,
// __make_unsigned, __remove_all_extents, __remove_const, __remove_cv,
// __remove_cvref, __remove_extent, __remove_pointer, __remove_reference_t,
// __remove_restrict, and __remove_volatile.
//
// These intrinsics are represented using a_type/tk_typeref entries.  To
// accommodate a possibly large number of such intrinsics in the future, a number
// of mutually-exclusive flags (e.g., is_decltype and is_alias) have been
// replaced by a new "kind" field in the a_type::variant.typeref sub-structure.
using X = __add_pointer(int);  // Same as: using X = int*;
