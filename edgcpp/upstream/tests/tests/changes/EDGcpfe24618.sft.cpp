//type:fp
//options_all:--c++20
//remark:Copying and substituting lambda expressions
// 4/20/26  [EDGcpfe/24618,EDGcpfe/25469,EDGcpfe/25535,EDGcpfe/25665,
//           EDGcpfe/26243,EDGcpfe/26272,EDGcpfe/26528,EDGcpfe/26711,
//           EDGcpfe/26725,EDGcpfe/27660,EDGcpfe/27779,EDGcpfe/27879,
//           EDGcpfe/28262,EDGcpfe/28391,EDGcpfe/28486,EDGcpfe/28767]
//
// Copying and substituting lambda expressions
//
// The front end was previously unable to copy and substitute lambda expressions.
// This caused any case requiring substitution of a lambda expression to fail.
//
// Some cases of lambda substitution are still not handled correctly, but these
// changes make most of the more straightforward situations now work as expected.
template<auto> constexpr bool True = true;
template<typename> concept C = True<[](int){}>;
static_assert(C<int>);  // Previously failed.  Now okay.
