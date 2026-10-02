//type:fn
//options:--c++20 --gn 150200:--c++20 --defer_parse_function_templates:--c++20
//options_all:-w

namespace ns
{
  struct X
  { };
}

template<typename T>
struct S
{
  friend int f(S)
  {
    return [](auto) {
      return g(ns::X{});        // error
    }(0);
  }
};

namespace ns
{
  int g(X);
}

int i = f(S<int>{});
