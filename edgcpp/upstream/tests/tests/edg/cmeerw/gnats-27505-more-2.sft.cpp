//type:fp
//options:--c++20:--c++20 --gn 140200:--c++20 --clang_version 190100:--ms_c++20 --microsoft_version 1942

namespace member_fn_constraints
{
  struct D
  {
    template<int> void f();
  };

  template<typename T>
  struct B
  {
    D dep(T);

    int g() requires requires (int i) { dep(1).template f<0>(); };
    int g() requires requires (int i) { dep(i).template f<0>(); };
  };

  int i = B<int &>().g();
}

namespace member_fn_constraints_char
{
  struct D
  {
    template<int> void f();
  };

  template<typename T>
  struct B
  {
    D dep(T);

    int g() requires requires (char c) { dep(1).template f<0>(); };
    int g() requires requires (char c) { dep(c).template f<0>(); };
  };

  int i = B<char &>().g();
}

namespace member_fn_constraints_ref
{
  struct D
  {
    template<int> void f();
  };

  template<typename T>
  struct B
  {
    D nondep(int &);

    D dep(T &);

    int g1() requires requires (T i) { nondep(i).template f<0>(); };
    int g1() requires requires (T i) { nondep(+i).template f<0>(); };

    int g2() requires requires (T i) { dep(i).template f<0>(); };
    int g2() requires requires (T i) { dep(+i).template f<0>(); };
  };

  int i = B<int>().g1() + B<int>().g2();
}
