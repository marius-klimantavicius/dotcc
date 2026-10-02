//type:fp
//options:--c++17:--c++17 --gn 150200:--c++17 --clang_version 220100:--ms_c++20 --microsoft_version 1950

namespace minimal
{
  template<auto ... Is>
  decltype((Is + ...)) f();
  int i = f<1>();
}

namespace braced_style_cast
{
  template<typename ... Ts>
  decltype((Ts{} + ...)) f();
  int i = f<int>();
}

namespace braced_style_cast_arg
{
  template<typename ... Ts>
  decltype((Ts{0} + ...)) f();
  int i = f<int>();
}

namespace functional_style_cast
{
  template<typename ... Ts>
  decltype((Ts() + ...)) f();
  int i = f<int>();
}

namespace functional_style_cast_arg
{
  template<typename ... Ts>
  decltype((Ts(0) + ...)) f();
  int i = f<int>();
}

namespace constant_templ_arg
{
  template<auto ... Is>
  decltype((Is + ...)) f();
  int i = f<1>();
}

namespace trailing_return_type
{
  template<auto ... Is>
  auto f() -> decltype((Is + ...));
  int i = f<1>();
}

namespace fn_param_type
{
  template<auto ... Is>
  int f(decltype((Is + ...)));
  int i = f<1>(1);
}
