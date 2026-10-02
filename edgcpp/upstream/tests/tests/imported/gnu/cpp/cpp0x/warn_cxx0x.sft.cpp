//type: fp
//options: --c++11 --c++03 --no_strict_gnu
// { dg-do compile { target c++11 } }
// { dg-options "-std=gnu++98 -Wc++11-compat" }
int static_assert; // { dg-warning "is a keyword" }
int nullptr; // { dg-warning "is a keyword" }

void foo()
{
  static_assert = 5;
  nullptr = 5;
}
