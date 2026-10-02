//type:fp
//options_all:--c++20
//remark:[6.6] Cv-qualification of non-type template parameters
// 8/1/23   [EDGcpfe/24171,EDGcpfe/24573,EDGcpfe/26140,EDGcpfe/26267]
//
// Cv-qualification of non-type template parameters
//
// The C++ Standard requires that top-level cv-qualifiers are ignored when
// determining the type of a non-type template parameter.  However, naming a
// non-type template parameter of class type denotes an lvalue of const-qualified
// type.  Previously, top-level cv-qualifiers were not ignored, and, during
// prototype instantiations, an lvalue representing a non-type template parameter
// of class type was not const-qualified.
struct B { };
void f(B &) = delete;
void f(const B &);
template<const int I, B V>
void g(decltype(I) &i) {
  i = 0;  // Previously a spurious error.  Now okay.
  f(V);   // Previously a spurious error.  Now okay.
}
