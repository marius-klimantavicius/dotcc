//type:fp
//options_all:--c++17 -tused -A
//
  struct S {
    using T = void();
    T * p = 0;        // OK: brace-or-equal-initializer
    virtual T f = 0;  // OK: pure-specifier
  };

//cwg: 2154
//title: Ambiguity of pure-specifier
//meeting: Jacksonville 2/16
//edg_status: Passes
