//type: fp
//options: --c++14
// { dg-do compile { target c++14 } }

template <class T> bool Foo = Foo<int>;
template <> bool Foo<int> = true;
int i = Foo<char>;
