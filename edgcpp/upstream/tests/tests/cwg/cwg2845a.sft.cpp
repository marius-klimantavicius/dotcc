//type:fn
//options_all:--c++23 
  template <auto V>
  void foo() {}

  void bar() {
    foo<[i = 3] { return i; }>();   // #1: error
    foo<[]{}>();                    // #2: error
  }

//cwg: 2845
//title: Make the closure type of a captureless lambda a structural type
//meeting: Tokyo 3/24
//edg_status: EDGcpfe/27139
