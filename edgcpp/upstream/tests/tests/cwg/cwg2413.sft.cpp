//type:fp
//options_all:--c++20 -tused -A
  template<class T> struct S {
    operator T::X(); // typename is not helpful here.
  };

//cwg: 2413
//title: typename in conversion-function-ids
//meeting: Virtual 11/20*
//edg_status: Passes
