//type:fp
//options_all:--c++11
//remark:[6.6] Type equivalence of dependent type-transforming intrinsics
// 10/5/23  [EDGcpfe/20557]
//
// Type equivalence of dependent type-transforming intrinsics
//
// Previously, the front end ignored type-transforming intrinsics such as
// __underlying_type and __add_pointer when comparing dependent types, which could
// result in spurious errors.
template<typename T> struct C { };
template<typename T> void f(C<__underlying_type(T)>) { }
template<typename T> void f(C<T>) { }  // Previously a spurious redefinition
                                       // error.  Now okay.
