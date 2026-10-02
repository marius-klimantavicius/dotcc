//type:fp
//options_all:--c++17 -tused -A
//
  void f() {
    f();  // #1
  }

//cwg: 1990
//title: Ambiguity due to optional decl-specifier-seq
//meeting: Kona 10/15
//edg_status: Passes
