//type: fn
//options:  --c++20: --c++20: --c++20: --c++20 --target linux_aarch64
# 1 "SemaCXX/builtin_vectorelements.cpp"
# 1 "<built-in>" 1
# 1 "<built-in>" 3
# 493 "<built-in>" 3
# 1 "<command line>" 1
# 1 "<built-in>" 2
# 1 "SemaCXX/builtin_vectorelements.cpp" 2







template <typename T>
using VecT __attribute__((vector_size(16))) = T;

struct FooT {
  template <typename T>
  using VecT __attribute__((vector_size(8))) = T;
};

void test_builtin_vectorelements() {
  using veci4 __attribute__((vector_size(16))) = int;
  (void) __builtin_vectorelements(veci4);

  using some_other_vec = veci4;
  (void) __builtin_vectorelements(some_other_vec);

  using some_int = int;
  (void) __builtin_vectorelements(some_int);

  class Foo {};
  __builtin_vectorelements(Foo);

  struct Bar { veci4 vec; };
  (void) __builtin_vectorelements(Bar{}.vec);

  struct Baz { using VecT = veci4; };
  (void) __builtin_vectorelements(Baz::VecT);

  (void) __builtin_vectorelements(FooT::VecT<long>);
  (void) __builtin_vectorelements(VecT<char>);

  constexpr int i4 = __builtin_vectorelements(veci4);
  constexpr int i4p8 = __builtin_vectorelements(veci4) + 8;
}
