//type:fp
//options:--c++14:--c++17:--c++20
//options_all:-w

namespace minimal
{
  struct C {
    template<typename T> static const auto v = false;
    template<> const auto v<int> = true;
  };
}

namespace non_tmpl
{
  const auto v = false;

  struct C
  {
    static const auto v = false;

    template<typename T>
    static const auto tv = false;

    template<typename T>
    static const auto tv<T *> = false;

    template<>
    const auto tv<int> = false;
  };

  template<typename T>
  const auto C::tv<T &> = false;

  template<>
  const auto C::tv<short> = false;

  void f()
  {
    const auto v = false;

    static_assert(!v, "");
    static_assert(!non_tmpl::v, "");
    static_assert(!C::v, "");
    static_assert(!C::tv<char>, "");
    static_assert(!C::tv<short>, "");
    static_assert(!C::tv<int>, "");

    static_assert(!C::tv<char *>, "");
    static_assert(!C::tv<char &>, "");
  }
};

namespace tmpl
{
  template<typename T>
  const auto v = false;

  template<typename T>
  const auto v<T *> = false;

  template<>
  const auto v<int> = false;

  template<typename U>
  struct C
  {
    static const auto v = false;

    template<typename T>
    static const auto tv = false;

    template<typename T>
    static const auto tv<T *> = false;

    template<>
    const auto tv<int> = false;
  };

  template<typename U>
  template<typename T>
  const auto C<U>::tv<T &> = false;

  template<>
  template<>
  const auto C<short>::tv<short> = false;

  template<typename U>
  void f()
  {
    const auto v = false;

    static_assert(!v, "");
    static_assert(!tmpl::v<U>, "");
    static_assert(!tmpl::v<U *>, "");
    static_assert(!C<U>::v, "");
    static_assert(!C<U>::template tv<U>, "");
    static_assert(!C<U>::template tv<U *>, "");
    static_assert(!C<U>::template tv<U &>, "");
  }

  template void f<char>();
  template void f<short>();
  template void f<int>();
};

#if __cpp_inline_variables
namespace inline_non_tmpl
{
  inline auto v = false;

  struct C
  {
    static inline auto v = false;

    template<typename T>
    static inline auto tv = false;

    template<typename T>
    static inline auto tv<T *> = false;

    template<>
    inline auto tv<int> = false;
  };

  template<typename T>
  inline auto C::tv<T &> = false;

  template<>
  inline auto C::tv<short> = false;

  void f()
  {
    v;
    C::v;
    C::tv<char>;
    C::tv<short>;
    C::tv<int>;

    C::tv<char *>;
    C::tv<char &>;
  }
};

namespace inline_tmpl
{
  template<typename T>
  inline auto v = false;

  template<typename T>
  inline auto v<T *> = false;

  template<>
  inline auto v<int> = false;

  template<typename U>
  struct C
  {
    static inline auto v = false;

    template<typename T>
    static inline auto tv = false;

    template<typename T>
    static inline auto tv<T *> = false;

    template<>
    inline auto tv<int> = false;
  };

  template<typename U>
  template<typename T>
  inline auto C<U>::tv<T &> = false;

  template<>
  template<>
  inline auto C<short>::tv<short> = false;

  template<typename U>
  void f()
  {
    v<U>;
    v<U *>;
    C<U>::v;
    C<U>::template tv<U>;
    C<U>::template tv<U *>;
    C<U>::template tv<U &>;
  }

  template void f<char>();
  template void f<short>();
  template void f<int>();
};
#endif
