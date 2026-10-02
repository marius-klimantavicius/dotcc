//type:fn
//options_all:--c++17 -tused -A
  template <class T> struct B {
    void g(T) { }
    void h(T);
    friend void i(B, T) { }
  };

  void f() {
    struct A { int x; };  // no linkage
    A a = { 1 };
    B<A> ba;              // declares B<A>::g(A) and B<A>::h(A)
    ba.g(a);              // OK
    ba.h(a);              // error: B<A>::h(A) not defined; A cannot be named in the another translation unit
    i(ba, a);             // OK
  }

//cwg: 2059
//title: Linkage and deduced return types
//meeting: Jacksonville 2/18
//edg_status: Passes
