//type: fp
//options: --c++17
// PR c++/89062
// { dg-do compile { target c++17 } }

template<class T> struct Foo { Foo(T); };

Foo<int> x(Foo{1});
