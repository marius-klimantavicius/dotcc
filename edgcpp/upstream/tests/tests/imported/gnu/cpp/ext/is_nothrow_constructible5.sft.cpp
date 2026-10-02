//type: fp
//options: --c++11
// PR c++/80991
// { dg-do compile { target c++11 } }

template<bool> void foo()
{
  static_assert(__is_nothrow_constructible(int, int), "");
}

void bar()
{
  foo<true>();
}
