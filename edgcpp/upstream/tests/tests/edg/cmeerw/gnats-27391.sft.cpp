//type:fp
//options:--gn 120300 --target linux_x86_64;fn:--gn 120400 --target linux_x86_64:--gn 130100 --target linux_x86_64;fn:--gn 130300 --target linux_x86_64:--gn 120300 --target linux_armv7;fn:--gn 120400 --target linux_armv7:--gn 130100 --target linux_armv7;fn:--gn 130200 --target linux_armv7
//options_all:--c++11

#if defined(__x86_64)
void f(const int ci, int i)
{
  __builtin_ia32_ldtilecfg(&ci); // only available in 12.4.0 and 13.3.0
  __builtin_ia32_ldtilecfg(&i);  // only available in 12.4.0 and 13.3.0
  __builtin_ia32_sttilecfg(&i);  // only available in 12.4.0 and 13.3.0
}
#endif

template<typename T>
struct is_int
{
  static const bool value = false;
};

template<>
struct is_int<int>
{
  static const bool value = true;
};

#if defined(__arm__)
static_assert(is_int<decltype(__builtin_arm_ssat(0, 0u))>::value, "returns int for 12.4.0 and 13.2.0");
static_assert(is_int<decltype(__builtin_arm_ssat16(0, 0u))>::value, "returns int for 12.4.0 and 13.2.0");
static_assert(is_int<decltype(__builtin_arm_usat16(0, 0u))>::value, "returns int for 12.4.0 and 13.2.0");
#endif
