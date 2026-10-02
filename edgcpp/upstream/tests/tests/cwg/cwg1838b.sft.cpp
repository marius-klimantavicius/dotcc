//type:fn
//options_all:--c++17 -tused -A
 namespace X {
    void f() { /* ... */ }  // OK: introduces X::f()

    namespace M {
      void g();             // OK: introduces X::M::g()
    }
    using M::g;
    void g();               // error: conflicts with X::M::g()
  }
