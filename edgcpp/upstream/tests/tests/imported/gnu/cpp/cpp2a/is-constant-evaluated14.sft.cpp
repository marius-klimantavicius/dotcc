//type: s
//options: --c++11
# 0 "./cpp2a/is-constant-evaluated14.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./cpp2a/is-constant-evaluated14.C"
# 11 "./cpp2a/is-constant-evaluated14.C"
# 1 "/mds/gnu/build/gcc-13.1.0/include/c++/13.1.0/initializer_list" 1 3
# 33 "/mds/gnu/build/gcc-13.1.0/include/c++/13.1.0/initializer_list" 3
       
# 34 "/mds/gnu/build/gcc-13.1.0/include/c++/13.1.0/initializer_list" 3





# 1 "/mds/gnu/build/gcc-13.1.0/include/c++/13.1.0/x86_64-pc-linux-gnu/bits/c++config.h" 1 3
# 306 "/mds/gnu/build/gcc-13.1.0/include/c++/13.1.0/x86_64-pc-linux-gnu/bits/c++config.h" 3

# 306 "/mds/gnu/build/gcc-13.1.0/include/c++/13.1.0/x86_64-pc-linux-gnu/bits/c++config.h" 3
namespace std
{
  typedef long unsigned int size_t;
  typedef long int ptrdiff_t;


  typedef decltype(nullptr) nullptr_t;


#pragma GCC visibility push(default)


  extern "C++" __attribute__ ((__noreturn__, __always_inline__))
  inline void __terminate() noexcept
  {
    void terminate() noexcept __attribute__ ((__noreturn__));
    terminate();
  }
#pragma GCC visibility pop
}
# 339 "/mds/gnu/build/gcc-13.1.0/include/c++/13.1.0/x86_64-pc-linux-gnu/bits/c++config.h" 3
namespace std
{
  inline namespace __cxx11 __attribute__((__abi_tag__ ("cxx11"))) { }
}
namespace __gnu_cxx
{
  inline namespace __cxx11 __attribute__((__abi_tag__ ("cxx11"))) { }
}
# 532 "/mds/gnu/build/gcc-13.1.0/include/c++/13.1.0/x86_64-pc-linux-gnu/bits/c++config.h" 3
namespace std
{
#pragma GCC visibility push(default)




  constexpr inline bool
  __is_constant_evaluated() noexcept
  {





    return __builtin_is_constant_evaluated();



  }
#pragma GCC visibility pop
}
# 679 "/mds/gnu/build/gcc-13.1.0/include/c++/13.1.0/x86_64-pc-linux-gnu/bits/c++config.h" 3
# 1 "/mds/gnu/build/gcc-13.1.0/include/c++/13.1.0/x86_64-pc-linux-gnu/bits/os_defines.h" 1 3
# 39 "/mds/gnu/build/gcc-13.1.0/include/c++/13.1.0/x86_64-pc-linux-gnu/bits/os_defines.h" 3
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
# 40 "/mds/gnu/build/gcc-13.1.0/include/c++/13.1.0/x86_64-pc-linux-gnu/bits/os_defines.h" 2 3
# 680 "/mds/gnu/build/gcc-13.1.0/include/c++/13.1.0/x86_64-pc-linux-gnu/bits/c++config.h" 2 3


# 1 "/mds/gnu/build/gcc-13.1.0/include/c++/13.1.0/x86_64-pc-linux-gnu/bits/cpu_defines.h" 1 3
# 683 "/mds/gnu/build/gcc-13.1.0/include/c++/13.1.0/x86_64-pc-linux-gnu/bits/c++config.h" 2 3
# 40 "/mds/gnu/build/gcc-13.1.0/include/c++/13.1.0/initializer_list" 2 3

namespace std __attribute__ ((__visibility__ ("default")))
{

  template<class _E>
    class initializer_list
    {
    public:
      typedef _E value_type;
      typedef const _E& reference;
      typedef const _E& const_reference;
      typedef size_t size_type;
      typedef const _E* iterator;
      typedef const _E* const_iterator;

    private:
      iterator _M_array;
      size_type _M_len;


      constexpr initializer_list(const_iterator __a, size_type __l)
      : _M_array(__a), _M_len(__l) { }

    public:
      constexpr initializer_list() noexcept
      : _M_array(0), _M_len(0) { }


      constexpr size_type
      size() const noexcept { return _M_len; }


      constexpr const_iterator
      begin() const noexcept { return _M_array; }


      constexpr const_iterator
      end() const noexcept { return begin() + size(); }
    };







  template<class _Tp>
    constexpr const _Tp*
    begin(initializer_list<_Tp> __ils) noexcept
    { return __ils.begin(); }







  template<class _Tp>
    constexpr const _Tp*
    end(initializer_list<_Tp> __ils) noexcept
    { return __ils.end(); }
}
# 12 "./cpp2a/is-constant-evaluated14.C" 2


# 13 "./cpp2a/is-constant-evaluated14.C"
struct A {
  constexpr A(int n) : n(n), m(__builtin_is_constant_evaluated()) { }
  constexpr A() : A(42) { }
  void verify_mce() const {
    if (m != 1) __builtin_abort();
  }
  int n;
  int m;
};

A a1 = {42};
A a2{42};
A a3(42);
A a4;
A a5{};

void f() {
  static A a1 = {42};
  static A a2{42};
  static A a3(42);
  static A a4;
  static A a5{};
  for (auto& a : {a1, a2, a3, a4, a5})
    a.verify_mce();
}

template<int... N>
void g() {
  static A a1 = {42};
  static A a2{42};
  static A a3(42);
  static A a4;
  static A a5{};
  static A a6 = {N...};
  static A a7{N...};
  static A a8(N...);
  for (auto& a : {a1, a2, a3, a4, a5, a6, a7, a8})
    a.verify_mce();
}

struct B {
  static A a1;
  static A a2;
  static A a3;
  static A a4;
  static A a5;
  static void verify_mce() {
    for (auto& a : {a1, a2, a3, a4, a5})
      a.verify_mce();
  }
};

A B::a1 = {42};
A B::a2{42};
A B::a3(42);
A B::a4;
A B::a5{};

template<int... N>
struct BT {
  static A a1;
  static A a2;
  static A a3;
  static A a4;
  static A a5;
  static A a6;
  static A a7;
  static A a8;
  static void verify_mce() {
    for (auto& a : {a1, a2, a3, a4, a5})
      a.verify_mce();
  }
};

template<int... N> A BT<N...>::a1 = {42};
template<int... N> A BT<N...>::a2{42};
template<int... N> A BT<N...>::a3(42);
template<int... N> A BT<N...>::a4;
template<int... N> A BT<N...>::a5{};
template<int... N> A BT<N...>::a6 = {N...};
template<int... N> A BT<N...>::a7{N...};
template<int... N> A BT<N...>::a8(N...);
# 123 "./cpp2a/is-constant-evaluated14.C"
int main() {
  for (auto& a : {a1, a2, a3, a4, a5})
    a.verify_mce();

  f();
  g<42>();
  g<>();

  B::verify_mce();
  BT<42>::verify_mce();
  BT<>::verify_mce();






}
