//type:fp
//options:--c++11:--c++20:--c++20 --gn 150100:--c++20 --clang_version 220100:--ms_c++20 --microsoft_version 1950

namespace minimal {
  namespace ns1 { template<typename T> using A = T; }
  namespace ns2 { template<typename T> using A = void; }
  namespace ns {
    using namespace ns1;
    template<typename> struct C {
      template<typename T> using type = A<T>;
    };
  }
  using namespace ns2;
  ns::C<int>::type<int> i = 1;
}

namespace outer_namespace_scope {
  using X = int;

  namespace ns
  {
    template<typename U> struct C
    {
      template<typename T> using type = X;
    };

    using X = void;
  }

  ns::C<int>::type<int> i = 1;
}

namespace decltype_fn_call {
  int fn(long);

  template<typename U> struct C
  {
    template<typename T> using type1 = decltype(fn(U{}));
    template<typename T> using type2 = decltype(fn(T{}));
  };

  void fn(int);

  C<int>::type1<int> i = 1;
  C<int>::type2<int> j = 1;
}
