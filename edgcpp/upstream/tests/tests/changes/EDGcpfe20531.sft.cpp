//type:fp
//options_all:--c++20
//remark:[5.1] C++20: Nested inline namespaces
// 1/9/19   [EDGcpfe/20531,EDGcpfe/20648]
//
// C++20: Nested inline namespaces
//
// Support for nested inline namespaces (P1094R2) has been added.
// (with --c++20):
namespace A::inline B::inline C {
  int i;
}
namespace A {
  inline namespace B {
    inline namespace C {
      int j;
    }
  }
}
