//type:fp
//options_all:--c++17 -A -tused
//
   struct A {
    struct B { int x; } b;
    int B;    // Permitted in C
  };

//cwg: 2063
//title: Type/nontype hiding in class scope
//meeting: Jacksonville 2/16
//edg_status: Passes
