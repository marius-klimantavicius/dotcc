//type:fp
//options:--c++20 -A:--c++ --clang_version 210100 --ms_extensions:--c++20 --clang_version 210100 --ms_extensions:--ms_c++20 --microsoft_version 1944

namespace minimal
{
  template<typename>
  struct C {
    template<typename>
    friend struct B;
    struct B;
  };
}

namespace friend_class
{
  namespace outer_class
  {
    namespace inner
    {
      struct C
      {
        friend struct B;

        struct B;
      };
    }

    template<int>
    struct B;

    namespace inner
    {
#if defined(_MSC_VER)
      B *p;
#else
      B<0> *p;
#endif
    }
  }

  namespace outer_template
  {
    namespace inner
    {
      template<typename T>
      struct C
      {
        friend struct B;

        struct B;
      };

      C<int> c;
    }

    template<int>
    struct B;

    namespace inner
    {
#if defined(_MSC_VER)
      B *p;
#else
      B<0> *p;
#endif
    }
  }
}

namespace friend_class_template
{
  namespace outer_class
  {
    namespace inner
    {
      struct C
      {
        template<typename>
        friend struct B;

        struct B;
      };
    }

    template<int>
    struct B;

    namespace inner
    {
#if defined(_MSC_VER)
      B<int> *p;
#else
      B<0> *p;
#endif
    }
  }

  namespace outer_template
  {
    namespace inner
    {
      template<typename T>
      struct C
      {
        template<typename>
        friend struct B;

        struct B;
      };

      C<int> c;
    }

    template<int>
    struct B;

    namespace inner
    {
#if defined(_MSC_VER)
      B<int> *p;
#else
      B<0> *p;
#endif
    }
  }
}
