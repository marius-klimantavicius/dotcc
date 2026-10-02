//type: fp
//options: --c++11
// PR c++/57673
// { dg-do compile { target c++11 } }

template< int ... p >
struct d {
  int n = sizeof ... ( p );
};
