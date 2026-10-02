//type:fp
//options_all:--c++20
//remark:[6.6] Subobjects as nontype template arguments
// 11/27/23 [EDGcpfe/26569,EDGcpfe/26743]
//
// Subobjects as nontype template arguments
//
// Prior to C++20, the standard required a nontype template argument of pointer or
// reference type to refer to a complete object.  This restriction has been
// removed in C++20 and a nontype template argument can now also refer to a
// subobject.
template<int &> struct C { };
struct B {
  int i, j;
} b;
C<b.j> c;  // Previously a spurious error.  Now okay.
