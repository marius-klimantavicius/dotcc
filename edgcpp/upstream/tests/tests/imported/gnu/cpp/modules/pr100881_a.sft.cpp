//type: fp
//options:  --c++20 --c++20 --modules
# 0 "./modules/pr100881_a.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./modules/pr100881_a.C"



module;
# 1 "/mds/gnu/build/gcc-15.1.0/include/c++/15.1.0/source_location" 1 3
# 33 "/mds/gnu/build/gcc-15.1.0/include/c++/15.1.0/source_location" 3
# 1 "/mds/gnu/build/gcc-15.1.0/include/c++/15.1.0/bits/version.h" 1 3
# 51 "/mds/gnu/build/gcc-15.1.0/include/c++/15.1.0/bits/version.h" 3
# 1 "/mds/gnu/build/gcc-15.1.0/include/c++/15.1.0/x86_64-pc-linux-gnu/bits/c++config.h" 1 3
# 37 "/mds/gnu/build/gcc-15.1.0/include/c++/15.1.0/x86_64-pc-linux-gnu/bits/c++config.h" 3
#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wvariadic-macros"

#pragma GCC diagnostic ignored "-Wc++11-extensions"
#pragma GCC diagnostic ignored "-Wc++23-extensions"
# 336 "/mds/gnu/build/gcc-15.1.0/include/c++/15.1.0/x86_64-pc-linux-gnu/bits/c++config.h" 3
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
# 369 "/mds/gnu/build/gcc-15.1.0/include/c++/15.1.0/x86_64-pc-linux-gnu/bits/c++config.h" 3
namespace std
{
  inline namespace __cxx11 __attribute__((__abi_tag__ ("cxx11"))) { }
}
namespace __gnu_cxx
{
  inline namespace __cxx11 __attribute__((__abi_tag__ ("cxx11"))) { }
}
# 573 "/mds/gnu/build/gcc-15.1.0/include/c++/15.1.0/x86_64-pc-linux-gnu/bits/c++config.h" 3
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
# 617 "/mds/gnu/build/gcc-15.1.0/include/c++/15.1.0/x86_64-pc-linux-gnu/bits/c++config.h" 3
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
# 727 "/mds/gnu/build/gcc-15.1.0/include/c++/15.1.0/x86_64-pc-linux-gnu/bits/c++config.h" 3
# 1 "/mds/gnu/build/gcc-15.1.0/include/c++/15.1.0/x86_64-pc-linux-gnu/bits/os_defines.h" 1 3
# 39 "/mds/gnu/build/gcc-15.1.0/include/c++/15.1.0/x86_64-pc-linux-gnu/bits/os_defines.h" 3
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
# 40 "/mds/gnu/build/gcc-15.1.0/include/c++/15.1.0/x86_64-pc-linux-gnu/bits/os_defines.h" 2 3
# 728 "/mds/gnu/build/gcc-15.1.0/include/c++/15.1.0/x86_64-pc-linux-gnu/bits/c++config.h" 2 3


# 1 "/mds/gnu/build/gcc-15.1.0/include/c++/15.1.0/x86_64-pc-linux-gnu/bits/cpu_defines.h" 1 3
# 731 "/mds/gnu/build/gcc-15.1.0/include/c++/15.1.0/x86_64-pc-linux-gnu/bits/c++config.h" 2 3
# 887 "/mds/gnu/build/gcc-15.1.0/include/c++/15.1.0/x86_64-pc-linux-gnu/bits/c++config.h" 3
namespace __gnu_cxx
{
  typedef __decltype(0.0bf16) __bfloat16_t;
}
# 949 "/mds/gnu/build/gcc-15.1.0/include/c++/15.1.0/x86_64-pc-linux-gnu/bits/c++config.h" 3
# 1 "/mds/gnu/build/gcc-15.1.0/include/c++/15.1.0/pstl/pstl_config.h" 1 3
# 950 "/mds/gnu/build/gcc-15.1.0/include/c++/15.1.0/x86_64-pc-linux-gnu/bits/c++config.h" 2 3



#pragma GCC diagnostic pop
# 52 "/mds/gnu/build/gcc-15.1.0/include/c++/15.1.0/bits/version.h" 2 3
# 34 "/mds/gnu/build/gcc-15.1.0/include/c++/15.1.0/source_location" 2 3




namespace std
{



  struct source_location
  {
  private:
    using uint_least32_t = unsigned int;
    struct __impl
    {
      const char* _M_file_name;
      const char* _M_function_name;
      unsigned _M_line;
      unsigned _M_column;
    };
    using __builtin_ret_type = decltype(__builtin_source_location());

  public:


    static consteval source_location
    current(__builtin_ret_type __p = __builtin_source_location()) noexcept
    {
      source_location __ret;
      __ret._M_impl = static_cast <const __impl*>(__p);
      return __ret;
    }

    constexpr source_location() noexcept { }


    constexpr uint_least32_t
    line() const noexcept
    { return _M_impl ? _M_impl->_M_line : 0u; }

    constexpr uint_least32_t
    column() const noexcept
    { return _M_impl ? _M_impl->_M_column : 0u; }

    constexpr const char*
    file_name() const noexcept
    { return _M_impl ? _M_impl->_M_file_name : ""; }

    constexpr const char*
    function_name() const noexcept
    { return _M_impl ? _M_impl->_M_function_name : ""; }

  private:
    const __impl* _M_impl = nullptr;
  };


}
# 6 "./modules/pr100881_a.C" 2

# 6 "./modules/pr100881_a.C"
export module pr100881;

export
consteval int
current_line_fn(const std::source_location& loc = std::source_location::current())
{
  return loc.line();
}

export
struct current_line_cls
{
  int line = std::source_location::current().line();
};

export
template<class T>
consteval int
current_line_fn_tmpl(const std::source_location& loc = std::source_location::current())
{
  return loc.line();
}

export
template<class T>
struct current_line_cls_tmpl
{
  int line = std::source_location::current().line();
};
