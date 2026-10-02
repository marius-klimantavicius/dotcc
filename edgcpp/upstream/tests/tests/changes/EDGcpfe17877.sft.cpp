//type:fp
//options_all:--microsoft_version 1910
//remark:[6.3] Tokens introducing expressions
// 9/28/21  [EDGcpfe/17877,EDGcpfe/24408]
//
// Tokens introducing expressions
//
// The front end's list of tokens that can introduce an expression previously
// missed several entries.  This could lead to spurious errors.
//
// This is now fixed.  The affected tokens are __assume, __builtin_addressof,
// __builtin_offsetof, co_await, co_yield, __is_assignable, __is_destructible,
// __is_nothrow_assignable, __is_nothrow_destructible,  __is_trivially_assignable,
// __is_trivially_constructible, __is_trivially_destructible, and __noop.
struct S {
  static const bool value = __is_trivially_constructible(int);
};                           // Previously an error.  Now okay.
