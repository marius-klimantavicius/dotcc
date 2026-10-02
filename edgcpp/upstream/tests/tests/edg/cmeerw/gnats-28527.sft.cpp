//type:fn
//options:--c++11:--c++20:--c++20 --gn 150200:--c++20 --clang_version 220100:--ms_c++20 --microsoft_version 1950
//options_all:-w -tused

namespace minimal
{
  template<typename T>
  using A1 = T;
  template<typename T, typename U, typename ... Vs>
  using A2 = A1<U>;
  template<typename ... Ts>
  using A3 = A2<Ts...>;         // error
}

namespace cwg1430_1
{
  template<class T, class U, class V>
  struct S {};

  template<class T, class V>
  using A = S<T, int, V>;

  template<class... Ts>
  void foo(A<Ts...>);           // error
}

namespace cwg1430_2
{
  template<class... x> class list{};
  template<class a, class... b> using tail=list<b...>;
  template<class...T> void f(tail<T...>); // error

  void f() {
    f<int,int>({});             // error
  }
}

namespace cwg1430_3
{
  namespace std
  {
    template<typename ...>
    struct tuple { };
  }

  template<typename A, typename...T> using X = std::tuple<T...>;
  template<typename A, typename...T> using Y = A;

  template<typename ...U> void f(X<U...> x, Y<U...> y); // error
}

namespace cwg1430_4
{
  template <template <typename...> class T> struct Template {
    template <typename... Us> using type = T<Us...>; // error for Clang/MSVC
  };

  template <class T> struct X { static constexpr T value = T(); };
  template <class T> using alias = X<T>;

  int f() { return Template<alias>::type<int>::value; }
}

namespace simple_alias_to_non_simple_alias
{
  template <template <typename...> class T> struct Template {
    template <typename... Us> using type = T<Us...>; // error
  };

  template <class T, class U> struct X { static constexpr T value = T(); };
  template <class T1, class U1> using A1 = X<U1, T1>;
  template <class T2, class U2> using A2 = A1<T2, U2>;

  int main() { return Template<A2>::type<int, int>::value; }
}
