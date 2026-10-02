//type:fp
//remark:[5.1] C++-generating back end: assertion failure with decltype-specifier return type
// 9/17/18  [EDGcpfe/20148]
//
// C++-generating back end: assertion failure with decltype-specifier return type
//
// Under some complex circumstances involving a function whose return type is
// a decltype-specifier whose operand refers to a parameter of that function,
// C++-generating back end configurations could abort with an assertion
// failure (in get_param_for_param_ref).  This is now fixed.
struct A {
  A();
  A(const A&);
};
struct B {
  B(const A&);
  A a;
};
struct C {
};

struct D {
  D(const B& br) : b(br) {}
  B b;
};
struct E : public D {
  E(const C&);
}; 
E f(int);
auto f(double x) -> decltype(f(int(x))) {
  return E{C()};  // Previously caused an abort
}
