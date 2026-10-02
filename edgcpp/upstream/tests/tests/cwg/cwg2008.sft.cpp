//type:fp
//options_all:--c++17 -tused -A
//
  template<typename T, typename U = int> struct S { };
  S<bool>* p; // the type of p is S<bool, int>*

//cwg: 2008
//title: Default template-arguments underspecified
//meeting: Jacksonville 2/16
//edg_status: Passes
