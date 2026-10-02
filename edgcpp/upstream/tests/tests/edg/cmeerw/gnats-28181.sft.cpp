//type:fp
//options:--c++20:--c++20 --gn 150100:--c++20 --clang_version 200100:--ms_c++20 --microsoft_version 1942

namespace minimal
{
  int g(int);
  template<typename ... T>
  int f(T ...) requires
    requires (T ... t) { g(t ...); };
  int i = f(1);
}

namespace lambda
{
  template<typename ... Ts> concept C = true;
  auto l = []<typename ... Ts>(Ts ...)
    requires requires (Ts ... ts) { C<decltype(ts) ...>; } {
      return 1;
    };
  int i = l(1, 2);
}

namespace requires_clause_param
{
  int g(int);

  template<typename ... T>
  int f(T ...) requires requires (T ... t) { g(t ...); };

  int i = f(1);
}

namespace fn_param
{
  int g(int);

  template<typename ... T>
  int f(T ... t) requires requires (T ...) { g(t ...); };

  int i = f(1);
}
