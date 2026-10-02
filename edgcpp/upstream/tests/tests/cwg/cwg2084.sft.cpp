//type:fp
//options_all:--c++17 -tused -A -w
//
  struct S {
    S();
  };
  union U {
    S s{};
  } u;

//cwg: 2084
//title: NSDMIs and deleted union default constructors
//meeting: Jacksonville 2/16
//edg_status: Passes
