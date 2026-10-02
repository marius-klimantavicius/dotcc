//type: rp
//options: --c++11
# 0 "./ext/underlying_type10.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./ext/underlying_type10.C"



# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/cassert" 1 3
# 45 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/cassert" 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/x86_64-pc-linux-gnu/bits/c++config.h" 1 3
# 37 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/x86_64-pc-linux-gnu/bits/c++config.h" 3
#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wvariadic-macros"

#pragma GCC diagnostic ignored "-Wc++11-extensions"
#pragma GCC diagnostic ignored "-Wc++23-extensions"
# 328 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/x86_64-pc-linux-gnu/bits/c++config.h" 3
namespace std
{
  typedef long unsigned int size_t;
  typedef long int ptrdiff_t;


  typedef decltype(nullptr) nullptr_t;


#pragma GCC visibility push(default)


  extern "C++" __attribute__ ((__noreturn__, __always_inline__))
  inline void __terminate() noexcept
  {
    void terminate() noexcept __attribute__ ((__noreturn__,__cold__));
    terminate();
  }
#pragma GCC visibility pop
}
# 361 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/x86_64-pc-linux-gnu/bits/c++config.h" 3
namespace std
{
  inline namespace __cxx11 __attribute__((__abi_tag__ ("cxx11"))) { }
}
namespace __gnu_cxx
{
  inline namespace __cxx11 __attribute__((__abi_tag__ ("cxx11"))) { }
}
# 565 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/x86_64-pc-linux-gnu/bits/c++config.h" 3
namespace std
{
#pragma GCC visibility push(default)




  __attribute__((__always_inline__))
  constexpr inline bool
  __is_constant_evaluated() noexcept
  {





    return __builtin_is_constant_evaluated();



  }
#pragma GCC visibility pop
}
# 609 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/x86_64-pc-linux-gnu/bits/c++config.h" 3
namespace std
{
#pragma GCC visibility push(default)

  extern "C++" __attribute__ ((__noreturn__)) __attribute__((__cold__))
  void
  __glibcxx_assert_fail
    (const char* __file, int __line, const char* __function,
     const char* __condition)
  noexcept;
#pragma GCC visibility pop
}
# 719 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/x86_64-pc-linux-gnu/bits/c++config.h" 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/x86_64-pc-linux-gnu/bits/os_defines.h" 1 3
# 39 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/x86_64-pc-linux-gnu/bits/os_defines.h" 3
# 1 "/usr/include/features.h" 1 3 4
# 375 "/usr/include/features.h" 3 4
# 1 "/usr/include/sys/cdefs.h" 1 3 4
# 392 "/usr/include/sys/cdefs.h" 3 4
# 1 "/usr/include/bits/wordsize.h" 1 3 4
# 393 "/usr/include/sys/cdefs.h" 2 3 4
# 376 "/usr/include/features.h" 2 3 4
# 399 "/usr/include/features.h" 3 4
# 1 "/usr/include/gnu/stubs.h" 1 3 4
# 10 "/usr/include/gnu/stubs.h" 3 4
# 1 "/usr/include/gnu/stubs-64.h" 1 3 4
# 11 "/usr/include/gnu/stubs.h" 2 3 4
# 400 "/usr/include/features.h" 2 3 4
# 40 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/x86_64-pc-linux-gnu/bits/os_defines.h" 2 3
# 720 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/x86_64-pc-linux-gnu/bits/c++config.h" 2 3


# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/x86_64-pc-linux-gnu/bits/cpu_defines.h" 1 3
# 723 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/x86_64-pc-linux-gnu/bits/c++config.h" 2 3
# 879 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/x86_64-pc-linux-gnu/bits/c++config.h" 3
namespace __gnu_cxx
{
  typedef __decltype(0.0bf16) __bfloat16_t;
}
# 945 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/x86_64-pc-linux-gnu/bits/c++config.h" 3
#pragma GCC diagnostic pop
# 46 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/cassert" 2 3
# 1 "/usr/include/assert.h" 1 3 4
# 65 "/usr/include/assert.h" 3 4
extern "C" {


extern void __assert_fail (const char *__assertion, const char *__file,
      unsigned int __line, const char *__function)
     throw () __attribute__ ((__noreturn__));


extern void __assert_perror_fail (int __errnum, const char *__file,
      unsigned int __line, const char *__function)
     throw () __attribute__ ((__noreturn__));




extern void __assert (const char *__assertion, const char *__file, int __line)
     throw () __attribute__ ((__noreturn__));


}
# 47 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/cassert" 2 3
# 5 "./ext/underlying_type10.C" 2


# 6 "./ext/underlying_type10.C"
enum E1 : unsigned { E1_en = 1 };
enum E2 : char { E2_en = 1 };
enum class E3 { a = -1 };
enum class E4 : unsigned char { c = 1 };
enum class E5 : int { a = -1, b = 1 };
enum class E6 : long { c = 0x7fffffffffffffffL };

template<typename T>
  struct underlying_type
  { typedef __underlying_type(T) type; };

template<typename T>
  void
  test(T t, typename underlying_type<T>::type v)
  {
    
# 21 "./ext/underlying_type10.C" 3 4
   ((
# 21 "./ext/underlying_type10.C"
   t == T(v)
# 21 "./ext/underlying_type10.C" 3 4
   ) ? static_cast<void> (0) : __assert_fail (
# 21 "./ext/underlying_type10.C"
   "t == T(v)"
# 21 "./ext/underlying_type10.C" 3 4
   , "./ext/underlying_type10.C", 21, __PRETTY_FUNCTION__))
# 21 "./ext/underlying_type10.C"
                      ;
  }

int main()
{
  test(E1::E1_en, 1);
  test(E2::E2_en, 1);
  test(E3::a, -1);
  test(E4::c, 1);
  test(E5::a, -1);
  test(E6::c, 0x7fffffffffffffffL);
}
