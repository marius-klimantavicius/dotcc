//type:fp
//options:--ms_c++17:--ms_c++20 --microsoft_version 1944:--ms_c++17 --target win32:--ms_c++17 --target win64

#pragma pack(8)

namespace minimal
{
  #pragma pack(1)
  struct alignas(16) A {};
  struct B { A a; };  // 16-byte aligned despite #pragma in effect.
  struct D : B {};    // Ditto.
  struct E : D {};    // Ditto.
  static_assert(alignof(E) == 16, "");  // Now accepted in Microsoft mode.
}

namespace pod_align_16
{
  struct alignas(16) A
  { };

  struct B
  {
    A a;
    char c;
  };

  struct D : B
  { };

  struct E : D
  { };

  static_assert(sizeof(A) == 16, "");
  static_assert(alignof(A) == 16, "");

  static_assert(sizeof(B) == 32, "");
  static_assert(alignof(B) == 16, "");

  static_assert(sizeof(D) == 32, "");
  static_assert(alignof(D) == 16, "");

  static_assert(sizeof(E) == 32, "");
  static_assert(alignof(E) == 16, "");
}

namespace non_pod_align_16
{
  struct alignas(16) A
  { };

  struct B
  {
  private:
    A a;
    char c;
  };

  struct D : B
  { };

  struct E : D
  { };

  static_assert(sizeof(A) == 16, "");
  static_assert(alignof(A) == 16, "");

  static_assert(sizeof(B) == 32, "");
  static_assert(alignof(B) == 16, "");

  static_assert(sizeof(D) == 32, "");
  static_assert(alignof(D) == 16, "");

  static_assert(sizeof(E) == 32, "");
  static_assert(alignof(E) == 16, "");
}

namespace pod_align_4
{
  struct alignas(4) A
  { };

  struct B
  {
    A a;
    char c;
  };

  struct D : B
  { };

  struct E : D
  { };

  static_assert(sizeof(A) == 4, "");
  static_assert(alignof(A) == 4, "");

  static_assert(sizeof(B) == 8, "");
  static_assert(alignof(B) == 4, "");

  static_assert(sizeof(D) == 8, "");
  static_assert(alignof(D) == 4, "");

  static_assert(sizeof(E) == 8, "");
  static_assert(alignof(E) == 4, "");
}

namespace non_pod_align_4
{
  struct alignas(4) A
  { };

  struct B
  {
  private:
    A a;
    char c;
  };

  struct D : B
  { };

  struct E : D
  { };

  static_assert(sizeof(A) == 4, "");
  static_assert(alignof(A) == 4, "");

  static_assert(sizeof(B) == 8, "");
  static_assert(alignof(B) == 4, "");

  static_assert(sizeof(D) == 8, "");
  static_assert(alignof(D) == 4, "");

  static_assert(sizeof(E) == 8, "");
  static_assert(alignof(E) == 4, "");
}
