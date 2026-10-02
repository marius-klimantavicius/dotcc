//type:fp
//options:--c++20 --gn 140200:--c++20 --clang_version 190100;fn:--ms_c++20 --microsoft_version 1936
//options_all:-w -tused

  namespace inner
  {
    struct I1;

    template<typename>
    struct I2;
  }

  using namespace inner;

  struct C
  {
    friend struct ::I1;

    template<typename>
    friend struct ::I2;

  private:
    static const int v = 0;
  };

  struct inner::I1
  {
    static int f()
    { return C::v; }
  };

  template<typename>
  struct inner::I2
  {
    static int f()
    { return C::v; }
  };

  int i = inner::I2<void>::f();
