//type:fn
//options_all:--c++20 -tused

template<class T>
void f() {
  new auto [] { T{} };
  new decltype(auto) [] { T{} };
}

void g()
{
  f<int>();
}
