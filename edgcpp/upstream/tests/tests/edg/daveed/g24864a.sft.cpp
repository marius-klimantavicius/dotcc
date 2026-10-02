//remark:Forwarding reference and declarative matching
//options:--c++20;fp

  template<typename T> using X = T;
  template<typename T> void f(X<T>&&) {}
  template void f(int&);  // Previously a spurious error.  Now okay.

