//type:fp
//options:--c++20:--c++20 --gn 160100:--c++20 --clang_version 220100:--ms_c++20 --microsoft_version 1950

namespace minimal
{
  template<typename ... Ts>
  concept X = sizeof ... (Ts) == 2;
  template<typename ... Ts> requires X<Ts ...>
  struct B;
  template<typename ... Ts> requires X<Ts ...>
  struct B { };
  B<int, int> b;
}

namespace class_template
{
  template<typename ... Ts>
  concept X = sizeof ... (Ts) == 2;

  template<typename ... Ts> requires X<Ts ...>
  struct B;

  template<typename ... Ts> requires X<Ts ...>
  struct B { };

  B<int, int> b;
}

namespace var_tmpl
{
  template<typename ... Ts>
  concept X = sizeof ... (Ts) == 2;

  template<typename ... Ts> requires X<Ts ...>
  extern int v;

  template<typename ... Ts> requires X<Ts ...>
  int v = 0;

  int i = v<int, int>;
}
