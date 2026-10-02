//type:fp
//options_all:--c++11
//remark:[4.10] Spurious error on use of "final", "sealed", or "abstract" specifier
// 5/6/14   [EDGcpfe/14542]
//
// Spurious error on use of "final", "sealed", or "abstract" specifier
//
// A spurious error had been issued in cases where a virtual function declaration
// has a "final" specifier and the class has a dependent base class that could
// declare the overridden function.  A similar situation arose when the C++/CLI
// "sealed" and "abstract" modifiers were used.  Now fixed.
// --c++11):
template <class T> struct D : T {
  void f() final;   // Overrides T::f()
};
