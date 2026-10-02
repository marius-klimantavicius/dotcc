//type:fp
//options_all:--c++17 -A -tused
//
  struct X {
    X(int);
    operator int();
    void foo() {
      ~X(0);
    }
  };

//cwg: 1971
//title: Unclear disambiguation of destructor and operator~
//meeting: Lenexa 5/15
//edg_status: Passes
