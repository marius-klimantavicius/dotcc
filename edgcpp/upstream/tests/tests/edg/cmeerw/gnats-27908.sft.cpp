//type:fn
//options:--c++14:--c++20:--c++20 --gn 140200:--c++20 --clang_version 190100:--ms_c++20 --microsoft_version 1942

namespace minimal
{
  template<typename T>
  auto g(T t) -> decltype(t(1L));
  template<typename T>
  auto f(T t) {
    g([] (auto a) { f(a); });
    return 1;
  }
  int i = f(1);
}

namespace with_noexcept
{
  template<typename T> T d();

  template<typename T>
  int n(T) noexcept(d<T>()(1L));

  struct C {
    template<typename T>
    static auto f1(T t) {
      return f2(1);
    }

    template<typename T>
    static auto f2(T t) {
      return n([] (auto a) { f1(a); });
    }
  };

  int i = C::f1(1);
}

namespace with_decltype
{
  template<typename T> T d();

  template<typename T>
  auto n(T) -> decltype(d<T>()(1L));

  struct C {
    template<typename T>
    static auto f1(T t) {
      return f2(1);
    }

    template<typename T>
    static auto f2(T t) {
      return n([] (auto a) { f1(a); });
    }
  };

  int i = C::f1(1);
}
