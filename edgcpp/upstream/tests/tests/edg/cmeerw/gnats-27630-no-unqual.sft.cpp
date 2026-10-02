//type:fp
//options:--c++20 -A:--c++20 --gn 140200:--c++20 --clang_version 190100;fn:--ms_c++20 --microsoft_version 1936
//options_all:-w -tused

namespace non_tmpl_non_elab
{
  namespace outer
  {
    struct O1;

    struct O2;
  }

  namespace ns
  {
    namespace inner
    {
      struct I1;

      struct I2;
    }

    using namespace inner;
    using namespace outer;

    struct C
    {
      friend O1;

      friend ns::O2;

      friend I1;

      friend ns::I2;

    private:
      static const int v = 0;
    };
  }

  struct outer::O1
  {
    static int f()
    { return ns::C::v; }
  };

  struct outer::O2
  {
    static int f()
    { return ns::C::v; }
  };

  struct ns::inner::I1
  {
    static int f()
    { return ns::C::v; }
  };

  struct ns::inner::I2
  {
    static int f()
    { return ns::C::v; }
  };
}

namespace non_tmpl
{
  namespace outer
  {
    template<typename>
    struct O;
  }

  namespace ns
  {
    namespace inner
    {
      template<typename>
      struct I;
    }

    using namespace inner;
    using namespace outer;

    struct C
    {
      template<typename>
      friend struct ns::O;

      template<typename>
      friend struct ns::I;

    private:
      static const int v = 0;
    };
  }

  template<typename>
  struct outer::O
  {
    static int f()
    { return ns::C::v; }
  };

  template<typename>
  struct ns::inner::I
  {
    static int f()
    { return ns::C::v; }
  };

  int i = outer::O<void>::f() + ns::inner::I<void>::f();
}

namespace using_decl
{
  namespace inner
  {
    template<typename>
    struct B;
  };

  using inner::B;

  class C
  {
    template<typename>
    friend struct B;

    static const bool v = false;
  };

  template<typename>
  struct inner::B
  {
    static const bool v = C::v;
  };

  inner::B<void> b;
}
