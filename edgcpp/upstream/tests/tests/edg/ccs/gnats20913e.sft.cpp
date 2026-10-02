//type:cp
//options_all:--c++20 -tused

template<typename T> auto f() ->
  decltype(T {(*new int[] {0})} );

void g()
{
  f<int>();
}
