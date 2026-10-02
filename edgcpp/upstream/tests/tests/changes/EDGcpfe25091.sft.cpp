//type:fp
//options_all:--c++14 --gn 70500 --parse
//remark:[6.4] C++-generating back end: unbounded recursion with alias template
// 2/28/22  [EDGcpfe/25091,EDGcpfe/25098]
//
// C++-generating back end: unbounded recursion with alias template
//
// In certain complex cases, the C++-generating back end could overflow the
// call stack with an unbounded recursion when generating code involving an
// alias template.  This is now fixed.
template<typename T> using A = T;
template<typename T> A<T> f(T);
struct B {
  void g() { f(c.d); }
private:
  struct C {
    struct D { } d;
  } c;
  friend struct E;
};
struct E {
  using T = B::C::D;  // Previously caused unbounded recursion involving
                      // A<B::C::D>
};
