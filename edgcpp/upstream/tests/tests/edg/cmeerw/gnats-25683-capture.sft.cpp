//type:fp
//options:--c++17:--c++17 --g++:--ms_c++17
//options_all:-tused

namespace capture_in_discarded_statement
{
  struct C
  {
    int val = 0;

    constexpr C(int v)
      : val(v)
    { }

    constexpr C(C &c)
    { ++c.val; }

    constexpr operator int() const
    { return val; }
  };

  template<typename T>
  constexpr auto foo()
  {
    T t(0);
    [=] (auto p) {
      if constexpr (false)
      {
        static_assert(T::dont_instantiate); // not instantiated
        (void) t;                           // should capture "t"
      }
    } (1);

    [=] (auto p) {
      if constexpr (sizeof(T) == 0)
      {
        static_assert(T::dont_instantiate); // not instantiated
        (void) t;                           // should capture "t"
      }
    } (1);

    return static_cast<int>(t);
  }

  static_assert(foo<int>() == 0, "capture not observable");
  static_assert(foo<C>() == 0, "capture observable"); // should fail


  template<typename T>
  constexpr auto bar()
  {
    T t(0);
    [=] (auto ... p) {
      if constexpr (sizeof ... (p) > 0)
      {
        (void) t;                           // should capture "t"
      }
    } (1);

    return static_cast<int>(t);
  }

  static_assert(bar<int>() == 0, "capture not observable");
  static_assert(bar<C>() == 1, "capture observable");
}
