//type:fp
//options_all:--c++17
//remark:[6.2] Default template arguments for template-dependent nontype template parameters
// 10/12/20 [EDGcpfe/20943,EDGcpfe/23454]
//
// Default template arguments for template-dependent nontype template parameters
//
// Previously this elicited an error in C++03 mode because the default template
// argument can never be a match for the parameter type.  Now, in nonstrict modes,
// that error is delayed until the template S is instantiated using that default
// argument (because that appears to be what other implementations do).  Also,
// some cases involving C++17 "auto" template parameters are now correctly
// handled.
//
// Previously, this example elicited an error because the front end attempted to
// bind &n to the parameter of type decltype(X) too early.
int n;
template<auto X, decltype(X) = &n> struct S {};  // Previously an error.
S<&n> sn;  // Okay.
