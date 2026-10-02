//type:fp
//options_all:--c++14 -tused -A
  template <typename T>
  struct S {};

  void f (S<int (*)[]>);

//cwg: 393
//title: Pointer to array of unknown bound in template argument list in parameter
//meeting: Urbana-Champaign 11/14
//edg_status: Passes
