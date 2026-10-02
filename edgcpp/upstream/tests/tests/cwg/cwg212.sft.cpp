//type:fn
//options_all:--c++17 -tused -A 
//
  template<class T> class X;

  X<char> ch;      // error: incomplete type X<char>

//cwg: 212
//title: Implicit instantiation is not described clearly enough
//meeting: Jacksonville 2/16
//edg_status: Passes
