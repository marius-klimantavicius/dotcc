//type:fp
//remark:[4.10] Abort in C++-generating back end on function declarator in trailing return type
// 7/15/14  [EDGcpfe/15186]
//
// Abort in C++-generating back end on function declarator in trailing return type
//
// The C++-generating back end sometimes aborted (in get_param_for_param_ref,
// il_to_str.c) with an internal error when attempting to render a reference to a
// parameter of a function declaration from within a function declarator appearing
// in the trailing return type of that declaration.
//
// This is now fixed.
template<class T> struct S {};
auto f(int p) -> S<void (decltype(p))>;  // Previously aborted in some
                                         // configurations.  Now okay.
