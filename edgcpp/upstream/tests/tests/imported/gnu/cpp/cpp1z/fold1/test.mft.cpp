//type: fn
//options: --c++17
# 0 "./cpp1z/fold1.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./cpp1z/fold1.C"




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
# 941 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/x86_64-pc-linux-gnu/bits/c++config.h" 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/pstl/pstl_config.h" 1 3
# 942 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/x86_64-pc-linux-gnu/bits/c++config.h" 2 3



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
# 6 "./cpp1z/fold1.C" 2
# 22 "./cpp1z/fold1.C"

# 22 "./cpp1z/fold1.C"
template<typename... Ts> auto unary_left_add (Ts... ts) { return (... + ts); } template<typename... Ts> auto unary_right_add (Ts... ts) { return (ts + ...); } template<typename T, typename... Ts> auto binary_left_add (T x, Ts... ts) { return (x + ... + ts); } template<typename T, typename... Ts> auto binary_right_add (T x, Ts... ts) { return (ts + ... + x); };
template<typename... Ts> auto unary_left_sub (Ts... ts) { return (... - ts); } template<typename... Ts> auto unary_right_sub (Ts... ts) { return (ts - ...); } template<typename T, typename... Ts> auto binary_left_sub (T x, Ts... ts) { return (x - ... - ts); } template<typename T, typename... Ts> auto binary_right_sub (T x, Ts... ts) { return (ts - ... - x); };

int main() {

  
# 27 "./cpp1z/fold1.C" 3 4
 ((
# 27 "./cpp1z/fold1.C"
 unary_left_add(1) == 1
# 27 "./cpp1z/fold1.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 27 "./cpp1z/fold1.C"
 "unary_left_add(1) == 1"
# 27 "./cpp1z/fold1.C" 3 4
 , "./cpp1z/fold1.C", 27, __PRETTY_FUNCTION__))
# 27 "./cpp1z/fold1.C"
                               ;
  
# 28 "./cpp1z/fold1.C" 3 4
 ((
# 28 "./cpp1z/fold1.C"
 unary_left_add(1, 2, 3) == 6
# 28 "./cpp1z/fold1.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 28 "./cpp1z/fold1.C"
 "unary_left_add(1, 2, 3) == 6"
# 28 "./cpp1z/fold1.C" 3 4
 , "./cpp1z/fold1.C", 28, __PRETTY_FUNCTION__))
# 28 "./cpp1z/fold1.C"
                                     ;


  
# 31 "./cpp1z/fold1.C" 3 4
 ((
# 31 "./cpp1z/fold1.C"
 unary_right_add(1) == 1
# 31 "./cpp1z/fold1.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 31 "./cpp1z/fold1.C"
 "unary_right_add(1) == 1"
# 31 "./cpp1z/fold1.C" 3 4
 , "./cpp1z/fold1.C", 31, __PRETTY_FUNCTION__))
# 31 "./cpp1z/fold1.C"
                                ;
  
# 32 "./cpp1z/fold1.C" 3 4
 ((
# 32 "./cpp1z/fold1.C"
 unary_right_add(1, 2, 3) == 6
# 32 "./cpp1z/fold1.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 32 "./cpp1z/fold1.C"
 "unary_right_add(1, 2, 3) == 6"
# 32 "./cpp1z/fold1.C" 3 4
 , "./cpp1z/fold1.C", 32, __PRETTY_FUNCTION__))
# 32 "./cpp1z/fold1.C"
                                      ;

  
# 34 "./cpp1z/fold1.C" 3 4
 ((
# 34 "./cpp1z/fold1.C"
 binary_left_add(1) == 1
# 34 "./cpp1z/fold1.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 34 "./cpp1z/fold1.C"
 "binary_left_add(1) == 1"
# 34 "./cpp1z/fold1.C" 3 4
 , "./cpp1z/fold1.C", 34, __PRETTY_FUNCTION__))
# 34 "./cpp1z/fold1.C"
                                ;
  
# 35 "./cpp1z/fold1.C" 3 4
 ((
# 35 "./cpp1z/fold1.C"
 binary_left_add(1, 1) == 2
# 35 "./cpp1z/fold1.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 35 "./cpp1z/fold1.C"
 "binary_left_add(1, 1) == 2"
# 35 "./cpp1z/fold1.C" 3 4
 , "./cpp1z/fold1.C", 35, __PRETTY_FUNCTION__))
# 35 "./cpp1z/fold1.C"
                                   ;
  
# 36 "./cpp1z/fold1.C" 3 4
 ((
# 36 "./cpp1z/fold1.C"
 binary_left_add(1, 1, 2, 3) == 7
# 36 "./cpp1z/fold1.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 36 "./cpp1z/fold1.C"
 "binary_left_add(1, 1, 2, 3) == 7"
# 36 "./cpp1z/fold1.C" 3 4
 , "./cpp1z/fold1.C", 36, __PRETTY_FUNCTION__))
# 36 "./cpp1z/fold1.C"
                                         ;

  
# 38 "./cpp1z/fold1.C" 3 4
 ((
# 38 "./cpp1z/fold1.C"
 binary_right_add(1) == 1
# 38 "./cpp1z/fold1.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 38 "./cpp1z/fold1.C"
 "binary_right_add(1) == 1"
# 38 "./cpp1z/fold1.C" 3 4
 , "./cpp1z/fold1.C", 38, __PRETTY_FUNCTION__))
# 38 "./cpp1z/fold1.C"
                                 ;
  
# 39 "./cpp1z/fold1.C" 3 4
 ((
# 39 "./cpp1z/fold1.C"
 binary_right_add(1, 1) == 2
# 39 "./cpp1z/fold1.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 39 "./cpp1z/fold1.C"
 "binary_right_add(1, 1) == 2"
# 39 "./cpp1z/fold1.C" 3 4
 , "./cpp1z/fold1.C", 39, __PRETTY_FUNCTION__))
# 39 "./cpp1z/fold1.C"
                                    ;
  
# 40 "./cpp1z/fold1.C" 3 4
 ((
# 40 "./cpp1z/fold1.C"
 binary_right_add(1, 1, 2, 3) == 7
# 40 "./cpp1z/fold1.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 40 "./cpp1z/fold1.C"
 "binary_right_add(1, 1, 2, 3) == 7"
# 40 "./cpp1z/fold1.C" 3 4
 , "./cpp1z/fold1.C", 40, __PRETTY_FUNCTION__))
# 40 "./cpp1z/fold1.C"
                                          ;


  
# 43 "./cpp1z/fold1.C" 3 4
 ((
# 43 "./cpp1z/fold1.C"
 unary_left_sub(1) == 1
# 43 "./cpp1z/fold1.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 43 "./cpp1z/fold1.C"
 "unary_left_sub(1) == 1"
# 43 "./cpp1z/fold1.C" 3 4
 , "./cpp1z/fold1.C", 43, __PRETTY_FUNCTION__))
# 43 "./cpp1z/fold1.C"
                               ;
  
# 44 "./cpp1z/fold1.C" 3 4
 ((
# 44 "./cpp1z/fold1.C"
 unary_left_sub(1, 2, 3) == -4
# 44 "./cpp1z/fold1.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 44 "./cpp1z/fold1.C"
 "unary_left_sub(1, 2, 3) == -4"
# 44 "./cpp1z/fold1.C" 3 4
 , "./cpp1z/fold1.C", 44, __PRETTY_FUNCTION__))
# 44 "./cpp1z/fold1.C"
                                      ;


  
# 47 "./cpp1z/fold1.C" 3 4
 ((
# 47 "./cpp1z/fold1.C"
 unary_right_sub(1) == 1
# 47 "./cpp1z/fold1.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 47 "./cpp1z/fold1.C"
 "unary_right_sub(1) == 1"
# 47 "./cpp1z/fold1.C" 3 4
 , "./cpp1z/fold1.C", 47, __PRETTY_FUNCTION__))
# 47 "./cpp1z/fold1.C"
                                ;
  
# 48 "./cpp1z/fold1.C" 3 4
 ((
# 48 "./cpp1z/fold1.C"
 unary_right_sub(1, 2, 3) == 2
# 48 "./cpp1z/fold1.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 48 "./cpp1z/fold1.C"
 "unary_right_sub(1, 2, 3) == 2"
# 48 "./cpp1z/fold1.C" 3 4
 , "./cpp1z/fold1.C", 48, __PRETTY_FUNCTION__))
# 48 "./cpp1z/fold1.C"
                                      ;

  
# 50 "./cpp1z/fold1.C" 3 4
 ((
# 50 "./cpp1z/fold1.C"
 binary_left_sub(1) == 1
# 50 "./cpp1z/fold1.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 50 "./cpp1z/fold1.C"
 "binary_left_sub(1) == 1"
# 50 "./cpp1z/fold1.C" 3 4
 , "./cpp1z/fold1.C", 50, __PRETTY_FUNCTION__))
# 50 "./cpp1z/fold1.C"
                                ;
  
# 51 "./cpp1z/fold1.C" 3 4
 ((
# 51 "./cpp1z/fold1.C"
 binary_left_sub(1, 1) == 0
# 51 "./cpp1z/fold1.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 51 "./cpp1z/fold1.C"
 "binary_left_sub(1, 1) == 0"
# 51 "./cpp1z/fold1.C" 3 4
 , "./cpp1z/fold1.C", 51, __PRETTY_FUNCTION__))
# 51 "./cpp1z/fold1.C"
                                   ;
  
# 52 "./cpp1z/fold1.C" 3 4
 ((
# 52 "./cpp1z/fold1.C"
 binary_left_sub(1, 1, 2, 3) == -5
# 52 "./cpp1z/fold1.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 52 "./cpp1z/fold1.C"
 "binary_left_sub(1, 1, 2, 3) == -5"
# 52 "./cpp1z/fold1.C" 3 4
 , "./cpp1z/fold1.C", 52, __PRETTY_FUNCTION__))
# 52 "./cpp1z/fold1.C"
                                          ;

  
# 54 "./cpp1z/fold1.C" 3 4
 ((
# 54 "./cpp1z/fold1.C"
 binary_right_sub(1) == 1
# 54 "./cpp1z/fold1.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 54 "./cpp1z/fold1.C"
 "binary_right_sub(1) == 1"
# 54 "./cpp1z/fold1.C" 3 4
 , "./cpp1z/fold1.C", 54, __PRETTY_FUNCTION__))
# 54 "./cpp1z/fold1.C"
                                 ;
  
# 55 "./cpp1z/fold1.C" 3 4
 ((
# 55 "./cpp1z/fold1.C"
 binary_right_sub(1, 1) == 0
# 55 "./cpp1z/fold1.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 55 "./cpp1z/fold1.C"
 "binary_right_sub(1, 1) == 0"
# 55 "./cpp1z/fold1.C" 3 4
 , "./cpp1z/fold1.C", 55, __PRETTY_FUNCTION__))
# 55 "./cpp1z/fold1.C"
                                    ;
  
# 56 "./cpp1z/fold1.C" 3 4
 ((
# 56 "./cpp1z/fold1.C"
 binary_right_sub(1, 1, 2, 3) == 1
# 56 "./cpp1z/fold1.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 56 "./cpp1z/fold1.C"
 "binary_right_sub(1, 1, 2, 3) == 1"
# 56 "./cpp1z/fold1.C" 3 4
 , "./cpp1z/fold1.C", 56, __PRETTY_FUNCTION__))
# 56 "./cpp1z/fold1.C"
                                          ;
}
