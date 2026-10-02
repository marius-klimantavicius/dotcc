//type: s
//options: --c++17
# 1 "./concepts/explicit-spec4.C"
# 1 "<built-in>"
# 1 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 1 "<command-line>" 2
# 1 "./concepts/explicit-spec4.C"



# 1 "/mds/gnu/build/gcc-9.3.0/include/c++/9.3.0/cassert" 1 3
# 41 "/mds/gnu/build/gcc-9.3.0/include/c++/9.3.0/cassert" 3
       
# 42 "/mds/gnu/build/gcc-9.3.0/include/c++/9.3.0/cassert" 3

# 1 "/mds/gnu/build/gcc-9.3.0/include/c++/9.3.0/x86_64-pc-linux-gnu/bits/c++config.h" 1 3
# 252 "/mds/gnu/build/gcc-9.3.0/include/c++/9.3.0/x86_64-pc-linux-gnu/bits/c++config.h" 3

# 252 "/mds/gnu/build/gcc-9.3.0/include/c++/9.3.0/x86_64-pc-linux-gnu/bits/c++config.h" 3
namespace std
{
  typedef long unsigned int size_t;
  typedef long int ptrdiff_t;


  typedef decltype(nullptr) nullptr_t;

}
# 274 "/mds/gnu/build/gcc-9.3.0/include/c++/9.3.0/x86_64-pc-linux-gnu/bits/c++config.h" 3
namespace std
{
  inline namespace __cxx11 __attribute__((__abi_tag__ ("cxx11"))) { }
}
namespace __gnu_cxx
{
  inline namespace __cxx11 __attribute__((__abi_tag__ ("cxx11"))) { }
}
# 524 "/mds/gnu/build/gcc-9.3.0/include/c++/9.3.0/x86_64-pc-linux-gnu/bits/c++config.h" 3
# 1 "/mds/gnu/build/gcc-9.3.0/include/c++/9.3.0/x86_64-pc-linux-gnu/bits/os_defines.h" 1 3
# 39 "/mds/gnu/build/gcc-9.3.0/include/c++/9.3.0/x86_64-pc-linux-gnu/bits/os_defines.h" 3
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
# 40 "/mds/gnu/build/gcc-9.3.0/include/c++/9.3.0/x86_64-pc-linux-gnu/bits/os_defines.h" 2 3
# 525 "/mds/gnu/build/gcc-9.3.0/include/c++/9.3.0/x86_64-pc-linux-gnu/bits/c++config.h" 2 3


# 1 "/mds/gnu/build/gcc-9.3.0/include/c++/9.3.0/x86_64-pc-linux-gnu/bits/cpu_defines.h" 1 3
# 528 "/mds/gnu/build/gcc-9.3.0/include/c++/9.3.0/x86_64-pc-linux-gnu/bits/c++config.h" 2 3
# 690 "/mds/gnu/build/gcc-9.3.0/include/c++/9.3.0/x86_64-pc-linux-gnu/bits/c++config.h" 3
# 1 "/mds/gnu/build/gcc-9.3.0/include/c++/9.3.0/pstl/pstl_config.h" 1 3
# 691 "/mds/gnu/build/gcc-9.3.0/include/c++/9.3.0/x86_64-pc-linux-gnu/bits/c++config.h" 2 3
# 44 "/mds/gnu/build/gcc-9.3.0/include/c++/9.3.0/cassert" 2 3
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
# 44 "/mds/gnu/build/gcc-9.3.0/include/c++/9.3.0/cassert" 2 3
# 5 "./concepts/explicit-spec4.C" 2


# 6 "./concepts/explicit-spec4.C"
template<typename T>
  concept bool C() { return __is_class(T); }

template<typename T>
  concept bool D() { return C<T>() && __is_empty(T); }

struct X { } x;
struct Y { int n; } y;

int called = 0;

template<typename T>
  struct S {
    void f() { called = 0; }
    void f() requires C<T>() { called = 0; }

    void g() requires C<T>() { }
    void g() requires D<T>() { }
  };

template<> void S<int>::f() { called = 1; }
template<> void S<X>::f() { called = 2; }

template<> void S<X>::g() { called = 3; }
template<> void S<Y>::g() { called = 4; }

int main() {
  S<double> sd;
  S<int> si;
  S<X> sx;
  S<Y> sy;

  sd.f();
  
# 39 "./concepts/explicit-spec4.C" 3 4
 ((
# 39 "./concepts/explicit-spec4.C"
 called == 0
# 39 "./concepts/explicit-spec4.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 39 "./concepts/explicit-spec4.C"
 "called == 0"
# 39 "./concepts/explicit-spec4.C" 3 4
 , "./concepts/explicit-spec4.C", 39, __PRETTY_FUNCTION__))
# 39 "./concepts/explicit-spec4.C"
                    ;
  si.f();
  
# 41 "./concepts/explicit-spec4.C" 3 4
 ((
# 41 "./concepts/explicit-spec4.C"
 called == 1
# 41 "./concepts/explicit-spec4.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 41 "./concepts/explicit-spec4.C"
 "called == 1"
# 41 "./concepts/explicit-spec4.C" 3 4
 , "./concepts/explicit-spec4.C", 41, __PRETTY_FUNCTION__))
# 41 "./concepts/explicit-spec4.C"
                    ;
  sx.f();
  
# 43 "./concepts/explicit-spec4.C" 3 4
 ((
# 43 "./concepts/explicit-spec4.C"
 called == 2
# 43 "./concepts/explicit-spec4.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 43 "./concepts/explicit-spec4.C"
 "called == 2"
# 43 "./concepts/explicit-spec4.C" 3 4
 , "./concepts/explicit-spec4.C", 43, __PRETTY_FUNCTION__))
# 43 "./concepts/explicit-spec4.C"
                    ;
  sy.f();
  
# 45 "./concepts/explicit-spec4.C" 3 4
 ((
# 45 "./concepts/explicit-spec4.C"
 called == 0
# 45 "./concepts/explicit-spec4.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 45 "./concepts/explicit-spec4.C"
 "called == 0"
# 45 "./concepts/explicit-spec4.C" 3 4
 , "./concepts/explicit-spec4.C", 45, __PRETTY_FUNCTION__))
# 45 "./concepts/explicit-spec4.C"
                    ;

  sx.g();
  
# 48 "./concepts/explicit-spec4.C" 3 4
 ((
# 48 "./concepts/explicit-spec4.C"
 called == 3
# 48 "./concepts/explicit-spec4.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 48 "./concepts/explicit-spec4.C"
 "called == 3"
# 48 "./concepts/explicit-spec4.C" 3 4
 , "./concepts/explicit-spec4.C", 48, __PRETTY_FUNCTION__))
# 48 "./concepts/explicit-spec4.C"
                    ;
  sy.g();
  
# 50 "./concepts/explicit-spec4.C" 3 4
 ((
# 50 "./concepts/explicit-spec4.C"
 called == 4
# 50 "./concepts/explicit-spec4.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 50 "./concepts/explicit-spec4.C"
 "called == 4"
# 50 "./concepts/explicit-spec4.C" 3 4
 , "./concepts/explicit-spec4.C", 50, __PRETTY_FUNCTION__))
# 50 "./concepts/explicit-spec4.C"
                    ;
}
