//type:cp
//options::-A:-Dconstexpr=const:-A -Dconstexpr=const
//options_all:--c++11 -r

void f() {
  constexpr static bool is_nothrow_destructible = true;
  (void)is_nothrow_destructible;
}
