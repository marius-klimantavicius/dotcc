//type: fn
//options:  --c++11
# 1 "SemaCXX/underlying_type.cpp"
# 1 "<built-in>" 1
# 1 "<built-in>" 3
# 452 "<built-in>" 3
# 1 "<command line>" 1
# 1 "<built-in>" 2
# 1 "SemaCXX/underlying_type.cpp" 2


# 1 "/mds/clang/clang-22.1.0/build/lib/clang/22/include/limits.h" 1
# 25 "/mds/clang/clang-22.1.0/build/lib/clang/22/include/limits.h"
# 1 "/usr/include/limits.h" 1 3 4
# 26 "/usr/include/limits.h" 3 4
# 1 "/usr/include/features.h" 1 3 4
# 345 "/usr/include/features.h" 3 4
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 346 "/usr/include/features.h" 2 3 4
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
# 27 "/usr/include/limits.h" 2 3 4
# 144 "/usr/include/limits.h" 3 4
# 1 "/usr/include/bits/posix1_lim.h" 1 3 4
# 160 "/usr/include/bits/posix1_lim.h" 3 4
# 1 "/usr/include/bits/local_lim.h" 1 3 4
# 38 "/usr/include/bits/local_lim.h" 3 4
# 1 "/usr/include/linux/limits.h" 1 3 4
# 39 "/usr/include/bits/local_lim.h" 2 3 4
# 161 "/usr/include/bits/posix1_lim.h" 2 3 4
# 145 "/usr/include/limits.h" 2 3 4



# 1 "/usr/include/bits/posix2_lim.h" 1 3 4
# 149 "/usr/include/limits.h" 2 3 4



# 1 "/usr/include/bits/xopen_lim.h" 1 3 4
# 33 "/usr/include/bits/xopen_lim.h" 3 4
# 1 "/usr/include/bits/stdio_lim.h" 1 3 4
# 34 "/usr/include/bits/xopen_lim.h" 2 3 4
# 153 "/usr/include/limits.h" 2 3 4
# 26 "/mds/clang/clang-22.1.0/build/lib/clang/22/include/limits.h" 2
# 4 "SemaCXX/underlying_type.cpp" 2

template<typename T, typename U>
struct is_same_type {
  static const bool value = false;
};
template <typename T>
struct is_same_type<T, T> {
  static const bool value = true;
};

__underlying_type(int) a;
__underlying_type(struct b) c;

enum class f : char;
static_assert(is_same_type<char, __underlying_type(f)>::value,
              "f has the wrong underlying type");

enum g {d = (-2147483647 -1) };
static_assert(is_same_type<int, __underlying_type(g)>::value,
              "g has the wrong underlying type");

__underlying_type(f) h;
static_assert(is_same_type<char, decltype(h)>::value,
              "h has the wrong type");

template <typename T>
struct underlying_type {
  typedef __underlying_type(T) type;
};

static_assert(is_same_type<underlying_type<f>::type, char>::value,
              "f has the wrong underlying type in the template");

underlying_type<int>::type e;

using uint = unsigned;
enum class foo : uint { bar };

static_assert(is_same_type<underlying_type<foo>::type, unsigned>::value,
              "foo has the wrong underlying type");

namespace PR19966 {
  void PR19966(enum Invalid) {


    __underlying_type(Invalid) dont_crash;

  }
  enum E {
    a = (__underlying_type(E)){}


  };
}

template<typename T> void f(__underlying_type(T));
template<typename T> void f(__underlying_type(T));
enum E {};
void PR26014() { f<E>(0); }

template<typename ...T> void f(__underlying_type(T) v);

namespace PR23421 {
template <class T>
using underlying_type_t = __underlying_type(T);

template <class T>
struct make_unsigned_impl { using type = underlying_type_t<T>; };
using AnotherType = make_unsigned_impl<E>::type;


template <typename T>
__underlying_type(T) ft();
auto x = &ft<E>;
}
