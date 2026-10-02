//type:fp
//options:--target linux_aarch64:--target win64;fn
//options_all:-w --ms_c++20

struct alignas(16) foo {
  unsigned long long data[2];
};

struct Base {
  foo a;
  int b;
};

struct Derived : public Base {
  int c;
};

static_assert(sizeof(Derived) == 32);
static_assert(__builtin_offsetof(Derived, c) == 24);
