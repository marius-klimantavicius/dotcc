//type:fp
//options_all:--c++20
//remark:[6.6] Implicit type context for requires-expression parameters
// 10/12/23 [EDGcpfe/26720]
//
// Implicit type context for requires-expression parameters
//
// The front end previously issued a spurious error on that example because it
// failed to recognize that the parameter declaration of a requires-expression is
// an implicit type context (i.e., no "typename" prefix is needed for T::X).
// That is now fixed.
template<typename T> concept C = requires(T::X x) { ++x; };
