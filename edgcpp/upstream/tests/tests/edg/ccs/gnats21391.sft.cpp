//type:fn
//options_all:--c++20 -tused

template<class T> auto f(T x) ->
  decltype(new decltype(auto) (T{}));

int g()
{
  auto x =  1;
  f<int>{};
  return x;
}
