//type: fn
//options:  --c++11 --exceptions --exceptions -D__GLIBCXX__=20100000L -DCLASS=array: --c++11 --exceptions --exceptions -D__GLIBCXX__=20100000L -DCLASS=array -DPR28423: --c++11 --exceptions --exceptions -D__GLIBCXX__=20100000L -DCLASS=pair: --c++11 --exceptions --exceptions -D__GLIBCXX__=20100000L -DCLASS=priority_queue: --c++11 --exceptions --exceptions -D__GLIBCXX__=20100000L -DCLASS=stack: --c++11 --exceptions --exceptions -D__GLIBCXX__=20100000L -DCLASS=queue: --c++11 --exceptions --exceptions -D__GLIBCXX__=20100000L -DCLASS=array -DNAMESPACE=__debug: --c++11 --exceptions --exceptions -D__GLIBCXX__=20100000L -DCLASS=array -DNAMESPACE=__profile: --c++11 --exceptions --exceptions -D__GLIBCXX__=20100000L -DCLASS=array -DMSVC
# 1 "SemaCXX/libstdcxx_pair_swap_hack.cpp"
# 1 "<built-in>" 1
# 1 "<built-in>" 3
# 452 "<built-in>" 3
# 1 "<command line>" 1
# 1 "<built-in>" 2
# 1 "SemaCXX/libstdcxx_pair_swap_hack.cpp" 2
# 69 "SemaCXX/libstdcxx_pair_swap_hack.cpp"
# 1 "SemaCXX/libstdcxx_pair_swap_hack.cpp" 1
# 28 "SemaCXX/libstdcxx_pair_swap_hack.cpp" 3




namespace std {
  template<typename T> void swap(T &, T &);
  template<typename T> void do_swap(T &a, T &b) noexcept(noexcept(swap(a, b))) {
    swap(a, b);
  }
# 45 "SemaCXX/libstdcxx_pair_swap_hack.cpp" 3
  template<typename A, typename B> struct CLASS {



    A member;

    void swap(CLASS &other) noexcept(noexcept(swap(member, other.member)));

  };
# 64 "SemaCXX/libstdcxx_pair_swap_hack.cpp" 3
}
# 70 "SemaCXX/libstdcxx_pair_swap_hack.cpp" 2

struct X {};
using PX = std::CLASS<X, X>;
using PI = std::CLASS<int, int>;
void swap(X &, X &) noexcept;
PX px;
PI pi;

static_assert(noexcept(px.swap(px)), "");
static_assert(!noexcept(pi.swap(pi)), "");

namespace sad {
  template<typename T> void swap(T &, T &);

  template<typename A, typename B> struct CLASS {
    void swap(CLASS &other)
      noexcept(noexcept(swap(*this, other)));

  };

  CLASS<int, int> pi;

  static_assert(!noexcept(pi.swap(pi)), "");
}
