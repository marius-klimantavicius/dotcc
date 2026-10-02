//type:fn
//options_all:--c++11
//remark:[4.13] __is_constructible/__is_nothrow_constructible/__is_trivially_constructible
// 12/16/16 [EDGcpfe/17852]
//
// __is_constructible/__is_nothrow_constructible/__is_trivially_constructible
//
// In non-Microsoft modes, the type traits helper functions __is_constructible,
// __is_nothrow_constructible, and __is_trivially_constructible now also require
// that the corresponding __is_...destructible type traits helper be "true" to
// produce a "true" value (std::is...constructible is defined in terms of a
// hypothetical variable definition that requires destruction).
//
// 12/16/16 [EDGcpfe/17852]
//
// Unions and __is_base_of
//
// The front end previously produced a "true" value for __is_base_of(U, U) where
// U is a union type.  Now it produces a "false" value, because that is the
// expected behavior of the std::is_base_of type trait.
struct N { ~N(); };
static_assert(__is_trivially_constructible(N), "");
  // Now an error in non-Microsoft modes.
