//type:fp
//options:--target win32:--target linux_i686:--target linux_armv7
//options_all:-w --c++20 --microsoft_version 1936 --pack_alignment 4

struct B
{
  alignas(16) int arr[4];
  int b;
};

static_assert(sizeof(B) == 32, "sizeof B");
static_assert(alignof(B) == 16, "alignof B");
static_assert(__builtin_offsetof(B, b) == 16, "offsetof b");

struct D : B
{
  int d1, d2, d3;
};

static_assert(sizeof(D) == 32, "sizeof D");
static_assert(__builtin_offsetof(D, d1) == 20, "offsetof d1");
static_assert(__builtin_offsetof(D, d2) == 24, "offsetof d2");
static_assert(__builtin_offsetof(D, d3) == 28, "offsetof d3");
