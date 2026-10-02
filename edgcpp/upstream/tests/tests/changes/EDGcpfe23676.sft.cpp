//type:fp
//options_all:--c++20
//remark:[6.6] Implicit type context for requires-expression parameters
// 10/16/23 [EDGcpfe/23676]
//
// Implicit type context for requires-expression parameters
//
// The changes for EDGcpfe/20270 missed this case: Redeclaring a function or
// function template with a qualified name causes the parameter declarations to
// be in an "implicit type context".  That is now fixed.
namespace N { template<typename T> int f(typename T::type); }
template<typename T> int N::f(T::type);  // Previously an error.  Now okay.
