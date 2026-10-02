//type:fp
//options:--target win64 -DBITS=64:--target linux_aarch64 -DHAS_BUG -DBITS=64:--target win32 -DBITS=32:--target linux_armv7 -DBITS=32
//options_all:-w --c++20 --microsoft_version 1936

#if HAS_BUG
#define BUG(x, y) x
#if BITS == 64
#define BUG32_64(x, y, z) y
#else
#define BUG32_64(x, y, z) x
#endif
#else
#define BUG(x, y) y
#define BUG32_64(x, y, z) z
#endif

namespace minimal
{
  struct B {
    alignas(16) int i;
  };
  struct D : B {
    int j;
  };

  static_assert(sizeof(D) == BUG(16, 32), "sizeof D");
}


namespace more_tests
{
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
    int d1, d2;
  };

  static_assert(sizeof(D) == BUG(32, 48), "sizeof D");
  static_assert(alignof(D) == 16, "alignof D");
  static_assert(__builtin_offsetof(D, d1) == BUG32_64(20, 24, 32), "offsetof d1");
  static_assert(__builtin_offsetof(D, d2) == BUG32_64(24, 28, 36), "offsetof d2");


  struct B2
  {
    int b1, b2;
  };

  struct DB : B, B2
  { };

  static_assert(sizeof(DB) == BUG(32, 48), "sizeof DB");
  static_assert(alignof(DB) == 16, "alignof DB");
  static_assert(__builtin_offsetof(DB, b1) == BUG32_64(20, 24, 32), "offsetof b1");
  static_assert(__builtin_offsetof(DB, b2) == BUG32_64(24, 28, 36), "offsetof b2");
}
