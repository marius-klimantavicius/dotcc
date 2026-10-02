//type:fp
//options:--c++11:--c++20:--c++20 --gn 150100:--c++20 --clang_version 210100:--ms_c++20 --microsoft_version 1944

namespace minimal
{
  using INT = int;
  template<int> struct B {};
  template<typename>
  struct C {
    template<INT i, B<i + 1> * = nullptr>
    friend int f(C, B<i>) {
      return 0;
    }
  };
  int i = f(C<int>{}, B<0>{});
}

namespace unary_op
{
  using INT = int;
  template<int> struct B {};
  template<typename>
  struct C {
    template<INT i, B<-i> * = nullptr>
    friend int f(C, B<i>) {
      return 0;
    }
  };
  int i = f(C<int>{}, B<0>{});
}

#if __cplusplus >= 202002
namespace cpp_20
{
  using INT = int;

  template<bool>
  struct X { };

  template<typename>
  struct C {
    template<INT i, X<i == 0> * = nullptr>
    friend int f(C) {
      return 0;
    }
  };

  int i = f<0>(C<int>{});
}
#endif
