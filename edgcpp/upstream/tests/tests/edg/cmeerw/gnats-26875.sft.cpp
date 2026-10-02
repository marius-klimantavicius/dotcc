//type:fp
//options:--c++14:--c++20:--ms_c++17 --microsoft_version 1916:--ms_c++20 --microsoft_version 1938:--c++20 --gn 140100:--c++20 --clang_version 180100

namespace minimal
{
  template<typename T>
  T var = reinterpret_cast<T>(nullptr);
  template long long var<long long>;
#ifdef _MSC_VER
  void *p = reinterpret_cast<void *>(nullptr);
#endif
}

namespace template_arg
{
#if defined(_MSC_VER) || (defined(__GNUC__) && !defined(__clang__))
  template<long long>
  struct C1
  { };

  C1<reinterpret_cast<long long>(nullptr)> c1;

  template<typename T, T>
  struct C2
  { };

  C2<long long, reinterpret_cast<long long>(nullptr)> c2;

  template<typename T, T = reinterpret_cast<T>(nullptr)>
  struct C3
  { };

  C3<long long> c3;
#endif

  struct C
  {
    int f();
    int m;
  };

#ifdef _MSC_VER
  C3<void *> c3vp;
  C3<int *> c3ip;

  C3<int C::*> c3imp;
  C3<int (C::*)()> c3imfp;
#endif
}

namespace dpdt_destination
{
  template<typename T>
  constexpr T f()
  {
    return reinterpret_cast<T>(nullptr);
  }

  template long long f();
  template unsigned long long f();

#if defined(_MSC_VER) || (defined(__GNUC__) && !defined(__clang__))
  static_assert(f<long long>() == 0, "");
  static_assert(f<unsigned long long>() == 0, "");
#endif

  struct C
  {
    int f();
    int m;
  };

#ifdef _MSC_VER
  template void *f();
  template char *f();
  template int *f();
#endif
}
