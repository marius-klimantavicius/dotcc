//options:;fn:--gnu_version 70400;fp:--clang_version 50100;fn:--microsoft_version 1920;fp
//options_all:--c++17

using size_t = decltype(sizeof(0));

namespace std { enum class align_val_t : size_t {}; }

template<typename...Ts> struct A
{
  void *operator new(size_t);
  void operator delete(void*, Ts...) = delete;
};

template<typename...Ts> struct alignas(32) B // over-aligned
{
  void *operator new(size_t);
  void operator delete(void*, Ts...) = delete;
};

auto *a1 = new A<>; // error expected
auto *a2 = new A<size_t>; // error expected
auto *a3 = new A<std::align_val_t>; // error expected
auto *a4 = new A<size_t, std::align_val_t>; // error expected
auto *a5 = new B<std::align_val_t>; // error expected
