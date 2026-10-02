//options_all:--c++23
  export module mod;
  extern "C++" void func();
  export extern "C++" {
    void func();
  }

//cwg: 2921
//title: Exporting redeclarations of entities not attached to a named module
//meeting: Wroclaw 11/24
//edg_status: Passes
