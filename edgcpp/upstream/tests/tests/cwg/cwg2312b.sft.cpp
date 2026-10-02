//type:fp
//options_all:--c++20 -tused --gnu_version 80100
struct S { mutable int x1 : 2; volatile double y1; };
  S f();
  const auto [ x, y ] = f();
 static_assert(__is_same_as(int,decltype(x)));
 static_assert(__is_same_as(const volatile double,decltype(y)));
