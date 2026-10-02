//type: fp
//options:  --c++11
// PR c++/58648
// { dg-require-effective-target c++11 }

template<int, typename...T, T...> int foo();

int i = foo<0>();
