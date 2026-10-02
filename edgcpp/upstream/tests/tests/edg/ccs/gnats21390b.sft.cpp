//type:fn
//options_all:--c++20 -tused

template<class T> auto f() ->
  decltype(new auto [] { T{} });

void g()
{
  f<int>();
}
