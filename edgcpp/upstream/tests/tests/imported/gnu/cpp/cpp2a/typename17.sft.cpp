//type: fp
//options: --c++20
// DR 2413 - typename in conversion-function-ids.
// { dg-do compile { target c++20 } }

template<class T> struct S {
  operator T::X();
};
