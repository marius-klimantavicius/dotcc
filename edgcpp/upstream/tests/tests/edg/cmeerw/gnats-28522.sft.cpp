//type:fp
//options:--c++11 -A:--c++20 -A:--c++11 --gn 150200:--c++11 --clang_version 210100:--ms_c++20 --microsoft_version 1950

namespace minimal
{
  template<typename ... Ts> int f(Ts &...); // #1
  template<typename T>      int f(T &&);    // #2
  int i;
  int j = f(i);
}

#if defined(_MSC_VER) || defined(__GNUC__) || defined(__clang__)
namespace deleted_fn
{
  template<typename ... Ts> int f(Ts &...) = delete;
  template<typename T> int f(T &&);
  int i;
  int j = f(i);
}
#endif

constexpr bool cwg1395 =
#if defined(_MSC_VER) || defined(__GNUC__) || defined(__clang__)
  false;
#else
  true;
#endif

template<int>
struct R { };

namespace pack_vs_non_pack
{
  template<typename ... T> R<1> f(T &...);
  template<typename T> R<2> f(T &);
  int i;
  R<2> v = f(i);
}

namespace ptr_vs_non_ptr
{
  template<typename T> R<1> f(T *);
  template<typename T> R<2> f(T);
  R<1> v = f((int *) 0);
}

namespace ptr_vs_non_ptr_both_packs
{
  template<typename ... T> R<1> f(T *...);
  template<typename ... T> R<2> f(T ...);
  R<1> v = f((int *) 0);
}

namespace ptr_vs_non_ptr_pack_vs_non_pack
{
  template<typename T> R<1> f(T *);
  template<typename ... T> R<2> f(T ...);
  R<1> v = f((int *) 0);
}

namespace trailing_pack
{
  template<typename T> R<1> f(T);
  template<typename T, typename ... Us> R<2> f(T, Us ...);

  R<1> r1 = f(1);
  R<2> r2 = f(1, 2);
}

namespace both_trailing_packs
{
  template<typename ... Ts> R<1> f(Ts ...);
  template<typename T, typename ... Us> R<2> f(T, Us ...);

  R<1> r0 = f();
  R<2> r1 = f(1);
  R<2> r2 = f(1, 2);
}

namespace lvalue_ref_pack_vs_rvalue_ref
{
  template<typename ... Ts> R<1> f(Ts &...);
  template<typename T>      R<2> f(T &&);
  int i;
  R<cwg1395 ? 1 : 2> r = f(i);
}

namespace rvalue_ref_pack_vs_lvalue_ref
{
  template<typename ... Ts> R<1> f(Ts &&...);
  template<typename T>      R<2> f(T &);
  int i;
  R<2> r = f(i);
}
