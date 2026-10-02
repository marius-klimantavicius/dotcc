//type:fp
//options_all:--c++11
//remark:[4.5] Abort on use of variadic non-template member function within itself
// 4/16/12  [EDGcpfe/12835]
//
// Abort on use of variadic non-template member function within itself
//
// The front end aborted ("determine_function_viability: no param, no default
// arg", though that message also comes up for other aborts we've seen)
// on processing a reference to a non-template member of a variadic
// class template, where the member has a parameter pack expansion in
// its parameter list, and the reference is within that member function,
// and prototype instantiations are being done.
template <class ...T> struct A {
  A() {}
  A(const T&...) {
    A a;  // Caused abort on processing looking for default constructor
  }
};
