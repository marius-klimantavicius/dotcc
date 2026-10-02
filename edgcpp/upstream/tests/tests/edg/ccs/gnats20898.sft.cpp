//options:--gnu_version 50400
//options_all:--c++11

struct myfloat4 { char buf[16]; };

template <typename T>
struct S1 {
  myfloat4 sums[2];
  enum { myval = T::val };

  void foo(char *smem) {
    reinterpret_cast<myfloat4*>(&smem[0+myval])[0] = sums[0];
  }
};

struct S2 { static constexpr int val = 0; };

void mmm() {
  S1<S2> baz;
  baz.foo(0);
}
