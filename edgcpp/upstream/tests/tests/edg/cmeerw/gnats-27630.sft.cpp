//type:fp
//options:--c++20 --gn 140200:--c++20 --clang_version 190100;fn:--ms_c++20 --microsoft_version 1936
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
      friend struct O1;         // does not find outer::O1 (except for MSVC)

      friend struct ns::O2;

      friend struct I1;         // gcc does not find inner::I1 here, but we do in gcc mode

      friend struct ns::I2;

    private:
      static const int v = 0;
    };

    struct O1
    {
#ifndef _MSC_VER
      static int f()
      { return ns::C::v; }
#endif
    };
  }

  struct outer::O1
  {
#ifdef _MSC_VER
    static int f()
    { return ns::C::v; }
#endif
  };

  struct outer::O2
  {
    static int f()
    { return ns::C::v; }
  };

  struct ns::inner::I1
  {
#if defined(__clang__) || !defined(__GNUC__)
    static int f()
    { return ns::C::v; }
#endif
  };

  struct ns::inner::I2
  {
    static int f()
    { return ns::C::v; }
  };
}

namespace tmpl
{
  namespace outer
  {
    template<typename>
    struct O1;

    template<typename>
    struct O2;
  }

  namespace ns
  {
    namespace inner
    {
      template<typename>
      struct I1;

      template<typename>
      struct I2;
    }

    using namespace inner;
    using namespace outer;

    struct C
    {
      template<typename>
      friend struct O1;         // lookup does not find outer::O1 (except for MSVC)

      template<typename>
      friend struct ns::O2;     // error with clang

      template<typename>
      friend struct I1;         // gcc does not find inner::I1 here, but we do in gcc mode

      template<typename>
      friend struct ns::I2;     // error with clang

    private:
      static const int v = 0;
    };

    template<typename>
    struct O1
    {
#ifndef _MSC_VER
      static int f()
      { return ns::C::v; }
#endif
    };
  }

  template<typename>
  struct outer::O1
  {
#ifdef _MSC_VER
    static int f()
    { return ns::C::v; }
#endif
  };

  template<typename>
  struct outer::O2
  {
    static int f()
    { return ns::C::v; }
  };

  template<typename>
  struct ns::inner::I1
  {
#if defined(__clang__) || !defined(__GNUC__)
    static int f()
    { return ns::C::v; }
#else
    static int f() { return 0; }
#endif
  };

  template<typename>
  struct ns::inner::I2
  {
    static int f()
    { return ns::C::v; }
  };

  int i = outer::O2<int>::f() +
          ns::inner::I1<int>::f() +
          ns::inner::I2<int>::f();

  int j =
#ifndef _MSC_VER
  ns::O1<int>::f();
#else
  outer::O1<int>::f();
#endif
}
