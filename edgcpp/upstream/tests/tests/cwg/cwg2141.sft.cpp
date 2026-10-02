//type:fn
//options_all:--c++17 -tused -A
//
  struct A { };

  void foo() {
    new struct A { };
  }

//cwg: 2141
//title: Ambiguity in new-expression with elaborated-type-specifier
//meeting: Jacksonville 2/16
//edg_status: Passes
