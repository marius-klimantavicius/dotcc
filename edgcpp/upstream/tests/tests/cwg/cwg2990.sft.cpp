//options_all:--c++23 -A
  export module M;
  namespace N { // external linkage, attached to global module, not exported
    void f();
  }
  namespace N { // error: exported namespace, redeclares non-exported namespace
    export void g();
  }

//cwg: 2990
//title: Exporting redeclarations of namespaces
//meeting: Hagenberg 2/25
//edg_status: Passes
