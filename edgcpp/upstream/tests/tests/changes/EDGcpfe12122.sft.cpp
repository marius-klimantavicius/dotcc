//type:fp
//options_all:--clang
//remark:[6.2] Clang compatibility: additional builtin type intrinsics
// 1/25/21  [EDGcpfe/12122,EDGcpfe/23794,EDGcpfe/23799]
//
// Clang compatibility: additional builtin type intrinsics
//
// The following builtin type intrinsics are now available in Clang emulation
// mode: __is_arithmetic, __is_complete_type, __is_compound, __is_const,
// __is_floating_point, __is_fundamental, __is_integral, __is_lvalue_reference,
// __is_member_function_pointer, __is_member_object_pointer, __is_member_pointer,
// __is_object, __is_pointer, __is_reference, __is_rvalue_reference, __is_scalar,
// __is_signed, __is_unsigned, __is_void, and __is_volatile.
//
// Additionally, enabled builtin type intrinsics now return TRUE when queried with
// __has_builtin.  Furthermore, the intrinsic __is_signed is treated specially in
// some contexts: If it appears immediately following decl-specifiers that include
// "static", "bool", and "const", the __is_signed is treated as an ordinary
// identifier from that point (with a warning).
//
// This matches Clang behavior and allows certain GCC headers that declared an
// __is_signed identifier to be parsed in modes that support the __is_signed
// intrinsic by default.
bool x = __is_signed(int);  // Okay.  "__is_signed" is a keyword.
static bool const __is_signed = false;  // Okay (with warning).
                                        // "__is_signed" is not a keyword.
bool y = __is_signed(int);  // Syntax error.
