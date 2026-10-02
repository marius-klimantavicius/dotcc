//type:fp
//options:--c++20:--c++20 --gn 150100:--c++20 --clang_version 200100:--ms_c++20 --microsoft_version 1942

namespace minimal
{
  template<typename T> constexpr bool v = true;
  template<typename T> struct C {
    static int f() requires v<T>;
  };
  namespace ns {
    int j = C<int>::f();
  }
}

namespace from_global_init
{
  namespace ns1
  {
    template<typename T>
    constexpr bool var = true;
  }

  namespace ns2
  {
    template<typename T>
    struct C
    {
      static int f() requires ns1::var<T>;
    };
  }

  namespace ns3
  {
    int j = ns2::C<int>::f();
  }
}

namespace from_lambda_block_scope
{
  namespace ns1
  {
    template<typename T>
    constexpr bool var = true;
  }

  namespace ns2
  {
    template<typename T>
    struct C
    {
      static int f() requires ns1::var<T>;
    };
  }

  namespace ns3
  {
    void f()
    {
      struct D
      {
        void g()
        {
          ns2::C<int>::f();
        }
      };
    }
  }
}
