//type: rp
//options: --c++11
# 0 "./cpp0x/variadic-function.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./cpp0x/variadic-function.C"




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
# 6 "./cpp0x/variadic-function.C" 2


# 7 "./cpp0x/variadic-function.C"
template<typename Signature>
class function;

template<typename R, typename... Args>
class invoker_base
{
 public:
  virtual ~invoker_base() { }
  virtual R invoke(Args...) = 0;
  virtual invoker_base* clone() = 0;
};

template<typename F, typename R, typename... Args>
class functor_invoker : public invoker_base<R, Args...>
{
 public:
  explicit functor_invoker(const F& f) : f(f) { }
  R invoke(Args... args) { return f(args...); }
  functor_invoker* clone() { return new functor_invoker(f); }

 private:
  F f;
};

template<typename R, typename... Args>
class function<R (Args...)> {
 public:
  typedef R result_type;

  function() : invoker (0) { }

  function(const function& other) : invoker(0) {
    if (other.invoker)
      invoker = other.invoker->clone();
  }

  template<typename F>
  function(const F& f) : invoker(0) {
    invoker = new functor_invoker<F, R, Args...>(f);
  }

  ~function() {
    if (invoker)
      delete invoker;
  }

  function& operator=(const function& other) {
    function(other).swap(*this);
    return *this;
  }

  template<typename F>
  function& operator=(const F& f) {
    function(f).swap(*this);
    return *this;
  }

  void swap(function& other) {
    invoker_base<R, Args...>* tmp = invoker;
    invoker = other.invoker;
    other.invoker = tmp;
  }

  result_type operator()(Args... args) const {
    
# 71 "./cpp0x/variadic-function.C" 3 4
   ((
# 71 "./cpp0x/variadic-function.C"
   invoker
# 71 "./cpp0x/variadic-function.C" 3 4
   ) ? static_cast<void> (0) : __assert_fail (
# 71 "./cpp0x/variadic-function.C"
   "invoker"
# 71 "./cpp0x/variadic-function.C" 3 4
   , "./cpp0x/variadic-function.C", 71, __PRETTY_FUNCTION__))
# 71 "./cpp0x/variadic-function.C"
                  ;
    return invoker->invoke(args...);
  }

 private:
  invoker_base<R, Args...>* invoker;
};

struct plus {
  template<typename T> T operator()(T x, T y) { return x + y; }
};

struct multiplies {
  template<typename T> T operator()(T x, T y) { return x * y; }
};

int main()
{
  function<int(int, int)> f1 = plus();
  
# 90 "./cpp0x/variadic-function.C" 3 4
 ((
# 90 "./cpp0x/variadic-function.C"
 f1(3, 5) == 8
# 90 "./cpp0x/variadic-function.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 90 "./cpp0x/variadic-function.C"
 "f1(3, 5) == 8"
# 90 "./cpp0x/variadic-function.C" 3 4
 , "./cpp0x/variadic-function.C", 90, __PRETTY_FUNCTION__))
# 90 "./cpp0x/variadic-function.C"
                      ;

  f1 = multiplies();
  
# 93 "./cpp0x/variadic-function.C" 3 4
 ((
# 93 "./cpp0x/variadic-function.C"
 f1(3, 5) == 15
# 93 "./cpp0x/variadic-function.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 93 "./cpp0x/variadic-function.C"
 "f1(3, 5) == 15"
# 93 "./cpp0x/variadic-function.C" 3 4
 , "./cpp0x/variadic-function.C", 93, __PRETTY_FUNCTION__))
# 93 "./cpp0x/variadic-function.C"
                       ;

  return 0;
}
