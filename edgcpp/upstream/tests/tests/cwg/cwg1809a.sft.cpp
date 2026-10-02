//type:fn
//options_all:--c++17 -tused -A
  template <int> int f(int);
  template <signed char> int f(int);
  int i2 = f<1>(0);   // ambiguous

//cwg: 1809
//title: Narrowing and template argument deduction
//meeting: Urbana-Champaign 11/14
//edg_status: Passes
