//type:fp
//options_all:--c++20 -tused -A
  template<typename ...T> auto f() {
    using F = int(*)(int (...p)[sizeof(sizeof(T))]);
    // ...
  }

//cwg: 2457
//title: Unexpanded parameter packs don't make a function type dependent
//meeting: Virtual 11/20
//edg_status: Passes
