//type:fp
//options_all:--microsoft_version 1928 --ms_c++latest
//remark:[6.3] Abort on use of variadic requires-expression parameters
// 3/10/21  [EDGcpfe/23932]
//
// Abort on use of variadic requires-expression parameters
//
// The front end could previously abort with an internal error in overload.c (in
// determine_function_viability) when expanding variadic parameters of a
// requires-expression.
//
// That is now fixed.
template<typename F, typename ...Ts> auto g(F fn, Ts ...ps) {
  return fn(ps...);
}
template<typename F, typename ...Ts>
  concept C = requires(F fn, Ts... ps) { g(fn, ps...); };
static_assert(C<void(*)(int), int>);
       // Previously aborted when substituting the g(fn, ps...) call in
       // the requires-expression above.
