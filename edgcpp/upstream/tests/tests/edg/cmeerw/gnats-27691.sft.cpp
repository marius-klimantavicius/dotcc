//type:fp
//options:--c++20:--c++20 --gn 140200:--c++20 --clang_version 190100:--ms_c++20 --microsoft_version 1942

namespace minimal
{
  template<typename T> auto f(T t) { t.g(); }
  template<typename T> requires
    requires (T t) { f(t); }
  using A = T;
  template<typename> struct C { void g() { A<C> a; } };
  A<C<int>> a;
}

namespace separate_concept
{
  template<typename T>
  auto f(T t)
  { t.g(); }

  template<typename T>
  concept X = requires (T t) { f(t); };

  template<typename T> requires X<T>
  using A = T;

  template<typename>
  struct C
  {
    void g() { A<C> a; }
  };

  A<C<int>> a;
}
