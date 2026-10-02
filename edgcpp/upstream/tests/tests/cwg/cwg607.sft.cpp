//type:fp
//options_all:--c++20 -tused -A
    namespace N {
      struct B { B(int); };
      typedef B typedef_B;
      struct D: B {
        D();
      };
    }

    N::D::D(): typedef_B(0) { }

//cwg: 607
//title: Lookup of mem-initializer-ids
//meeting: Virtual 11/20*
//edg_status: Passes
