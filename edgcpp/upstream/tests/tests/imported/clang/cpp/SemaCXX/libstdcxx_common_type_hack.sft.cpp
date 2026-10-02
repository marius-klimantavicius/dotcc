//type: fn
//options:  --c++11 -D__GLIBCXX__=20100000L
# 1 "SemaCXX/libstdcxx_common_type_hack.cpp"
# 1 "<built-in>" 1
# 1 "<built-in>" 3
# 452 "<built-in>" 3
# 1 "<command line>" 1
# 1 "<built-in>" 2
# 1 "SemaCXX/libstdcxx_common_type_hack.cpp" 2
# 25 "SemaCXX/libstdcxx_common_type_hack.cpp"
# 1 "SemaCXX/libstdcxx_common_type_hack.cpp" 1
# 11 "SemaCXX/libstdcxx_common_type_hack.cpp" 3
namespace std {
  template<typename T> T &&declval();

  template<typename...Ts> struct common_type {};
  template<typename A, typename B> struct common_type<A, B> {


    typedef decltype(true ? declval<A>() : declval<B>()) type;
  };
}
# 26 "SemaCXX/libstdcxx_common_type_hack.cpp" 2

using T = int;
using T = std::common_type<int, int>::type;

using U = int;
using U = decltype(true ? std::declval<int>() : std::declval<int>());
