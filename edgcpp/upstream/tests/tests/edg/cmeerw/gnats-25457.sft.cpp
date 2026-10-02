//type:fp
//options:--ms_c++20 --microsoft_version 1936:--ms_c++17 --microsoft_version 1936:--ms_c++20 --microsoft_version 1930:--ms_c++17 --microsoft_version 1927:--c++11 --microsoft_version 1900:--c++11 --microsoft_version 1800:--c++17 --gn 130200:--c++17 --clang_version 160000
//options_all:-w

#if defined(_MSC_VER) && _MSC_VER >= 1900 && _MSVC_LANG < 202002L
namespace minimal
{
  struct C {};
  C &c = C();
  static_assert(!__is_convertible_to(C, C &), "Unexpected");
}
#endif


#ifdef __GNUC__
#define is_convertible __is_convertible
#else
#define is_convertible __is_convertible_to
#endif

#if defined(_MSC_VER)
#if _MSC_VER < 1900
const bool permissive_traits = true;
const bool permissive = true;
#else
const bool permissive_traits = false;
#if _MSC_VER >= 1928 && _MSVC_LANG >= 202002L
const bool permissive = false;
#else
const bool permissive = true;
#endif
#endif
#else
const bool permissive_traits = false;
const bool permissive = false;
#endif


struct C { };

template<typename T>
T get();


void lvalue_ref(int &);
void rvalue_ref(int &&);

void lvalue_ref(C &);
void rvalue_ref(C &&);


template<typename T>
decltype(lvalue_ref(get<T>()), true) can_bind_to_lvalue_ref(int);

template<typename T>
int can_bind_to_lvalue_ref(long);

template<typename T>
decltype(rvalue_ref(get<T>()), true) can_bind_to_rvalue_ref(int);

template<typename T>
int can_bind_to_rvalue_ref(long);


namespace fundamental_types
{
  static_assert(!is_convertible(int, int &), "Unexpected");
  static_assert(!is_convertible(int &&, int &), "Unexpected");
  static_assert(!is_convertible(int &, int &&), "Unexpected");

  static_assert(sizeof(can_bind_to_lvalue_ref<int>(0)) != 1, "Unexpected");
  static_assert(sizeof(can_bind_to_lvalue_ref<int &&>(0)) != 1, "Unexpected");
  static_assert(sizeof(can_bind_to_lvalue_ref<int &>(0)) == 1, "Unexpected");

  static_assert(sizeof(can_bind_to_rvalue_ref<int>(0)) == 1, "Unexpected");
  static_assert(sizeof(can_bind_to_rvalue_ref<int &&>(0)) == 1, "Unexpected");
  static_assert(sizeof(can_bind_to_rvalue_ref<int &>(0)) != 1, "Unexpected");
}

namespace class_types
{
  static_assert(is_convertible(C, C &) == permissive_traits, "Unexpected");
  static_assert(is_convertible(C &&, C &) == permissive_traits, "Unexpected");
  static_assert(!is_convertible(C &, C &&), "Unexpected");

  static_assert((sizeof(can_bind_to_lvalue_ref<C>(0)) != 1) != permissive, "Unexpected");
  static_assert((sizeof(can_bind_to_lvalue_ref<C &&>(0)) != 1) != permissive, "Unexpected");
  static_assert(sizeof(can_bind_to_lvalue_ref<C &>(0)) == 1, "Unexpected");

  static_assert(sizeof(can_bind_to_rvalue_ref<C>(0)) == 1, "Unexpected");
  static_assert(sizeof(can_bind_to_rvalue_ref<C &&>(0)) == 1, "Unexpected");
  static_assert(sizeof(can_bind_to_rvalue_ref<C &>(0)) != 1, "Unexpected");
}
