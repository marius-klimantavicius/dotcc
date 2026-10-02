//type:fp
//options:--c++20:--c++20 --gn 140200:--c++20 --clang_version 190100:--ms_c++20 --microsoft_version 1936

namespace minimal
{
  template<typename> struct B { };
  template<typename ...> concept X = true;
  template<typename ... Ts,
           bool = X<B<Ts> ...>>
  void f();
}

namespace or_expression
{
  template<typename>
  struct B
  { };

  template<typename ...>
  concept X = true;

  template<typename ... Ts, bool = X<B<Ts> ...> || X<B<Ts> ...>>
  int f();

  template<typename ... Ts>
  int g(bool = X<B<Ts> ...> || X<B<Ts> ...>);

  template<typename ... Ts>
  int h(decltype(X<B<Ts> ...> || X<B<Ts> ...>));

  int i = f<int, long>() + g<int, long>() + h<int, long>(true);
}

namespace nested_struct
{
  template<typename>
  struct B
  { };

  template<typename ...>
  concept X = true;

  template<typename ... Ts>
  struct C
  {
    template<bool = X<B<Ts> ...> || X<B<Ts> ...>>
    struct N
    { };

    template<bool = X<B<Ts> ...> || X<B<Ts> ...>>
    static int f();

    template<typename = void>
    static int g(bool = X<B<Ts> ...> || X<B<Ts> ...>);

    static int h(bool = X<B<Ts> ...> || X<B<Ts> ...>);
  };

  C<int, long>::N<> n;

  int i = C<int, long>::f() + C<int, long>::g<>() + C<int, long>::h();
}
