//type:fp
//options_all:--c++17 -tused -A
 namespace X {
    void f() { /* ... */ }  // OK: introduces X::f()

    namespace M {
      void g();             // OK: introduces X::M::g()
    }
    using M::g;
  }

//cwg: 1838
//title: Definition via unqualified-id and using-declaration
//meeting: Urbana-Champaign 11/14
//edg_status: Passes
