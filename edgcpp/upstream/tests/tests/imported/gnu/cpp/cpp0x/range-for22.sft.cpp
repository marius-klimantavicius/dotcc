//type: fp
//options: --c++11
// { dg-do compile { target c++11 } }

template<int I> void f() {
  for (auto i: {I} );
}
