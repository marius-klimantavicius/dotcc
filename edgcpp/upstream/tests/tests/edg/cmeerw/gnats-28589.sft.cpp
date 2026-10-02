//type:fp
//options:--c++20:--c++20 --gn 150100:--c++20 --clang_version 210100:--ms_c++20 --microsoft_version 1950

namespace minimal
{
  template<bool ... Bs> requires (Bs && ...)
  int f();
  namespace ns {
    int operator ""_udl(unsigned long long);
  }
  using ns::operator ""_udl;
  int i = f<true, true>() + 2_udl;
}

namespace template_requires_left_fold
{
  template<bool ... Bs> requires (... && Bs)
  int f();

  template<bool ... Bs> requires (true && ... && Bs)
  int g();

  namespace ns {
    int operator ""_udl(unsigned long long);
  }
  using ns::operator ""_udl;
  int i = f<true, true>() + 2_udl;
  int j = g<true, true>() + 2_udl;
}

namespace template_requires_right_fold
{
  template<bool ... Bs> requires (Bs && ...)
  int f();

  template<bool ... Bs> requires (Bs && ... && true)
  int g();

  namespace ns {
    int operator ""_udl(unsigned long long);
  }
  using ns::operator ""_udl;

  int i = f<true, true>() + 2_udl;
  int j = g<true, true>() + 2_udl;
}

namespace trailing_requires_left_fold
{
  template<bool ... Bs>
  int f() requires (... && Bs);

  template<bool ... Bs>
  int g() requires (true && ... && Bs);

  namespace ns {
    int operator ""_udl(unsigned long long);
  }
  using ns::operator ""_udl;

  int i = f<true, true>() + 2_udl;
  int j = g<true, true>() + 2_udl;
}

namespace trailing_requires_right_fold
{
  template<bool ... Bs>
  int f() requires (Bs && ...);

  template<bool ... Bs>
  int g() requires (Bs && ... && true);

  namespace ns {
    int operator ""_udl(unsigned long long);
  }
  using ns::operator ""_udl;

  int i = f<true, true>() + 2_udl;
  int j = g<true, true>() + 2_udl;
}

namespace old_pr
{
  namespace ns
  {
    constexpr int operator ""_udl(unsigned long long) {
      return 0;
    }
  }

  template<typename T, T VAL>
  struct A { };

  using namespace ns;
  A<int, 1_udl> a;
}
