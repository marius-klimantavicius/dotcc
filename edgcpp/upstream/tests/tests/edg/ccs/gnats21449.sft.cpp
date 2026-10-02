//type:fn
//options_all:--c++20

template<class T>
auto f(T x) ->
  decltype(new decltype(auto) (T {(*new int[] {0})} T() } ({0})));

void g()
{
  f<int[59]>{};
}
