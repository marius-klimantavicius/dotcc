//type:fp
//options_all:--c++17
//remark:[6.6] Deducing the type of a non-type template parameter used as a template template
// 7/27/23  [EDGcpfe/26526]
//
// Deducing the type of a non-type template parameter used as a template template
// parameter argument
//
// C++17 allows the type of a non-type template parameter to be deduced from the
// type of a non-type template argument.  However, the changes for
// EDGcpfe/25949,EDGcpfe/26013 in version 6.5 (see entry of 2/22/23) introduced a
// regression for cases where the non-type template parameter to be deduced is
// used as an argument for a template template parameter.
// --c++17:
template<int> struct A { };
template<typename T, T I, template<int> class TT>
int f(TT<I>);
int i = f(A<1>{});  // Previously a spurious error.  Now okay.
