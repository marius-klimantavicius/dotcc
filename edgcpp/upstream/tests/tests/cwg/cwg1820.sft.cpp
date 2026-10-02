//type:fn
//options_all:--c++20 -tused -A
  namespace N { typedef int T; }
  typedef int N::T;

//cwg: 1820
//title: Qualified typedef names
//meeting: Virtual 11/20*
//edg_status: Passes
