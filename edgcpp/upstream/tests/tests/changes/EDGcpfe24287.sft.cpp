//type:fp
//options_all:--c++20
//remark:[6.4] Concatenation involving UTF-8 string literals, function name identifiers
// 3/30/22  [EDGcpfe/24287]
//
// Concatenation involving UTF-8 string literals, function name identifiers
//
// According to the C++ Standard, it is permitted to concatenate a string
// literal with a literal prefix with an unprefixed string literal, in either
// order; the result has the type and value implied by the prefixed literal.
// The front end previously reported spurious errors, however, in C++20 mode
// when concatenating a UTF-8 string literal with an unprefixed literal,
// although the same code compiled successfully in C++17 mode.  This
// difference resulted from the fact that in C++17, a UTF-8 string literal had
// the same character type as an unprefixed string literal, while in C++20 the
// character type of a UTF-8 string literal is char8_t, a distinct type.  This
// is now fixed.
//
// In addition, in Microsoft mode, the function name identifiers such as
// __FUNCTION__ can be concatenated with string literals.  In the IL, the
// resulting string constant previously had an incorrect value for the field
// variant.string.literal_kind.  This is now fixed.
// --microsoft --c++20:
//
// Finally, the IL display output previously failed to include the value of
// the variant.string.literal_kind field.  This has now been added, with the
// literal kind displayed in human-readable form.
auto s1 = u8"a" "bc";  // Previously an error, now okay (same as u8"abc"
                       // with literal type "array of 4 const char8_t")
auto s2 = "x" u8"yz";  // Previously an error, now okay (same as u8"xyz"
                       // with literal type "array of 4 const char8_t")
