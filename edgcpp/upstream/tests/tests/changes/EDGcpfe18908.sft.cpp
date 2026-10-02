//type:fp
//options_all:--gnu_version 60000
//remark:[5.0] Spurious variadic template deduction error
// 11/2/17  [EDGcpfe/18908]
//
// Spurious variadic template deduction error
//
// The changes for EDGcpfe/18524 introduced a regression (in version 4.14) in
// certain cases involving argument deduction for variadic function templates.
//
// This is now fixed.
template<char...> struct S {};
template<char... Cs> auto f(S<Cs...>) -> S<Cs...>;
template<char C, char... P1, char... P2>
  auto f(S<P1...>, S<C>, S<P2>...)->decltype(f(S<P1..., C>(), S<P2>()...));
template<char... Cs>
  auto g(S<Cs...>)->decltype(f(S<Cs>()...));
template<typename...> struct C {};
using CT = C<decltype(g(S<'a', 'a' ,'a'>()))>;
  // Previously triggered an error for failing to deduce the arguments
  // for the call to g.
