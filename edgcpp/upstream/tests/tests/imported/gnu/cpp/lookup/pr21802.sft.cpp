//type: rp
//options: 
# 0 "./lookup/pr21802.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./lookup/pr21802.C"





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
# 7 "./lookup/pr21802.C" 2


# 8 "./lookup/pr21802.C"
struct X;
int I = 6;





template <typename T>
inline int operator+(const X &, T x) { return x; }
inline int operator-(const X &, int x) { return x; }
inline int operator*(const X &, int x) { return x; }
inline int operator/(const X &, int x) { return x; }
inline int operator+=(const X &, int x) { return x; }

struct X
{
  X () : m (1) { }
  template <typename T>
  int operator%(T x) { return m + x; }
  virtual int operator>>(int x) { return m + x; }
  int operator<<(int x) { return m + x; }
  int operator&(int x) { return m + x; }
  int operator|(int x) { return m + x; }
  int operator^(int x) { return m + x; }
  int operator&&(int x) { return m + x; }
  int operator||(int x) { return m + x; }
  friend int operator==(X o, int x) { return o.m + x; }
  int operator!=(int x) { return m + x; }
  int operator<(int x) { return m + x; }
  int operator<=(int x) { return m + x; }
  int operator>(int x) { return m + x; }
  int operator>=(int x) { return m + x; }
  int operator*() { return m + I; }
  int operator!() { return m + I; }
  int operator~() { return m + I; }
  int operator++() { return m + I + 100; }
  int operator--() { return m + I + 100; }
  int operator++(int) { return m + I; }
  int operator--(int) { return m + I; }
  int operator()() { return m + I; }
  int operator,(int x) { return m + x; }
  int operator[](int x) { return m + x; }
  int operator*=(int x) { return m + x; }
  int operator-=(int x) { return m + x; }
  int operator/=(int x) { return m + x; }
  virtual int operator& () { return m + I; }
  int m;
};
struct Y : virtual X
{

  int operator>>(int x) { return m + x + 1; }
  int operator& () { return m + I + 1; }


  template <typename T>
  int operator&(T x) { return m + x + 1; }
  friend int operator==(Y o, int x) { return o.m + x + 1; }
  int operator!=(int x) { return m + x + 1; }
};




template <typename T>
void
Foo1 (T)
{
  Y x;
  { int t = x + I; 
# 77 "./lookup/pr21802.C" 3 4
                  ((
# 77 "./lookup/pr21802.C"
                  t == 6
# 77 "./lookup/pr21802.C" 3 4
                  ) ? static_cast<void> (0) : __assert_fail (
# 77 "./lookup/pr21802.C"
                  "t == 6"
# 77 "./lookup/pr21802.C" 3 4
                  , "./lookup/pr21802.C", 77, __PRETTY_FUNCTION__))
# 77 "./lookup/pr21802.C"
                                 ; }
  { int t = x - I; 
# 78 "./lookup/pr21802.C" 3 4
                  ((
# 78 "./lookup/pr21802.C"
                  t == 6
# 78 "./lookup/pr21802.C" 3 4
                  ) ? static_cast<void> (0) : __assert_fail (
# 78 "./lookup/pr21802.C"
                  "t == 6"
# 78 "./lookup/pr21802.C" 3 4
                  , "./lookup/pr21802.C", 78, __PRETTY_FUNCTION__))
# 78 "./lookup/pr21802.C"
                                 ; }
  { int t = x * I; 
# 79 "./lookup/pr21802.C" 3 4
                  ((
# 79 "./lookup/pr21802.C"
                  t == 6
# 79 "./lookup/pr21802.C" 3 4
                  ) ? static_cast<void> (0) : __assert_fail (
# 79 "./lookup/pr21802.C"
                  "t == 6"
# 79 "./lookup/pr21802.C" 3 4
                  , "./lookup/pr21802.C", 79, __PRETTY_FUNCTION__))
# 79 "./lookup/pr21802.C"
                                 ; }
  { int t = x / I; 
# 80 "./lookup/pr21802.C" 3 4
                  ((
# 80 "./lookup/pr21802.C"
                  t == 6
# 80 "./lookup/pr21802.C" 3 4
                  ) ? static_cast<void> (0) : __assert_fail (
# 80 "./lookup/pr21802.C"
                  "t == 6"
# 80 "./lookup/pr21802.C" 3 4
                  , "./lookup/pr21802.C", 80, __PRETTY_FUNCTION__))
# 80 "./lookup/pr21802.C"
                                 ; }
  { int t = (x+=I); 
# 81 "./lookup/pr21802.C" 3 4
                   ((
# 81 "./lookup/pr21802.C"
                   t == 6
# 81 "./lookup/pr21802.C" 3 4
                   ) ? static_cast<void> (0) : __assert_fail (
# 81 "./lookup/pr21802.C"
                   "t == 6"
# 81 "./lookup/pr21802.C" 3 4
                   , "./lookup/pr21802.C", 81, __PRETTY_FUNCTION__))
# 81 "./lookup/pr21802.C"
                                  ; }

  { int t = x % I; 
# 83 "./lookup/pr21802.C" 3 4
                  ((
# 83 "./lookup/pr21802.C"
                  t == 7
# 83 "./lookup/pr21802.C" 3 4
                  ) ? static_cast<void> (0) : __assert_fail (
# 83 "./lookup/pr21802.C"
                  "t == 7"
# 83 "./lookup/pr21802.C" 3 4
                  , "./lookup/pr21802.C", 83, __PRETTY_FUNCTION__))
# 83 "./lookup/pr21802.C"
                                 ; }
  { int t = x << I; 
# 84 "./lookup/pr21802.C" 3 4
                   ((
# 84 "./lookup/pr21802.C"
                   t == 7
# 84 "./lookup/pr21802.C" 3 4
                   ) ? static_cast<void> (0) : __assert_fail (
# 84 "./lookup/pr21802.C"
                   "t == 7"
# 84 "./lookup/pr21802.C" 3 4
                   , "./lookup/pr21802.C", 84, __PRETTY_FUNCTION__))
# 84 "./lookup/pr21802.C"
                                  ; }
  { int t = x | I; 
# 85 "./lookup/pr21802.C" 3 4
                  ((
# 85 "./lookup/pr21802.C"
                  t == 7
# 85 "./lookup/pr21802.C" 3 4
                  ) ? static_cast<void> (0) : __assert_fail (
# 85 "./lookup/pr21802.C"
                  "t == 7"
# 85 "./lookup/pr21802.C" 3 4
                  , "./lookup/pr21802.C", 85, __PRETTY_FUNCTION__))
# 85 "./lookup/pr21802.C"
                                 ; }
  { int t = x && I; 
# 86 "./lookup/pr21802.C" 3 4
                   ((
# 86 "./lookup/pr21802.C"
                   t == 7
# 86 "./lookup/pr21802.C" 3 4
                   ) ? static_cast<void> (0) : __assert_fail (
# 86 "./lookup/pr21802.C"
                   "t == 7"
# 86 "./lookup/pr21802.C" 3 4
                   , "./lookup/pr21802.C", 86, __PRETTY_FUNCTION__))
# 86 "./lookup/pr21802.C"
                                  ; }
  { int t = x || I; 
# 87 "./lookup/pr21802.C" 3 4
                   ((
# 87 "./lookup/pr21802.C"
                   t == 7
# 87 "./lookup/pr21802.C" 3 4
                   ) ? static_cast<void> (0) : __assert_fail (
# 87 "./lookup/pr21802.C"
                   "t == 7"
# 87 "./lookup/pr21802.C" 3 4
                   , "./lookup/pr21802.C", 87, __PRETTY_FUNCTION__))
# 87 "./lookup/pr21802.C"
                                  ; }
  { int t = x < I; 
# 88 "./lookup/pr21802.C" 3 4
                  ((
# 88 "./lookup/pr21802.C"
                  t == 7
# 88 "./lookup/pr21802.C" 3 4
                  ) ? static_cast<void> (0) : __assert_fail (
# 88 "./lookup/pr21802.C"
                  "t == 7"
# 88 "./lookup/pr21802.C" 3 4
                  , "./lookup/pr21802.C", 88, __PRETTY_FUNCTION__))
# 88 "./lookup/pr21802.C"
                                 ; }
  { int t = x <= I; 
# 89 "./lookup/pr21802.C" 3 4
                   ((
# 89 "./lookup/pr21802.C"
                   t == 7
# 89 "./lookup/pr21802.C" 3 4
                   ) ? static_cast<void> (0) : __assert_fail (
# 89 "./lookup/pr21802.C"
                   "t == 7"
# 89 "./lookup/pr21802.C" 3 4
                   , "./lookup/pr21802.C", 89, __PRETTY_FUNCTION__))
# 89 "./lookup/pr21802.C"
                                  ; }
  { int t = x > I; 
# 90 "./lookup/pr21802.C" 3 4
                  ((
# 90 "./lookup/pr21802.C"
                  t == 7
# 90 "./lookup/pr21802.C" 3 4
                  ) ? static_cast<void> (0) : __assert_fail (
# 90 "./lookup/pr21802.C"
                  "t == 7"
# 90 "./lookup/pr21802.C" 3 4
                  , "./lookup/pr21802.C", 90, __PRETTY_FUNCTION__))
# 90 "./lookup/pr21802.C"
                                 ; }
  { int t = x >= I; 
# 91 "./lookup/pr21802.C" 3 4
                   ((
# 91 "./lookup/pr21802.C"
                   t == 7
# 91 "./lookup/pr21802.C" 3 4
                   ) ? static_cast<void> (0) : __assert_fail (
# 91 "./lookup/pr21802.C"
                   "t == 7"
# 91 "./lookup/pr21802.C" 3 4
                   , "./lookup/pr21802.C", 91, __PRETTY_FUNCTION__))
# 91 "./lookup/pr21802.C"
                                  ; }
  { int t = *x; 
# 92 "./lookup/pr21802.C" 3 4
               ((
# 92 "./lookup/pr21802.C"
               t == 7
# 92 "./lookup/pr21802.C" 3 4
               ) ? static_cast<void> (0) : __assert_fail (
# 92 "./lookup/pr21802.C"
               "t == 7"
# 92 "./lookup/pr21802.C" 3 4
               , "./lookup/pr21802.C", 92, __PRETTY_FUNCTION__))
# 92 "./lookup/pr21802.C"
                              ; }
  { int t = !x; 
# 93 "./lookup/pr21802.C" 3 4
               ((
# 93 "./lookup/pr21802.C"
               t == 7
# 93 "./lookup/pr21802.C" 3 4
               ) ? static_cast<void> (0) : __assert_fail (
# 93 "./lookup/pr21802.C"
               "t == 7"
# 93 "./lookup/pr21802.C" 3 4
               , "./lookup/pr21802.C", 93, __PRETTY_FUNCTION__))
# 93 "./lookup/pr21802.C"
                              ; }
  { int t = ~x; 
# 94 "./lookup/pr21802.C" 3 4
               ((
# 94 "./lookup/pr21802.C"
               t == 7
# 94 "./lookup/pr21802.C" 3 4
               ) ? static_cast<void> (0) : __assert_fail (
# 94 "./lookup/pr21802.C"
               "t == 7"
# 94 "./lookup/pr21802.C" 3 4
               , "./lookup/pr21802.C", 94, __PRETTY_FUNCTION__))
# 94 "./lookup/pr21802.C"
                              ; }
  { int t = x++; 
# 95 "./lookup/pr21802.C" 3 4
                ((
# 95 "./lookup/pr21802.C"
                t == 7
# 95 "./lookup/pr21802.C" 3 4
                ) ? static_cast<void> (0) : __assert_fail (
# 95 "./lookup/pr21802.C"
                "t == 7"
# 95 "./lookup/pr21802.C" 3 4
                , "./lookup/pr21802.C", 95, __PRETTY_FUNCTION__))
# 95 "./lookup/pr21802.C"
                               ; }
  { int t = x--; 
# 96 "./lookup/pr21802.C" 3 4
                ((
# 96 "./lookup/pr21802.C"
                t == 7
# 96 "./lookup/pr21802.C" 3 4
                ) ? static_cast<void> (0) : __assert_fail (
# 96 "./lookup/pr21802.C"
                "t == 7"
# 96 "./lookup/pr21802.C" 3 4
                , "./lookup/pr21802.C", 96, __PRETTY_FUNCTION__))
# 96 "./lookup/pr21802.C"
                               ; }
  { int t = ++x; 
# 97 "./lookup/pr21802.C" 3 4
                ((
# 97 "./lookup/pr21802.C"
                t == 107
# 97 "./lookup/pr21802.C" 3 4
                ) ? static_cast<void> (0) : __assert_fail (
# 97 "./lookup/pr21802.C"
                "t == 107"
# 97 "./lookup/pr21802.C" 3 4
                , "./lookup/pr21802.C", 97, __PRETTY_FUNCTION__))
# 97 "./lookup/pr21802.C"
                                 ; }
  { int t = --x; 
# 98 "./lookup/pr21802.C" 3 4
                ((
# 98 "./lookup/pr21802.C"
                t == 107
# 98 "./lookup/pr21802.C" 3 4
                ) ? static_cast<void> (0) : __assert_fail (
# 98 "./lookup/pr21802.C"
                "t == 107"
# 98 "./lookup/pr21802.C" 3 4
                , "./lookup/pr21802.C", 98, __PRETTY_FUNCTION__))
# 98 "./lookup/pr21802.C"
                                 ; }
  { int t = x (); 
# 99 "./lookup/pr21802.C" 3 4
                 ((
# 99 "./lookup/pr21802.C"
                 t == 7
# 99 "./lookup/pr21802.C" 3 4
                 ) ? static_cast<void> (0) : __assert_fail (
# 99 "./lookup/pr21802.C"
                 "t == 7"
# 99 "./lookup/pr21802.C" 3 4
                 , "./lookup/pr21802.C", 99, __PRETTY_FUNCTION__))
# 99 "./lookup/pr21802.C"
                                ; }
  { int t = (x, I); 
# 100 "./lookup/pr21802.C" 3 4
                   ((
# 100 "./lookup/pr21802.C"
                   t == 7
# 100 "./lookup/pr21802.C" 3 4
                   ) ? static_cast<void> (0) : __assert_fail (
# 100 "./lookup/pr21802.C"
                   "t == 7"
# 100 "./lookup/pr21802.C" 3 4
                   , "./lookup/pr21802.C", 100, __PRETTY_FUNCTION__))
# 100 "./lookup/pr21802.C"
                                  ; }
  { int t = x[I]; 
# 101 "./lookup/pr21802.C" 3 4
                 ((
# 101 "./lookup/pr21802.C"
                 t == 7
# 101 "./lookup/pr21802.C" 3 4
                 ) ? static_cast<void> (0) : __assert_fail (
# 101 "./lookup/pr21802.C"
                 "t == 7"
# 101 "./lookup/pr21802.C" 3 4
                 , "./lookup/pr21802.C", 101, __PRETTY_FUNCTION__))
# 101 "./lookup/pr21802.C"
                                ; }
  { int t = (x-=I); 
# 102 "./lookup/pr21802.C" 3 4
                   ((
# 102 "./lookup/pr21802.C"
                   t == 7
# 102 "./lookup/pr21802.C" 3 4
                   ) ? static_cast<void> (0) : __assert_fail (
# 102 "./lookup/pr21802.C"
                   "t == 7"
# 102 "./lookup/pr21802.C" 3 4
                   , "./lookup/pr21802.C", 102, __PRETTY_FUNCTION__))
# 102 "./lookup/pr21802.C"
                                  ; }
  { int t = (x/=I); 
# 103 "./lookup/pr21802.C" 3 4
                   ((
# 103 "./lookup/pr21802.C"
                   t == 7
# 103 "./lookup/pr21802.C" 3 4
                   ) ? static_cast<void> (0) : __assert_fail (
# 103 "./lookup/pr21802.C"
                   "t == 7"
# 103 "./lookup/pr21802.C" 3 4
                   , "./lookup/pr21802.C", 103, __PRETTY_FUNCTION__))
# 103 "./lookup/pr21802.C"
                                  ; }
  { int t = (x*=I); 
# 104 "./lookup/pr21802.C" 3 4
                   ((
# 104 "./lookup/pr21802.C"
                   t == 7
# 104 "./lookup/pr21802.C" 3 4
                   ) ? static_cast<void> (0) : __assert_fail (
# 104 "./lookup/pr21802.C"
                   "t == 7"
# 104 "./lookup/pr21802.C" 3 4
                   , "./lookup/pr21802.C", 104, __PRETTY_FUNCTION__))
# 104 "./lookup/pr21802.C"
                                  ; }

  { int t = x >> I; 
# 106 "./lookup/pr21802.C" 3 4
                   ((
# 106 "./lookup/pr21802.C"
                   t == 8
# 106 "./lookup/pr21802.C" 3 4
                   ) ? static_cast<void> (0) : __assert_fail (
# 106 "./lookup/pr21802.C"
                   "t == 8"
# 106 "./lookup/pr21802.C" 3 4
                   , "./lookup/pr21802.C", 106, __PRETTY_FUNCTION__))
# 106 "./lookup/pr21802.C"
                                  ; }
  { int t = x & I; 
# 107 "./lookup/pr21802.C" 3 4
                  ((
# 107 "./lookup/pr21802.C"
                  t == 8
# 107 "./lookup/pr21802.C" 3 4
                  ) ? static_cast<void> (0) : __assert_fail (
# 107 "./lookup/pr21802.C"
                  "t == 8"
# 107 "./lookup/pr21802.C" 3 4
                  , "./lookup/pr21802.C", 107, __PRETTY_FUNCTION__))
# 107 "./lookup/pr21802.C"
                                 ; }
  { int t = &x; 
# 108 "./lookup/pr21802.C" 3 4
               ((
# 108 "./lookup/pr21802.C"
               t == 8
# 108 "./lookup/pr21802.C" 3 4
               ) ? static_cast<void> (0) : __assert_fail (
# 108 "./lookup/pr21802.C"
               "t == 8"
# 108 "./lookup/pr21802.C" 3 4
               , "./lookup/pr21802.C", 108, __PRETTY_FUNCTION__))
# 108 "./lookup/pr21802.C"
                              ; }
  { int t = x == I; 
# 109 "./lookup/pr21802.C" 3 4
                   ((
# 109 "./lookup/pr21802.C"
                   t == 8
# 109 "./lookup/pr21802.C" 3 4
                   ) ? static_cast<void> (0) : __assert_fail (
# 109 "./lookup/pr21802.C"
                   "t == 8"
# 109 "./lookup/pr21802.C" 3 4
                   , "./lookup/pr21802.C", 109, __PRETTY_FUNCTION__))
# 109 "./lookup/pr21802.C"
                                  ; }
  { int t = x != I; 
# 110 "./lookup/pr21802.C" 3 4
                   ((
# 110 "./lookup/pr21802.C"
                   t == 8
# 110 "./lookup/pr21802.C" 3 4
                   ) ? static_cast<void> (0) : __assert_fail (
# 110 "./lookup/pr21802.C"
                   "t == 8"
# 110 "./lookup/pr21802.C" 3 4
                   , "./lookup/pr21802.C", 110, __PRETTY_FUNCTION__))
# 110 "./lookup/pr21802.C"
                                  ; }
}

template <typename T>
void
Foo2 (T)
{
  X x;
  { int t = x + I; 
# 118 "./lookup/pr21802.C" 3 4
                  ((
# 118 "./lookup/pr21802.C"
                  t == 6
# 118 "./lookup/pr21802.C" 3 4
                  ) ? static_cast<void> (0) : __assert_fail (
# 118 "./lookup/pr21802.C"
                  "t == 6"
# 118 "./lookup/pr21802.C" 3 4
                  , "./lookup/pr21802.C", 118, __PRETTY_FUNCTION__))
# 118 "./lookup/pr21802.C"
                                 ; }
  { int t = x - I; 
# 119 "./lookup/pr21802.C" 3 4
                  ((
# 119 "./lookup/pr21802.C"
                  t == 6
# 119 "./lookup/pr21802.C" 3 4
                  ) ? static_cast<void> (0) : __assert_fail (
# 119 "./lookup/pr21802.C"
                  "t == 6"
# 119 "./lookup/pr21802.C" 3 4
                  , "./lookup/pr21802.C", 119, __PRETTY_FUNCTION__))
# 119 "./lookup/pr21802.C"
                                 ; }
  { int t = x * I; 
# 120 "./lookup/pr21802.C" 3 4
                  ((
# 120 "./lookup/pr21802.C"
                  t == 6
# 120 "./lookup/pr21802.C" 3 4
                  ) ? static_cast<void> (0) : __assert_fail (
# 120 "./lookup/pr21802.C"
                  "t == 6"
# 120 "./lookup/pr21802.C" 3 4
                  , "./lookup/pr21802.C", 120, __PRETTY_FUNCTION__))
# 120 "./lookup/pr21802.C"
                                 ; }
  { int t = x / I; 
# 121 "./lookup/pr21802.C" 3 4
                  ((
# 121 "./lookup/pr21802.C"
                  t == 6
# 121 "./lookup/pr21802.C" 3 4
                  ) ? static_cast<void> (0) : __assert_fail (
# 121 "./lookup/pr21802.C"
                  "t == 6"
# 121 "./lookup/pr21802.C" 3 4
                  , "./lookup/pr21802.C", 121, __PRETTY_FUNCTION__))
# 121 "./lookup/pr21802.C"
                                 ; }
  { int t = (x+=I); 
# 122 "./lookup/pr21802.C" 3 4
                   ((
# 122 "./lookup/pr21802.C"
                   t == 6
# 122 "./lookup/pr21802.C" 3 4
                   ) ? static_cast<void> (0) : __assert_fail (
# 122 "./lookup/pr21802.C"
                   "t == 6"
# 122 "./lookup/pr21802.C" 3 4
                   , "./lookup/pr21802.C", 122, __PRETTY_FUNCTION__))
# 122 "./lookup/pr21802.C"
                                  ; }

  { int t = x % I; 
# 124 "./lookup/pr21802.C" 3 4
                  ((
# 124 "./lookup/pr21802.C"
                  t == 7
# 124 "./lookup/pr21802.C" 3 4
                  ) ? static_cast<void> (0) : __assert_fail (
# 124 "./lookup/pr21802.C"
                  "t == 7"
# 124 "./lookup/pr21802.C" 3 4
                  , "./lookup/pr21802.C", 124, __PRETTY_FUNCTION__))
# 124 "./lookup/pr21802.C"
                                 ; }
  { int t = x >> I; 
# 125 "./lookup/pr21802.C" 3 4
                   ((
# 125 "./lookup/pr21802.C"
                   t == 7
# 125 "./lookup/pr21802.C" 3 4
                   ) ? static_cast<void> (0) : __assert_fail (
# 125 "./lookup/pr21802.C"
                   "t == 7"
# 125 "./lookup/pr21802.C" 3 4
                   , "./lookup/pr21802.C", 125, __PRETTY_FUNCTION__))
# 125 "./lookup/pr21802.C"
                                  ; }
  { int t = x << I; 
# 126 "./lookup/pr21802.C" 3 4
                   ((
# 126 "./lookup/pr21802.C"
                   t == 7
# 126 "./lookup/pr21802.C" 3 4
                   ) ? static_cast<void> (0) : __assert_fail (
# 126 "./lookup/pr21802.C"
                   "t == 7"
# 126 "./lookup/pr21802.C" 3 4
                   , "./lookup/pr21802.C", 126, __PRETTY_FUNCTION__))
# 126 "./lookup/pr21802.C"
                                  ; }
  { int t = x | I; 
# 127 "./lookup/pr21802.C" 3 4
                  ((
# 127 "./lookup/pr21802.C"
                  t == 7
# 127 "./lookup/pr21802.C" 3 4
                  ) ? static_cast<void> (0) : __assert_fail (
# 127 "./lookup/pr21802.C"
                  "t == 7"
# 127 "./lookup/pr21802.C" 3 4
                  , "./lookup/pr21802.C", 127, __PRETTY_FUNCTION__))
# 127 "./lookup/pr21802.C"
                                 ; }
  { int t = x && I; 
# 128 "./lookup/pr21802.C" 3 4
                   ((
# 128 "./lookup/pr21802.C"
                   t == 7
# 128 "./lookup/pr21802.C" 3 4
                   ) ? static_cast<void> (0) : __assert_fail (
# 128 "./lookup/pr21802.C"
                   "t == 7"
# 128 "./lookup/pr21802.C" 3 4
                   , "./lookup/pr21802.C", 128, __PRETTY_FUNCTION__))
# 128 "./lookup/pr21802.C"
                                  ; }
  { int t = x || I; 
# 129 "./lookup/pr21802.C" 3 4
                   ((
# 129 "./lookup/pr21802.C"
                   t == 7
# 129 "./lookup/pr21802.C" 3 4
                   ) ? static_cast<void> (0) : __assert_fail (
# 129 "./lookup/pr21802.C"
                   "t == 7"
# 129 "./lookup/pr21802.C" 3 4
                   , "./lookup/pr21802.C", 129, __PRETTY_FUNCTION__))
# 129 "./lookup/pr21802.C"
                                  ; }
  { int t = x == I; 
# 130 "./lookup/pr21802.C" 3 4
                   ((
# 130 "./lookup/pr21802.C"
                   t == 7
# 130 "./lookup/pr21802.C" 3 4
                   ) ? static_cast<void> (0) : __assert_fail (
# 130 "./lookup/pr21802.C"
                   "t == 7"
# 130 "./lookup/pr21802.C" 3 4
                   , "./lookup/pr21802.C", 130, __PRETTY_FUNCTION__))
# 130 "./lookup/pr21802.C"
                                  ; }
  { int t = x != I; 
# 131 "./lookup/pr21802.C" 3 4
                   ((
# 131 "./lookup/pr21802.C"
                   t == 7
# 131 "./lookup/pr21802.C" 3 4
                   ) ? static_cast<void> (0) : __assert_fail (
# 131 "./lookup/pr21802.C"
                   "t == 7"
# 131 "./lookup/pr21802.C" 3 4
                   , "./lookup/pr21802.C", 131, __PRETTY_FUNCTION__))
# 131 "./lookup/pr21802.C"
                                  ; }
  { int t = x < I; 
# 132 "./lookup/pr21802.C" 3 4
                  ((
# 132 "./lookup/pr21802.C"
                  t == 7
# 132 "./lookup/pr21802.C" 3 4
                  ) ? static_cast<void> (0) : __assert_fail (
# 132 "./lookup/pr21802.C"
                  "t == 7"
# 132 "./lookup/pr21802.C" 3 4
                  , "./lookup/pr21802.C", 132, __PRETTY_FUNCTION__))
# 132 "./lookup/pr21802.C"
                                 ; }
  { int t = x <= I; 
# 133 "./lookup/pr21802.C" 3 4
                   ((
# 133 "./lookup/pr21802.C"
                   t == 7
# 133 "./lookup/pr21802.C" 3 4
                   ) ? static_cast<void> (0) : __assert_fail (
# 133 "./lookup/pr21802.C"
                   "t == 7"
# 133 "./lookup/pr21802.C" 3 4
                   , "./lookup/pr21802.C", 133, __PRETTY_FUNCTION__))
# 133 "./lookup/pr21802.C"
                                  ; }
  { int t = x > I; 
# 134 "./lookup/pr21802.C" 3 4
                  ((
# 134 "./lookup/pr21802.C"
                  t == 7
# 134 "./lookup/pr21802.C" 3 4
                  ) ? static_cast<void> (0) : __assert_fail (
# 134 "./lookup/pr21802.C"
                  "t == 7"
# 134 "./lookup/pr21802.C" 3 4
                  , "./lookup/pr21802.C", 134, __PRETTY_FUNCTION__))
# 134 "./lookup/pr21802.C"
                                 ; }
  { int t = x >= I; 
# 135 "./lookup/pr21802.C" 3 4
                   ((
# 135 "./lookup/pr21802.C"
                   t == 7
# 135 "./lookup/pr21802.C" 3 4
                   ) ? static_cast<void> (0) : __assert_fail (
# 135 "./lookup/pr21802.C"
                   "t == 7"
# 135 "./lookup/pr21802.C" 3 4
                   , "./lookup/pr21802.C", 135, __PRETTY_FUNCTION__))
# 135 "./lookup/pr21802.C"
                                  ; }
  { int t = *x; 
# 136 "./lookup/pr21802.C" 3 4
               ((
# 136 "./lookup/pr21802.C"
               t == 7
# 136 "./lookup/pr21802.C" 3 4
               ) ? static_cast<void> (0) : __assert_fail (
# 136 "./lookup/pr21802.C"
               "t == 7"
# 136 "./lookup/pr21802.C" 3 4
               , "./lookup/pr21802.C", 136, __PRETTY_FUNCTION__))
# 136 "./lookup/pr21802.C"
                              ; }
  { int t = !x; 
# 137 "./lookup/pr21802.C" 3 4
               ((
# 137 "./lookup/pr21802.C"
               t == 7
# 137 "./lookup/pr21802.C" 3 4
               ) ? static_cast<void> (0) : __assert_fail (
# 137 "./lookup/pr21802.C"
               "t == 7"
# 137 "./lookup/pr21802.C" 3 4
               , "./lookup/pr21802.C", 137, __PRETTY_FUNCTION__))
# 137 "./lookup/pr21802.C"
                              ; }
  { int t = ~x; 
# 138 "./lookup/pr21802.C" 3 4
               ((
# 138 "./lookup/pr21802.C"
               t == 7
# 138 "./lookup/pr21802.C" 3 4
               ) ? static_cast<void> (0) : __assert_fail (
# 138 "./lookup/pr21802.C"
               "t == 7"
# 138 "./lookup/pr21802.C" 3 4
               , "./lookup/pr21802.C", 138, __PRETTY_FUNCTION__))
# 138 "./lookup/pr21802.C"
                              ; }
  { int t = x++; 
# 139 "./lookup/pr21802.C" 3 4
                ((
# 139 "./lookup/pr21802.C"
                t == 7
# 139 "./lookup/pr21802.C" 3 4
                ) ? static_cast<void> (0) : __assert_fail (
# 139 "./lookup/pr21802.C"
                "t == 7"
# 139 "./lookup/pr21802.C" 3 4
                , "./lookup/pr21802.C", 139, __PRETTY_FUNCTION__))
# 139 "./lookup/pr21802.C"
                               ; }
  { int t = x--; 
# 140 "./lookup/pr21802.C" 3 4
                ((
# 140 "./lookup/pr21802.C"
                t == 7
# 140 "./lookup/pr21802.C" 3 4
                ) ? static_cast<void> (0) : __assert_fail (
# 140 "./lookup/pr21802.C"
                "t == 7"
# 140 "./lookup/pr21802.C" 3 4
                , "./lookup/pr21802.C", 140, __PRETTY_FUNCTION__))
# 140 "./lookup/pr21802.C"
                               ; }
  { int t = ++x; 
# 141 "./lookup/pr21802.C" 3 4
                ((
# 141 "./lookup/pr21802.C"
                t == 107
# 141 "./lookup/pr21802.C" 3 4
                ) ? static_cast<void> (0) : __assert_fail (
# 141 "./lookup/pr21802.C"
                "t == 107"
# 141 "./lookup/pr21802.C" 3 4
                , "./lookup/pr21802.C", 141, __PRETTY_FUNCTION__))
# 141 "./lookup/pr21802.C"
                                 ; }
  { int t = --x; 
# 142 "./lookup/pr21802.C" 3 4
                ((
# 142 "./lookup/pr21802.C"
                t == 107
# 142 "./lookup/pr21802.C" 3 4
                ) ? static_cast<void> (0) : __assert_fail (
# 142 "./lookup/pr21802.C"
                "t == 107"
# 142 "./lookup/pr21802.C" 3 4
                , "./lookup/pr21802.C", 142, __PRETTY_FUNCTION__))
# 142 "./lookup/pr21802.C"
                                 ; }
  { int t = x (); 
# 143 "./lookup/pr21802.C" 3 4
                 ((
# 143 "./lookup/pr21802.C"
                 t == 7
# 143 "./lookup/pr21802.C" 3 4
                 ) ? static_cast<void> (0) : __assert_fail (
# 143 "./lookup/pr21802.C"
                 "t == 7"
# 143 "./lookup/pr21802.C" 3 4
                 , "./lookup/pr21802.C", 143, __PRETTY_FUNCTION__))
# 143 "./lookup/pr21802.C"
                                ; }
  { int t = (x, I); 
# 144 "./lookup/pr21802.C" 3 4
                   ((
# 144 "./lookup/pr21802.C"
                   t == 7
# 144 "./lookup/pr21802.C" 3 4
                   ) ? static_cast<void> (0) : __assert_fail (
# 144 "./lookup/pr21802.C"
                   "t == 7"
# 144 "./lookup/pr21802.C" 3 4
                   , "./lookup/pr21802.C", 144, __PRETTY_FUNCTION__))
# 144 "./lookup/pr21802.C"
                                  ; }
  { int t = x[I]; 
# 145 "./lookup/pr21802.C" 3 4
                 ((
# 145 "./lookup/pr21802.C"
                 t == 7
# 145 "./lookup/pr21802.C" 3 4
                 ) ? static_cast<void> (0) : __assert_fail (
# 145 "./lookup/pr21802.C"
                 "t == 7"
# 145 "./lookup/pr21802.C" 3 4
                 , "./lookup/pr21802.C", 145, __PRETTY_FUNCTION__))
# 145 "./lookup/pr21802.C"
                                ; }
  { int t = &x; 
# 146 "./lookup/pr21802.C" 3 4
               ((
# 146 "./lookup/pr21802.C"
               t == 7
# 146 "./lookup/pr21802.C" 3 4
               ) ? static_cast<void> (0) : __assert_fail (
# 146 "./lookup/pr21802.C"
               "t == 7"
# 146 "./lookup/pr21802.C" 3 4
               , "./lookup/pr21802.C", 146, __PRETTY_FUNCTION__))
# 146 "./lookup/pr21802.C"
                              ; }
  { int t = (x-=I); 
# 147 "./lookup/pr21802.C" 3 4
                   ((
# 147 "./lookup/pr21802.C"
                   t == 7
# 147 "./lookup/pr21802.C" 3 4
                   ) ? static_cast<void> (0) : __assert_fail (
# 147 "./lookup/pr21802.C"
                   "t == 7"
# 147 "./lookup/pr21802.C" 3 4
                   , "./lookup/pr21802.C", 147, __PRETTY_FUNCTION__))
# 147 "./lookup/pr21802.C"
                                  ; }
  { int t = (x/=I); 
# 148 "./lookup/pr21802.C" 3 4
                   ((
# 148 "./lookup/pr21802.C"
                   t == 7
# 148 "./lookup/pr21802.C" 3 4
                   ) ? static_cast<void> (0) : __assert_fail (
# 148 "./lookup/pr21802.C"
                   "t == 7"
# 148 "./lookup/pr21802.C" 3 4
                   , "./lookup/pr21802.C", 148, __PRETTY_FUNCTION__))
# 148 "./lookup/pr21802.C"
                                  ; }
  { int t = (x*=I); 
# 149 "./lookup/pr21802.C" 3 4
                   ((
# 149 "./lookup/pr21802.C"
                   t == 7
# 149 "./lookup/pr21802.C" 3 4
                   ) ? static_cast<void> (0) : __assert_fail (
# 149 "./lookup/pr21802.C"
                   "t == 7"
# 149 "./lookup/pr21802.C" 3 4
                   , "./lookup/pr21802.C", 149, __PRETTY_FUNCTION__))
# 149 "./lookup/pr21802.C"
                                  ; }
  { int t = x & I; 
# 150 "./lookup/pr21802.C" 3 4
                  ((
# 150 "./lookup/pr21802.C"
                  t == 7
# 150 "./lookup/pr21802.C" 3 4
                  ) ? static_cast<void> (0) : __assert_fail (
# 150 "./lookup/pr21802.C"
                  "t == 7"
# 150 "./lookup/pr21802.C" 3 4
                  , "./lookup/pr21802.C", 150, __PRETTY_FUNCTION__))
# 150 "./lookup/pr21802.C"
                                 ; }
}

template <typename T>
void
Foo3 (T)
{
  Y o;
  X &x = o;
  { int t = x + I; 
# 159 "./lookup/pr21802.C" 3 4
                  ((
# 159 "./lookup/pr21802.C"
                  t == 6
# 159 "./lookup/pr21802.C" 3 4
                  ) ? static_cast<void> (0) : __assert_fail (
# 159 "./lookup/pr21802.C"
                  "t == 6"
# 159 "./lookup/pr21802.C" 3 4
                  , "./lookup/pr21802.C", 159, __PRETTY_FUNCTION__))
# 159 "./lookup/pr21802.C"
                                 ; }
  { int t = x - I; 
# 160 "./lookup/pr21802.C" 3 4
                  ((
# 160 "./lookup/pr21802.C"
                  t == 6
# 160 "./lookup/pr21802.C" 3 4
                  ) ? static_cast<void> (0) : __assert_fail (
# 160 "./lookup/pr21802.C"
                  "t == 6"
# 160 "./lookup/pr21802.C" 3 4
                  , "./lookup/pr21802.C", 160, __PRETTY_FUNCTION__))
# 160 "./lookup/pr21802.C"
                                 ; }
  { int t = x * I; 
# 161 "./lookup/pr21802.C" 3 4
                  ((
# 161 "./lookup/pr21802.C"
                  t == 6
# 161 "./lookup/pr21802.C" 3 4
                  ) ? static_cast<void> (0) : __assert_fail (
# 161 "./lookup/pr21802.C"
                  "t == 6"
# 161 "./lookup/pr21802.C" 3 4
                  , "./lookup/pr21802.C", 161, __PRETTY_FUNCTION__))
# 161 "./lookup/pr21802.C"
                                 ; }
  { int t = x / I; 
# 162 "./lookup/pr21802.C" 3 4
                  ((
# 162 "./lookup/pr21802.C"
                  t == 6
# 162 "./lookup/pr21802.C" 3 4
                  ) ? static_cast<void> (0) : __assert_fail (
# 162 "./lookup/pr21802.C"
                  "t == 6"
# 162 "./lookup/pr21802.C" 3 4
                  , "./lookup/pr21802.C", 162, __PRETTY_FUNCTION__))
# 162 "./lookup/pr21802.C"
                                 ; }
  { int t = (x+=I); 
# 163 "./lookup/pr21802.C" 3 4
                   ((
# 163 "./lookup/pr21802.C"
                   t == 6
# 163 "./lookup/pr21802.C" 3 4
                   ) ? static_cast<void> (0) : __assert_fail (
# 163 "./lookup/pr21802.C"
                   "t == 6"
# 163 "./lookup/pr21802.C" 3 4
                   , "./lookup/pr21802.C", 163, __PRETTY_FUNCTION__))
# 163 "./lookup/pr21802.C"
                                  ; }

  { int t = x % I; 
# 165 "./lookup/pr21802.C" 3 4
                  ((
# 165 "./lookup/pr21802.C"
                  t == 7
# 165 "./lookup/pr21802.C" 3 4
                  ) ? static_cast<void> (0) : __assert_fail (
# 165 "./lookup/pr21802.C"
                  "t == 7"
# 165 "./lookup/pr21802.C" 3 4
                  , "./lookup/pr21802.C", 165, __PRETTY_FUNCTION__))
# 165 "./lookup/pr21802.C"
                                 ; }
  { int t = x << I; 
# 166 "./lookup/pr21802.C" 3 4
                   ((
# 166 "./lookup/pr21802.C"
                   t == 7
# 166 "./lookup/pr21802.C" 3 4
                   ) ? static_cast<void> (0) : __assert_fail (
# 166 "./lookup/pr21802.C"
                   "t == 7"
# 166 "./lookup/pr21802.C" 3 4
                   , "./lookup/pr21802.C", 166, __PRETTY_FUNCTION__))
# 166 "./lookup/pr21802.C"
                                  ; }
  { int t = x | I; 
# 167 "./lookup/pr21802.C" 3 4
                  ((
# 167 "./lookup/pr21802.C"
                  t == 7
# 167 "./lookup/pr21802.C" 3 4
                  ) ? static_cast<void> (0) : __assert_fail (
# 167 "./lookup/pr21802.C"
                  "t == 7"
# 167 "./lookup/pr21802.C" 3 4
                  , "./lookup/pr21802.C", 167, __PRETTY_FUNCTION__))
# 167 "./lookup/pr21802.C"
                                 ; }
  { int t = x && I; 
# 168 "./lookup/pr21802.C" 3 4
                   ((
# 168 "./lookup/pr21802.C"
                   t == 7
# 168 "./lookup/pr21802.C" 3 4
                   ) ? static_cast<void> (0) : __assert_fail (
# 168 "./lookup/pr21802.C"
                   "t == 7"
# 168 "./lookup/pr21802.C" 3 4
                   , "./lookup/pr21802.C", 168, __PRETTY_FUNCTION__))
# 168 "./lookup/pr21802.C"
                                  ; }
  { int t = x || I; 
# 169 "./lookup/pr21802.C" 3 4
                   ((
# 169 "./lookup/pr21802.C"
                   t == 7
# 169 "./lookup/pr21802.C" 3 4
                   ) ? static_cast<void> (0) : __assert_fail (
# 169 "./lookup/pr21802.C"
                   "t == 7"
# 169 "./lookup/pr21802.C" 3 4
                   , "./lookup/pr21802.C", 169, __PRETTY_FUNCTION__))
# 169 "./lookup/pr21802.C"
                                  ; }
  { int t = x == I; 
# 170 "./lookup/pr21802.C" 3 4
                   ((
# 170 "./lookup/pr21802.C"
                   t == 7
# 170 "./lookup/pr21802.C" 3 4
                   ) ? static_cast<void> (0) : __assert_fail (
# 170 "./lookup/pr21802.C"
                   "t == 7"
# 170 "./lookup/pr21802.C" 3 4
                   , "./lookup/pr21802.C", 170, __PRETTY_FUNCTION__))
# 170 "./lookup/pr21802.C"
                                  ; }
  { int t = x != I; 
# 171 "./lookup/pr21802.C" 3 4
                   ((
# 171 "./lookup/pr21802.C"
                   t == 7
# 171 "./lookup/pr21802.C" 3 4
                   ) ? static_cast<void> (0) : __assert_fail (
# 171 "./lookup/pr21802.C"
                   "t == 7"
# 171 "./lookup/pr21802.C" 3 4
                   , "./lookup/pr21802.C", 171, __PRETTY_FUNCTION__))
# 171 "./lookup/pr21802.C"
                                  ; }
  { int t = x < I; 
# 172 "./lookup/pr21802.C" 3 4
                  ((
# 172 "./lookup/pr21802.C"
                  t == 7
# 172 "./lookup/pr21802.C" 3 4
                  ) ? static_cast<void> (0) : __assert_fail (
# 172 "./lookup/pr21802.C"
                  "t == 7"
# 172 "./lookup/pr21802.C" 3 4
                  , "./lookup/pr21802.C", 172, __PRETTY_FUNCTION__))
# 172 "./lookup/pr21802.C"
                                 ; }
  { int t = x <= I; 
# 173 "./lookup/pr21802.C" 3 4
                   ((
# 173 "./lookup/pr21802.C"
                   t == 7
# 173 "./lookup/pr21802.C" 3 4
                   ) ? static_cast<void> (0) : __assert_fail (
# 173 "./lookup/pr21802.C"
                   "t == 7"
# 173 "./lookup/pr21802.C" 3 4
                   , "./lookup/pr21802.C", 173, __PRETTY_FUNCTION__))
# 173 "./lookup/pr21802.C"
                                  ; }
  { int t = x > I; 
# 174 "./lookup/pr21802.C" 3 4
                  ((
# 174 "./lookup/pr21802.C"
                  t == 7
# 174 "./lookup/pr21802.C" 3 4
                  ) ? static_cast<void> (0) : __assert_fail (
# 174 "./lookup/pr21802.C"
                  "t == 7"
# 174 "./lookup/pr21802.C" 3 4
                  , "./lookup/pr21802.C", 174, __PRETTY_FUNCTION__))
# 174 "./lookup/pr21802.C"
                                 ; }
  { int t = x >= I; 
# 175 "./lookup/pr21802.C" 3 4
                   ((
# 175 "./lookup/pr21802.C"
                   t == 7
# 175 "./lookup/pr21802.C" 3 4
                   ) ? static_cast<void> (0) : __assert_fail (
# 175 "./lookup/pr21802.C"
                   "t == 7"
# 175 "./lookup/pr21802.C" 3 4
                   , "./lookup/pr21802.C", 175, __PRETTY_FUNCTION__))
# 175 "./lookup/pr21802.C"
                                  ; }
  { int t = *x; 
# 176 "./lookup/pr21802.C" 3 4
               ((
# 176 "./lookup/pr21802.C"
               t == 7
# 176 "./lookup/pr21802.C" 3 4
               ) ? static_cast<void> (0) : __assert_fail (
# 176 "./lookup/pr21802.C"
               "t == 7"
# 176 "./lookup/pr21802.C" 3 4
               , "./lookup/pr21802.C", 176, __PRETTY_FUNCTION__))
# 176 "./lookup/pr21802.C"
                              ; }
  { int t = !x; 
# 177 "./lookup/pr21802.C" 3 4
               ((
# 177 "./lookup/pr21802.C"
               t == 7
# 177 "./lookup/pr21802.C" 3 4
               ) ? static_cast<void> (0) : __assert_fail (
# 177 "./lookup/pr21802.C"
               "t == 7"
# 177 "./lookup/pr21802.C" 3 4
               , "./lookup/pr21802.C", 177, __PRETTY_FUNCTION__))
# 177 "./lookup/pr21802.C"
                              ; }
  { int t = ~x; 
# 178 "./lookup/pr21802.C" 3 4
               ((
# 178 "./lookup/pr21802.C"
               t == 7
# 178 "./lookup/pr21802.C" 3 4
               ) ? static_cast<void> (0) : __assert_fail (
# 178 "./lookup/pr21802.C"
               "t == 7"
# 178 "./lookup/pr21802.C" 3 4
               , "./lookup/pr21802.C", 178, __PRETTY_FUNCTION__))
# 178 "./lookup/pr21802.C"
                              ; }
  { int t = x++; 
# 179 "./lookup/pr21802.C" 3 4
                ((
# 179 "./lookup/pr21802.C"
                t == 7
# 179 "./lookup/pr21802.C" 3 4
                ) ? static_cast<void> (0) : __assert_fail (
# 179 "./lookup/pr21802.C"
                "t == 7"
# 179 "./lookup/pr21802.C" 3 4
                , "./lookup/pr21802.C", 179, __PRETTY_FUNCTION__))
# 179 "./lookup/pr21802.C"
                               ; }
  { int t = x--; 
# 180 "./lookup/pr21802.C" 3 4
                ((
# 180 "./lookup/pr21802.C"
                t == 7
# 180 "./lookup/pr21802.C" 3 4
                ) ? static_cast<void> (0) : __assert_fail (
# 180 "./lookup/pr21802.C"
                "t == 7"
# 180 "./lookup/pr21802.C" 3 4
                , "./lookup/pr21802.C", 180, __PRETTY_FUNCTION__))
# 180 "./lookup/pr21802.C"
                               ; }
  { int t = ++x; 
# 181 "./lookup/pr21802.C" 3 4
                ((
# 181 "./lookup/pr21802.C"
                t == 107
# 181 "./lookup/pr21802.C" 3 4
                ) ? static_cast<void> (0) : __assert_fail (
# 181 "./lookup/pr21802.C"
                "t == 107"
# 181 "./lookup/pr21802.C" 3 4
                , "./lookup/pr21802.C", 181, __PRETTY_FUNCTION__))
# 181 "./lookup/pr21802.C"
                                 ; }
  { int t = --x; 
# 182 "./lookup/pr21802.C" 3 4
                ((
# 182 "./lookup/pr21802.C"
                t == 107
# 182 "./lookup/pr21802.C" 3 4
                ) ? static_cast<void> (0) : __assert_fail (
# 182 "./lookup/pr21802.C"
                "t == 107"
# 182 "./lookup/pr21802.C" 3 4
                , "./lookup/pr21802.C", 182, __PRETTY_FUNCTION__))
# 182 "./lookup/pr21802.C"
                                 ; }
  { int t = x (); 
# 183 "./lookup/pr21802.C" 3 4
                 ((
# 183 "./lookup/pr21802.C"
                 t == 7
# 183 "./lookup/pr21802.C" 3 4
                 ) ? static_cast<void> (0) : __assert_fail (
# 183 "./lookup/pr21802.C"
                 "t == 7"
# 183 "./lookup/pr21802.C" 3 4
                 , "./lookup/pr21802.C", 183, __PRETTY_FUNCTION__))
# 183 "./lookup/pr21802.C"
                                ; }
  { int t = (x, I); 
# 184 "./lookup/pr21802.C" 3 4
                   ((
# 184 "./lookup/pr21802.C"
                   t == 7
# 184 "./lookup/pr21802.C" 3 4
                   ) ? static_cast<void> (0) : __assert_fail (
# 184 "./lookup/pr21802.C"
                   "t == 7"
# 184 "./lookup/pr21802.C" 3 4
                   , "./lookup/pr21802.C", 184, __PRETTY_FUNCTION__))
# 184 "./lookup/pr21802.C"
                                  ; }
  { int t = x[I]; 
# 185 "./lookup/pr21802.C" 3 4
                 ((
# 185 "./lookup/pr21802.C"
                 t == 7
# 185 "./lookup/pr21802.C" 3 4
                 ) ? static_cast<void> (0) : __assert_fail (
# 185 "./lookup/pr21802.C"
                 "t == 7"
# 185 "./lookup/pr21802.C" 3 4
                 , "./lookup/pr21802.C", 185, __PRETTY_FUNCTION__))
# 185 "./lookup/pr21802.C"
                                ; }
  { int t = (x-=I); 
# 186 "./lookup/pr21802.C" 3 4
                   ((
# 186 "./lookup/pr21802.C"
                   t == 7
# 186 "./lookup/pr21802.C" 3 4
                   ) ? static_cast<void> (0) : __assert_fail (
# 186 "./lookup/pr21802.C"
                   "t == 7"
# 186 "./lookup/pr21802.C" 3 4
                   , "./lookup/pr21802.C", 186, __PRETTY_FUNCTION__))
# 186 "./lookup/pr21802.C"
                                  ; }
  { int t = (x/=I); 
# 187 "./lookup/pr21802.C" 3 4
                   ((
# 187 "./lookup/pr21802.C"
                   t == 7
# 187 "./lookup/pr21802.C" 3 4
                   ) ? static_cast<void> (0) : __assert_fail (
# 187 "./lookup/pr21802.C"
                   "t == 7"
# 187 "./lookup/pr21802.C" 3 4
                   , "./lookup/pr21802.C", 187, __PRETTY_FUNCTION__))
# 187 "./lookup/pr21802.C"
                                  ; }
  { int t = (x*=I); 
# 188 "./lookup/pr21802.C" 3 4
                   ((
# 188 "./lookup/pr21802.C"
                   t == 7
# 188 "./lookup/pr21802.C" 3 4
                   ) ? static_cast<void> (0) : __assert_fail (
# 188 "./lookup/pr21802.C"
                   "t == 7"
# 188 "./lookup/pr21802.C" 3 4
                   , "./lookup/pr21802.C", 188, __PRETTY_FUNCTION__))
# 188 "./lookup/pr21802.C"
                                  ; }

  { int t = x & I; 
# 190 "./lookup/pr21802.C" 3 4
                  ((
# 190 "./lookup/pr21802.C"
                  t == 7
# 190 "./lookup/pr21802.C" 3 4
                  ) ? static_cast<void> (0) : __assert_fail (
# 190 "./lookup/pr21802.C"
                  "t == 7"
# 190 "./lookup/pr21802.C" 3 4
                  , "./lookup/pr21802.C", 190, __PRETTY_FUNCTION__))
# 190 "./lookup/pr21802.C"
                                 ; }
  { int t = x >> I; 
# 191 "./lookup/pr21802.C" 3 4
                   ((
# 191 "./lookup/pr21802.C"
                   t == 8
# 191 "./lookup/pr21802.C" 3 4
                   ) ? static_cast<void> (0) : __assert_fail (
# 191 "./lookup/pr21802.C"
                   "t == 8"
# 191 "./lookup/pr21802.C" 3 4
                   , "./lookup/pr21802.C", 191, __PRETTY_FUNCTION__))
# 191 "./lookup/pr21802.C"
                                  ; }
  { int t = &x; 
# 192 "./lookup/pr21802.C" 3 4
               ((
# 192 "./lookup/pr21802.C"
               t == 8
# 192 "./lookup/pr21802.C" 3 4
               ) ? static_cast<void> (0) : __assert_fail (
# 192 "./lookup/pr21802.C"
               "t == 8"
# 192 "./lookup/pr21802.C" 3 4
               , "./lookup/pr21802.C", 192, __PRETTY_FUNCTION__))
# 192 "./lookup/pr21802.C"
                              ; }
}

template <typename T>
void
Foo4 (T)
{
  Y x;
  { int t = operator+ (x, I); 
# 200 "./lookup/pr21802.C" 3 4
                             ((
# 200 "./lookup/pr21802.C"
                             t == 6
# 200 "./lookup/pr21802.C" 3 4
                             ) ? static_cast<void> (0) : __assert_fail (
# 200 "./lookup/pr21802.C"
                             "t == 6"
# 200 "./lookup/pr21802.C" 3 4
                             , "./lookup/pr21802.C", 200, __PRETTY_FUNCTION__))
# 200 "./lookup/pr21802.C"
                                            ; }
  { int t = operator- (x, I); 
# 201 "./lookup/pr21802.C" 3 4
                             ((
# 201 "./lookup/pr21802.C"
                             t == 6
# 201 "./lookup/pr21802.C" 3 4
                             ) ? static_cast<void> (0) : __assert_fail (
# 201 "./lookup/pr21802.C"
                             "t == 6"
# 201 "./lookup/pr21802.C" 3 4
                             , "./lookup/pr21802.C", 201, __PRETTY_FUNCTION__))
# 201 "./lookup/pr21802.C"
                                            ; }
  { int t = operator* (x, I); 
# 202 "./lookup/pr21802.C" 3 4
                             ((
# 202 "./lookup/pr21802.C"
                             t == 6
# 202 "./lookup/pr21802.C" 3 4
                             ) ? static_cast<void> (0) : __assert_fail (
# 202 "./lookup/pr21802.C"
                             "t == 6"
# 202 "./lookup/pr21802.C" 3 4
                             , "./lookup/pr21802.C", 202, __PRETTY_FUNCTION__))
# 202 "./lookup/pr21802.C"
                                            ; }
  { int t = operator/ (x, I); 
# 203 "./lookup/pr21802.C" 3 4
                             ((
# 203 "./lookup/pr21802.C"
                             t == 6
# 203 "./lookup/pr21802.C" 3 4
                             ) ? static_cast<void> (0) : __assert_fail (
# 203 "./lookup/pr21802.C"
                             "t == 6"
# 203 "./lookup/pr21802.C" 3 4
                             , "./lookup/pr21802.C", 203, __PRETTY_FUNCTION__))
# 203 "./lookup/pr21802.C"
                                            ; }
  { int t = operator+= (x, I); 
# 204 "./lookup/pr21802.C" 3 4
                              ((
# 204 "./lookup/pr21802.C"
                              t == 6
# 204 "./lookup/pr21802.C" 3 4
                              ) ? static_cast<void> (0) : __assert_fail (
# 204 "./lookup/pr21802.C"
                              "t == 6"
# 204 "./lookup/pr21802.C" 3 4
                              , "./lookup/pr21802.C", 204, __PRETTY_FUNCTION__))
# 204 "./lookup/pr21802.C"
                                             ; }

  { int t = x.operator% (I); 
# 206 "./lookup/pr21802.C" 3 4
                            ((
# 206 "./lookup/pr21802.C"
                            t == 7
# 206 "./lookup/pr21802.C" 3 4
                            ) ? static_cast<void> (0) : __assert_fail (
# 206 "./lookup/pr21802.C"
                            "t == 7"
# 206 "./lookup/pr21802.C" 3 4
                            , "./lookup/pr21802.C", 206, __PRETTY_FUNCTION__))
# 206 "./lookup/pr21802.C"
                                           ; }
  { int t = x.operator<< (I); 
# 207 "./lookup/pr21802.C" 3 4
                             ((
# 207 "./lookup/pr21802.C"
                             t == 7
# 207 "./lookup/pr21802.C" 3 4
                             ) ? static_cast<void> (0) : __assert_fail (
# 207 "./lookup/pr21802.C"
                             "t == 7"
# 207 "./lookup/pr21802.C" 3 4
                             , "./lookup/pr21802.C", 207, __PRETTY_FUNCTION__))
# 207 "./lookup/pr21802.C"
                                            ; }
  { int t = x.operator| (I); 
# 208 "./lookup/pr21802.C" 3 4
                            ((
# 208 "./lookup/pr21802.C"
                            t == 7
# 208 "./lookup/pr21802.C" 3 4
                            ) ? static_cast<void> (0) : __assert_fail (
# 208 "./lookup/pr21802.C"
                            "t == 7"
# 208 "./lookup/pr21802.C" 3 4
                            , "./lookup/pr21802.C", 208, __PRETTY_FUNCTION__))
# 208 "./lookup/pr21802.C"
                                           ; }
  { int t = x.operator&& (I); 
# 209 "./lookup/pr21802.C" 3 4
                             ((
# 209 "./lookup/pr21802.C"
                             t == 7
# 209 "./lookup/pr21802.C" 3 4
                             ) ? static_cast<void> (0) : __assert_fail (
# 209 "./lookup/pr21802.C"
                             "t == 7"
# 209 "./lookup/pr21802.C" 3 4
                             , "./lookup/pr21802.C", 209, __PRETTY_FUNCTION__))
# 209 "./lookup/pr21802.C"
                                            ; }
  { int t = x.operator|| (I); 
# 210 "./lookup/pr21802.C" 3 4
                             ((
# 210 "./lookup/pr21802.C"
                             t == 7
# 210 "./lookup/pr21802.C" 3 4
                             ) ? static_cast<void> (0) : __assert_fail (
# 210 "./lookup/pr21802.C"
                             "t == 7"
# 210 "./lookup/pr21802.C" 3 4
                             , "./lookup/pr21802.C", 210, __PRETTY_FUNCTION__))
# 210 "./lookup/pr21802.C"
                                            ; }
  { int t = x.operator< (I); 
# 211 "./lookup/pr21802.C" 3 4
                            ((
# 211 "./lookup/pr21802.C"
                            t == 7
# 211 "./lookup/pr21802.C" 3 4
                            ) ? static_cast<void> (0) : __assert_fail (
# 211 "./lookup/pr21802.C"
                            "t == 7"
# 211 "./lookup/pr21802.C" 3 4
                            , "./lookup/pr21802.C", 211, __PRETTY_FUNCTION__))
# 211 "./lookup/pr21802.C"
                                           ; }
  { int t = x.operator<= (I); 
# 212 "./lookup/pr21802.C" 3 4
                             ((
# 212 "./lookup/pr21802.C"
                             t == 7
# 212 "./lookup/pr21802.C" 3 4
                             ) ? static_cast<void> (0) : __assert_fail (
# 212 "./lookup/pr21802.C"
                             "t == 7"
# 212 "./lookup/pr21802.C" 3 4
                             , "./lookup/pr21802.C", 212, __PRETTY_FUNCTION__))
# 212 "./lookup/pr21802.C"
                                            ; }
  { int t = x.operator> (I); 
# 213 "./lookup/pr21802.C" 3 4
                            ((
# 213 "./lookup/pr21802.C"
                            t == 7
# 213 "./lookup/pr21802.C" 3 4
                            ) ? static_cast<void> (0) : __assert_fail (
# 213 "./lookup/pr21802.C"
                            "t == 7"
# 213 "./lookup/pr21802.C" 3 4
                            , "./lookup/pr21802.C", 213, __PRETTY_FUNCTION__))
# 213 "./lookup/pr21802.C"
                                           ; }
  { int t = x.operator>= (I); 
# 214 "./lookup/pr21802.C" 3 4
                             ((
# 214 "./lookup/pr21802.C"
                             t == 7
# 214 "./lookup/pr21802.C" 3 4
                             ) ? static_cast<void> (0) : __assert_fail (
# 214 "./lookup/pr21802.C"
                             "t == 7"
# 214 "./lookup/pr21802.C" 3 4
                             , "./lookup/pr21802.C", 214, __PRETTY_FUNCTION__))
# 214 "./lookup/pr21802.C"
                                            ; }
  { int t = x.operator* (); 
# 215 "./lookup/pr21802.C" 3 4
                           ((
# 215 "./lookup/pr21802.C"
                           t == 7
# 215 "./lookup/pr21802.C" 3 4
                           ) ? static_cast<void> (0) : __assert_fail (
# 215 "./lookup/pr21802.C"
                           "t == 7"
# 215 "./lookup/pr21802.C" 3 4
                           , "./lookup/pr21802.C", 215, __PRETTY_FUNCTION__))
# 215 "./lookup/pr21802.C"
                                          ; }
  { int t = x.operator! (); 
# 216 "./lookup/pr21802.C" 3 4
                           ((
# 216 "./lookup/pr21802.C"
                           t == 7
# 216 "./lookup/pr21802.C" 3 4
                           ) ? static_cast<void> (0) : __assert_fail (
# 216 "./lookup/pr21802.C"
                           "t == 7"
# 216 "./lookup/pr21802.C" 3 4
                           , "./lookup/pr21802.C", 216, __PRETTY_FUNCTION__))
# 216 "./lookup/pr21802.C"
                                          ; }
  { int t = x.operator~ (); 
# 217 "./lookup/pr21802.C" 3 4
                           ((
# 217 "./lookup/pr21802.C"
                           t == 7
# 217 "./lookup/pr21802.C" 3 4
                           ) ? static_cast<void> (0) : __assert_fail (
# 217 "./lookup/pr21802.C"
                           "t == 7"
# 217 "./lookup/pr21802.C" 3 4
                           , "./lookup/pr21802.C", 217, __PRETTY_FUNCTION__))
# 217 "./lookup/pr21802.C"
                                          ; }
  { int t = x.operator++ (0); 
# 218 "./lookup/pr21802.C" 3 4
                             ((
# 218 "./lookup/pr21802.C"
                             t == 7
# 218 "./lookup/pr21802.C" 3 4
                             ) ? static_cast<void> (0) : __assert_fail (
# 218 "./lookup/pr21802.C"
                             "t == 7"
# 218 "./lookup/pr21802.C" 3 4
                             , "./lookup/pr21802.C", 218, __PRETTY_FUNCTION__))
# 218 "./lookup/pr21802.C"
                                            ; }
  { int t = x.operator-- (0); 
# 219 "./lookup/pr21802.C" 3 4
                             ((
# 219 "./lookup/pr21802.C"
                             t == 7
# 219 "./lookup/pr21802.C" 3 4
                             ) ? static_cast<void> (0) : __assert_fail (
# 219 "./lookup/pr21802.C"
                             "t == 7"
# 219 "./lookup/pr21802.C" 3 4
                             , "./lookup/pr21802.C", 219, __PRETTY_FUNCTION__))
# 219 "./lookup/pr21802.C"
                                            ; }
  { int t = x.operator++ (); 
# 220 "./lookup/pr21802.C" 3 4
                            ((
# 220 "./lookup/pr21802.C"
                            t == 107
# 220 "./lookup/pr21802.C" 3 4
                            ) ? static_cast<void> (0) : __assert_fail (
# 220 "./lookup/pr21802.C"
                            "t == 107"
# 220 "./lookup/pr21802.C" 3 4
                            , "./lookup/pr21802.C", 220, __PRETTY_FUNCTION__))
# 220 "./lookup/pr21802.C"
                                             ; }
  { int t = x.operator-- (); 
# 221 "./lookup/pr21802.C" 3 4
                            ((
# 221 "./lookup/pr21802.C"
                            t == 107
# 221 "./lookup/pr21802.C" 3 4
                            ) ? static_cast<void> (0) : __assert_fail (
# 221 "./lookup/pr21802.C"
                            "t == 107"
# 221 "./lookup/pr21802.C" 3 4
                            , "./lookup/pr21802.C", 221, __PRETTY_FUNCTION__))
# 221 "./lookup/pr21802.C"
                                             ; }
  { int t = x.operator() (); 
# 222 "./lookup/pr21802.C" 3 4
                            ((
# 222 "./lookup/pr21802.C"
                            t == 7
# 222 "./lookup/pr21802.C" 3 4
                            ) ? static_cast<void> (0) : __assert_fail (
# 222 "./lookup/pr21802.C"
                            "t == 7"
# 222 "./lookup/pr21802.C" 3 4
                            , "./lookup/pr21802.C", 222, __PRETTY_FUNCTION__))
# 222 "./lookup/pr21802.C"
                                           ; }
  { int t = x.operator, (I); 
# 223 "./lookup/pr21802.C" 3 4
                            ((
# 223 "./lookup/pr21802.C"
                            t == 7
# 223 "./lookup/pr21802.C" 3 4
                            ) ? static_cast<void> (0) : __assert_fail (
# 223 "./lookup/pr21802.C"
                            "t == 7"
# 223 "./lookup/pr21802.C" 3 4
                            , "./lookup/pr21802.C", 223, __PRETTY_FUNCTION__))
# 223 "./lookup/pr21802.C"
                                           ; }
  { int t = x.operator[] (I); 
# 224 "./lookup/pr21802.C" 3 4
                             ((
# 224 "./lookup/pr21802.C"
                             t == 7
# 224 "./lookup/pr21802.C" 3 4
                             ) ? static_cast<void> (0) : __assert_fail (
# 224 "./lookup/pr21802.C"
                             "t == 7"
# 224 "./lookup/pr21802.C" 3 4
                             , "./lookup/pr21802.C", 224, __PRETTY_FUNCTION__))
# 224 "./lookup/pr21802.C"
                                            ; }
  { int t = x.operator-= (I); 
# 225 "./lookup/pr21802.C" 3 4
                             ((
# 225 "./lookup/pr21802.C"
                             t == 7
# 225 "./lookup/pr21802.C" 3 4
                             ) ? static_cast<void> (0) : __assert_fail (
# 225 "./lookup/pr21802.C"
                             "t == 7"
# 225 "./lookup/pr21802.C" 3 4
                             , "./lookup/pr21802.C", 225, __PRETTY_FUNCTION__))
# 225 "./lookup/pr21802.C"
                                            ; }
  { int t = x.operator/= (I); 
# 226 "./lookup/pr21802.C" 3 4
                             ((
# 226 "./lookup/pr21802.C"
                             t == 7
# 226 "./lookup/pr21802.C" 3 4
                             ) ? static_cast<void> (0) : __assert_fail (
# 226 "./lookup/pr21802.C"
                             "t == 7"
# 226 "./lookup/pr21802.C" 3 4
                             , "./lookup/pr21802.C", 226, __PRETTY_FUNCTION__))
# 226 "./lookup/pr21802.C"
                                            ; }
  { int t = x.operator*= (I); 
# 227 "./lookup/pr21802.C" 3 4
                             ((
# 227 "./lookup/pr21802.C"
                             t == 7
# 227 "./lookup/pr21802.C" 3 4
                             ) ? static_cast<void> (0) : __assert_fail (
# 227 "./lookup/pr21802.C"
                             "t == 7"
# 227 "./lookup/pr21802.C" 3 4
                             , "./lookup/pr21802.C", 227, __PRETTY_FUNCTION__))
# 227 "./lookup/pr21802.C"
                                            ; }

  { int t = x.operator>> (I); 
# 229 "./lookup/pr21802.C" 3 4
                             ((
# 229 "./lookup/pr21802.C"
                             t == 8
# 229 "./lookup/pr21802.C" 3 4
                             ) ? static_cast<void> (0) : __assert_fail (
# 229 "./lookup/pr21802.C"
                             "t == 8"
# 229 "./lookup/pr21802.C" 3 4
                             , "./lookup/pr21802.C", 229, __PRETTY_FUNCTION__))
# 229 "./lookup/pr21802.C"
                                            ; }
  { int t = x.operator& (); 
# 230 "./lookup/pr21802.C" 3 4
                           ((
# 230 "./lookup/pr21802.C"
                           t == 8
# 230 "./lookup/pr21802.C" 3 4
                           ) ? static_cast<void> (0) : __assert_fail (
# 230 "./lookup/pr21802.C"
                           "t == 8"
# 230 "./lookup/pr21802.C" 3 4
                           , "./lookup/pr21802.C", 230, __PRETTY_FUNCTION__))
# 230 "./lookup/pr21802.C"
                                          ; }
  { int t = x.operator& (I); 
# 231 "./lookup/pr21802.C" 3 4
                            ((
# 231 "./lookup/pr21802.C"
                            t == 8
# 231 "./lookup/pr21802.C" 3 4
                            ) ? static_cast<void> (0) : __assert_fail (
# 231 "./lookup/pr21802.C"
                            "t == 8"
# 231 "./lookup/pr21802.C" 3 4
                            , "./lookup/pr21802.C", 231, __PRETTY_FUNCTION__))
# 231 "./lookup/pr21802.C"
                                           ; }
  { int t = operator== (x, I); 
# 232 "./lookup/pr21802.C" 3 4
                              ((
# 232 "./lookup/pr21802.C"
                              t == 8
# 232 "./lookup/pr21802.C" 3 4
                              ) ? static_cast<void> (0) : __assert_fail (
# 232 "./lookup/pr21802.C"
                              "t == 8"
# 232 "./lookup/pr21802.C" 3 4
                              , "./lookup/pr21802.C", 232, __PRETTY_FUNCTION__))
# 232 "./lookup/pr21802.C"
                                             ; }
  { int t = x.operator!= (I); 
# 233 "./lookup/pr21802.C" 3 4
                             ((
# 233 "./lookup/pr21802.C"
                             t == 8
# 233 "./lookup/pr21802.C" 3 4
                             ) ? static_cast<void> (0) : __assert_fail (
# 233 "./lookup/pr21802.C"
                             "t == 8"
# 233 "./lookup/pr21802.C" 3 4
                             , "./lookup/pr21802.C", 233, __PRETTY_FUNCTION__))
# 233 "./lookup/pr21802.C"
                                            ; }
}






inline int operator+(const Y&, int) { return 11; }
inline int operator-(const Y&, int) { return 11; }
inline int operator*(const Y&, int) { return 11; }
inline int operator/(const Y&, int) { return 11; }
inline int operator%(const Y&, int) { return 11; }
inline int operator>>(const Y&, int) { return 11; }
inline int operator<<(const Y&, int) { return 11; }
inline int operator&(const Y&, int) { return 11; }
inline int operator|(const Y&, int) { return 11; }
inline int operator^(const Y&, int) { return 11; }
inline int operator&&(const Y&, int) { return 11; }
inline int operator||(const Y&, int) { return 11; }
inline int operator==(const Y&, int) { return 11; }
inline int operator!=(const Y&, int) { return 11; }
inline int operator<(const Y&, int) { return 11; }
inline int operator<=(const Y&, int) { return 11; }
inline int operator>(const Y&, int) { return 11; }
inline int operator>=(const Y&, int) { return 11; }
inline int operator*(const Y&) { return 11; }
inline int operator!(const Y&) { return 11; }
inline int operator~(const Y&) { return 11; }
inline int operator++(const Y&) { return 11; }
inline int operator--(const Y&) { return 11; }
inline int operator++(const Y&, int) { return 11; }
inline int operator--(const Y&, int) { return 11; }
inline int operator,(const Y&, int) { return 11; }
inline int operator&(const Y&) { return 11; }
inline int operator+=(const Y&, int x) { return 11; }
inline int operator*=(const Y&, int x) { return 11; }
inline int operator-=(const Y&, int x) { return 11; }
inline int operator/=(const Y&, int x) { return 11; }

int
main ()
{
  Foo1 (0);
  Foo2 (0);
  Foo3 (0);
  Foo4 (0);
}
