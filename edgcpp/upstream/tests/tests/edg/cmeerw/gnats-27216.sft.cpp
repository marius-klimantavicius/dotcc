//type:fp
//options:--c++14 -A:--c++20 -A:--c++20 --gn 130200:--c++20 --clang_version 180100:--ms_c++20 --microsoft_version 1936
//options_all:-tused

namespace minimal
{
  int g(int, int);
  template<typename ... Ts>
  inline int f(Ts ... t) {
    return [] (auto ... p) {
      return g(Ts{p} ...);
    } (t ...);
  }
  auto i = f(1, 2);
}

namespace lambda_auto_param
{
  template<typename T>
  void g(T, T);

  template<typename ... Ts>
  inline int f(Ts ... t)
  {
    return [&] (auto ... p) {
      g(static_cast<Ts>(p) ...);
      g(Ts() + p ...);
      g(t + p ...);
      g(static_cast<int>(Ts() + p) ...);
      g(static_cast<int>(t + p) ...);
      return 0;
    } (t ...);
  }

  auto i = f(1, 2);
}

#if __cpp_generic_lambdas >= 201707
namespace nested_lambda
{
  template <typename T>
  void g(T, T);

  template<typename... Ts>
  inline int f(Ts ... t)
  {
    [&] (auto ... p) {
      return [&] () {
        g(static_cast<Ts>(p) ...);
        g(static_cast<int>(p + t) ...);
        return 0;
      } ();
    } (t ...);

    [&]<typename ... Us> (Us ... u) {
      return [&] () {
        g(static_cast<Ts>(u) ...);
        g(static_cast<int>(u + t) ...);

        g(static_cast<Us>(t) ...);
        g(static_cast<int>(Us() + Ts()) ...);
        g(static_cast<int>(u + Ts()) ...);
        g(static_cast<int>(Us() + t) ...);

        return 0;
      } ();
    } (t ...);

    [&]<typename ... Us> (Us ... u) {
      [&]<typename ... Vs> (Vs ... v) {
        g(static_cast<Ts>(t) ...);
        g(static_cast<Ts>(u) ...);
        g(static_cast<Ts>(v) ...);
        g(static_cast<Ts>(u + v) ...);
        g(static_cast<Ts>(t + v) ...);

        // This one doesn't work, but seems to be a separate issue
        // g(static_cast<int>(t) ...);

        g(static_cast<int>(u) ...);
        g(static_cast<int>(v) ...);
        g(static_cast<int>(u + t) ...);
        g(static_cast<int>(u + v) ...);
        g(static_cast<int>(t + v) ...);
        g(static_cast<int>(u + t + v) ...);

        g(static_cast<Us>(t) ...);
        g(static_cast<Us>(t + v) ...);
        g(static_cast<int>(Us() + Ts()) ...);
        g(static_cast<int>(Us() + Ts() + Vs()) ...);
        g(static_cast<int>(u + Ts()) ...);
        g(static_cast<int>(Us() + t) ...);

        g(static_cast<int>(v + Ts()) ...);
        g(static_cast<int>(Us() + v) ...);

        return 0;
      } (t ...);

      [&, ...c = u]<typename ... Vs> (Vs ... v) {
        g(static_cast<Ts>(t) ...);
        g(static_cast<Ts>(c) ...);
        g(static_cast<Ts>(v) ...);
        g(static_cast<Ts>(c + v) ...);
        g(static_cast<Ts>(t + v) ...);

        // This one doesn't work, but seems to be a separate issue
        // g(static_cast<int>(t) ...);

        g(static_cast<int>(c) ...);
        g(static_cast<int>(v) ...);
        g(static_cast<int>(c + t) ...);
        g(static_cast<int>(c + v) ...);
        g(static_cast<int>(t + v) ...);
        g(static_cast<int>(c + t + v) ...);

        g(static_cast<Us>(t) ...);
        g(static_cast<Us>(t + v) ...);
        g(static_cast<int>(Us() + Ts()) ...);
        g(static_cast<int>(Us() + Ts() + Vs()) ...);
        g(static_cast<int>(c + Ts()) ...);
        g(static_cast<int>(Us() + t) ...);

        g(static_cast<int>(v + Ts()) ...);
        g(static_cast<int>(Us() + v) ...);

        return 0;
      } (t ...);

      return 0;
    } (t ...);

    return 0;
  }

  int i = f(1, 2);
}
#endif

namespace related_test_1
{
  template<unsigned N, typename ...Types> struct get_nth_type;

  template<unsigned N, typename Head, typename ...Tail>
  struct get_nth_type<N, Head, Tail...> : get_nth_type<N-1, Tail...> { };

  template<typename Head, typename ...Tail>
  struct get_nth_type<0, Head, Tail...> {
    typedef Head type;
  };

  struct no_type {};

  template<unsigned N>
  struct get_nth_type<N> {
    typedef no_type type;
  };

  template<typename ...Args>
  typename get_nth_type<0, Args...>::type first_arg(Args...);

  int *ip1 = first_arg<int *>(0);
}

namespace related_test_2
{
  template <typename T, int ...M> struct C {
    template <T... N> using Fn = T(int(*...A)[N]);
    Fn<1, M..., 4> *p;
  };
}

namespace related_test_3
{
  template <typename T, T...> struct B;
  template <bool... Bools> using and_c = B<bool, +Bools...>;
  template <typename T, typename U> using Constructible = int;

  template <typename... Ts> struct C
  {
    template <typename... Us, typename = and_c<Constructible<Ts, Us>{}...> >
    C();
  };
}
