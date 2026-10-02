//type:fp
//options_all:--c++17 -tused -A
  enum E { e1 };
  using T = void *;
  void f() {
    false ? new enum E : T();
  }

//cwg: 1966
//title: Colon following enumeration elaborated-type-specifier
//meeting: Lenexa 5/15
//edg_status: Passes
